using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Defines which code hierarchy objects has to be cached.
	/// </summary>
	[Flags]
	public enum AssemblyCacheMode : int
	{
		None = 0,
		Type = 1,
		Method = Type << 1,
		Field = Method << 1,
		Property = Field << 1,
		Event = Property << 1,
		Resource = Event << 1,
		CustomAttribute = Resource << 1,
		SecurityAttribute = CustomAttribute << 1,
		All = unchecked((int)0xffffffff),
		Members = Type | Method | Field | Property | Event,
	}
}
