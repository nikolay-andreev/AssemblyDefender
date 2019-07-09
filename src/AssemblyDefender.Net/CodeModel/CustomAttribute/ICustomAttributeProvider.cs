using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface ICustomAttributeProvider
	{
		CustomAttributeCollection CustomAttributes
		{
			get;
		}
	}
}
