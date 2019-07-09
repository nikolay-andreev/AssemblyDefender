using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class EventReference : Signature, IEventSignature
	{
		#region Fields

		private string _name;
		private TypeSignature _eventType;
		private TypeSignature _owner;

		#endregion

		#region Ctors

		private EventReference()
		{
		}

		public EventReference(string name, TypeSignature eventType, TypeSignature owner)
		{
			if (eventType == null)
				throw new ArgumentNullException("eventType");

			if (owner == null)
				throw new ArgumentNullException("owner");

			_name = name.NullIfEmpty();
			_eventType = eventType;
			_owner = owner;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public TypeSignature EventType
		{
			get { return _eventType; }
		}

		public TypeSignature Owner
		{
			get { return _owner; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Event; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(null, SignaturePrintingFlags.None);
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _eventType);
			module.AddSignature(ref _owner);
		}

		#endregion

		#region IEventSignature

		ITypeSignature IEventSignature.EventType
		{
			get { return _eventType; }
		}

		ITypeSignature IEventSignature.Owner
		{
			get { return _owner; }
		}

		#endregion
	}
}
