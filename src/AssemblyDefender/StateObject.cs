using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;
using AssemblyDefender.Net;

namespace AssemblyDefender
{
	public abstract class StateObject
	{
		private bool _isChanged = true;
		protected int _rid;
		protected ModuleState _state;

		public bool IsChanged
		{
			get { return _isChanged; }
		}

		public int RID
		{
			get { return _rid; }
		}

		protected void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_state.OnChanged();
			}
		}

		internal void Init(int rid, ModuleState state)
		{
			_rid = rid;
			_state = state;
		}

		protected T GetOrCreateObject<T>(ref int rid)
			where T : StateObject, new()
		{
			if (rid > 0)
			{
				return _state.GetObject<T>(rid);
			}
			else
			{
				var item = new T();
				_state.AddObject(item);
				rid = item.RID;
				OnChanged();

				return item;
			}
		}

		protected internal virtual void Read(IBinaryAccessor accessor)
		{
			_isChanged = false;
		}

		protected internal virtual void Write(IBinaryAccessor accessor)
		{
			_isChanged = false;
		}

		protected string ReadString(IBinaryAccessor accessor)
		{
			return _state.GetString(accessor.Read7BitEncodedInt());
		}

		protected ISignature ReadSignature(IBinaryAccessor accessor)
		{
			return _state.GetSignature(accessor.Read7BitEncodedInt());
		}

		protected T[] ReadSignatures<T>(IBinaryAccessor accessor)
			where T : Signature
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return null;

			var signatures = new T[count];
			for (int i = 0; i < count; i++)
			{
				signatures[i] = (T)_state.GetSignature(accessor.Read7BitEncodedInt());
			}

			return signatures;
		}

		protected int[] ReadIntArray(IBinaryAccessor accessor)
		{
			int count = accessor.Read7BitEncodedInt();
			int[] value = new int[count];
			for (int i = 0; i < count; i++)
			{
				value[i] = accessor.Read7BitEncodedInt();
			}

			return value;
		}

		protected void WriteString(IBinaryAccessor accessor, string value)
		{
			accessor.Write7BitEncodedInt(_state.AddString(value));
		}

		protected void WriteSignature(IBinaryAccessor accessor, ISignature signature)
		{
			accessor.Write7BitEncodedInt(_state.AddSignature(signature));
		}

		protected void WriteSignatures(IBinaryAccessor accessor, Signature[] signatures)
		{
			int count = (signatures != null) ? signatures.Length : 0;
			accessor.Write7BitEncodedInt(count);
			for (int i = 0; i < count; i++)
			{
				accessor.Write7BitEncodedInt(_state.AddSignature(signatures[i]));
			}
		}

		protected void WriteIntArray(IBinaryAccessor accessor, int[] value)
		{
			int count = (value != null) ? value.Length : 0;
			accessor.Write7BitEncodedInt(count);

			for (int i = 0; i < count; i++)
			{
				accessor.Write7BitEncodedInt(value[i]);
			}

		}
	}
}
