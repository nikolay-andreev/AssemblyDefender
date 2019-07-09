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
	public class CustomAttributeCtorArgumentCollection : CodeNode, IEnumerable<CustomAttributeTypedArgument>
	{
		#region Fields

		private CustomAttribute _customAttribute;
		private List<CustomAttributeTypedArgument> _list = new List<CustomAttributeTypedArgument>();

		#endregion

		#region Ctors

		internal CustomAttributeCtorArgumentCollection(CustomAttribute customAttribute)
			: base(customAttribute)
		{
			_customAttribute = customAttribute;

			if (customAttribute.IsNew)
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public CustomAttributeTypedArgument this[int index]
		{
			get { return _list[index]; }
			set
			{
				_list[index] = value;
				OnChanged();
			}
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public CustomAttribute CustomAttribute
		{
			get { return _customAttribute; }
		}

		#endregion

		#region Methods

		public int IndexOf(CustomAttributeTypedArgument item)
		{
			return _list.IndexOf(item);
		}

		public void Add(CustomAttributeTypedArgument item)
		{
			_list.Add(item);
			OnChanged();
		}

		private void Insert(int index, CustomAttributeTypedArgument item)
		{
			_list.Insert(index, item);
			OnChanged();
		}

		public bool Remove(CustomAttributeTypedArgument item)
		{
			if (!_list.Remove(item))
				return false;

			OnChanged();
			return true;
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
			OnChanged();
		}

		public void Clear()
		{
			_list.Clear();
			OnChanged();
		}

		public void CopyTo(CustomAttributeCtorArgumentCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		public IEnumerator<CustomAttributeTypedArgument> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void Load(IBinaryAccessor accessor)
		{
			foreach (var argument in _customAttribute.Constructor.Arguments)
			{
				var value = LoadValue(accessor, argument);
				_list.Add(value);
			}
		}

		private CustomAttributeTypedArgument LoadValue(IBinaryAccessor accessor, TypeSignature typeSig)
		{
			bool isArray = false;
			while (typeSig.ElementType != null)
			{
				if (typeSig.ElementCode == TypeElementCode.Array)
					isArray = true;

				typeSig = typeSig.ElementType;
			}

			var typeRef = typeSig as TypeReference;
			if (typeRef == null)
			{
				throw new CodeModelException(string.Format(SR.AssemblyLoadError, _module.Location));
			}

			int length = isArray ? CustomAttributeHelper.ReadArrayLength(accessor) : 1;

			return CustomAttributeTypedArgument.Load(accessor, _module, typeRef, isArray, length);
		}

		#endregion
	}
}
