using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Net.Fusion.UnmanagedApi;

namespace AssemblyDefender.Net.Fusion
{
	public class AssemblyName
	{
		#region Fields

		private UnmanagedApi.IAssemblyName _assemblyName;

		#endregion

		#region Ctors

		public AssemblyName()
		{
			HRESULT.ThrowOnFailure(FusionNative.CreateAssemblyNameObject(out _assemblyName, null, 0, IntPtr.Zero));
		}

		public AssemblyName(string fullName)
		{
			HRESULT.ThrowOnFailure(FusionNative.CreateAssemblyNameObject(out _assemblyName, null, 0, IntPtr.Zero));
			this.FullName = fullName;
		}

		public AssemblyName(UnmanagedApi.IAssemblyName assemblyName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");

			_assemblyName = assemblyName;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return GetStringProperty((int)ASM_NAME_PROPERTY.NAME); }
			set { SetStringProperty((int)ASM_NAME_PROPERTY.NAME, value); }
		}

		public string Culture
		{
			get { return GetStringProperty((int)ASM_NAME_PROPERTY.CULTURE); }
			set { SetStringProperty((int)ASM_NAME_PROPERTY.CULTURE, value); }
		}

		public string CodeBase
		{
			get { return GetStringProperty((int)ASM_NAME_PROPERTY.CODEBASE_URL); }
			set { SetStringProperty((int)ASM_NAME_PROPERTY.CODEBASE_URL, value); }
		}

		public short MajorVersion
		{
			get { return GetInt16Property((int)ASM_NAME_PROPERTY.MAJOR_VERSION); }
			set { SetInt16Property((int)ASM_NAME_PROPERTY.MAJOR_VERSION, value); }
		}

		public short MinorVersion
		{
			get { return GetInt16Property((int)ASM_NAME_PROPERTY.MINOR_VERSION); }
			set { SetInt16Property((int)ASM_NAME_PROPERTY.MINOR_VERSION, value); }
		}

		public short BuildNumber
		{
			get { return GetInt16Property((int)ASM_NAME_PROPERTY.BUILD_NUMBER); }
			set { SetInt16Property((int)ASM_NAME_PROPERTY.BUILD_NUMBER, value); }
		}

		public short RevisionNumber
		{
			get { return GetInt16Property((int)ASM_NAME_PROPERTY.REVISION_NUMBER); }
			set { SetInt16Property((int)ASM_NAME_PROPERTY.REVISION_NUMBER, value); }
		}

		public byte[] PublicKey
		{
			get { return GetByteArrayProperty((int)ASM_NAME_PROPERTY.PUBLIC_KEY); }
			set { SetByteArrayProperty((int)ASM_NAME_PROPERTY.PUBLIC_KEY, value); }
		}

		public byte[] PublicKeyToken
		{
			get { return GetByteArrayProperty((int)ASM_NAME_PROPERTY.PUBLIC_KEY_TOKEN); }
			set { SetByteArrayProperty((int)ASM_NAME_PROPERTY.PUBLIC_KEY_TOKEN, value); }
		}

		public string FullName
		{
			get
			{
				return GetDisplayName(
					ASM_DISPLAY_FLAGS.VERSION |
					ASM_DISPLAY_FLAGS.CULTURE |
					ASM_DISPLAY_FLAGS.PUBLIC_KEY_TOKEN);
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					var assemblyRef = AssemblyReference.Parse(value);
					this.Name = assemblyRef.Name;
					this.Culture = assemblyRef.Culture;
					this.Version = assemblyRef.Version;
					this.PublicKeyToken = assemblyRef.PublicKeyToken;
				}
			}
		}

		public Version Version
		{
			get
			{
				return new Version(MajorVersion, MinorVersion, BuildNumber, RevisionNumber);
			}
			set
			{
				if (value != null)
				{
					MajorVersion = (short)value.Major;
					MinorVersion = (short)value.Minor;
					BuildNumber = (short)value.Build;
					RevisionNumber = (short)value.Revision;
				}
				else
				{
					MajorVersion = 0;
					MinorVersion = 0;
					BuildNumber = 0;
					RevisionNumber = 0;
				}
			}
		}

		public UnmanagedApi.IAssemblyName IAssemblyName
		{
			get { return _assemblyName; }
		}

		#endregion

		#region Methods

		public string GetDisplayName(ASM_DISPLAY_FLAGS flags)
		{
			int size = 0;
			_assemblyName.GetDisplayName(null, ref size, (uint)flags);
			if (size <= 0)
				return null;

			var builder = new StringBuilder(size);
			_assemblyName.GetDisplayName(builder, ref size, (uint)flags);

			return builder.ToString();
		}

		private string GetStringProperty(int propertyID)
		{
			int size = 0;
			_assemblyName.GetProperty(propertyID, IntPtr.Zero, ref size);

			if (size == 0)
				return null;

			IntPtr ptr = IntPtr.Zero;
			try
			{
				ptr = Marshal.AllocHGlobal(size);

				_assemblyName.GetProperty(propertyID, ptr, ref size);

				return Marshal.PtrToStringUni(ptr);
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		private void SetStringProperty(int propertyID, string value)
		{
			IntPtr ptr = IntPtr.Zero;
			try
			{
				if (value != null)
					value += "\0";
				else
					value = "";

				int size = Encoding.Unicode.GetByteCount(value);
				ptr = Marshal.StringToHGlobalUni(value);

				_assemblyName.SetProperty(propertyID, ptr, size);
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		private short GetInt16Property(int propertyID)
		{
			int size = 0;
			_assemblyName.GetProperty(propertyID, IntPtr.Zero, ref size);

			if (size == 0)
				return 0;

			IntPtr ptr = IntPtr.Zero;
			try
			{
				ptr = Marshal.AllocHGlobal(size);

				_assemblyName.GetProperty(propertyID, ptr, ref size);

				return Marshal.ReadInt16(ptr);
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		private void SetInt16Property(int propertyID, short value)
		{
			IntPtr ptr = IntPtr.Zero;
			try
			{
				ptr = Marshal.AllocHGlobal(2);
				Marshal.WriteInt16(ptr, value);

				_assemblyName.SetProperty(propertyID, ptr, 2);
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		private byte[] GetByteArrayProperty(int propertyID)
		{
			int size = 0;
			_assemblyName.GetProperty(propertyID, IntPtr.Zero, ref size);

			if (size == 0)
				return null;

			IntPtr ptr = IntPtr.Zero;
			try
			{
				ptr = Marshal.AllocHGlobal(size);

				_assemblyName.GetProperty(propertyID, ptr, ref size);

				byte[] value = new byte[size];
				Marshal.Copy(ptr, value, 0, size);

				return value;
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		private void SetByteArrayProperty(int propertyID, byte[] value)
		{
			IntPtr ptr = IntPtr.Zero;
			try
			{
				int size = value != null ? value.Length : 0;

				ptr = Marshal.AllocHGlobal(size);
				Marshal.Copy(value, 0, ptr, size);

				_assemblyName.SetProperty(propertyID, ptr, size);
			}
			finally
			{
				if (ptr != IntPtr.Zero)
					Marshal.FreeHGlobal(ptr);
			}
		}

		#endregion
	}
}
