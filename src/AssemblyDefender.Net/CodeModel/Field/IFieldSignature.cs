using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface IFieldSignature : ISignature
	{
		string Name
		{
			get;
		}

		ITypeSignature FieldType
		{
			get;
		}

		ITypeSignature Owner
		{
			get;
		}
	}
}
