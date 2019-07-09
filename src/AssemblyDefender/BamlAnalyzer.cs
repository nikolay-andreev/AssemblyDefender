using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Resources;

namespace AssemblyDefender
{
	public class BamlAnalyzer
	{
		#region Fields

		private State _state;
		private BuildAssembly _assembly;

		#endregion

		#region Ctors

		private BamlAnalyzer(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
		}

		#endregion

		#region Methods

		private void Analyze()
		{
			bool hasWpfResource = false;

			string resourceName = BamlUtils.GetWpfResourceName(_assembly.Name);

			foreach (BuildResource resource in _assembly.Resources)
			{
				if (resource.Name != resourceName)
					continue;

				resource.IsWpf = true;
				hasWpfResource = true;

				bool hasBaml = false;

				var data = resource.GetData();

				if (!ResourceUtils.IsResource(data))
					continue;

				using (var reader = new ResourceReaderEx(data))
				{
					while (reader.Read())
					{
						if (reader.TypeCode != ResourceTypeCode.Stream && reader.TypeCode != ResourceTypeCode.ByteArray)
							continue;

						if (0 != string.Compare(Path.GetExtension(reader.Name), BamlImage.FileExtension, true))
							continue;

						byte[] resourceData = reader.Data;
						var resourceDataStream = new MemoryStream(resourceData);
						resourceDataStream.Position += 4;

						var bamlImage = BamlImage.Load(resourceDataStream);
						if (bamlImage == null)
							continue;

						Process(bamlImage);

						hasBaml = true;
					}
				}

				if (hasBaml)
				{
					resource.HasWpfBaml = true;
				}
			}

			if (hasWpfResource)
			{
				_assembly.HasWpfResource = true;
			}
		}

		private void Process(BamlImage bamlImage)
		{
			_state = new State();
			_state.Node = bamlImage.FirstNode;

			ProcessNodes();

			_state = null;
		}

		private void ProcessNodes()
		{
			while (_state.Node != null)
			{
				switch (_state.Node.NodeType)
				{
					case BamlNodeType.Element:
						ProcessElement();
						break;

					case BamlNodeType.TypeInfo:
						ProcessTypeDeclaration();
						break;

					case BamlNodeType.AttributeInfo:
						ProcessMemberDeclaration();
						break;

					case BamlNodeType.PropertyWithConverter:
						ProcessPropertyWithConverter();
						break;
				}

				MoveNext();
			}
		}

		private void ProcessElement()
		{
			var bamlElement = (BamlElement)_state.Node;

			if (_state.RootElementType == null)
			{
				_state.RootElementType = Resolve(bamlElement.Type);
			}
		}

		private void ProcessTypeDeclaration()
		{
			var bamlType = (BamlTypeInfo)_state.Node;

			var type = bamlType.Resolve(_assembly) as BuildType;
			if (type != null)
			{
				type.Strip = false;
			}
		}

		private void ProcessMemberDeclaration()
		{
			var bamlProperty = (BamlPropertyInfo)_state.Node;

			var ownerType = bamlProperty.Type.Resolve(_assembly) as BuildType;
			if (ownerType == null)
				return;

			var field = ownerType.Fields.Find(bamlProperty.Name) as BuildField;
			if (field != null)
			{
				field.Strip = false;
			}

			var property = ownerType.Properties.Find(bamlProperty.Name) as BuildProperty;
			if (property != null)
			{
				property.Strip = false;
			}

			var e = ownerType.Events.Find(bamlProperty.Name) as BuildEvent;
			if (e != null)
			{
				e.Strip = false;
			}
		}

		private void ProcessPropertyWithConverter()
		{
			var bamlPropertyWithConverter = (BamlPropertyWithConverter)_state.Node;
			if (string.IsNullOrEmpty(bamlPropertyWithConverter.Value))
				return;

			if (IsType(bamlPropertyWithConverter.ConverterType, BamlKnownTypeCode.StringConverter))
			{
				if (_state.RootElementType != null)
				{
					ProcessPropertyWithConverterEvent(bamlPropertyWithConverter);
				}
			}
		}

		private void ProcessPropertyWithConverterEvent(BamlPropertyWithConverter bamlPropertyWithConverter)
		{
			var rootElementType = _state.RootElementType as BuildType;
			if (rootElementType == null)
				return;

			string name = bamlPropertyWithConverter.Value;

			foreach (BuildMethod method in rootElementType.Methods)
			{
				if (method.Name != name)
					continue;

				if (method.Strip)
				{
					method.Strip = false;
				}
			}
		}

		private TypeDeclaration Resolve(IBamlType bamlType)
		{
			return bamlType.Resolve(_assembly);
		}

		private bool IsType(IBamlType bamlType, BamlKnownTypeCode typeCode)
		{
			var bamlKnownType = bamlType as BamlKnownType;
			if (bamlKnownType == null)
				return false;

			if (bamlKnownType.KnownCode != typeCode)
				return false;

			return true;
		}

		private bool IsProperty(IBamlProperty bamlProperty, string name, BamlKnownTypeCode typeCode)
		{
			var bamlPropertyInfo = bamlProperty as BamlPropertyInfo;
			if (bamlPropertyInfo == null)
				return false;

			if (bamlPropertyInfo.Name != name)
				return false;

			if (!IsType(bamlPropertyInfo.Type, typeCode))
				return false;

			return true;
		}

		private bool MoveNext()
		{
			var node = _state.Node;
			while (true)
			{
				BamlNode nextNode = null;
				if (node.IsBlock)
				{
					nextNode = ((BamlBlock)node).FirstChild;
				}

				if (nextNode == null)
				{
					while (node.NextSibling == null && node.Parent != null)
					{
						node = node.Parent;
					}

					nextNode = node.NextSibling;
				}

				if (nextNode != null)
				{
					if (IsSkipNode(nextNode))
					{
						node = nextNode;
						continue;
					}

					_state.Node = nextNode;
					return true;
				}
				else
				{
					_state.Node = null;
					return false;
				}
			}
		}

		private bool MoveNext(BamlNodeType type)
		{
			var next = GetNext(_state.Node);
			if (next == null)
				return false;

			if (next.NodeType != type)
				return false;

			MoveNext();
			return true;
		}

		private bool MoveFirstChild(BamlNodeType type)
		{
			var node = _state.Node;
			if (!node.IsBlock)
				return false;

			var nextNode = ((BamlBlock)node).FirstChild;
			while (nextNode != null && IsSkipNode(nextNode))
			{
				nextNode = nextNode.NextSibling;
			}

			if (nextNode == null)
				return false;

			if (nextNode.NodeType != type)
				return false;

			MoveNext();
			return true;
		}

		private BamlNode GetNext(BamlNode node)
		{
			while (true)
			{
				BamlNode nextNode = null;
				if (node.IsBlock)
				{
					nextNode = ((BamlBlock)node).FirstChild;
				}

				if (nextNode == null)
				{
					while (node.NextSibling == null && node.Parent != null)
					{
						node = node.Parent;
					}

					nextNode = node.NextSibling;
				}

				if (nextNode != null)
				{
					if (IsSkipNode(nextNode))
					{
						node = nextNode;
						continue;
					}

					return nextNode;
				}
				else
				{
					return null;
				}
			}
		}

		private bool IsSkipNode(BamlNode node)
		{
			return
				node.NodeType == BamlNodeType.LineNumberAndPosition ||
				node.NodeType == BamlNodeType.LinePosition;
		}

		#endregion

		#region Static

		public static void Analyze(BuildAssembly assembly)
		{
			var analyzer = new BamlAnalyzer(assembly);
			analyzer.Analyze();
		}

		#endregion

		#region Nested types

		private class State
		{
			internal BamlNode Node;
			internal TypeDeclaration RootElementType;
		}

		#endregion
	}
}
