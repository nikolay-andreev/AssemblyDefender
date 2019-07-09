using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedEvent : IEvent
	{
		#region Fields

		private IEvent _declaringEvent;
		private IType _eventType;
		private IType _ownerType;
		private IMethod _addMethod;
		private IMethod _removeMethod;
		private IMethod _invokeMethod;
		private IModule _module;

		#endregion

		#region Ctors

		internal ReferencedEvent(IEvent declaringEvent, IType ownerType)
		{
			_declaringEvent = declaringEvent;
			_ownerType = ownerType;
			_module = declaringEvent.Module;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _declaringEvent.Name; }
		}

		public bool HasGenericContext
		{
			get { return _ownerType.HasGenericContext; }
		}

		public IType EventType
		{
			get
			{
				if (_eventType == null)
				{
					_eventType = AssemblyManager.Resolve(((IEventSignature)_declaringEvent).EventType, this, true);
				}

				return _eventType;
			}
		}

		public IType Owner
		{
			get { return _ownerType; }
		}

		public IMethod AddMethod
		{
			get
			{
				if (_addMethod == null)
				{
					_addMethod = AssemblyManager.Resolve(((IEventBase)_declaringEvent).AddMethod, this, false, true);
				}

				return _addMethod;
			}
		}

		public IMethod RemoveMethod
		{
			get
			{
				if (_removeMethod == null)
				{
					_removeMethod = AssemblyManager.Resolve(((IEventBase)_declaringEvent).RemoveMethod, this, false, true);
				}

				return _removeMethod;
			}
		}

		public IMethod InvokeMethod
		{
			get
			{
				if (_invokeMethod == null)
				{
					_invokeMethod = AssemblyManager.Resolve(((IEventBase)_declaringEvent).InvokeMethod, this, false, true);
				}

				return _invokeMethod;
			}
		}

		public IEvent DeclaringEvent
		{
			get { return _declaringEvent; }
		}

		public EntityType EntityType
		{
			get { return EntityType.Event; }
		}

		public SignatureType SignatureType
		{
			get { return SignatureType.Event; }
		}

		public IAssembly Assembly
		{
			get { return _module.Assembly; }
		}

		public IModule Module
		{
			get { return _module; }
		}

		public AssemblyManager AssemblyManager
		{
			get { return _module.AssemblyManager; }
		}

		ITypeSignature IEventSignature.EventType
		{
			get { return EventType; }
		}

		ITypeSignature IEventSignature.Owner
		{
			get { return Owner; }
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

		#region Methods

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		public IType GetGenericArgument(bool isMethod, int position)
		{
			var genericArguments = _ownerType.GenericArguments;
			if (genericArguments.Count > position)
			{
				return genericArguments[position];
			}

			return null;
		}

		internal IEvent Intern()
		{
			IEvent e = this;
			AssemblyManager.InternNode<IEvent>(ref e);

			return e;
		}

		#endregion
	}
}
