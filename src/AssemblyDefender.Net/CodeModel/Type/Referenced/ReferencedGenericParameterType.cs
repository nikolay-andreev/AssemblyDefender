using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedGenericParameterType : ReferencedType
	{
		private bool _isMethod;
		private int _position;

		internal ReferencedGenericParameterType(AssemblyManager assemblyManager, bool isMethod, int position)
			: base(assemblyManager)
		{
			_isMethod = isMethod;
			_position = position;
		}

		public override string Name
		{
			get { return (_isMethod ? "!!" : "!") + _position.ToString(); }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.GenericParameter; }
		}

		public override void GetGenericParameter(out bool isMethod, out int position)
		{
			isMethod = _isMethod;
			position = _position;
		}
	}
}
