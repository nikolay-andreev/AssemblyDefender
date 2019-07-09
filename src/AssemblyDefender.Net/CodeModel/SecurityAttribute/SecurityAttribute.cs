using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class SecurityAttribute : MemberNode
	{
		#region Fields

		private bool _failedToLoad;
		private SecurityAction _action;
		private TypeReference _type;
		private string _xml;
		private CustomAttributeNamedArgumentCollection _namedArguments;

		#endregion

		#region Ctors

		protected internal SecurityAttribute(Module module, int rid)
			: base(module)
		{
			_rid = rid;

			if (_rid > 0)
			{
				Load();
			}
			else
			{
				IsNew = true;
				_rid = _module.SecurityAttributeTable.Add(this);
			}
		}

		#endregion

		#region Properties

		public SecurityAction Action
		{
			get { return _action; }
			set
			{
				_action = value;
				OnChanged();
			}
		}

		public TypeReference Type
		{
			get { return _type; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Type");

				_type = value;
				_module.AddSignature(ref _type);

				OnChanged();
			}
		}

		public CustomAttributeNamedArgumentCollection NamedArguments
		{
			get
			{
				if (_namedArguments == null)
				{
					_namedArguments = new CustomAttributeNamedArgumentCollection(this);
				}

				return _namedArguments;
			}
		}

		public string Xml
		{
			get { return _xml; }
			set
			{
				_xml = value;
				OnChanged();
			}
		}

		public bool FailedToLoad
		{
			get { return _failedToLoad; }
		}

		public override MemberType MemberType
		{
			get { return MemberType.SecurityAttribute; }
		}

		#endregion

		#region Methods

		public byte[] GetRawData()
		{
			if (_rid == 0)
				return null;

			var image = _module.Image;

			DeclSecurityRow row;
			int offset;
			int size;
			image.GetSecurityAttribute(_rid, out row, out offset, out size);

			return image.GetBlob(row.PermissionSet, offset, size);
		}

		public void CopyTo(SecurityAttribute copy)
		{
			copy._action = _action;
			copy._type = _type;
			copy._xml = _xml;
			NamedArguments.CopyTo(copy.NamedArguments);
		}

		protected internal override void OnDeleted()
		{
			_module.SecurityAttributeTable.Remove(_rid);
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.SecurityAttributeTable.Change(_rid);
				_module.OnChanged();
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			DeclSecurityRow row;
			int offset;
			int size;
			image.GetSecurityAttribute(_rid, out row, out offset, out size);

			_action = row.Action;

			if (offset == 0)
			{
				_xml = Encoding.Unicode.GetString(image.GetBlob(row.PermissionSet));
			}
			else
			{
				try
				{
					using (var accessor = image.OpenBlob(row.PermissionSet))
					{
						accessor.Position += offset;
						Load(accessor);
					}
				}
				catch (Exception)
				{
					// Blob is not self descriptive. In case of enum we need to resolve reference to
					// enum type in order to get enum underlying type (int16, int32, ...). If enum type
					// could not be resolved exception is thrown.
					_failedToLoad = true;
				}
			}
		}

		protected void Load(IBinaryAccessor accessor)
		{
			int typeLength = accessor.ReadCompressedInteger();
			string typeName = accessor.ReadString(typeLength, Encoding.UTF8);
			_type = TypeSignature.Parse(typeName, true) as TypeReference;
			if (_type == null)
			{
				throw new InvalidDataException();
			}

			_type = (TypeReference)_type.Relocate(_module);

			accessor.ReadCompressedInteger(); // Blob size

			int argumentCount = accessor.ReadCompressedInteger();

			_namedArguments = new CustomAttributeNamedArgumentCollection(this);
			_namedArguments.Load(accessor, argumentCount);
		}

		#endregion
	}
}
