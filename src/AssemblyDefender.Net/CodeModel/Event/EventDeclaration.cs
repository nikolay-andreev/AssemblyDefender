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
	public class EventDeclaration : MemberNode, IEvent, ICustomAttributeProvider
	{
		#region Fields

		private const int LoadAddSemanticFlag = 1;
		private const int LoadRemoveSemanticFlag = 2;
		private const int LoadInvokeSemanticFlag = 3;
		private string _name;
		private int _flags;
		private int _ownerTypeRID;
		private TypeSignature _eventType;
		private MethodReference _addMethod;
		private MethodReference _removeMethod;
		private MethodReference _invokeMethod;
		private CustomAttributeCollection _customAttributes;
		private IType _resolvedEventType;
		private IMethod _resolvedAddMethod;
		private IMethod _resolvedRemoveMethod;
		private IMethod _resolvedInvokeMethod;
		private int _opFlags;

		#endregion

		#region Ctors

		protected EventDeclaration(Module module)
			: base(module)
		{
		}

		protected internal EventDeclaration(Module module, int rid, int typeRID)
			: base(module)
		{
			_rid = rid;
			_ownerTypeRID = typeRID;

			if (_rid > 0)
			{
				Load();
			}
			else
			{
				IsNew = true;
				_rid = _module.EventTable.Add(this);
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

		public bool IsSpecialName
		{
			get { return _flags.IsBitsOn(EventFlags.SpecialName); }
			set
			{
				_flags = _flags.SetBits(EventFlags.SpecialName, value);
				OnChanged();
			}
		}

		public bool IsRuntimeSpecialName
		{
			get { return _flags.IsBitsOn(EventFlags.RTSpecialName); }
			set
			{
				_flags = _flags.SetBits(EventFlags.RTSpecialName, value);
				OnChanged();
			}
		}

		public TypeSignature EventType
		{
			get { return _eventType; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("EventType");

				_eventType = value;
				_module.AddSignature(ref _eventType);

				OnChanged();
			}
		}

		public MethodReference AddMethod
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadAddSemanticFlag))
				{
					LoadSemantics();
				}

				return _addMethod;
			}
			set
			{
				_addMethod = value;
				_module.AddSignature(ref _addMethod);
				_opFlags = _opFlags.SetBitAtIndex(LoadAddSemanticFlag, false);
				OnChanged();
			}
		}

		public MethodReference RemoveMethod
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadRemoveSemanticFlag))
				{
					LoadSemantics();
				}

				return _removeMethod;
			}
			set
			{
				_removeMethod = value;
				_module.AddSignature(ref _removeMethod);
				_opFlags = _opFlags.SetBitAtIndex(LoadRemoveSemanticFlag, false);
				OnChanged();
			}
		}

		public MethodReference InvokeMethod
		{
			get
			{
				if (_opFlags.IsBitAtIndexOn(LoadInvokeSemanticFlag))
				{
					LoadSemantics();
				}

				return _invokeMethod;
			}
			set
			{
				_invokeMethod = value;
				_module.AddSignature(ref _invokeMethod);
				_opFlags = _opFlags.SetBitAtIndex(LoadInvokeSemanticFlag, false);
				OnChanged();
			}
		}

		public CustomAttributeCollection CustomAttributes
		{
			get
			{
				if (_customAttributes == null)
				{
					int token = MetadataToken.Get(MetadataTokenType.Event, _rid);
					_customAttributes = new CustomAttributeCollection(this, token);
				}

				return _customAttributes;
			}
		}

		public override MemberType MemberType
		{
			get { return MemberType.Event; }
		}

		#endregion

		#region Methods

		public TypeDeclaration GetOwnerType()
		{
			return _module.GetType(_ownerTypeRID);
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public void CopyTo(EventDeclaration copy)
		{
			copy._name = _name;
			copy._flags = _flags;
			copy._eventType = _eventType;
			copy._addMethod = AddMethod;
			copy._removeMethod = RemoveMethod;
			copy._invokeMethod = InvokeMethod;
			CustomAttributes.CopyTo(copy.CustomAttributes);
		}

		internal void InvalidatedSignatures()
		{
			_resolvedEventType = null;
			_resolvedAddMethod = null;
			_resolvedRemoveMethod = null;
			_resolvedInvokeMethod = null;
		}

		protected internal override void OnDeleted()
		{
			_module.EventTable.Remove(_rid);
		}

		protected internal override void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_module.OnChanged();
				_module.EventTable.Change(_rid);
			}
		}

		protected void Load()
		{
			var image = _module.Image;

			EventRow row;
			image.GetEvent(_rid, out row);

			_name = image.GetString(row.Name);

			_flags = row.Flags;

			_eventType = TypeSignature.Load(_module, MetadataToken.DecompressTypeDefOrRef(row.EventType));

			_opFlags = _opFlags.SetBitAtIndex(LoadAddSemanticFlag, true);
			_opFlags = _opFlags.SetBitAtIndex(LoadRemoveSemanticFlag, true);
			_opFlags = _opFlags.SetBitAtIndex(LoadInvokeSemanticFlag, true);
		}

		protected void LoadSemantics()
		{
			var image = _module.Image;

			int token = MetadataToken.Get(MetadataTokenType.Event, _rid);

			int[] rids;
			image.GetMethodSemanticsByAssociation(MetadataToken.CompressHasSemantic(token), out rids);

			for (int i = 0; i < rids.Length; i++)
			{
				MethodSemanticsRow row;
				image.GetMethodSemantics(rids[i], out row);

				var methodRef = MethodReference.LoadMethodDef(_module, row.Method);
				switch (row.Semantic)
				{
					case MethodSemanticFlags.AddOn:
						if (_opFlags.IsBitAtIndexOn(LoadAddSemanticFlag))
						{
							_addMethod = methodRef;
						}
						break;

					case MethodSemanticFlags.RemoveOn:
						if (_opFlags.IsBitAtIndexOn(LoadRemoveSemanticFlag))
						{
							_removeMethod = methodRef;
						}
						break;

					case MethodSemanticFlags.Fire:
						if (_opFlags.IsBitAtIndexOn(LoadInvokeSemanticFlag))
						{
							_invokeMethod = methodRef;
						}
						break;
				}
			}

			_opFlags = _opFlags.SetBitAtIndex(LoadAddSemanticFlag, false);
			_opFlags = _opFlags.SetBitAtIndex(LoadRemoveSemanticFlag, false);
			_opFlags = _opFlags.SetBitAtIndex(LoadInvokeSemanticFlag, false);
		}

		#endregion

		#region IEvent Members

		IType IEvent.EventType
		{
			get
			{
				if (_resolvedEventType == null)
				{
					_resolvedEventType = AssemblyManager.Resolve(_eventType, this, true);
				}

				return _resolvedEventType;
			}
		}

		IType IEvent.Owner
		{
			get { return GetOwnerType(); }
		}

		IMethod IEvent.AddMethod
		{
			get
			{
				if (_resolvedAddMethod == null && AddMethod != null)
				{
					_resolvedAddMethod = AssemblyManager.Resolve(AddMethod, this, false, true);
				}

				return _resolvedAddMethod;
			}
		}

		IMethod IEvent.RemoveMethod
		{
			get
			{
				if (_resolvedRemoveMethod == null && RemoveMethod != null)
				{
					_resolvedRemoveMethod = AssemblyManager.Resolve(RemoveMethod, this, false, true);
				}

				return _resolvedRemoveMethod;
			}
		}

		IMethod IEvent.InvokeMethod
		{
			get
			{
				if (_resolvedInvokeMethod == null && InvokeMethod != null)
				{
					_resolvedInvokeMethod = AssemblyManager.Resolve(InvokeMethod, this, false, true);
				}

				return _resolvedInvokeMethod;
			}
		}

		IEvent IEvent.DeclaringEvent
		{
			get { return this; }
		}

		IMethodSignature IEventBase.AddMethod
		{
			get { return AddMethod; }
		}

		IMethodSignature IEventBase.RemoveMethod
		{
			get { return RemoveMethod; }
		}

		IMethodSignature IEventBase.InvokeMethod
		{
			get { return InvokeMethod; }
		}

		#endregion

		#region IEventSignature Members

		ITypeSignature IEventSignature.EventType
		{
			get { return _eventType; }
		}

		ITypeSignature IEventSignature.Owner
		{
			get { return GetOwnerType(); }
		}

		SignatureType ISignature.SignatureType
		{
			get { return SignatureType.Event; }
		}

		#endregion

		#region ICodeNode Members

		EntityType ICodeNode.EntityType
		{
			get { return EntityType.Event; }
		}

		IAssembly ICodeNode.Assembly
		{
			get { return Assembly; }
		}

		IModule ICodeNode.Module
		{
			get { return _module; }
		}

		bool ICodeNode.HasGenericContext
		{
			get { return false; }
		}

		IType ICodeNode.GetGenericArgument(bool isMethod, int position)
		{
			return null;
		}

		#endregion
	}
}
