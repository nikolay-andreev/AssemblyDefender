using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The metadata table stream #~ or #-
	/// </summary>
	public class MetadataTableStream
	{
		#region Fields

		private byte _schemaMajorVersion = 2;
		private byte _schemaMinorVersion = 0;
		private bool _isOptimized;
		private ulong _sortMask;
		private MetadataScope _metadata;
		private AssemblyTable _assemblyTable;
		private AssemblyOSTable _assemblyOSTable;
		private AssemblyProcessorTable _assemblyProcessorTable;
		private AssemblyRefTable _assemblyRefTable;
		private AssemblyRefOSTable _assemblyRefOSTable;
		private AssemblyRefProcessorTable _assemblyRefProcessorTable;
		private ClassLayoutTable _classLayoutTable;
		private ConstantTable _constantTable;
		private CustomAttributeTable _customAttributeTable;
		private DeclSecurityTable _declSecurityTable;
		private ENCLogTable _enclogTable;
		private ENCMapTable _encmapTable;
		private EventTable _eventTable;
		private EventMapTable _eventMapTable;
		private EventPtrTable _eventPtrTable;
		private ExportedTypeTable _exportedTypeTable;
		private FieldTable _fieldTable;
		private FieldLayoutTable _fieldLayoutTable;
		private FieldMarshalTable _fieldMarshalTable;
		private FieldPtrTable _fieldPtrTable;
		private FieldRVATable _fieldRVATable;
		private FileTable _fileTable;
		private GenericParamTable _genericParamTable;
		private GenericParamConstraintTable _genericParamConstraintTable;
		private ImplMapTable _implMapTable;
		private InterfaceImplTable _interfaceImplTable;
		private ManifestResourceTable _manifestResourceTable;
		private MemberRefTable _memberRefTable;
		private MethodTable _methodTable;
		private MethodImplTable _methodImplTable;
		private MethodPtrTable _methodPtrTable;
		private MethodSemanticsTable _methodSemanticsTable;
		private MethodSpecTable _methodSpecTable;
		private ModuleTable _moduleTable;
		private ModuleRefTable _moduleRefTable;
		private NestedClassTable _nestedClassTable;
		private ParamTable _paramTable;
		private ParamPtrTable _paramPtrTable;
		private PropertyTable _propertyTable;
		private PropertyMapTable _propertyMapTable;
		private PropertyPtrTable _propertyPtrTable;
		private StandAloneSigTable _standAloneSigTable;
		private TypeDefTable _typeDefTable;
		private TypeRefTable _typeRefTable;
		private TypeSpecTable _typeSpecTable;
		private MetadataTable[] _tables;

		#endregion

		#region Ctors

		internal MetadataTableStream(MetadataScope metadata)
		{
			_metadata = metadata;
			_isOptimized = true;
			_sortMask = 0xffffffffffffffff;

			InitializeTables();
		}

		#endregion

		#region Properties

		public MetadataTable this[int index]
		{
			get { return _tables[index]; }
		}

		/// <summary>
		/// Major version of the table schema (1 for v1.0 and v1.1; 2 for v2.0).
		/// </summary>
		public byte SchemaMajorVersion
		{
			get { return _schemaMajorVersion; }
			set { _schemaMajorVersion = value; }
		}

		/// <summary>
		/// Minor version of the table schema (0 for all versions).
		/// </summary>
		public byte SchemaMinorVersion
		{
			get { return _schemaMinorVersion; }
			set { _schemaMinorVersion = value; }
		}

		/// <summary>
		/// #~: A compressed (optimized) metadata stream. This stream contains an optimized
		/// system of metadata tables.
		///
		/// #-: An uncompressed (unoptimized) metadata stream. This stream contains an unoptimized
		/// system of metadata tables, which includes at least one intermediate lookup table (pointer table).
		///
		/// The streams #~ and #- are mutually exclusive—that is, the metadata structure of the module
		/// is either optimized or unoptimized;
		/// </summary>
		public bool IsOptimized
		{
			get { return _isOptimized; }
			set { _isOptimized = value; }
		}

		public MetadataScope Metadata
		{
			get { return _metadata; }
		}

		public AssemblyTable AssemblyTable
		{
			get { return _assemblyTable; }
		}

		public AssemblyOSTable AssemblyOSTable
		{
			get { return _assemblyOSTable; }
		}

		public AssemblyProcessorTable AssemblyProcessorTable
		{
			get { return _assemblyProcessorTable; }
		}

		public AssemblyRefTable AssemblyRefTable
		{
			get { return _assemblyRefTable; }
		}

		public AssemblyRefOSTable AssemblyRefOSTable
		{
			get { return _assemblyRefOSTable; }
		}

		public AssemblyRefProcessorTable AssemblyRefProcessorTable
		{
			get { return _assemblyRefProcessorTable; }
		}

		public ClassLayoutTable ClassLayoutTable
		{
			get { return _classLayoutTable; }
		}

		public ConstantTable ConstantTable
		{
			get { return _constantTable; }
		}

		public CustomAttributeTable CustomAttributeTable
		{
			get { return _customAttributeTable; }
		}

		public DeclSecurityTable DeclSecurityTable
		{
			get { return _declSecurityTable; }
		}

		public ENCLogTable ENCLogTable
		{
			get { return _enclogTable; }
		}

		public ENCMapTable ENCMapTable
		{
			get { return _encmapTable; }
		}

		public EventTable EventTable
		{
			get { return _eventTable; }
		}

		public EventMapTable EventMapTable
		{
			get { return _eventMapTable; }
		}

		public EventPtrTable EventPtrTable
		{
			get { return _eventPtrTable; }
		}

		public ExportedTypeTable ExportedTypeTable
		{
			get { return _exportedTypeTable; }
		}

		public FieldTable FieldTable
		{
			get { return _fieldTable; }
		}

		public FieldLayoutTable FieldLayoutTable
		{
			get { return _fieldLayoutTable; }
		}

		public FieldMarshalTable FieldMarshalTable
		{
			get { return _fieldMarshalTable; }
		}

		public FieldPtrTable FieldPtrTable
		{
			get { return _fieldPtrTable; }
		}

		public FieldRVATable FieldRVATable
		{
			get { return _fieldRVATable; }
		}

		public FileTable FileTable
		{
			get { return _fileTable; }
		}

		public GenericParamTable GenericParamTable
		{
			get { return _genericParamTable; }
		}

		public GenericParamConstraintTable GenericParamConstraintTable
		{
			get { return _genericParamConstraintTable; }
		}

		public ImplMapTable ImplMapTable
		{
			get { return _implMapTable; }
		}

		public InterfaceImplTable InterfaceImplTable
		{
			get { return _interfaceImplTable; }
		}

		public ManifestResourceTable ManifestResourceTable
		{
			get { return _manifestResourceTable; }
		}

		public MemberRefTable MemberRefTable
		{
			get { return _memberRefTable; }
		}

		public MethodTable MethodTable
		{
			get { return _methodTable; }
		}

		public MethodImplTable MethodImplTable
		{
			get { return _methodImplTable; }
		}

		public MethodPtrTable MethodPtrTable
		{
			get { return _methodPtrTable; }
		}

		public MethodSemanticsTable MethodSemanticsTable
		{
			get { return _methodSemanticsTable; }
		}

		public MethodSpecTable MethodSpecTable
		{
			get { return _methodSpecTable; }
		}

		public ModuleTable ModuleTable
		{
			get { return _moduleTable; }
		}

		public ModuleRefTable ModuleRefTable
		{
			get { return _moduleRefTable; }
		}

		public NestedClassTable NestedClassTable
		{
			get { return _nestedClassTable; }
		}

		public ParamTable ParamTable
		{
			get { return _paramTable; }
		}

		public ParamPtrTable ParamPtrTable
		{
			get { return _paramPtrTable; }
		}

		public PropertyTable PropertyTable
		{
			get { return _propertyTable; }
		}

		public PropertyMapTable PropertyMapTable
		{
			get { return _propertyMapTable; }
		}

		public PropertyPtrTable PropertyPtrTable
		{
			get { return _propertyPtrTable; }
		}

		public StandAloneSigTable StandAloneSigTable
		{
			get { return _standAloneSigTable; }
		}

		public TypeDefTable TypeDefTable
		{
			get { return _typeDefTable; }
		}

		public TypeRefTable TypeRefTable
		{
			get { return _typeRefTable; }
		}

		public TypeSpecTable TypeSpecTable
		{
			get { return _typeSpecTable; }
		}

		internal ulong SortMask
		{
			get { return _sortMask; }
		}

		#endregion

		#region Methods

		internal void Read(IBinaryAccessor accessor)
		{
			// Reserved, 4 bytes
			accessor.ReadInt32();

			_schemaMajorVersion = accessor.ReadByte();
			_schemaMinorVersion = accessor.ReadByte();

			byte heapFlags = accessor.ReadByte();

			// Reserved, 1 bytes
			accessor.ReadByte();

			ulong validMask = accessor.ReadUInt64();
			_sortMask = accessor.ReadUInt64();

			// Row counts
			int[] rowCounts = new int[MetadataConstants.TableCount];
			for (int tableType = 0; tableType < MetadataConstants.TableCount; tableType++)
			{
				if ((validMask & (1UL << tableType)) != 0)
				{
					rowCounts[tableType] = accessor.ReadInt32();
				}
			}

			var compressionInfo = TableCompressionInfo.Create(_metadata, rowCounts, heapFlags);

			// Tables
			for (int tableType = 0; tableType < MetadataConstants.TableCount; tableType++)
			{
				_tables[tableType].Read(accessor, compressionInfo, rowCounts[tableType]);
			}
		}

		private void InitializeTables()
		{
			_assemblyTable = new AssemblyTable(this);
			_assemblyOSTable = new AssemblyOSTable(this);
			_assemblyProcessorTable = new AssemblyProcessorTable(this);
			_assemblyRefTable = new AssemblyRefTable(this);
			_assemblyRefOSTable = new AssemblyRefOSTable(this);
			_assemblyRefProcessorTable = new AssemblyRefProcessorTable(this);
			_classLayoutTable = new ClassLayoutTable(this);
			_constantTable = new ConstantTable(this);
			_customAttributeTable = new CustomAttributeTable(this);
			_declSecurityTable = new DeclSecurityTable(this);
			_enclogTable = new ENCLogTable(this);
			_encmapTable = new ENCMapTable(this);
			_eventTable = new EventTable(this);
			_eventMapTable = new EventMapTable(this);
			_eventPtrTable = new EventPtrTable(this);
			_exportedTypeTable = new ExportedTypeTable(this);
			_fieldTable = new FieldTable(this);
			_fieldLayoutTable = new FieldLayoutTable(this);
			_fieldMarshalTable = new FieldMarshalTable(this);
			_fieldPtrTable = new FieldPtrTable(this);
			_fieldRVATable = new FieldRVATable(this);
			_fileTable = new FileTable(this);
			_genericParamTable = new GenericParamTable(this);
			_genericParamConstraintTable = new GenericParamConstraintTable(this);
			_implMapTable = new ImplMapTable(this);
			_interfaceImplTable = new InterfaceImplTable(this);
			_manifestResourceTable = new ManifestResourceTable(this);
			_memberRefTable = new MemberRefTable(this);
			_methodTable = new MethodTable(this);
			_methodImplTable = new MethodImplTable(this);
			_methodPtrTable = new MethodPtrTable(this);
			_methodSemanticsTable = new MethodSemanticsTable(this);
			_methodSpecTable = new MethodSpecTable(this);
			_moduleTable = new ModuleTable(this);
			_moduleRefTable = new ModuleRefTable(this);
			_nestedClassTable = new NestedClassTable(this);
			_paramTable = new ParamTable(this);
			_paramPtrTable = new ParamPtrTable(this);
			_propertyTable = new PropertyTable(this);
			_propertyMapTable = new PropertyMapTable(this);
			_propertyPtrTable = new PropertyPtrTable(this);
			_standAloneSigTable = new StandAloneSigTable(this);
			_typeDefTable = new TypeDefTable(this);
			_typeRefTable = new TypeRefTable(this);
			_typeSpecTable = new TypeSpecTable(this);

			_tables = new MetadataTable[]
			{
				_moduleTable,
				_typeRefTable,
				_typeDefTable,
				_fieldPtrTable,
				_fieldTable,
				_methodPtrTable,
				_methodTable,
				_paramPtrTable,
				_paramTable,
				_interfaceImplTable,
				_memberRefTable,
				_constantTable,
				_customAttributeTable,
				_fieldMarshalTable,
				_declSecurityTable,
				_classLayoutTable,
				_fieldLayoutTable,
				_standAloneSigTable,
				_eventMapTable,
				_eventPtrTable,
				_eventTable,
				_propertyMapTable,
				_propertyPtrTable,
				_propertyTable,
				_methodSemanticsTable,
				_methodImplTable,
				_moduleRefTable,
				_typeSpecTable,
				_implMapTable,
				_fieldRVATable,
				_enclogTable,
				_encmapTable,
				_assemblyTable,
				_assemblyProcessorTable,
				_assemblyOSTable,
				_assemblyRefTable,
				_assemblyRefProcessorTable,
				_assemblyRefOSTable,
				_fileTable,
				_exportedTypeTable,
				_manifestResourceTable,
				_nestedClassTable,
				_genericParamTable,
				_methodSpecTable,
				_genericParamConstraintTable,
			};

			for (int i = 0; i < _tables.Length; i++)
			{
				_tables[i].Initialize();
			}
		}

		#endregion

		#region Sorting

		public bool IsSorted(int tableType)
		{
			return ((_sortMask & (1UL << tableType)) != 0);
		}

		public void SetSorted(bool value)
		{
			_sortMask = value ? 0xffffffffffffffff : 0UL;
		}

		public void SetSorted(int tableType, bool value)
		{
			ulong mask = 1UL << tableType;
			if (value)
				_sortMask |= mask;
			else
				_sortMask &= ~mask;
		}

		public int[][] Sort()
		{
			int[] classLayoutMap = SortClassLayoutTable();
			int[] constantMap = SortConstantTable();
			int[] fieldLayoutMap = SortFieldLayoutTable();
			int[] fieldMarshalMap = SortFieldMarshalTable();
			int[] fieldRVAMap = SortFieldRVATable();
			int[] propertyMap = SortPropertyMapTable();
			int[] eventMap = SortEventMapTable();
			int[] implMapMap = SortImplMapTable();
			int[] interfaceImplMap = SortInterfaceImplTable();
			int[] methodImplMap = SortMethodImplTable();
			int[] methodSemanticsMap = SortMethodSemanticsTable();
			int[] nestedClassMap = SortNestedClassTable();
			int[] declSecurityMap = SortDeclSecurityTable();
			int[] genericParamMap = SortGenericParamTable();

			// Update GenericParamConstraint table
			if (genericParamMap != null)
			{
				int count = _genericParamConstraintTable.Count;
				int[] owners = new int[count];
				_genericParamConstraintTable.Get(1, 0, count, owners);

				for (int i = 0; i < count; i++)
				{
					owners[i] = genericParamMap[owners[i] - 1];
				}

				_genericParamConstraintTable.Update(1, 0, count, owners);
			}

			int[] genericParamConstraintMap = SortGenericParamConstraintTable();

			// Update CustomAttribute table
			{
				int count = _customAttributeTable.Count;
				int[] parents = new int[count];
				_customAttributeTable.Get(1, 0, count, parents);

				for (int i = 0; i < count; i++)
				{
					int parentToken = MetadataToken.DecompressHasCustomAttribute(parents[i]);
					int parentRID = MetadataToken.GetRID(parentToken);
					int parentType = MetadataToken.GetType(parentToken);
					bool changed = false;
					switch (parentType)
					{
						case MetadataTokenType.GenericParam:
							{
								if (genericParamMap != null)
								{
									parentRID = genericParamMap[parentRID - 1];
									changed = true;
								}
							}
							break;

						case MetadataTokenType.GenericParamConstraint:
							{
								if (genericParamConstraintMap != null)
								{
									parentRID = genericParamConstraintMap[parentRID - 1];
									changed = true;
								}
							}
							break;

						case MetadataTokenType.InterfaceImpl:
							{
								if (interfaceImplMap != null)
								{
									parentRID = interfaceImplMap[parentRID - 1];
									changed = true;
								}
							}
							break;

						case MetadataTokenType.DeclSecurity:
							{
								if (declSecurityMap != null)
								{
									parentRID = declSecurityMap[parentRID - 1];
									changed = true;
								}
							}
							break;
					}

					if (changed)
					{
						parents[i] = MetadataToken.CompressHasCustomAttribute(MetadataToken.Get(parentType, parentRID));
					}
				}

				_customAttributeTable.Update(1, 0, count, parents);
			}

			int[] customAttributeMap = SortCustomAttributeTable();

			return new int[][]
			{
				classLayoutMap,
				constantMap,
				fieldLayoutMap,
				fieldMarshalMap,
				fieldRVAMap,
				propertyMap,
				eventMap,
				genericParamMap,
				genericParamConstraintMap,
				implMapMap,
				interfaceImplMap,
				methodImplMap,
				methodSemanticsMap,
				nestedClassMap,
				declSecurityMap,
				customAttributeMap,
			};
		}

		public int[] SortClassLayoutTable()
		{
			if (!CanSortAndMark(_classLayoutTable))
				return null;

			return SortTable<ClassLayoutRow>(_classLayoutTable, new ClassLayoutTableSortComparer(_classLayoutTable));
		}

		public int[] SortConstantTable()
		{
			if (!CanSortAndMark(_constantTable))
				return null;

			return SortTable<ConstantRow>(_constantTable, new ConstantTableSortComparer(_constantTable));
		}

		public int[] SortFieldLayoutTable()
		{
			if (!CanSortAndMark(_fieldLayoutTable))
				return null;

			return SortTable<FieldLayoutRow>(_fieldLayoutTable, new FieldLayoutTableSortComparer(_fieldLayoutTable));
		}

		public int[] SortFieldMarshalTable()
		{
			if (!CanSortAndMark(_fieldMarshalTable))
				return null;

			return SortTable<FieldMarshalRow>(_fieldMarshalTable, new FieldMarshalTableSortComparer(_fieldMarshalTable));
		}

		public int[] SortFieldRVATable()
		{
			if (!CanSortAndMark(_fieldRVATable))
				return null;

			return SortTable<FieldRVARow>(_fieldRVATable, new FieldRVATableSortComparer(_fieldRVATable));
		}

		public int[] SortPropertyMapTable()
		{
			if (!CanSortAndMark(_propertyMapTable))
				return null;

			return SortTable<PropertyMapRow>(_propertyMapTable, new PropertyMapTableSortComparer(_propertyMapTable));
		}

		public int[] SortEventMapTable()
		{
			if (!CanSortAndMark(_eventMapTable))
				return null;

			return SortTable<EventMapRow>(_eventMapTable, new EventMapTableSortComparer(_eventMapTable));
		}

		public int[] SortImplMapTable()
		{
			if (!CanSortAndMark(_implMapTable))
				return null;

			return SortTable<ImplMapRow>(_implMapTable, new ImplMapTableSortComparer(_implMapTable));
		}

		public int[] SortInterfaceImplTable()
		{
			if (!CanSortAndMark(_interfaceImplTable))
				return null;

			return SortTable<InterfaceImplRow>(_interfaceImplTable, new InterfaceImplTableSortComparer(_interfaceImplTable));
		}

		public int[] SortMethodImplTable()
		{
			if (!CanSortAndMark(_methodImplTable))
				return null;

			return SortTable<MethodImplRow>(_methodImplTable, new MethodImplTableSortComparer(_methodImplTable));
		}

		public int[] SortMethodSemanticsTable()
		{
			if (!CanSortAndMark(_methodSemanticsTable))
				return null;

			return SortTable<MethodSemanticsRow>(_methodSemanticsTable, new MethodSemanticsTableSortComparer(_methodSemanticsTable));
		}

		public int[] SortNestedClassTable()
		{
			if (!CanSortAndMark(_nestedClassTable))
				return null;

			return SortTable<NestedClassRow>(_nestedClassTable, new NestedClassTableSortComparer(_nestedClassTable));
		}

		public int[] SortDeclSecurityTable()
		{
			if (!CanSortAndMark(_declSecurityTable))
				return null;

			return SortTable<DeclSecurityRow>(_declSecurityTable, new DeclSecurityTableSortComparer(_declSecurityTable));
		}

		public int[] SortGenericParamTable()
		{
			if (!CanSortAndMark(_genericParamTable))
				return null;

			return SortTable<GenericParamRow>(_genericParamTable, new GenericParamTableSortComparer(_genericParamTable));
		}

		public int[] SortGenericParamConstraintTable()
		{
			if (!CanSortAndMark(_genericParamConstraintTable))
				return null;

			return SortTable<GenericParamConstraintRow>(_genericParamConstraintTable, new GenericParamConstraintTableSortComparer(_genericParamConstraintTable));
		}

		public int[] SortCustomAttributeTable()
		{
			if (!CanSortAndMark(_customAttributeTable))
				return null;

			return SortTable<CustomAttributeRow>(_customAttributeTable, new CustomAttributeTableSortComparer(_customAttributeTable));
		}

		private bool CanSortAndMark(MetadataTable table)
		{
			if (IsSorted(table.Type))
				return false;

			SetSorted(table.Type, true);

			if (table.Count == 0)
				return false;

			return true;
		}

		private int[] SortTable<T>(MetadataTable<T> table, IComparer<int> comparer)
			where T : struct
		{
			int count = table.Count;

			// Create rids
			int[] indexes = new int[count];
			for (int i = 0; i < count; i++)
			{
				indexes[i] = i;
			}

			// Sort rids.
			Array.Sort<int>(indexes, 0, count, comparer);

			var oldRows = table.GetRows();
			var newRows = new T[count];
			int[] ridMap = new int[count];

			// Map
			for (int i = 0; i < count; i++)
			{
				int mapIndex = indexes[i];
				ridMap[mapIndex] = i + 1;
				newRows[i] = oldRows[mapIndex];
			}

			table.SetRows(newRows);

			return ridMap;
		}

		#endregion

		#region Optimization

		public void Optimize()
		{
			if (_isOptimized)
				return;

			OptimizeTable<EventRow>(_eventTable, _eventPtrTable);
			OptimizeTable<FieldRow>(_fieldTable, _fieldPtrTable);
			OptimizeTable<MethodRow>(_methodTable, _methodPtrTable);
			OptimizeTable<ParamRow>(_paramTable, _paramPtrTable);
			OptimizeTable<PropertyRow>(_propertyTable, _propertyPtrTable);
			_isOptimized = true;
		}

		public void Deoptimize()
		{
			if (!_isOptimized)
				return;

			DeoptimizeTable<EventRow>(_eventTable, _eventPtrTable);
			DeoptimizeTable<FieldRow>(_fieldTable, _fieldPtrTable);
			DeoptimizeTable<MethodRow>(_methodTable, _methodPtrTable);
			DeoptimizeTable<ParamRow>(_paramTable, _paramPtrTable);
			DeoptimizeTable<PropertyRow>(_propertyTable, _propertyPtrTable);
			_isOptimized = false;
		}

		private void OptimizeTable<T>(MetadataTable<T> table, MetadataPtrTable ptrTable)
			where T : struct
		{
			int count = table.Count;
			if (count == 0)
				return;

			if (ptrTable.Count == 0)
				return;

			int[] rids = new int[count];
			for (int i = 0; i < count; i++)
			{
				rids[i] = ptrTable.Get(i + 1);
			}

			table.Map(rids);
			ptrTable.Clear();
		}

		private void DeoptimizeTable<T>(MetadataTable<T> table, MetadataPtrTable ptrTable)
			where T : struct
		{
			int count = table.Count;
			if (count == 0)
				return;

			ptrTable.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				ptrTable.Add(i + 1);
			}
		}

		#endregion
	}
}
