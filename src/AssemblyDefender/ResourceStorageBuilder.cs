using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ResourceStorageBuilder : BuildTask
	{
		public override void Build()
		{
			var moduleBuilder = PE.Tasks.Get<ModuleBuilder>();
			if (moduleBuilder == null)
				return;

			var module = (BuildModule)moduleBuilder.Module;
			if (!module.IsPrimeModule)
				return;

			var resourceStorage = ((BuildAssembly)module.Assembly).ResourceStorage;
			resourceStorage.Build(moduleBuilder);
		}
	}
}
