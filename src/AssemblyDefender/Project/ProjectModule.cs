using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectModule
	{
		#region Fields

		private int _flags;
		private string _name;
		private Dictionary<string, ProjectNamespace> _namespaces;
		private Dictionary<ITypeSignature, ProjectType> _types;
		private Dictionary<IMethodSignature, ProjectMethod> _methods;
		private Dictionary<IFieldSignature, ProjectField> _fields;
		private Dictionary<IPropertySignature, ProjectProperty> _properties;
		private Dictionary<IEventSignature, ProjectEvent> _events;

		#endregion

		#region Ctors

		public ProjectModule()
		{
		}

		internal ProjectModule(IBinaryAccessor accessor, ProjectReadState state)
		{
			Read(accessor, state);
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public bool NameChanged
		{
			get { return _flags.IsBitAtIndexOn(0); }
			set { _flags = _flags.SetBitAtIndex(0, value); }
		}

		public bool ObfuscateControlFlow
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public bool ObfuscateControlFlowChanged
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public bool RenameMembers
		{
			get { return _flags.IsBitAtIndexOn(3); }
			set { _flags = _flags.SetBitAtIndex(3, value); }
		}

		public bool RenameMembersChanged
		{
			get { return _flags.IsBitAtIndexOn(4); }
			set { _flags = _flags.SetBitAtIndex(4, value); }
		}

		public bool RenamePublicTypes
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public bool RenamePublicTypesChanged
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		public bool RenamePublicMethods
		{
			get { return _flags.IsBitAtIndexOn(7); }
			set { _flags = _flags.SetBitAtIndex(7, value); }
		}

		public bool RenamePublicMethodsChanged
		{
			get { return _flags.IsBitAtIndexOn(8); }
			set { _flags = _flags.SetBitAtIndex(8, value); }
		}

		public bool RenamePublicFields
		{
			get { return _flags.IsBitAtIndexOn(9); }
			set { _flags = _flags.SetBitAtIndex(9, value); }
		}

		public bool RenamePublicFieldsChanged
		{
			get { return _flags.IsBitAtIndexOn(10); }
			set { _flags = _flags.SetBitAtIndex(10, value); }
		}

		public bool RenamePublicProperties
		{
			get { return _flags.IsBitAtIndexOn(11); }
			set { _flags = _flags.SetBitAtIndex(11, value); }
		}

		public bool RenamePublicPropertiesChanged
		{
			get { return _flags.IsBitAtIndexOn(12); }
			set { _flags = _flags.SetBitAtIndex(12, value); }
		}

		public bool RenamePublicEvents
		{
			get { return _flags.IsBitAtIndexOn(13); }
			set { _flags = _flags.SetBitAtIndex(13, value); }
		}

		public bool RenamePublicEventsChanged
		{
			get { return _flags.IsBitAtIndexOn(14); }
			set { _flags = _flags.SetBitAtIndex(14, value); }
		}

		public bool EncryptIL
		{
			get { return _flags.IsBitAtIndexOn(15); }
			set { _flags = _flags.SetBitAtIndex(15, value); }
		}

		public bool EncryptILChanged
		{
			get { return _flags.IsBitAtIndexOn(16); }
			set { _flags = _flags.SetBitAtIndex(16, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _flags.IsBitAtIndexOn(17); }
			set { _flags = _flags.SetBitAtIndex(17, value); }
		}

		public bool ObfuscateStringsChanged
		{
			get { return _flags.IsBitAtIndexOn(18); }
			set { _flags = _flags.SetBitAtIndex(18, value); }
		}

		public bool RemoveUnusedMembers
		{
			get { return _flags.IsBitAtIndexOn(19); }
			set { _flags = _flags.SetBitAtIndex(19, value); }
		}

		public bool RemoveUnusedMembersChanged
		{
			get { return _flags.IsBitAtIndexOn(20); }
			set { _flags = _flags.SetBitAtIndex(20, value); }
		}

		public bool SealTypes
		{
			get { return _flags.IsBitAtIndexOn(21); }
			set { _flags = _flags.SetBitAtIndex(21, value); }
		}

		public bool SealTypesChanged
		{
			get { return _flags.IsBitAtIndexOn(22); }
			set { _flags = _flags.SetBitAtIndex(22, value); }
		}

		public bool DevirtualizeMethods
		{
			get { return _flags.IsBitAtIndexOn(23); }
			set { _flags = _flags.SetBitAtIndex(23, value); }
		}

		public bool DevirtualizeMethodsChanged
		{
			get { return _flags.IsBitAtIndexOn(24); }
			set { _flags = _flags.SetBitAtIndex(24, value); }
		}

		public Dictionary<string, ProjectNamespace> Namespaces
		{
			get
			{
				if (_namespaces == null)
				{
					_namespaces = new Dictionary<string, ProjectNamespace>();
				}

				return _namespaces;
			}
		}

		public Dictionary<ITypeSignature, ProjectType> Types
		{
			get
			{
				if (_types == null)
				{
					_types = new Dictionary<ITypeSignature, ProjectType>(SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);
				}

				return _types;
			}
		}

		public Dictionary<IMethodSignature, ProjectMethod> Methods
		{
			get
			{
				if (_methods == null)
				{
					_methods = new Dictionary<IMethodSignature, ProjectMethod>(SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);
				}

				return _methods;
			}
		}

		public Dictionary<IFieldSignature, ProjectField> Fields
		{
			get
			{
				if (_fields == null)
				{
					_fields = new Dictionary<IFieldSignature, ProjectField>(SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);
				}

				return _fields;
			}
		}

		public Dictionary<IPropertySignature, ProjectProperty> Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = new Dictionary<IPropertySignature, ProjectProperty>(SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);
				}

				return _properties;
			}
		}

		public Dictionary<IEventSignature, ProjectEvent> Events
		{
			get
			{
				if (_events == null)
				{
					_events = new Dictionary<IEventSignature, ProjectEvent>(SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);
				}

				return _events;
			}
		}

		internal bool IsEmpty
		{
			get
			{
				if (_flags != 0)
					return false;

				if (_namespaces != null && _namespaces.Count > 0)
					return false;

				if (_types != null && _types.Count > 0)
					return false;

				if (_methods != null && _methods.Count > 0)
					return false;

				if (_fields != null && _fields.Count > 0)
					return false;

				if (_properties != null && _properties.Count > 0)
					return false;

				if (_events != null && _events.Count > 0)
					return false;

				return true;
			}
		}

		#endregion

		#region Methods

		internal void Scavenge(ProjectScavengeState state)
		{
			if (!state.RenameMembers)
			{
				RenameMembers = false;
				RenameMembersChanged = false;
				RenamePublicTypes = false;
				RenamePublicTypesChanged = false;
				RenamePublicMethods = false;
				RenamePublicMethodsChanged = false;
				RenamePublicFields = false;
				RenamePublicFieldsChanged = false;
				RenamePublicProperties = false;
				RenamePublicPropertiesChanged = false;
				RenamePublicEvents = false;
				RenamePublicEventsChanged = false;
			}

			if (!state.ObfuscateControlFlow)
			{
				ObfuscateControlFlow = false;
				ObfuscateControlFlowChanged = false;
			}

			if (!state.EncryptIL)
			{
				EncryptIL = false;
				EncryptILChanged = false;
			}

			if (!state.ObfuscateStrings)
			{
				ObfuscateStrings = false;
				ObfuscateStringsChanged = false;
			}

			if (!state.RemoveUnusedMembers)
			{
				RemoveUnusedMembers = false;
				RemoveUnusedMembersChanged = false;
			}

			if (!state.SealTypes)
			{
				SealTypes = false;
				SealTypesChanged = false;
			}

			if (!state.DevirtualizeMethods)
			{
				DevirtualizeMethods = false;
				DevirtualizeMethodsChanged = false;
			}

			if (_namespaces != null && _namespaces.Count > 0)
			{
				var namespaces = _namespaces.ToArray();
				foreach (var kvp in namespaces)
				{
					var ns = kvp.Value;

					ns.Scavenge(state);

					if (ns.IsEmpty)
						_namespaces.Remove(kvp.Key);
				}
			}

			if (_types != null && _types.Count > 0)
			{
				var types = _types.ToArray();
				foreach (var kvp in types)
				{
					var type = kvp.Value;

					type.Scavenge(state);

					if (type.IsEmpty)
						_types.Remove(kvp.Key);
				}
			}

			if (_methods != null && _methods.Count > 0)
			{
				var methods = _methods.ToArray();
				foreach (var kvp in methods)
				{
					var method = kvp.Value;

					method.Scavenge(state);

					if (method.IsEmpty)
						_methods.Remove(kvp.Key);
				}
			}

			if (_fields != null && _fields.Count > 0)
			{
				var fields = _fields.ToArray();
				foreach (var kvp in fields)
				{
					var field = kvp.Value;

					field.Scavenge(state);

					if (field.IsEmpty)
						_fields.Remove(kvp.Key);
				}
			}

			if (_properties != null && _properties.Count > 0)
			{
				var properties = _properties.ToArray();
				foreach (var kvp in properties)
				{
					var property = kvp.Value;

					property.Scavenge(state);

					if (property.IsEmpty)
						_properties.Remove(kvp.Key);
				}
			}

			if (_events != null && _events.Count > 0)
			{
				var events = _events.ToArray();
				foreach (var kvp in events)
				{
					var e = kvp.Value;

					e.Scavenge(state);

					if (e.IsEmpty)
						_events.Remove(kvp.Key);
				}
			}
		}

		internal void Read(IBinaryAccessor accessor, ProjectReadState state)
		{
			_flags = accessor.ReadInt32();

			if (NameChanged)
				_name = state.GetString(accessor.Read7BitEncodedInt());

			ReadNamespaces(accessor, state);
			ReadTypes(accessor, state);
			ReadMethods(accessor, state);
			ReadFields(accessor, state);
			ReadProperties(accessor, state);
			ReadEvents(accessor, state);
		}

		private void ReadNamespaces(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_namespaces = new Dictionary<string, ProjectNamespace>(count);

			for (int i = 0; i < count; i++)
			{
				string name = state.GetString(accessor.Read7BitEncodedInt());
				_namespaces.Add(name, new ProjectNamespace(accessor, state));
			}
		}

		private void ReadTypes(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_types = new Dictionary<ITypeSignature, ProjectType>(count, SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);

			for (int i = 0; i < count; i++)
			{
				var typeSig = (TypeSignature)state.Signatures[accessor.Read7BitEncodedInt()];
				_types.Add(typeSig, new ProjectType(accessor, state));
			}
		}

		private void ReadMethods(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_methods = new Dictionary<IMethodSignature, ProjectMethod>(count, SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);

			for (int i = 0; i < count; i++)
			{
				var methodSig = (MethodSignature)state.Signatures[accessor.Read7BitEncodedInt()];
				_methods.Add(methodSig, new ProjectMethod(accessor, state));
			}
		}

		private void ReadFields(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_fields = new Dictionary<IFieldSignature, ProjectField>(count, SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);

			for (int i = 0; i < count; i++)
			{
				var fieldSig = (FieldReference)state.Signatures[accessor.Read7BitEncodedInt()];
				_fields.Add(fieldSig, new ProjectField(accessor, state));
			}
		}

		private void ReadProperties(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_properties = new Dictionary<IPropertySignature, ProjectProperty>(count, SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);

			for (int i = 0; i < count; i++)
			{
				var propertySig = (PropertyReference)state.Signatures[accessor.Read7BitEncodedInt()];
				_properties.Add(propertySig, new ProjectProperty(accessor, state));
			}
		}

		private void ReadEvents(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_events = new Dictionary<IEventSignature, ProjectEvent>(count, SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName);

			for (int i = 0; i < count; i++)
			{
				var eventSig = (EventReference)state.Signatures[accessor.Read7BitEncodedInt()];
				_events.Add(eventSig, new ProjectEvent(accessor, state));
			}
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write((int)_flags);

			if (NameChanged)
				accessor.Write7BitEncodedInt(state.SetString(_name));

			WriteNamespaces(accessor, state);
			WriteTypes(accessor, state);
			WriteMethods(accessor, state);
			WriteFields(accessor, state);
			WriteProperties(accessor, state);
			WriteEvents(accessor, state);
		}

		private void WriteNamespaces(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _namespaces != null ? _namespaces.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _namespaces)
			{
				accessor.Write7BitEncodedInt(state.SetString(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		private void WriteTypes(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _types != null ? _types.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _types)
			{
				accessor.Write7BitEncodedInt(state.Signatures.Add(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		private void WriteMethods(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _methods != null ? _methods.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _methods)
			{
				accessor.Write7BitEncodedInt(state.Signatures.Add(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		private void WriteFields(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _fields != null ? _fields.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _fields)
			{
				accessor.Write7BitEncodedInt(state.Signatures.Add(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		private void WriteProperties(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _properties != null ? _properties.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _properties)
			{
				accessor.Write7BitEncodedInt(state.Signatures.Add(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		private void WriteEvents(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _events != null ? _events.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _events)
			{
				accessor.Write7BitEncodedInt(state.Signatures.Add(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		#endregion
	}
}
