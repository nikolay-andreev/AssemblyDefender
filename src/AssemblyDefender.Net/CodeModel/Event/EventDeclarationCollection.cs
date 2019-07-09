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
	public class EventDeclarationCollection : MemberNodeCollection<EventDeclaration>
	{
		#region Fields

		private TypeDeclaration _type;

		#endregion

		#region Ctors

		internal EventDeclarationCollection(TypeDeclaration type)
			: base(type)
		{
			_type = type;

			if (type.IsNew)
			{
				IsNew = true;
			}
			else
			{
				Load();
			}
		}

		#endregion

		#region Properties

		public TypeDeclaration Type
		{
			get { return _type; }
		}

		#endregion

		#region Methods

		public EventDeclaration Find(string name, bool throwIfMissing = false)
		{
			foreach (var eventDecl in this)
			{
				if (eventDecl.Name == name)
					return eventDecl;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.EventNotFound, _type.ToString(), name));
			}

			return null;
		}

		public void CopyTo(EventDeclarationCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override EventDeclaration GetItem(int rid)
		{
			return _module.EventTable.Get(rid, _type.RID);
		}

		protected override EventDeclaration CreateItem()
		{
			return _module.CreateEvent(0, _type.RID);
		}

		private void Load()
		{
			var image = _module.Image;

			int[] rids;
			image.GetEventsByType(_type.RID, out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
