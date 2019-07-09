using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender
{
	public delegate void ProjectBuildTaskEventHandler(object sender, ProjectBuildTaskEventArgs e);

	public class ProjectBuildTaskEventArgs : EventArgs
	{
		private BuildAssembly _assembly;
		private ProjectBuildTaskType _taskType;

		public ProjectBuildTaskEventArgs(BuildAssembly assembly, ProjectBuildTaskType taskType)
		{
			_assembly = assembly;
			_taskType = taskType;
		}

		public BuildAssembly Assembly
		{
			get { return _assembly; }
		}

		public ProjectBuildTaskType TaskType
		{
			get { return _taskType; }
		}
	}
}
