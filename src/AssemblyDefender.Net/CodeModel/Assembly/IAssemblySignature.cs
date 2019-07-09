using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public interface IAssemblySignature : ISignature
	{
		string Name
		{
			get;
		}

		string Culture
		{
			get;
		}

		Version Version
		{
			get;
		}

		byte[] PublicKeyToken
		{
			get;
		}

		ProcessorArchitecture ProcessorArchitecture
		{
			get;
		}

		bool IsStrongNameSigned
		{
			get;
		}
	}
}
