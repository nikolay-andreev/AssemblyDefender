using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender
{
	internal class ProjectScavengeState
	{
		internal bool RenameMembers;
		internal bool ObfuscateControlFlow;
		internal bool EncryptIL;
		internal bool ObfuscateStrings;
		internal bool RemoveUnusedMembers;
		internal bool SealTypes;
		internal bool DevirtualizeMethods;
		internal bool ObfuscateResources;

		internal ProjectScavengeState(ProjectAssembly assembly)
		{
			RenameMembers = assembly.RenameMembers;
			ObfuscateControlFlow = assembly.ObfuscateControlFlow;
			EncryptIL = assembly.EncryptIL;
			ObfuscateStrings = assembly.ObfuscateStrings;
			RemoveUnusedMembers = assembly.RemoveUnusedMembers;
			SealTypes = assembly.SealTypes;
			DevirtualizeMethods = assembly.DevirtualizeMethods;
			ObfuscateResources = assembly.ObfuscateResources;
		}
	}
}
