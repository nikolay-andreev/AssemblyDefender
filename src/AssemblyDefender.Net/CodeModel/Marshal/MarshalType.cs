using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Indicates how to marshal the data between managed and unmanaged code.
	/// </summary>
	/// <example>
	/// marshal(int32) marshal(bool[5]), marshal(bool[+1]), marshal(bool[7+1])
	/// </example>
	public abstract class MarshalType : CodeNode
	{
		#region Ctors

		protected MarshalType(CodeNode parent)
			: base(parent)
		{
		}

		#endregion

		#region Properties

		public abstract UnmanagedType UnmanagedType
		{
			get;
		}

		#endregion

		#region Methods

		public abstract void CopyTo(MarshalType copy);

		protected internal virtual void Load(IBinaryAccessor accessor)
		{
		}

		#endregion

		#region Static

		internal static MarshalType CreateNew(CodeNode parent, UnmanagedType type)
		{
			var marshalType = Create(parent, type);
			marshalType.IsNew = true;

			return marshalType;
		}

		private static MarshalType Create(CodeNode parent, UnmanagedType type)
		{
			switch (type)
			{
				case UnmanagedType.LPArray:
					return new LPArrayMarshalType(parent);

				case UnmanagedType.ByValArray:
					return new ByValArrayMarshalType(parent);

				case UnmanagedType.ByValTStr:
					return new ByValTStrMarshalType(parent);

				case UnmanagedType.SafeArray:
					return new SafeArrayMarshalType(parent);

				case UnmanagedType.CustomMarshaler:
					return new CustomMarshalType(parent);

				default:
					return new PrimitiveMarshalType(parent, type);
			}
		}

		internal static MarshalType Load(CodeNode parent, int parentToken)
		{
			var image = parent.Module.Image;

			int blobID;
			if (!image.GetFieldMarshalNativeTypeByParent(MetadataToken.CompressHasFieldMarshal(parentToken), out blobID))
				return null;

			using (var accessor = image.OpenBlob(blobID))
			{
				return Load(accessor, parent);
			}
		}

		private static MarshalType Load(IBinaryAccessor accessor, CodeNode parent)
		{
			var unmanagedType = (UnmanagedType)accessor.ReadByte();
			var marshalType = CreateNew(parent, unmanagedType);
			marshalType.Load(accessor);

			return marshalType;
		}

		#endregion
	}
}
