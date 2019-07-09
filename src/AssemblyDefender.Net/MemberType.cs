using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum MemberType
	{
		Type,
		Method,
		Field,
		Property,
		Event,
		Resource,
		CustomAttribute,
		SecurityAttribute,
	}
}
