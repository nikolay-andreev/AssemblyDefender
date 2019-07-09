using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;
using AssemblyDefender.Common.Serialization;

namespace AssemblyDefender
{
	public class ModuleState
	{
		#region Fields

		private bool _isNew;
		private bool _isChanged;
		private int _moduleTableOffset;
		private int _typeTableOffset;
		private int _methodTableOffset;
		private int _fieldTableOffset;
		private int _propertyTableOffset;
		private int _eventTableOffset;
		private int _resourceTableOffset;
		private int _bufferLength;
		private string _outputPath;
		private string _outputFilePath;
		private string _stateFilePath;
		private string _buildFilePath;
		private string _strongNameKeyFilePath;
		private string _strongNameKeyPassword;
		private byte[] _buffer;
		private BlobList _blobs;
		private StringSerializer _strings;
		private SignatureSerializer _signatures;
		private ModuleObjectState _objects;
		private BuildModule _module;
		private List<string> _signFiles;

		#endregion

		#region Ctors

		internal ModuleState()
		{
			_isNew = true;
		}

		#endregion

		#region Properties

		internal bool IsNew
		{
			get { return _isNew; }
		}

		internal bool IsChanged
		{
			get { return _isChanged; }
		}

		internal string OutputPath
		{
			get { return _outputPath; }
			set { _outputPath = value; }
		}

		internal string OutputFilePath
		{
			get { return _outputFilePath; }
		}

		internal string StateFilePath
		{
			get { return _stateFilePath; }
		}

		internal string BuildFilePath
		{
			get { return _buildFilePath; }
		}

		internal bool SignAssembly
		{
			get { return _strongNameKeyFilePath != null; }
		}

		internal string StrongNameKeyFilePath
		{
			get { return _strongNameKeyFilePath; }
			set { _strongNameKeyFilePath = value; }
		}

		internal string StrongNameKeyPassword
		{
			get { return _strongNameKeyPassword; }
			set { _strongNameKeyPassword = value; }
		}

		internal BuildModule Module
		{
			get { return _module; }
		}

		internal List<string> SignFiles
		{
			get
			{
				if (_signFiles == null)
				{
					_signFiles = new List<string>();
				}

				return _signFiles;
			}
		}

		#endregion

		#region Methods

		public byte[] GetBlob(int rid)
		{
			if (rid == 0)
				return null;

			return _blobs[rid - 1];
		}

		public IBinaryAccessor OpenBlob(int rid)
		{
			if (rid == 0)
				return null;

			return _blobs.Open(rid - 1);
		}

		public int AddBlob(byte[] value)
		{
			_blobs.Add(value);
			return _blobs.Count;
		}

		public bool SetBlob(ref int rid, byte[] value)
		{
			return SetBlob(ref rid, value, 0, value != null ? value.Length : 0);
		}

		public bool SetBlob(ref int rid, byte[] value, int index, int count)
		{
			if (count > 0)
			{
				if (rid > 0)
				{
					_blobs[rid - 1] = value;
					return false;
				}
				else
				{
					_blobs.Add(value);
					rid = _blobs.Count;
					return true;
				}
			}
			else
			{
				if (rid > 0)
				{
					_blobs.RemoveAt(rid - 1);
					rid = 0;
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public string GetString(int rid)
		{
			return _strings[rid - 1];
		}

		public int AddString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return 0;

			return _strings.Add(value) + 1;
		}

		public ISignature GetSignature(int rid)
		{
			if (rid == 0)
				return null;

			return _signatures[rid - 1];
		}

		public int AddSignature(ISignature value)
		{
			if (value == null)
				return 0;

			return _signatures.Add(value) + 1;
		}

		internal T GetObject<T>(int rid, bool throwIfMissing = false)
			where T : StateObject, new()
		{
			if (rid > 0)
			{
				return _objects.Get<T>(rid - 1);
			}

			if (throwIfMissing)
			{
				throw new InvalidOperationException();
			}

			return null;
		}

		internal int AddObject(StateObject item)
		{
			if (item == null)
				return 0;

			if (item.RID > 0)
				return item.RID;

			return _objects.Add(item) + 1;
		}

		internal void Save()
		{
			if (!_isChanged && !_isNew)
				return;

			if (_isNew)
			{
				// Set paths
				string fileName = _module.NameChanged ? _module.NewName : _module.Name;

				var assembly = (BuildAssembly)_module.Assembly;
				string outputPath = assembly.OutputPath;
				if (outputPath == null)
					outputPath = Path.GetDirectoryName(assembly.Location);

				_outputFilePath = Path.Combine(outputPath, fileName);
				_stateFilePath = _outputFilePath + ".adstate";
				_buildFilePath = _outputFilePath + ".adbuild";
			}

			using (var accessor = new StreamAccessor(new FileStream(_stateFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
			{
				accessor.Write7BitEncodedInt(_bufferLength);
				accessor.Write(_buffer, 0, _bufferLength);

				_objects.Serialize(accessor);

				_blobs.Serialize(accessor);

				var signatureBlob = new Blob();
				_signatures.Serialize(new BlobAccessor(signatureBlob));

				var stringBlob = new Blob();
				_strings.Serialize(new BlobAccessor(stringBlob));
				StrongCryptoUtils.Encrypt(stringBlob.GetBuffer(), 0, stringBlob.Length);
				accessor.Write7BitEncodedInt(stringBlob.Length);
				accessor.Write(stringBlob.GetBuffer(), 0, stringBlob.Length);

				accessor.Write(signatureBlob.GetBuffer(), 0, signatureBlob.Length);
			}

			_isNew = false;
			_isChanged = false;
		}

		internal void Load(BuildModule module)
		{
			if (_module != null)
				throw new InvalidOperationException();

			_module = module;

			if (_isNew)
			{
				if (_bufferLength == 0)
				{
					var image = _module.Image;

					_moduleTableOffset = BuildAssembly.RowSize * image.GetAssemblyCount();
					_typeTableOffset = _moduleTableOffset + BuildModule.RowSize;
					_methodTableOffset = _typeTableOffset + (BuildType.RowSize * image.GetTypeDefCount());
					_fieldTableOffset = _methodTableOffset + (BuildMethod.RowSize * image.GetMethodCount());
					_propertyTableOffset = _fieldTableOffset + (BuildField.RowSize * image.GetFieldCount());
					_eventTableOffset = _propertyTableOffset + (BuildProperty.RowSize * image.GetPropertyCount());
					_resourceTableOffset = _eventTableOffset + (BuildEvent.RowSize * image.GetEventCount());
					_bufferLength = _resourceTableOffset + (BuildResource.RowSize * image.GetManifestResourceCount());
				}

				_buffer = new byte[_bufferLength];
				_blobs = new BlobList();
				_strings = new StringSerializer();
				_signatures = new SignatureList(_strings);
				_objects = new ModuleObjectState(this);
			}
			else
			{
				using (var accessor = new StreamAccessor(new FileStream(_stateFilePath, FileMode.Open, FileAccess.Read, FileShare.None)))
				{
					_bufferLength = accessor.Read7BitEncodedInt();
					_buffer = accessor.ReadBytes(_bufferLength);

					_objects = new ModuleObjectState(this, accessor);

					_blobs = new BlobList(accessor);

					var stringBlob = new Blob(accessor.ReadBytes(accessor.Read7BitEncodedInt()));
					StrongCryptoUtils.Decrypt(stringBlob.GetBuffer(), 0, stringBlob.Length);
					_strings = new StringSerializer(new BlobAccessor(stringBlob));

					_signatures = new SignatureList(accessor, _strings);
				}
			}

			_strings.DelayWrite = true;
			_signatures.DelayWrite = true;
		}

		internal void Close()
		{
			_buffer = null;
			_blobs = null;
			_strings = null;
			_signatures = null;
			_module = null;
		}

		internal void Scavenge()
		{
			_objects.Scavenge();
		}

		internal void OnChanged()
		{
			_isChanged = true;
		}

		private void EnsureBuffer(int bufferLength)
		{
			if (bufferLength <= _bufferLength)
				return;

			if (bufferLength > _buffer.Length)
			{
				int capacity = _buffer.Length + 0x100;
				if (capacity < bufferLength)
					capacity = bufferLength;

				byte[] buffer = new byte[capacity];
				Buffer.BlockCopy(_buffer, 0, buffer, 0, _bufferLength);
				_buffer = buffer;
			}

			_bufferLength = bufferLength;
			_isChanged = true;
		}

		#endregion

		#region Table

		internal bool GetTableBit(int offset, int index)
		{
			return _buffer[offset].IsBitAtIndexOn(index);
		}

		internal void SetTableBit(int offset, int index, bool value)
		{
			_buffer[offset] = _buffer[offset].SetBitAtIndex(index, value);
			_isChanged = true;
		}

		internal byte GetTableByte(int offset)
		{
			return _buffer[offset];
		}

		internal void SetTableByte(int offset, byte value)
		{
			_buffer[offset] = value;
			_isChanged = true;
		}

		internal short GetTableInt16(int offset)
		{
			return BufferUtils.ReadInt16(_buffer, ref offset);
		}

		internal void SetTableInt16(int offset, short value)
		{
			BufferUtils.Write(_buffer, ref offset, (short)value);
			_isChanged = true;
		}

		internal int GetTableInt32(int offset)
		{
			return BufferUtils.ReadInt32(_buffer, ref offset);
		}

		internal void SetTableInt32(int offset, int value)
		{
			BufferUtils.Write(_buffer, ref offset, (int)value);
			_isChanged = true;
		}

		internal long GetTableInt64(int offset)
		{
			return BufferUtils.ReadInt64(_buffer, ref offset);
		}

		internal void SetTableInt64(int offset, long value)
		{
			BufferUtils.Write(_buffer, ref offset, (long)value);
			_isChanged = true;
		}

		internal string GetTableString(int offset)
		{
			int stringID = BufferUtils.ReadInt32(_buffer, ref offset);
			if (stringID == 0)
				return null;

			return _strings[stringID - 1];
		}

		internal void SetTableString(int offset, string value)
		{
			int stringID;
			if (!string.IsNullOrEmpty(value))
				stringID = _strings.Add(value) + 1;
			else
				stringID = 0;

			BufferUtils.Write(_buffer, ref offset, (int)stringID);
			_isChanged = true;
		}

		internal ISignature GetTableSignature(int offset)
		{
			int signatureID = BufferUtils.ReadInt32(_buffer, ref offset);
			if (signatureID == 0)
				return null;

			return _signatures[signatureID - 1];
		}

		internal void SetTableSignature(int offset, ISignature value)
		{
			int signatureID;
			if (value != null)
				signatureID = _signatures.Add(value) + 1;
			else
				signatureID = 0;

			BufferUtils.Write(_buffer, ref offset, (int)signatureID);
			_isChanged = true;
		}

		internal byte[] GetTableBlob(int offset)
		{
			int blobID = BufferUtils.ReadInt32(_buffer, ref offset);
			if (blobID == 0)
				return null;

			return _blobs[blobID - 1];
		}

		internal IBinaryAccessor OpenTableBlob(int offset)
		{
			int blobID = BufferUtils.ReadInt32(_buffer, ref offset);
			if (blobID == 0)
				return null;

			return _blobs.Open(blobID - 1);
		}

		internal void SetTableBlob(int offset, byte[] value)
		{
			SetTableBlob(offset, value, 0, value != null ? value.Length : 0);
		}

		internal void SetTableBlob(int offset, byte[] value, int index, int count)
		{
			int blobID = BufferUtils.ReadInt32(_buffer, ref offset);
			if (SetBlob(ref blobID, value, index, count))
			{
				offset -= 4;
				BufferUtils.Write(_buffer, ref offset, (int)blobID);
			}

			_isChanged = true;
		}

		internal T GetTableObject<T>(int offset)
			where T : StateObject, new()
		{
			int rid = GetTableInt32(offset);
			return GetObject<T>(rid);
		}

		internal T GetOrCreateTableObject<T>(int offset)
			where T : StateObject, new()
		{
			int rid = GetTableInt32(offset);
			if (rid > 0)
			{
				return GetObject<T>(rid);
			}
			else
			{
				var item = new T();
				AddObject(item);
				SetTableInt32(offset, item.RID);

				return item;
			}
		}

		internal void SetTableObject(int offset, StateObject item)
		{
			int rid = AddObject(item);
			SetTableInt32(offset, rid);
		}

		#endregion

		#region Offsets

		internal int GetModuleOffset()
		{
			return _moduleTableOffset;
		}

		internal int GetTypeOffset(int rid)
		{
			if (rid > 0)
			{
				return _typeTableOffset + (BuildType.RowSize * (rid - 1));
			}
			else
			{
				int offset = _bufferLength;
				EnsureBuffer(_bufferLength + BuildType.RowSize);

				return offset;
			}
		}

		internal int GetMethodOffset(int rid)
		{
			if (rid > 0)
			{
				return _methodTableOffset + (BuildMethod.RowSize * (rid - 1));
			}
			else
			{
				int offset = _bufferLength;
				EnsureBuffer(_bufferLength + BuildMethod.RowSize);

				return offset;
			}
		}

		internal int GetFieldOffset(int rid)
		{
			if (rid > 0)
			{
				return _fieldTableOffset + (BuildField.RowSize * (rid - 1));
			}
			else
			{
				int offset = _bufferLength;
				EnsureBuffer(_bufferLength + BuildField.RowSize);

				return offset;
			}
		}

		internal int GetPropertyOffset(int rid)
		{
			if (rid > 0)
			{
				return _propertyTableOffset + (BuildProperty.RowSize * (rid - 1));
			}
			else
			{
				int offset = _bufferLength;
				EnsureBuffer(_bufferLength + BuildProperty.RowSize);

				return offset;
			}
		}

		internal int GetEventOffset(int rid)
		{
			if (rid > 0)
			{
				return _eventTableOffset + (BuildEvent.RowSize * (rid - 1));
			}
			else
			{
				int offset = _bufferLength;
				EnsureBuffer(_bufferLength + BuildEvent.RowSize);

				return offset;
			}
		}

		internal int GetResourceOffset(int rid)
		{
			if (rid > 0)
			{
				return _resourceTableOffset + (BuildResource.RowSize * (rid - 1));
			}
			else
			{
				int offset = _bufferLength;
				EnsureBuffer(_bufferLength + BuildResource.RowSize);

				return offset;
			}
		}

		#endregion

		#region Nested types

		private class SignatureList : SignatureSerializer
		{
			private StringSerializer _strings;

			internal SignatureList(StringSerializer strings)
				: this(null, strings)
			{
			}

			internal SignatureList(IBinaryAccessor accessor, StringSerializer strings)
			{
				_strings = strings;
				_comparer = SignatureComparer.Default;

				if (accessor != null)
					Deserialize(accessor);
				else
					_blob = new Blob();
			}

			protected override string ReadString(ref int pos)
			{
				int stringID = _blob.Read7BitEncodedInt(ref pos);
				if (stringID == 0)
					return null;

				return _strings[stringID - 1];
			}

			protected override void WriteString(ref int pos, string value)
			{
				int stringID;
				if (!string.IsNullOrEmpty(value))
					stringID = _strings.Add(value) + 1;
				else
					stringID = 0;

				_blob.Write7BitEncodedInt(ref pos, stringID);
			}
		}

		#endregion
	}
}
