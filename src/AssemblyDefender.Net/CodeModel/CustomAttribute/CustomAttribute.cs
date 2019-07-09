using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class CustomAttribute : MemberNode
	{
		#region Fields

		private bool _failedToLoad;
		private MethodReference _constructor;
		private CustomAttributeCtorArgumentCollection _ctorArguments;
		private CustomAttributeNamedArgumentCollection _namedArguments;

		#endregion

		#region Ctors

		protected CustomAttribute(Module module)
			: base(module)
		{
		}

		protected internal CustomAttribute(Module module, int rid)
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
				_rid = _module.CustomAttributeTable.Add(this);
			}
		}

		#endregion

		#region Properties

		public bool FailedToLoad
		{
			get { return _failedToLoad; }
		}

		public MethodReference Constructor
		{
			get { return _constructor; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Constructor");

				_constructor = value;
				_module.AddSignature(ref _constructor);

				OnChanged();
			}
		}

		public CustomAttributeCtorArgumentCollection CtorArguments
		{
			get
			{
				if (_ctorArguments == null)
				{
					_ctorArguments = new CustomAttributeCtorArgumentCollection(this);
				}

				return _ctorArguments;
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

		public override MemberType MemberType
		{
			get { return MemberType.CustomAttribute; }
		}

		#endregion

		#region Methods

		public byte[] GetRawData()
		{
			if (_rid == 0)
				return null;

			var image = _module.Image;

			CustomAttributeRow row;
			image.GetCustomAttribute(_rid, out row);

			return image.GetBlob(row.Value);
		}

		public void CopyTo(CustomAttribute copy)
		{
			copy._constructor = _constructor;
			CtorArguments.CopyTo(copy.CtorArguments);
			NamedArguments.CopyTo(copy.NamedArguments);
		}

		protected internal override void OnDeleted()
		{
			_module.CustomAttributeTable.Remove(_rid);
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.CustomAttributeTable.Change(_rid);
				_module.OnChanged();
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			CustomAttributeRow row;
			image.GetCustomAttribute(_rid, out row);

			_constructor = MethodReference.LoadMethodDefOrRef(_module, MetadataToken.DecompressCustomAttributeType(row.Type));

			try
			{
				using (var accessor = image.OpenBlob(row.Value))
				{
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

		protected void Load(IBinaryAccessor accessor)
		{
			// The encoded blob begins with the prolog, which is always the 2-byte value 0x0001.
			// This is actually the version of the custom attribute blob encoding scheme, which hasn't changed
			// since its introduction, so the prolog is the same for all existing versions of the runtime.
			short prolog = accessor.ReadInt16();
			if (prolog != 1)
			{
				throw new CodeModelException(string.Format(SR.AssemblyLoadError, _module.Location));
			}

			// Ctor arguments.
			_ctorArguments = new CustomAttributeCtorArgumentCollection(this);
			_ctorArguments.Load(accessor);

			// Named arguments.
			_namedArguments = new CustomAttributeNamedArgumentCollection(this);
			_namedArguments.Load(accessor);
		}

		#endregion
	}
}
