using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;
using Build = Microsoft.Build.Evaluation;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model
{
	public class MSBuildAssemblyList : IEnumerable<MSBuildAssembly>
	{
		#region Fields

		private List<MSBuildAssembly> _list = new List<MSBuildAssembly>();

		#endregion

		#region Ctors

		public MSBuildAssemblyList(string projectFilePath)
		{
			Load(projectFilePath);
		}

		#endregion

		#region Properties


		#endregion

		#region Methods

		public IEnumerator<MSBuildAssembly> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void Load(string projectFilePath)
		{
			var projects = new Build.ProjectCollection();

			string ext = Path.GetExtension(projectFilePath).ToLower();
			if (ext == ".sln")
			{
				LoadSolution(projectFilePath, projects);
			}
			else
			{
				LoadProject(projectFilePath, projects);
			}
		}

		private void LoadSolution(string solutionFilePath, Build.ProjectCollection projects)
		{
			if (!File.Exists(solutionFilePath))
				return;

			string[] lines = File.ReadAllLines(solutionFilePath);
			for (int i = 0; i < lines.Length; i++)
			{
				string line = (lines[i] ?? "").TrimStart();
				if (!line.StartsWith("Project"))
					continue;

				int startIndex = line.IndexOf(", \"");
				if (startIndex < 0)
					continue;

				startIndex += 3;

				int endIndex = line.IndexOf('"', startIndex);
				if (endIndex < 0)
					continue;

				string projectFilePath = line.Substring(startIndex, endIndex - startIndex);
				projectFilePath = Path.Combine(Path.GetDirectoryName(solutionFilePath), projectFilePath);

				if (!File.Exists(projectFilePath))
					continue;

				LoadProject(projectFilePath, projects);
			}
		}

		private void LoadProject(string projectFilePath, Build.ProjectCollection projects)
		{
			if (LoadProject(projectFilePath, projects, "Release"))
				return;

			LoadProject(projectFilePath, projects, null);
		}

		private bool LoadProject(string projectFilePath, Build.ProjectCollection projects, string config)
		{
			var project = LoadBuildProject(projectFilePath, projects, config);
			if (project == null)
				return false;

			string outputPath = project.GetPropertyValue("OutputPath");
			if (string.IsNullOrEmpty(outputPath))
				return false;

			string assemblyName = project.GetPropertyValue("AssemblyName");
			if (string.IsNullOrEmpty(assemblyName))
				return false;

			string projectFolder = Path.GetDirectoryName(projectFilePath);

			outputPath = PathUtils.MakeAbsolutePath(outputPath, projectFolder);

			string assemblyFilePath = FindAssembly(outputPath, assemblyName);
			if (string.IsNullOrEmpty(assemblyFilePath))
				return false;

			var assembly = new MSBuildAssembly();
			assembly.FilePath = assemblyFilePath;

			if (IsSigned(project))
			{
				LoadStrongName(assembly, project, projectFolder);
			}

			_list.Add(assembly);

			return true;
		}

		private void LoadStrongName(MSBuildAssembly assembly, Build.Project project, string projectFolder)
		{
			string keyFile = GetKeyFile(project, projectFolder);
			string password = null;
			if (StrongNameUtils.IsPKCS12File(keyFile))
			{
				if (!StrongNamePasswordCache.TryGet(keyFile, out password))
					return;
			}

			assembly.Sign = true;
			assembly.DelaySign = IsDelaySign(project);
			assembly.KeyFilePath = keyFile;
			assembly.KeyPassword = password;
		}

		private bool IsSigned(Build.Project project)
		{
			return GetBoolProperty(project, "SignAssembly");
		}

		private bool IsDelaySign(Build.Project project)
		{
			return GetBoolProperty(project, "DelaySign");
		}

		private string GetKeyFile(Build.Project project, string outputPath)
		{
			string keyFilePath = project.GetPropertyValue("AssemblyOriginatorKeyFile");
			if (string.IsNullOrEmpty(keyFilePath))
				return null;

			return PathUtils.MakeAbsolutePath(keyFilePath, outputPath);
		}

		private string FindAssembly(string outputPath, string assemblyName)
		{
			string outputFilePathWithoutExt = Path.Combine(outputPath, assemblyName);
			string outputFilePath = outputFilePathWithoutExt + ".dll";
			if (File.Exists(outputFilePath))
				return outputFilePath;

			outputFilePath = outputFilePathWithoutExt + ".exe";
			if (File.Exists(outputFilePath))
				return outputFilePath;

			return null;
		}

		private bool GetBoolProperty(Build.Project project, string name)
		{
			string s = project.GetPropertyValue(name);
			if (string.IsNullOrEmpty(s))
				return false;

			bool result;
			if (!bool.TryParse(s, out result))
				return false;

			return true;
		}

		private static Build.Project LoadBuildProject(string filePath, Build.ProjectCollection projects, string config)
		{
			try
			{
				Dictionary<string, string> globalProperties = null;
				if (!string.IsNullOrEmpty(config))
				{
					globalProperties = new Dictionary<string, string>();
					globalProperties.Add("Configuration", "Release");
				}

				return projects.LoadProject(filePath, globalProperties, null);
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion
	}
}
