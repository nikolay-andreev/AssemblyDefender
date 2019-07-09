using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Baml.Parser;

namespace AssemblyDefender.Baml
{
	public class XamlTypeName
	{
		#region Fields

		private string _name;
		private string _prefix;
		private int _subscript;
		private List<XamlTypeName> _typeArguments;

		#endregion

		#region Ctors

		public XamlTypeName()
		{
		}

		public XamlTypeName(string name)
			: this(name, null, null)
		{
		}

		public XamlTypeName(string name, string prefix)
			: this(name, prefix, null)
		{
		}

		public XamlTypeName(string name, string prefix, List<XamlTypeName> typeArguments)
		{
			_name = name;
			_prefix = prefix;
			_typeArguments = typeArguments;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public string Prefix
		{
			get { return _prefix; }
			set { _prefix = value; }
		}

		public int Subscript
		{
			get { return _subscript; }
			set { _subscript = value; }
		}

		public bool HasTypeArgs
		{
			get { return _typeArguments != null && _typeArguments.Count > 0; }
		}

		public List<XamlTypeName> TypeArguments
		{
			get
			{
				if (_typeArguments == null)
				{
					_typeArguments = new List<XamlTypeName>();
				}

				return _typeArguments;
			}
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			var sb = new StringBuilder();
			Print(sb);

			return sb.ToString();
		}

		private void Print(StringBuilder sb)
		{
			if (!string.IsNullOrEmpty(_prefix))
			{
				sb.Append(_prefix);
				sb.Append(":");
			}

			if (HasTypeArgs)
			{
				sb.Append(_name);
				sb.Append("(");
				Print(sb, _typeArguments);
				sb.Append(")");

				if (_subscript > 0)
				{
					sb.Append("[");

					for (int i = 1; i < _subscript; i++)
					{
						sb.Append(",");
					}

					sb.Append("]");
				}

			}
			else
			{
				sb.Append(_name);
			}
		}

		#endregion

		#region Static

		public static string ToString(IEnumerable<XamlTypeName> typeNameList)
		{
			var sb = new StringBuilder();
			Print(sb, typeNameList);

			return sb.ToString();
		}

		public static XamlTypeName Parse(string typeName)
		{
			return XamlTypeNameParser.Parse(typeName);
		}

		public static IList<XamlTypeName> ParseList(string typeNameList)
		{
			return XamlTypeNameParser.ParseList(typeNameList);
		}

		private static void Print(StringBuilder sb, IEnumerable<XamlTypeName> typeNameList)
		{
			bool isFirst = true;

			foreach (var name in typeNameList)
			{
				if (isFirst)
					isFirst = false;
				else
					sb.Append(", ");

				name.Print(sb);
			}
		}

		#endregion
	}
}
