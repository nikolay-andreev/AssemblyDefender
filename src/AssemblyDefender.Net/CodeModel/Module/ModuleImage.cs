using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Defines managed assembly image.
	/// </summary>
	public class ModuleImage
	{
		#region Fields

		private PEImage _pe;
		private IMetadata _metadata;
		private CorHeader _corHeader;
		private TLS _tls;
		private Module _module;

		#endregion

		#region Ctors

		public ModuleImage(Module module, PEImage pe, IMetadata metadata, CorHeader corHeader)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			if (pe == null)
				throw new ArgumentNullException("pe");

			if (metadata == null)
				throw new ArgumentNullException("metadata");

			_module = module;
			_pe = pe;
			_metadata = metadata;
			_corHeader = corHeader;

			Initialize();
		}

		#endregion

		#region Properties

		public string Location
		{
			get { return _pe.Location; }
		}

		public string FrameworkVersionMoniker
		{
			get { return _metadata.FrameworkVersionMoniker; }
		}

		public bool IsILOnly
		{
			get { return (CorFlags & CorFlags.ILOnly) == CorFlags.ILOnly; }
		}

		public bool IsStrongNameSigned
		{
			get { return (CorFlags & CorFlags.StrongNameSigned) == CorFlags.StrongNameSigned; }
		}

		public CorFlags CorFlags
		{
			get { return _corHeader.Flags; }
		}

		public bool Is32Bits
		{
			get { return _pe != null ? _pe.Is32Bits : true; }
		}

		public ImageCharacteristics ImageCharacteristics
		{
			get
			{
				return _pe != null ? _pe.Characteristics :
					ImageCharacteristics.EXECUTABLE_IMAGE |
					ImageCharacteristics.MACHINE_32BIT;
			}
		}

		public DllCharacteristics DllCharacteristics
		{
			get
			{
				return _pe != null ? _pe.DllCharacteristics :
					DllCharacteristics.DYNAMIC_BASE |
					DllCharacteristics.NX_COMPAT |
					DllCharacteristics.NO_SEH;
			}
		}

		public MachineType MachineType
		{
			get { return _pe != null ? _pe.Machine : MachineType.I386; }
		}

		public ulong ImageBase
		{
			get { return _pe != null ? _pe.ImageBase : 0x400000; }
		}

		public uint SectionAlignment
		{
			get { return _pe != null ? _pe.SectionAlignment : 0x2000; }
		}

		public uint FileAlignment
		{
			get { return _pe != null ? _pe.FileAlignment : 0x200; }
		}

		public SubsystemType SubsystemType
		{
			get { return _pe != null ? _pe.Subsystem : SubsystemType.WINDOWS_CUI; }
		}

		public CorHeader CorHeader
		{
			get { return _corHeader; }
		}

		public ulong SizeOfStackReserve
		{
			get { return _pe != null ? _pe.SizeOfStackReserve : 0x100000; }
		}

		public PEImage PE
		{
			get { return _pe; }
		}

		public TLS TLS
		{
			get
			{
				if (_tls == null)
				{
					_tls = TLS.TryLoad(_pe) ?? new TLS();
				}

				return _tls;
			}
		}

		#endregion

		#region Methods

		public IBinaryAccessor OpenImage(long position)
		{
			return _pe.OpenImage(position);
		}

		public bool TryOpenImageToRVA(uint rva, out IBinaryAccessor accessor)
		{
			return _pe.TryOpenImageToSectionData(rva, out accessor);
		}

		public string GetString(int id)
		{
			return _metadata.GetString(id);
		}

		public string GetUserString(int id)
		{
			return _metadata.GetUserString(id);
		}

		public Guid GetGuid(int id)
		{
			return _metadata.GetGuid(id);
		}

		public byte[] GetBlob(int id)
		{
			return _metadata.GetBlob(id);
		}

		public byte[] GetBlob(int id, int offset, int size)
		{
			return _metadata.GetBlob(id, offset, size);
		}

		public byte GetBlobByte(int id, int offset)
		{
			return _metadata.GetBlobByte(id, offset);
		}

		public IBinaryAccessor OpenBlob(int id)
		{
			return _metadata.OpenBlob(id);
		}

		public byte[] GetManifestResourceData(int offset)
		{
			PESection section;
			long position;
			if (!_pe.ResolvePositionToSectionData(_corHeader.Resources.RVA, out position, out section))
				return null;

			position += offset;

			using (var accessor = _pe.OpenImage(position))
			{
				int length = accessor.ReadInt32();

				return accessor.ReadBytes(length);
			}
		}

		internal void Close()
		{
			if (_pe != null)
			{
				_pe.Dispose();
				_pe = null;
			}

			_module = null;
			_metadata = null;
		}

		private void Initialize()
		{
			InitializeTables();
			InitializeSignatures();
		}

		#endregion

		#region Tables

		private int[] GetTableValues(int tableType, int columnID)
		{
			int count = _metadata.GetTableRowCount(tableType);
			int[] values = new int[count];
			_metadata.GetTableValues(tableType, 1, columnID, count, values);

			return values;
		}

		private int[] SortTableValues(int count, IComparer<int> comparer)
		{
			// Create map
			int[] map = new int[count];
			for (int i = 0; i < count; i++)
			{
				map[i] = i;
			}

			// Sort
			Array.Sort<int>(map, comparer);

			return map;
		}

		private void MapTableValues(ref int[] values, int[] map)
		{
			int count = values.Length;
			int[] newValues = new int[count];
			for (int i = 0; i < count; i++)
			{
				newValues[i] = values[map[i]];
			}

			values = newValues;
		}

		private bool BinarySearchTable(int[] keys, int key, out int rid)
		{
			if (!BinarySearchArray(keys, key, out rid))
				return false;

			rid++;

			return true;
		}

		private bool BinarySearchTable(int[] keys, int[] ridMap, int key, out int rid)
		{
			if (!BinarySearchArray(keys, key, out rid))
				return false;

			if (ridMap != null)
				rid = ridMap[rid];

			rid++;

			return true;
		}

		private void BinarySearchTable(int[] keys, int key, out int[] rids)
		{
			int fromIndex;
			int toIndex;
			BinarySearchArrayRange(keys, key, out fromIndex, out toIndex);

			int count = toIndex - fromIndex;
			rids = new int[count];
			if (count > 0)
			{
				for (int i = 0, rid = fromIndex + 1; i < count; i++, rid++)
				{
					rids[i] = rid;
				}
			}
		}

		private void BinarySearchTable(int[] keys, int[] ridMap, int key, out int[] rids)
		{
			int fromIndex;
			int toIndex;
			BinarySearchArrayRange(keys, key, out fromIndex, out toIndex);

			int count = toIndex - fromIndex;
			rids = new int[count];
			if (count > 0)
			{
				if (ridMap != null)
				{
					for (int i = 0, j = fromIndex; i < count; i++, j++)
					{
						rids[i] = ridMap[j] + 1;
					}
				}
				else
				{
					for (int i = 0, rid = fromIndex + 1; i < count; i++, rid++)
					{
						rids[i] = rid;
					}
				}
			}
		}

		private bool BinarySearchArray(int[] array, int key, out int index)
		{
			int num = 0;
			int num2 = array.Length - 1;
			while (num <= num2)
			{
				int num3 = num + ((num2 - num) >> 1);
				int num4 = array[num3].CompareTo(key);
				if (num4 == 0)
				{
					index = num3;
					return true;
				}

				if (num4 < 0)
					num = num3 + 1;
				else
					num2 = num3 - 1;
			}

			index = ~num;
			return false;
		}

		private void BinarySearchArrayRange(int[] array, int key, out int fromIndex, out int toIndex)
		{
			// Find item in center of the range.
			if (!BinarySearchArray(array, key, out fromIndex))
			{
				toIndex = fromIndex;
				return;
			}

			toIndex = fromIndex + 1;

			// Binary search has located random index of sequential values. Check prev and next items.
			while (fromIndex > 0)
			{
				if (array[fromIndex - 1] != key)
					break;

				fromIndex--;
			}

			int count = array.Length;
			while (toIndex < count)
			{
				if (array[toIndex] != key)
					break;

				toIndex++;
			}
		}

		private int ResolveMethod(int typeRID, int memberRID)
		{
			int[] rids;
			GetMethodsByType(typeRID, out rids);

			int count = rids.Length;

			if (count > 0)
			{
				var methodRef = (MethodReference)MethodReference.LoadMemberRef(_module, memberRID);

				for (int i = 0; i < count; i++)
				{
					int rid = rids[i];
					if (MatchMethod(rid, methodRef))
					{
						return rid;
					}
				}
			}

			return 0;
		}

		private bool MatchMethod(int rid, MethodReference methodRef)
		{
			MethodRow row;
			_metadata.GetMethod(rid, out row);

			if (_metadata.GetString(row.Name) != methodRef.Name)
				return false;

			using (var accessor = _metadata.OpenBlob(row.Signature))
			{
				byte sigType = accessor.ReadByte();
				bool hasThis = (sigType & Metadata.SignatureType.HasThis) == Metadata.SignatureType.HasThis;
				if (hasThis != methodRef.HasThis)
					return false;

				var callConv = (MethodCallingConvention)(sigType & Metadata.SignatureType.MethodCallConvMask);
				if (callConv != methodRef.CallConv)
					return false;

				if ((sigType & Metadata.SignatureType.Generic) == Metadata.SignatureType.Generic)
				{
					int genParamCount = accessor.ReadCompressedInteger();
					if (genParamCount != methodRef.GenericParameterCount)
						return false;
				}

				int paramCount = accessor.ReadCompressedInteger();
				if (paramCount != methodRef.GetArgumentCountNoVarArgs())
					return false;

				// Return type.
				var returnType = TypeSignature.Load(accessor, _module);
				if (!returnType.Equals(methodRef.ReturnType))
					return false;

				// Parameters
				for (int i = 0; i < paramCount; i++)
				{
					var paramType = TypeSignature.Load(accessor, _module);
					if (!paramType.Equals(methodRef.Arguments[i]))
						return false;
				}
			}

			return true;
		}

		private void InitializeTables()
		{
			if (!_metadata.IsOptimized)
			{
				_eventPtr = GetTableValues(MetadataTableType.EventPtr, 0);
				_fieldPtr = GetTableValues(MetadataTableType.FieldPtr, 0);
				_methodPtr = GetTableValues(MetadataTableType.MethodPtr, 0);
				_paramPtr = GetTableValues(MetadataTableType.ParamPtr, 0);
				_propertyPtr = GetTableValues(MetadataTableType.PropertyPtr, 0);
				_metadata.UnloadTable(MetadataTableType.EventPtr);
				_metadata.UnloadTable(MetadataTableType.FieldPtr);
				_metadata.UnloadTable(MetadataTableType.MethodPtr);
				_metadata.UnloadTable(MetadataTableType.ParamPtr);
				_metadata.UnloadTable(MetadataTableType.PropertyPtr);
			}
		}

		#region Assembly

		public int GetAssemblyCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Assembly);
		}

		public void GetAssembly(int rid, out AssemblyRow row)
		{
			_metadata.GetAssembly(rid, out row);
		}

		#endregion

		#region AssemblyRef

		public int GetAssemblyRefCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.AssemblyRef);
		}

		public void GetAssemblyRef(int rid, out AssemblyRefRow row)
		{
			_metadata.GetAssemblyRef(rid, out row);
		}

		#endregion

		#region ClassLayout

		private int[] _classLayoutParent;
		private int[] _classLayoutParentMap;

		public int GetClassLayoutCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.ClassLayout);
		}

		public void GetClassLayout(int rid, out ClassLayoutRow row)
		{
			_metadata.GetClassLayout(rid, out row);
		}

		public bool GetClassLayoutByParent(int typeRID, out int rid)
		{
			if (_classLayoutParent == null)
			{
				InitializeClassLayoutParent();
			}

			return BinarySearchTable(_classLayoutParent, _classLayoutParentMap, typeRID, out rid);
		}

		private void InitializeClassLayoutParent()
		{
			_classLayoutParent = GetTableValues(MetadataTableType.ClassLayout, 2);

			if (!_metadata.IsTableSorted(MetadataTableType.ClassLayout))
			{
				var comparer = new ClassLayoutTableSortComparer(_classLayoutParent);
				_classLayoutParentMap = SortTableValues(_classLayoutParent.Length, comparer);
				MapTableValues(ref _classLayoutParent, _classLayoutParentMap);
			}
		}

		#endregion

		#region Constant

		private int[] _constantParent;
		private int[] _constantParentMap;

		public int GetConstantCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Constant);
		}

		public void GetConstant(int rid, out ConstantRow row)
		{
			_metadata.GetConstant(rid, out row);
		}

		public bool GetConstantByParent(int parentToken, out int rid)
		{
			if (_constantParent == null)
			{
				InitializeConstantParent();
			}

			return BinarySearchTable(_constantParent, _constantParentMap, parentToken, out rid);
		}

		private void InitializeConstantParent()
		{
			_constantParent = GetTableValues(MetadataTableType.Constant, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.Constant))
			{
				var comparer = new ConstantTableSortComparer(_constantParent);
				_constantParentMap = SortTableValues(_constantParent.Length, comparer);
				MapTableValues(ref _constantParent, _constantParentMap);
			}
		}

		#endregion

		#region CustomAttribute

		private int[] _customAttributeParent;
		private int[] _customAttributeParentMap;

		public int GetCustomAttributeCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.CustomAttribute);
		}

		public void GetCustomAttribute(int rid, out CustomAttributeRow row)
		{
			_metadata.GetCustomAttribute(rid, out row);
		}

		public void GetCustomAttributesByParent(int parentToken, out int[] rids)
		{
			if (_customAttributeParent == null)
			{
				InitializeCustomAttributeParent();
			}

			BinarySearchTable(_customAttributeParent, _customAttributeParentMap, parentToken, out rids);
		}

		private void InitializeCustomAttributeParent()
		{
			_customAttributeParent = GetTableValues(MetadataTableType.CustomAttribute, 0);

			if (!_metadata.IsTableSorted(MetadataTableType.CustomAttribute))
			{
				var comparer = new CustomAttributeTableSortComparer(_customAttributeParent);
				_customAttributeParentMap = SortTableValues(_customAttributeParent.Length, comparer);
				MapTableValues(ref _customAttributeParent, _customAttributeParentMap);
			}
		}

		#endregion

		#region SecurityAttribute

		private int[] _securityAttributeParent;
		private SecurityAttributeRow[] _securityAttributeRows;

		public int GetSecurityAttributeCount()
		{
			if (_securityAttributeParent == null)
			{
				InitializeSecurityAttributeParent();
			}

			return _securityAttributeParent.Length;
		}

		public void GetSecurityAttribute(int rid, out DeclSecurityRow row, out int offset, out int size)
		{
			if (_securityAttributeParent == null)
			{
				InitializeSecurityAttributeParent();
			}

			var secRow = _securityAttributeRows[rid - 1];
			offset = secRow.BlobOffset;
			size = secRow.BlobSize;
			_metadata.GetDeclSecurity(secRow.DeclSecurity, out row);
		}

		public void GetSecurityAttributesByParent(int parentToken, out int[] rids)
		{
			if (_securityAttributeParent == null)
			{
				InitializeSecurityAttributeParent();
			}

			BinarySearchTable(_securityAttributeParent, parentToken, out rids);
		}

		private void InitializeSecurityAttributeParent()
		{
			int count = _metadata.GetTableRowCount(MetadataTableType.DeclSecurity);
			var parents = new List<int>(count);
			var rows = new List<SecurityAttributeRow>(count);

			int[] declSecurityParent = GetTableValues(MetadataTableType.DeclSecurity, 1);
			int[] blobOffsets = GetTableValues(MetadataTableType.DeclSecurity, 2);

			int[] declSecurityParentMap = null;
			if (!_metadata.IsTableSorted(MetadataTableType.DeclSecurity))
			{
				var comparer = new DeclSecurityTableSortComparer(declSecurityParent);
				declSecurityParentMap = SortTableValues(declSecurityParent.Length, comparer);
			}

			// Create security attributes (DeclSecurity + Offset)
			for (int i = 0; i < count; i++)
			{
				int declSecRID;
				int parent;
				int blobOffset;
				if (declSecurityParentMap != null)
				{
					declSecRID = declSecurityParentMap[i] + 1;
					parent = declSecurityParent[declSecurityParentMap[i]];
					blobOffset = blobOffsets[declSecurityParentMap[i]];
				}
				else
				{
					declSecRID = i + 1;
					parent = declSecurityParent[i];
					blobOffset = blobOffsets[i];
				}

				try
				{
					using (var accessor = _metadata.OpenBlob(blobOffset))
					{
						long basePosition = accessor.Position;

						// An XML text cannot begin with a dot, so the system identifies the type of encoding (XML or binary)
						// by the very first byte.
						if (accessor.ReadByte() == 0x2E)
						{
							int length = accessor.ReadCompressedInteger();
							for (int j = 0; j < length; j++)
							{
								parents.Add(parent);

								long startPos = accessor.Position;

								// Type name (UTF8)
								int size = accessor.ReadCompressedInteger();
								accessor.Position += size;

								// Blob
								size = accessor.ReadCompressedInteger();
								accessor.Position += size;

								rows.Add(new SecurityAttributeRow(
									(int)(startPos - basePosition),
									(int)(accessor.Position - startPos),
									declSecRID));
							}
						}
						else
						{
							parents.Add(parent);
							rows.Add(new SecurityAttributeRow(0, 0, declSecRID));
						}
					}
				}
				catch (Exception ex)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, Location), ex);
				}
			}

			_securityAttributeParent = parents.ToArray();
			_securityAttributeRows = rows.ToArray();
		}

		private struct SecurityAttributeRow
		{
			internal int BlobOffset;
			internal int BlobSize;
			internal int DeclSecurity;

			internal SecurityAttributeRow(int blobOffset, int blobSize, int declSecurity)
			{
				this.BlobOffset = blobOffset;
				this.BlobSize = blobSize;
				this.DeclSecurity = declSecurity;
			}
		}

		#endregion

		#region Event

		private int[] _eventPtr;

		public int GetEventCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Event);
		}

		public void GetEvent(int rid, out EventRow row)
		{
			if (_eventPtr != null)
				rid = _eventPtr[rid - 1];

			_metadata.GetEvent(rid, out row);
		}

		public void GetEventsByType(int typeRID, out int[] rids)
		{
			int eventMapRID;
			if (!GetEventMapByParent(typeRID, out eventMapRID))
			{
				rids = new int[0];
				return;
			}

			int fromRID = _eventMapEventList[eventMapRID - 1];
			int toRID;
			if (eventMapRID < _eventMapEventList.Length)
			{
				toRID = _eventMapEventList[eventMapRID];
			}
			else
			{
				toRID = _metadata.GetTableRowCount(MetadataTableType.Event) + 1;
			}

			int count = toRID - fromRID;
			rids = new int[count];
			for (int i = 0, rid = fromRID; i < count; i++, rid++)
			{
				rids[i] = rid;
			}
		}

		#endregion

		#region EventMap

		private int[] _eventMapParent;
		private int[] _eventMapEventList;

		private bool GetEventMapByParent(int typeRID, out int rid)
		{
			if (_eventMapParent == null)
			{
				InitializeEventMap();
			}

			return BinarySearchTable(_eventMapParent, typeRID, out rid);
		}

		private void InitializeEventMap()
		{
			_eventMapParent = GetTableValues(MetadataTableType.EventMap, 0);
			_eventMapEventList = GetTableValues(MetadataTableType.EventMap, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.EventMap))
			{
				var comparer = new EventMapTableSortComparer(_eventMapParent);
				var sortMap = SortTableValues(_eventMapParent.Length, comparer);
				MapTableValues(ref _eventMapParent, sortMap);
				MapTableValues(ref _eventMapEventList, sortMap);
			}

			_metadata.UnloadTable(MetadataTableType.EventMap);
		}

		#endregion

		#region ExportedType

		public int GetExportedTypeCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.ExportedType);
		}

		public void GetExportedType(int rid, out ExportedTypeRow row)
		{
			_metadata.GetExportedType(rid, out row);
		}

		#endregion

		#region Field

		private int[] _fieldPtr;

		public int GetFieldCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Field);
		}

		public void GetField(int rid, out FieldRow row)
		{
			if (_fieldPtr != null)
				rid = _fieldPtr[rid - 1];

			_metadata.GetField(rid, out row);
		}

		public void GetFieldsByType(int typeRID, out int[] rids)
		{
			int fromRID = _metadata.GetTableValue(MetadataTableType.TypeDef, typeRID, 4);

			int nextRID = typeRID + 1;
			int toRID;
			if (nextRID <= _metadata.GetTableRowCount(MetadataTableType.TypeDef))
			{
				toRID = _metadata.GetTableValue(MetadataTableType.TypeDef, nextRID, 4);
			}
			else
			{
				toRID = _metadata.GetTableRowCount(MetadataTableType.Field) + 1;
			}

			int count = toRID - fromRID;
			rids = new int[count];
			for (int i = 0, rid = fromRID; i < count; i++, rid++)
			{
				rids[i] = rid;
			}
		}

		#endregion

		#region FieldLayout

		private int[] _fieldLayoutOffset;
		private int[] _fieldLayoutField;

		public bool GetFieldLayoutOffsetByField(int fieldRID, out int offset)
		{
			if (_fieldLayoutField == null)
			{
				InitializeFieldLayout();
			}

			int index;
			if (!BinarySearchArray(_fieldLayoutField, fieldRID, out index))
			{
				offset = 0;
				return false;
			}

			offset = _fieldLayoutOffset[index];
			return true;
		}

		private void InitializeFieldLayout()
		{
			_fieldLayoutOffset = GetTableValues(MetadataTableType.FieldLayout, 0);
			_fieldLayoutField = GetTableValues(MetadataTableType.FieldLayout, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.FieldLayout))
			{
				var comparer = new FieldLayoutTableSortComparer(_fieldLayoutField);
				var sortMap = SortTableValues(_fieldLayoutField.Length, comparer);
				MapTableValues(ref _fieldLayoutField, sortMap);
				MapTableValues(ref _fieldLayoutOffset, sortMap);
			}

			_metadata.UnloadTable(MetadataTableType.FieldLayout);
		}

		#endregion

		#region FieldMarshal

		private int[] _fieldMarshalParent;
		private int[] _fieldMarshalNativeType;

		public bool GetFieldMarshalNativeTypeByParent(int parentToken, out int nativeType)
		{
			if (_fieldMarshalParent == null)
			{
				InitializeFieldMarshal();
			}

			int index;
			if (!BinarySearchArray(_fieldMarshalParent, parentToken, out index))
			{
				nativeType = 0;
				return false;
			}

			nativeType = _fieldMarshalNativeType[index];
			return true;
		}

		private void InitializeFieldMarshal()
		{
			_fieldMarshalParent = GetTableValues(MetadataTableType.FieldMarshal, 0);
			_fieldMarshalNativeType = GetTableValues(MetadataTableType.FieldMarshal, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.FieldMarshal))
			{
				var comparer = new FieldMarshalTableSortComparer(_fieldMarshalParent);
				var sortMap = SortTableValues(_fieldMarshalParent.Length, comparer);
				MapTableValues(ref _fieldMarshalParent, sortMap);
				MapTableValues(ref _fieldMarshalNativeType, sortMap);
			}

			_metadata.UnloadTable(MetadataTableType.FieldMarshal);
		}

		#endregion

		#region FieldRVA

		private int[] _fieldRVA;
		private int[] _fieldRVAField;
		private int[] _fieldRVASize;

		public void GetFieldRVA(int rid, out uint rva, out int size)
		{
			if (_fieldRVA == null)
			{
				InitializeFieldRVA();
			}

			int index = rid - 1;

			rva = (uint)_fieldRVA[index];

			size = _fieldRVASize[index];
			if (size < 0)
			{
				int fieldRID = _fieldRVAField[index];
				var fieldRef = FieldReference.LoadFieldDef(_module, fieldRID);
				if (!fieldRef.FieldType.GetSize(_module, out size))
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, _module.Location));
				}

				if (size < 0)
					size = 0;

				_fieldRVASize[index] = size;
			}
		}

		public bool GetFieldRVAByField(int fieldRID, out int rid)
		{
			if (_fieldRVAField == null)
			{
				InitializeFieldRVA();
			}

			return BinarySearchTable(_fieldRVAField, fieldRID, out rid);
		}

		private void InitializeFieldRVA()
		{
			_fieldRVA = GetTableValues(MetadataTableType.FieldRVA, 0);
			_fieldRVAField = GetTableValues(MetadataTableType.FieldRVA, 1);

			int count = _fieldRVA.Length;

			if (!_metadata.IsTableSorted(MetadataTableType.FieldRVA))
			{
				var comparer = new FieldRVATableSortComparer(_fieldRVAField);
				int[] sortMap = SortTableValues(count, comparer);
				MapTableValues(ref _fieldRVAField, sortMap);
				MapTableValues(ref _fieldRVA, sortMap);
			}

			_fieldRVASize = new int[_fieldRVA.Length];
			for (int i = 0; i < count; i++)
			{
				_fieldRVASize[i] = -1;
			}

			_metadata.UnloadTable(MetadataTableType.FieldRVA);
		}

		#endregion

		#region File

		public int GetFileCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.File);
		}

		public void GetFile(int rid, out FileRow row)
		{
			_metadata.GetFile(rid, out row);
		}

		#endregion

		#region GenericParam

		private int[] _genericParamOwner;
		private int[] _genericParamOwnerMap;

		public int GetGenericParamCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.GenericParam);
		}

		public void GetGenericParam(int rid, out GenericParamRow row)
		{
			_metadata.GetGenericParam(rid, out row);
		}

		public void GetGenericParamsByOwner(int ownerToken, out int[] rids)
		{
			if (_genericParamOwner == null)
			{
				InitializeGenericParamOwner();
			}

			BinarySearchTable(_genericParamOwner, _genericParamOwnerMap, ownerToken, out rids);
		}

		private void InitializeGenericParamOwner()
		{
			_genericParamOwner = GetTableValues(MetadataTableType.GenericParam, 2);

			if (!_metadata.IsTableSorted(MetadataTableType.GenericParam))
			{
				int[] numbers = GetTableValues(MetadataTableType.GenericParam, 0);
				var comparer = new GenericParamTableSortComparer(_genericParamOwner, numbers);
				_genericParamOwnerMap = SortTableValues(_genericParamOwner.Length, comparer);
				MapTableValues(ref _genericParamOwner, _genericParamOwnerMap);
			}
		}

		#endregion

		#region GenericParamConstraint

		private int[] _genericParamConstraint;
		private int[] _genericParamConstraintOwner;

		public void GetGenericParamConstraintsByOwner(int genericParamRID, out int[] constraints)
		{
			if (_genericParamConstraintOwner == null)
			{
				InitializeGenericParamConstraint();
			}

			int[] rids;
			BinarySearchTable(_genericParamConstraintOwner, genericParamRID, out rids);

			int count = rids.Length;
			constraints = new int[count];

			for (int i = 0; i < count; i++)
			{
				constraints[i] = _genericParamConstraint[rids[i] - 1];
			}
		}

		private void InitializeGenericParamConstraint()
		{
			_genericParamConstraintOwner = GetTableValues(MetadataTableType.GenericParamConstraint, 0);
			_genericParamConstraint = GetTableValues(MetadataTableType.GenericParamConstraint, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.GenericParamConstraint))
			{
				var comparer = new GenericParamConstraintTableSortComparer(_genericParamConstraintOwner);
				int[] sortMap = SortTableValues(_genericParamConstraintOwner.Length, comparer);
				MapTableValues(ref _genericParamConstraint, sortMap);
				MapTableValues(ref _genericParamConstraintOwner, sortMap);
			}

			_metadata.UnloadTable(MetadataTableType.GenericParamConstraint);
		}

		#endregion

		#region ImplMap

		private int[] _implMapMemberForwarded;
		private int[] _implMapMemberForwardedMap;

		public int GetImplMapCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.ImplMap);
		}

		public void GetImplMap(int rid, out ImplMapRow row)
		{
			_metadata.GetImplMap(rid, out row);
		}

		public bool GetImplMapByMemberForwarded(int memberForwardedToken, out int rid)
		{
			if (_implMapMemberForwarded == null)
			{
				InitializeImplMapMemberForwarded();
			}

			return BinarySearchTable(_implMapMemberForwarded, _implMapMemberForwardedMap, memberForwardedToken, out rid);
		}

		private void InitializeImplMapMemberForwarded()
		{
			_implMapMemberForwarded = GetTableValues(MetadataTableType.ImplMap, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.ImplMap))
			{
				var comparer = new ImplMapTableSortComparer(_implMapMemberForwarded);
				_implMapMemberForwardedMap = SortTableValues(_implMapMemberForwarded.Length, comparer);
				MapTableValues(ref _implMapMemberForwarded, _implMapMemberForwardedMap);
			}
		}

		#endregion

		#region InterfaceImpl

		private int[] _interfaceImpl;
		private int[] _interfaceImplClass;

		public void GetInterfaceImplsByClass(int typeRID, out int[] interfaces)
		{
			if (_interfaceImplClass == null)
			{
				InitializeInterfaceImpl();
			}

			int[] rids;
			BinarySearchTable(_interfaceImplClass, typeRID, out rids);

			int count = rids.Length;
			interfaces = new int[count];

			for (int i = 0; i < count; i++)
			{
				interfaces[i] = _interfaceImpl[rids[i] - 1];
			}
		}

		private void InitializeInterfaceImpl()
		{
			_interfaceImplClass = GetTableValues(MetadataTableType.InterfaceImpl, 0);
			_interfaceImpl = GetTableValues(MetadataTableType.InterfaceImpl, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.InterfaceImpl))
			{
				var comparer = new InterfaceImplTableSortComparer(_interfaceImplClass);
				int[] sortMap = SortTableValues(_interfaceImplClass.Length, comparer);
				MapTableValues(ref _interfaceImpl, sortMap);
				MapTableValues(ref _interfaceImplClass, sortMap);
			}

			_metadata.UnloadTable(MetadataTableType.InterfaceImpl);
		}

		#endregion

		#region ManifestResource

		public int GetManifestResourceCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.ManifestResource);
		}

		public void GetManifestResource(int rid, out ManifestResourceRow row)
		{
			_metadata.GetManifestResource(rid, out row);
		}

		#endregion

		#region MemberRef

		public int GetMemberRefCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.MemberRef);
		}

		public void GetMemberRef(int rid, out MemberRefRow row)
		{
			_metadata.GetMemberRef(rid, out row);
		}

		public int GetMemberRefSignature(int rid)
		{
			return _metadata.GetTableValue(MetadataTableType.MemberRef, rid, 2);
		}

		#endregion

		#region Method

		private int[] _methodPtr;

		public int GetMethodCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.MethodDef);
		}

		public void GetMethod(int rid, out MethodRow row)
		{
			if (_methodPtr != null)
				rid = _methodPtr[rid - 1];

			_metadata.GetMethod(rid, out row);
		}

		public void GetMethodsByType(int typeRID, out int[] rids)
		{
			int fromRID = _metadata.GetTableValue(MetadataTableType.TypeDef, typeRID, 5);

			int nextRID = typeRID + 1;
			int toRID;
			if (nextRID <= _metadata.GetTableRowCount(MetadataTableType.TypeDef))
			{
				toRID = _metadata.GetTableValue(MetadataTableType.TypeDef, nextRID, 5);
			}
			else
			{
				toRID = _metadata.GetTableRowCount(MetadataTableType.MethodDef) + 1;
			}

			int count = toRID - fromRID;
			rids = new int[count];
			for (int i = 0, rid = fromRID; i < count; i++, rid++)
			{
				rids[i] = rid;
			}
		}

		#endregion

		#region MethodImpl

		private int[] _methodImplBody;
		private int[] _methodImplDeclaration;

		public void GetMethodImplsByBody(int methodRID, out int[] implementations)
		{
			if (_methodImplBody == null)
			{
				InitializeMethodImplBody();
			}

			int fromIndex;
			int toIndex;
			BinarySearchArrayRange(_methodImplBody, methodRID, out fromIndex, out toIndex);

			int count = toIndex - fromIndex;
			implementations = new int[count];
			for (int i = 0, j = fromIndex; i < count; i++, j++)
			{
				implementations[i] = _methodImplDeclaration[j];
			}
		}

		private void InitializeMethodImplBody()
		{
			_methodImplBody = GetTableValues(MetadataTableType.MethodImpl, 1);
			_methodImplDeclaration = GetTableValues(MetadataTableType.MethodImpl, 2);

			// Create body rids.
			int count = _methodImplBody.Length;
			for (int i = 0; i < count; i++)
			{
				int bodyRID;
				int bodyToken = MetadataToken.DecompressMethodDefOrRef(_methodImplBody[i]);
				switch (MetadataToken.GetType(bodyToken))
				{
					case MetadataTokenType.Method:
						{
							bodyRID = MetadataToken.GetRID(bodyToken);
						}
						break;

					case MetadataTokenType.MemberRef:
						{
							int memberRID = MetadataToken.GetRID(bodyToken);
							int typeRID = _metadata.GetTableValue(MetadataTableType.MethodImpl, i + 1, 0);
							bodyRID = ResolveMethod(typeRID, memberRID);
						}
						break;

					default:
						throw new CodeModelException(string.Format(SR.AssemblyLoadError, Location));
				}

				if (bodyRID == 0)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, Location));
				}

				_methodImplBody[i] = bodyRID;
			}

			// Sort
			int[] sortMap = SortTableValues(_methodImplBody.Length, new TableSortComparer(_methodImplBody));
			MapTableValues(ref _methodImplBody, sortMap);
			MapTableValues(ref _methodImplDeclaration, sortMap);

			_metadata.UnloadTable(MetadataTableType.MethodImpl);
		}

		#endregion

		#region MethodSemantics

		private int[] _methodSemanticsAssociation;
		private int[] _methodSemanticsAssociationMap;

		public int GetMethodSemanticsCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.MethodSemantics);
		}

		public void GetMethodSemantics(int rid, out MethodSemanticsRow row)
		{
			_metadata.GetMethodSemantics(rid, out row);
		}

		public void GetMethodSemanticsByAssociation(int associationToken, out int[] rids)
		{
			if (_methodSemanticsAssociation == null)
			{
				InitializeMethodSemanticsAssociation();
			}

			BinarySearchTable(_methodSemanticsAssociation, _methodSemanticsAssociationMap, associationToken, out rids);
		}

		private void InitializeMethodSemanticsAssociation()
		{
			_methodSemanticsAssociation = GetTableValues(MetadataTableType.MethodSemantics, 2);

			if (!_metadata.IsTableSorted(MetadataTableType.MethodSemantics))
			{
				var comparer = new MethodSemanticsTableSortComparer(_methodSemanticsAssociation);
				_methodSemanticsAssociationMap = SortTableValues(_methodSemanticsAssociation.Length, comparer);
				MapTableValues(ref _methodSemanticsAssociation, _methodSemanticsAssociationMap);
			}
		}

		#endregion

		#region MethodSpec

		public int GetMethodSpecCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.MethodSpec);
		}

		public void GetMethodSpec(int rid, out MethodSpecRow row)
		{
			_metadata.GetMethodSpec(rid, out row);
		}

		#endregion

		#region Module

		public int GetModuleCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Module);
		}

		public void GetModule(int rid, out ModuleRow row)
		{
			_metadata.GetModule(rid, out row);
		}

		#endregion

		#region ModuleRef

		public int GetModuleRefCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.ModuleRef);
		}

		public void GetModuleRef(int rid, out ModuleRefRow row)
		{
			_metadata.GetModuleRef(rid, out row);
		}

		#endregion

		#region NestedClass

		private int[] _nesToEnc_NestedClass;
		private int[] _nesToEnc_EnclosingClass;
		private int[] _encToNes_NestedClass;
		private int[] _encToNes_EnclosingClass;
		private int[] _isNestedClassBits;

		public bool IsTypeNested(int typeRID)
		{
			if (_isNestedClassBits == null)
			{
				InitializeIsNestedClassBits();
			}

			int index = typeRID - 1;
			return ((_isNestedClassBits[index / 0x20] & (((int)1) << (index % 0x20))) != 0);
		}

		public bool GetEnclosingTypeByNested(int nestedRID, out int enclosingRID)
		{
			if (_nesToEnc_EnclosingClass == null)
			{
				InitializeNestedToEnclosingClass();
			}

			int index;
			if (!BinarySearchArray(_nesToEnc_NestedClass, nestedRID, out index))
			{
				enclosingRID = index;
				return false;
			}

			enclosingRID = _nesToEnc_EnclosingClass[index];
			return true;
		}

		public void GetNestedTypesByEnclosing(int enclosingRID, out int[] rids)
		{
			if (_encToNes_NestedClass == null)
			{
				InitializeEnclosingToNestedClass();
			}

			int fromIndex;
			int toIndex;
			BinarySearchArrayRange(_encToNes_EnclosingClass, enclosingRID, out fromIndex, out toIndex);

			int count = toIndex - fromIndex;
			rids = new int[count];
			for (int i = 0, j = fromIndex; i < count; i++, j++)
			{
				rids[i] = _encToNes_NestedClass[j];
			}
		}

		private void InitializeNestedToEnclosingClass()
		{
			_nesToEnc_NestedClass = GetTableValues(MetadataTableType.NestedClass, 0);
			_nesToEnc_EnclosingClass = GetTableValues(MetadataTableType.NestedClass, 1);

			int count = _nesToEnc_NestedClass.Length;
			var comparer = new TableSortComparer(_nesToEnc_NestedClass);
			var sortMap = SortTableValues(count, comparer);
			MapTableValues(ref _nesToEnc_NestedClass, sortMap);
			MapTableValues(ref _nesToEnc_EnclosingClass, sortMap);

			_metadata.UnloadTable(MetadataTableType.NestedClass);
		}

		private void InitializeEnclosingToNestedClass()
		{
			if (_nesToEnc_NestedClass == null)
			{
				InitializeNestedToEnclosingClass();
			}

			int count = _nesToEnc_NestedClass.Length;
			_encToNes_EnclosingClass = new int[count];
			_encToNes_NestedClass = new int[count];
			Array.Copy(_nesToEnc_EnclosingClass, 0, _encToNes_EnclosingClass, 0, count);
			Array.Copy(_nesToEnc_NestedClass, 0, _encToNes_NestedClass, 0, count);

			var comparer = new TableSortComparer(_encToNes_EnclosingClass);
			var sortMap = SortTableValues(count, comparer);
			MapTableValues(ref _encToNes_EnclosingClass, sortMap);
			MapTableValues(ref _encToNes_NestedClass, sortMap);
		}

		private void InitializeIsNestedClassBits()
		{
			if (_nesToEnc_NestedClass == null)
			{
				InitializeNestedToEnclosingClass();
			}

			int typeCount = GetTypeDefCount();
			int nestedClassCount = _nesToEnc_NestedClass.Length;
			int arrayLength = (((typeCount - 1) / 0x20) + 1);

			_isNestedClassBits = new int[arrayLength];

			for (int i = 0; i < nestedClassCount; i++)
			{
				int index = _nesToEnc_NestedClass[i] - 1;
				_isNestedClassBits[index / 0x20] |= ((int)1) << (index % 0x20);
			}
		}

		#endregion

		#region Param

		private int[] _paramPtr;

		public int GetParamCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Param);
		}

		public void GetParam(int rid, out ParamRow row)
		{
			if (_paramPtr != null)
				rid = _paramPtr[rid - 1];

			_metadata.GetParam(rid, out row);
		}

		public void GetParamsByMethod(int methodRID, out int[] rids)
		{
			int fromRID = _metadata.GetTableValue(MetadataTableType.MethodDef, methodRID, 5);

			int nextRID = methodRID + 1;
			int toRID;
			if (nextRID <= _metadata.GetTableRowCount(MetadataTableType.MethodDef))
			{
				toRID = _metadata.GetTableValue(MetadataTableType.MethodDef, nextRID, 5);
			}
			else
			{
				toRID = _metadata.GetTableRowCount(MetadataTableType.Param) + 1;
			}

			int count = toRID - fromRID;
			rids = new int[count];
			for (int i = 0, rid = fromRID; i < count; i++, rid++)
			{
				rids[i] = rid;
			}
		}

		#endregion

		#region Property

		private int[] _propertyPtr;

		public int GetPropertyCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.Property);
		}

		public void GetProperty(int rid, out PropertyRow row)
		{
			if (_propertyPtr != null)
				rid = _propertyPtr[rid - 1];

			_metadata.GetProperty(rid, out row);
		}

		public void GetPropertiesByType(int typeRID, out int[] rids)
		{
			int propertyMapRID;
			if (!GetPropertyMapByParent(typeRID, out propertyMapRID))
			{
				rids = new int[0];
				return;
			}

			int fromRID = _propertyMapPropertyList[propertyMapRID - 1];
			int toRID;
			if (propertyMapRID < _propertyMapPropertyList.Length)
			{
				toRID = _propertyMapPropertyList[propertyMapRID];
			}
			else
			{
				toRID = _metadata.GetTableRowCount(MetadataTableType.Property) + 1;
			}

			int count = toRID - fromRID;
			rids = new int[count];
			for (int i = 0, rid = fromRID; i < count; i++, rid++)
			{
				rids[i] = rid;
			}
		}

		#endregion

		#region PropertyMap

		private int[] _propertyMapParent;
		private int[] _propertyMapPropertyList;

		private bool GetPropertyMapByParent(int typeRID, out int rid)
		{
			if (_propertyMapParent == null)
			{
				InitializePropertyMap();
			}

			return BinarySearchTable(_propertyMapParent, typeRID, out rid);
		}

		private void InitializePropertyMap()
		{
			_propertyMapParent = GetTableValues(MetadataTableType.PropertyMap, 0);
			_propertyMapPropertyList = GetTableValues(MetadataTableType.PropertyMap, 1);

			if (!_metadata.IsTableSorted(MetadataTableType.PropertyMap))
			{
				var comparer = new PropertyMapTableSortComparer(_propertyMapParent);
				var sortMap = SortTableValues(_propertyMapParent.Length, comparer);
				MapTableValues(ref _propertyMapParent, sortMap);
				MapTableValues(ref _propertyMapPropertyList, sortMap);
			}

			_metadata.UnloadTable(MetadataTableType.PropertyMap);
		}

		#endregion

		#region StandAloneSig

		public int GetStandAloneSigCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.StandAloneSig);
		}

		public int GetStandAloneSig(int rid)
		{
			return _metadata.GetTableValue(MetadataTableType.StandAloneSig, rid, 0);
		}

		#endregion

		#region TypeDef

		private int[] _methodToTypeRID;
		private int[] _fieldToTypeRID;
		private int[] _propertyToTypeRID;
		private int[] _eventToTypeRID;

		public int GetTypeDefCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.TypeDef);
		}

		public void GetTypeDef(int rid, out TypeDefRow row)
		{
			_metadata.GetTypeDef(rid, out row);
		}

		public int GetTypeDefExtends(int rid)
		{
			return _metadata.GetTableValue(MetadataTableType.TypeDef, rid, 3);
		}

		public int GetTypeByMethod(int methodRID)
		{
			if (_methodToTypeRID == null)
			{
				InitializeMethodToType();
			}

			return _methodToTypeRID[methodRID - 1];
		}

		public int GetTypeByField(int fieldRID)
		{
			if (_fieldToTypeRID == null)
			{
				InitializeFieldToType();
			}

			return _fieldToTypeRID[fieldRID - 1];
		}

		public int GetTypeByProperty(int propertyRID)
		{
			if (_propertyToTypeRID == null)
			{
				InitializePropertyToType();
			}

			return _propertyToTypeRID[propertyRID - 1];
		}

		public int GetTypeByEvent(int eventRID)
		{
			if (_eventToTypeRID == null)
			{
				InitializeEventToType();
			}

			return _eventToTypeRID[eventRID - 1];
		}

		private void InitializeMethodToType()
		{
			int typeCount = GetTypeDefCount();
			int methodCount = GetMethodCount();

			_methodToTypeRID = new int[methodCount];

			int[] methodList = GetTableValues(MetadataTableType.TypeDef, 5);

			for (int typeIndex = 0; typeIndex < typeCount; typeIndex++)
			{
				int fromIndex = methodList[typeIndex] - 1;

				int nextIndex = typeIndex + 1;
				int toIndex;
				if (nextIndex < typeCount)
				{
					toIndex = methodList[nextIndex] - 1;
					if (toIndex > methodCount)
						toIndex = methodCount;
				}
				else
				{
					toIndex = methodCount;
				}

				int typeRID = typeIndex + 1;
				for (int i = fromIndex; i < toIndex; i++)
				{
					_methodToTypeRID[i] = typeRID;
				}
			}
		}

		private void InitializeFieldToType()
		{
			int typeCount = GetTypeDefCount();
			int fieldCount = GetFieldCount();

			_fieldToTypeRID = new int[fieldCount];

			int[] fieldList = GetTableValues(MetadataTableType.TypeDef, 4);

			for (int typeIndex = 0; typeIndex < typeCount; typeIndex++)
			{
				int fromIndex = fieldList[typeIndex] - 1;

				int nextIndex = typeIndex + 1;
				int toIndex;
				if (nextIndex < typeCount)
				{
					toIndex = fieldList[nextIndex] - 1;
					if (toIndex > fieldCount)
						toIndex = fieldCount;
				}
				else
				{
					toIndex = fieldCount;
				}

				int typeRID = typeIndex + 1;
				for (int i = fromIndex; i < toIndex; i++)
				{
					_fieldToTypeRID[i] = typeRID;
				}
			}
		}

		private void InitializePropertyToType()
		{
			if (_propertyMapParent == null)
			{
				InitializePropertyMap();
			}

			int typeCount = GetTypeDefCount();
			int propertyCount = GetPropertyCount();
			int mapCount = _propertyMapParent.Length;

			_propertyToTypeRID = new int[propertyCount];

			for (int mapIndex = 0; mapIndex < mapCount; mapIndex++)
			{
				int fromIndex = _propertyMapPropertyList[mapIndex] - 1;

				int nextMapIndex = mapIndex + 1;
				int toIndex;
				if (nextMapIndex < mapCount)
				{
					toIndex = _propertyMapPropertyList[nextMapIndex] - 1;
					if (toIndex > propertyCount)
						toIndex = propertyCount;
				}
				else
				{
					toIndex = propertyCount;
				}

				int typeRID = _propertyMapParent[mapIndex];
				for (int i = fromIndex; i < toIndex; i++)
				{
					_propertyToTypeRID[i] = typeRID;
				}
			}
		}

		private void InitializeEventToType()
		{
			if (_eventMapParent == null)
			{
				InitializeEventMap();
			}

			int typeCount = GetTypeDefCount();
			int eventCount = GetEventCount();
			int mapCount = _eventMapParent.Length;

			_eventToTypeRID = new int[eventCount];

			for (int mapIndex = 0; mapIndex < mapCount; mapIndex++)
			{
				int fromIndex = _eventMapEventList[mapIndex] - 1;

				int nextMapIndex = mapIndex + 1;
				int toIndex;
				if (nextMapIndex < mapCount)
				{
					toIndex = _eventMapEventList[nextMapIndex] - 1;
					if (toIndex > eventCount)
						toIndex = eventCount;
				}
				else
				{
					toIndex = eventCount;
				}

				int typeRID = _eventMapParent[mapIndex];
				for (int i = fromIndex; i < toIndex; i++)
				{
					_eventToTypeRID[i] = typeRID;
				}
			}
		}

		#endregion

		#region TypeRef

		public int GetTypeRefCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.TypeRef);
		}

		public void GetTypeRef(int rid, out TypeRefRow row)
		{
			_metadata.GetTypeRef(rid, out row);
		}

		#endregion

		#region TypeSpec

		public int GetTypeSpecCount()
		{
			return _metadata.GetTableRowCount(MetadataTableType.TypeSpec);
		}

		public int GetTypeSpec(int rid)
		{
			return _metadata.GetTableValue(MetadataTableType.TypeSpec, rid, 0);
		}

		#endregion

		#endregion

		#region Signatures

		internal AssemblyReference AssemblyDefSignature;
		internal ModuleReference ModuleDefSignature;
		internal AssemblyReference[] AssemblyRefSignatures;
		internal ModuleReference[] ModuleRefSignatures;
		internal FileReference[] FileSignatures;
		internal TypeReference[] TypeSignatures;
		internal TypeReference[] TypeRefSignatures;
		internal TypeSignature[] TypeSpecSignatures;
		internal TypeReference[] ExportedTypeSignatures;
		internal MethodSignature[] MethodSignatures;
		internal MethodSignature[] MethodSpecSignatures;
		internal FieldReference[] FieldSignatures;
		internal Signature[] MemberRefSignatures;
		internal Signature[] StandAloneSigSignatures;

		private void InitializeSignatures()
		{
			AssemblyRefSignatures = new AssemblyReference[GetAssemblyRefCount()];
			ModuleRefSignatures = new ModuleReference[GetModuleRefCount()];
			FileSignatures = new FileReference[GetFileCount()];
			TypeSignatures = new TypeReference[GetTypeDefCount()];
			TypeRefSignatures = new TypeReference[GetTypeRefCount()];
			TypeSpecSignatures = new TypeSignature[GetTypeSpecCount()];
			ExportedTypeSignatures = new TypeReference[GetExportedTypeCount()];
			MethodSignatures = new MethodSignature[GetMethodCount()];
			MethodSpecSignatures = new MethodSignature[GetMethodSpecCount()];
			FieldSignatures = new FieldReference[GetFieldCount()];
			MemberRefSignatures = new Signature[GetMemberRefCount()];
			StandAloneSigSignatures = new Signature[GetStandAloneSigCount()];
		}

		#endregion
	}
}
