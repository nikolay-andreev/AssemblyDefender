using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class Resource : MemberNode, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadDataFromImageFlag = 1;
		private const int LoadDataFromStateFlag = 2;
		private string _name;
		private int _offset;
		private ResourceVisibilityFlags _visibility;
		private Signature _owner;
		private CustomAttributeCollection _customAttributes;
		private int _opFlags;

		#endregion

		#region Ctors

		protected internal Resource(Module module, int rid)
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
				_rid = _module.ResourceTable.Add(this);
			}
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value.NullIfEmpty();
				OnChanged();
			}
		}

		public int Offset
		{
			get { return _offset; }
			set
			{
				_offset = value;
				_opFlags = _opFlags.SetBitAtIndex(LoadDataFromImageFlag, false);
				OnChanged();
			}
		}

		public ResourceVisibilityFlags Visibility
		{
			get { return _visibility; }
			set
			{
				_visibility = value;
				OnChanged();
			}
		}

		/// <summary>
		/// AssemblyReference or FileReference
		/// </summary>
		public Signature Owner
		{
			get { return _owner; }
			set
			{
				_owner = value;
				_module.AddSignature(ref _owner);

				OnChanged();
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.ManifestResource, _rid);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public override MemberType MemberType
		{
			get { return MemberType.Resource; }
		}

		#endregion

		#region Methods

		public byte[] GetData()
		{
			if (_opFlags.IsBitAtIndexOn(LoadDataFromImageFlag))
			{
				// Get data from field rva.
				return GetDataFromImage() ?? BufferUtils.EmptyArray;
			}

			// Get data from state.
			if (_opFlags.IsBitAtIndexOn(LoadDataFromStateFlag))
			{
				return GetDataFromState() ?? BufferUtils.EmptyArray;
			}

			return BufferUtils.EmptyArray;
		}

		public void SetData(byte[] data)
		{
			_opFlags = _opFlags.SetBitAtIndex(LoadDataFromImageFlag, false);
			_opFlags = _opFlags.SetBitAtIndex(LoadDataFromStateFlag, (data != null && data.Length > 0));
			_module.ResourceBlob[_rid - 1] = data;

			OnChanged();
		}

		protected byte[] GetDataFromImage()
		{
			var image = _module.Image;
			if (image == null)
				return null;

			return image.GetManifestResourceData(_offset);
		}

		protected byte[] GetDataFromState()
		{
			return _module.ResourceBlob[_rid - 1];
		}

		public void CopyTo(Resource copy)
		{
			copy._name = _name;
			copy._offset = _offset;
			copy._visibility = _visibility;
			copy._owner = _owner;
			CustomAttributes.CopyTo(copy.CustomAttributes);

			{
				byte[] data = GetData();
				copy.SetData(data);
			}
		}

		protected internal override void OnDeleted()
		{
			_module.ResourceTable.Remove(_rid);
			_module.ResourceBlob[_rid - 1] = null;
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.OnChanged();
				_module.ResourceTable.Change(_rid);
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			ManifestResourceRow row;
			image.GetManifestResource(_rid, out row);

			_name = image.GetString(row.Name);
			_visibility = (ResourceVisibilityFlags)row.Flags;
			_offset = row.Offset;

			int implementationToken = MetadataToken.DecompressImplementation(row.Implementation);

			switch (MetadataToken.GetType(implementationToken))
			{
				case MetadataTokenType.AssemblyRef:
					{
						int rid = MetadataToken.GetRID(implementationToken);
						_owner = AssemblyReference.LoadRef(_module, rid);
					}
					break;

				case MetadataTokenType.File:
					{
						int rid = MetadataToken.GetRID(implementationToken);
						_owner = FileReference.Load(_module, rid);
					}
					break;

				default:
					{
						_opFlags = _opFlags.SetBitAtIndex(LoadDataFromImageFlag, true);
					}
					break;
			}
		}

		#endregion
	}
}
