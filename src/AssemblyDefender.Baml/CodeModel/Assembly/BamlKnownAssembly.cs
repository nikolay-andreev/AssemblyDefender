using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public class BamlKnownAssembly : IBamlAssembly
	{
		private BamlKnownAssemblyCode _knownCode;

		public BamlKnownAssembly()
		{
		}

		public BamlKnownAssembly(BamlKnownAssemblyCode knownCode)
		{
			_knownCode = knownCode;
		}

		public BamlKnownAssemblyCode KnownCode
		{
			get { return _knownCode; }
			set { _knownCode = value; }
		}

		BamlAssemblyKind IBamlAssembly.Kind
		{
			get { return BamlAssemblyKind.Known; }
		}

		public Assembly Resolve(Assembly ownerAssembly)
		{
			var assemblyRef = ToReference(ownerAssembly);
			if (assemblyRef == null)
				return null;

			return (Assembly)assemblyRef.Resolve(ownerAssembly.Module);
		}

		public AssemblyReference ToReference(Assembly ownerAssembly)
		{
			switch (_knownCode)
			{
				case BamlKnownAssemblyCode.MsCorLib:
					return AssemblyReference.GetMscorlib(ownerAssembly);

				case BamlKnownAssemblyCode.System:
					return AssemblyReference.GetSystem(ownerAssembly);

				case BamlKnownAssemblyCode.PresentationCore:
					return ToReference("PresentationCore", ownerAssembly);

				case BamlKnownAssemblyCode.PresentationFramework:
					return ToReference("PresentationFramework", ownerAssembly);

				case BamlKnownAssemblyCode.WindowsBase:
					return ToReference("WindowsBase", ownerAssembly);

				default:
					return null;
			}
		}

		private AssemblyReference ToReference(string assemblyName, Assembly ownerAssembly)
		{
			var assemblyRef = ownerAssembly.AssemblyReferences.Find(assemblyName, true);
			if (assemblyRef != null)
				return assemblyRef;

			var mscorlibRef = AssemblyReference.GetMscorlib(ownerAssembly);

			return new AssemblyReference(
				assemblyName,
				CodeModelUtils.NeutralCulture,
				mscorlibRef.Version,
				MicrosoftNetFramework.BclPublicKeyToken);
		}

		public override string ToString()
		{
			return string.Format("KnownAssembly: {0}", _knownCode.ToString());
		}
	}
}
