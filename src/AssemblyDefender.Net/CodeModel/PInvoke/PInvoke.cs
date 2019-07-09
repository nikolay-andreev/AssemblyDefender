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
	public class PInvoke : CodeNode
	{
		#region Fields

		private string _importName;
		private ModuleReference _importScope;
		private int _flags;

		#endregion

		#region Ctors

		internal PInvoke(CodeNode parent, int rid)
			: base(parent)
		{
			if (rid > 0)
			{
				Load(rid);
			}
			else
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the DLL file that contains the entry point.
		/// </summary>
		public string ImportName
		{
			get { return _importName; }
			set
			{
				_importName = value;
				OnChanged();
			}
		}

		/// <summary>
		/// Indicates the name or ordinal of the DLL entry point to be called.
		/// </summary>
		public ModuleReference ImportScope
		{
			get { return _importScope; }
			set
			{
				_importScope = value;
				_module.AddSignature(ref _importScope);

				OnChanged();
			}
		}

		/// <summary>
		/// Indicates the calling convention of an entry point.
		/// </summary>
		public UnmanagedCallingConvention CallConv
		{
			get { return (UnmanagedCallingConvention)_flags.GetBits(ImplMapFlags.CallConvMask); }
			set
			{
				_flags = _flags.SetBits(ImplMapFlags.CallConvMask, (int)value);
				OnChanged();
			}
		}

		/// <summary>
		/// Indicates how to marshal string parameters to the method and controls name mangling.
		/// </summary>
		public UnmanagedCharSet CharSet
		{
			get { return (UnmanagedCharSet)_flags.GetBits(ImplMapFlags.CharSetMask); }
			set
			{
				_flags = _flags.SetBits(ImplMapFlags.CharSetMask, (int)value);
				OnChanged();
			}
		}

		/// <summary>
		/// Enables or disables best-fit mapping behavior when converting Unicode characters to ANSI characters.
		/// </summary>
		public bool? BestFitMapping
		{
			get
			{
				if (_flags.IsBitsOn(ImplMapFlags.BestFitOn))
					return true;
				else if (_flags.IsBitsOn(ImplMapFlags.BestFitOff))
					return false;
				else
					return null;
			}
			set
			{
				if (!value.HasValue)
					_flags = _flags.SetBits(ImplMapFlags.BestFitOn, false).SetBits(ImplMapFlags.BestFitOff, false);
				else if (value.Value)
					_flags = _flags.SetBits(ImplMapFlags.BestFitOn, true).SetBits(ImplMapFlags.BestFitOff, false);
				else
					_flags = _flags.SetBits(ImplMapFlags.BestFitOn, false).SetBits(ImplMapFlags.BestFitOff, true);

				OnChanged();
			}
		}

		/// <summary>
		/// Controls whether the System.Runtime.InteropServices.DllImportAttribute. CharSet field causes the
		/// common language runtime to search an unmanaged DLL for entry-point names other than the one specified.
		/// </summary>
		public bool ExactSpelling
		{
			get { return _flags.IsBitsOn(ImplMapFlags.NoMangle); }
			set
			{
				_flags = _flags.SetBits(ImplMapFlags.NoMangle, value);
				OnChanged();
			}
		}

		/// <summary>
		/// Indicates whether the callee calls the SetLastError Win32 API function before returning from the
		/// attributed method.
		/// </summary>
		public bool SetLastError
		{
			get { return _flags.IsBitsOn(ImplMapFlags.LastError); }
			set
			{
				_flags = _flags.SetBits(ImplMapFlags.LastError, value);
				OnChanged();
			}
		}

		/// <summary>
		/// Enables or disables the throwing of an exception on an unmappable Unicode character that is
		/// converted to an ANSI "?" character.
		/// </summary>
		public bool? ThrowOnUnmappableChar
		{
			get
			{
				if (_flags.IsBitsOn(ImplMapFlags.CharMapErrorOn))
					return true;
				else if (_flags.IsBitsOn(ImplMapFlags.CharMapErrorOff))
					return false;
				else
					return null;
			}
			set
			{
				if (!value.HasValue)
					_flags = _flags.SetBits(ImplMapFlags.CharMapErrorOn, false).SetBits(ImplMapFlags.CharMapErrorOff, false);
				else if (value.Value)
					_flags = _flags.SetBits(ImplMapFlags.CharMapErrorOn, true).SetBits(ImplMapFlags.CharMapErrorOff, false);
				else
					_flags = _flags.SetBits(ImplMapFlags.CharMapErrorOn, false).SetBits(ImplMapFlags.CharMapErrorOff, true);

				OnChanged();
			}
		}

		#endregion

		#region Methods

		public void CopyTo(PInvoke copy)
		{
			copy._importName = _importName;
			copy._importScope = _importScope;
			copy._flags = _flags;
		}

		private void Load(int rid)
		{
			var image = _module.Image;

			ImplMapRow row;
			image.GetImplMap(rid, out row);

			_importName = image.GetString(row.ImportName);
			_importScope = ModuleReference.LoadRef(_module, row.ImportScope);
			_flags = row.MappingFlags;
		}

		#endregion

		#region Static

		internal static PInvoke Load(CodeNode parent, int ownerToken)
		{
			var image = parent.Module.Image;

			int rid;
			if (!image.GetImplMapByMemberForwarded(MetadataToken.CompressMemberForwarded(ownerToken), out rid))
				return null;

			return new PInvoke(parent, rid);
		}

		#endregion
	}
}
