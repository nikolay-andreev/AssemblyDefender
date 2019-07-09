using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;
using CA = AssemblyDefender.Net.CustomAttributes;

namespace AssemblyDefender
{
	public class ProjectBuilder : Builder
	{
		#region Fields

		public event ProjectBuildTaskEventHandler TaskStarted;
		public event ProjectBuildTaskEventHandler TaskCompleted;
		public static readonly int StageCount = 5;
		private int _assemblyCount;
		private bool _obfuscateControlFlow;
		private bool _renameAssemblies;
		private bool _renameMembers;
		private bool _encryptIL;
		private bool _obfuscateResources;
		private bool _obfuscateStrings;
		private bool _removeUnusedMembers;
		private bool _sealTypes;
		private bool _devirtualizeMethods;
		private int _evaluationPeriodInDays;
		private string _projectFilePath;
		private string _logFilePath;
		private Project _project;
		private BuildLog _log;
		private MemberNameGenerator _nameGenerator;
		private List<TupleStruct<string, string>> _renamedAssemblyNames;

		#endregion

		#region Ctors

		public ProjectBuilder(Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			_project = project;
			_projectFilePath = project.Location;

			if (!string.IsNullOrEmpty(_projectFilePath))
			{
				_logFilePath = Path.ChangeExtension(_projectFilePath, BuildLog.FileExtension);
			}
		}

		#endregion

		#region Properties

		public int EvaluationPeriodInDays
		{
			get { return _evaluationPeriodInDays; }
			set { _evaluationPeriodInDays = value; }
		}

		#endregion

		#region Methods

		public int GetTaskCount()
		{
			return _project.Assemblies.Count * StageCount;
		}

		protected override void OnBuild()
		{
			_assemblyCount = _project.Assemblies.Count;
			if (_assemblyCount == 0)
				return;

			Load();
			Analyze();
			Analyze2();
			Analyze3();
			Change();
		}

		protected override void Close()
		{
			if (_assemblyCount > 0)
			{
				SaveLog();
			}

			base.Close();
		}

		private BuildLog OpenLog()
		{
			if (string.IsNullOrEmpty(_logFilePath))
				return null;

			if (!File.Exists(_logFilePath))
				return null;

			var log = BuildLog.LoadFile(_logFilePath);
			if (log == null)
				return null;

			return log;
		}

		private void SaveLog()
		{
			if (string.IsNullOrEmpty(_logFilePath))
				return;

			_log.BuildDate = DateTime.Now;
			_log.BuildStatus = Status;
			_log.Error = (Exception != null) ? new BuildErrorLog(Exception) : null;
			_log.SaveFile(_logFilePath);
		}

		private void FireTaskStarted(BuildAssembly assembly, ProjectBuildTaskType taskType)
		{
			var handler = TaskStarted;
			if (handler != null)
			{
				handler(this, new ProjectBuildTaskEventArgs(assembly, taskType));
			}
		}

		private void FireTaskCompleted(BuildAssembly assembly, ProjectBuildTaskType taskType)
		{
			var handler = TaskCompleted;
			if (handler != null)
			{
				handler(this, new ProjectBuildTaskEventArgs(assembly, taskType));
			}
		}

		private void Load()
		{
			if (CancellationPending)
				return;

			_log = OpenLog();

			if (_log == null)
			{
				_log = new BuildLog();
			}

			_nameGenerator = new MemberNameGenerator(_log, Random);

			// Add assemblies
			foreach (var projectAssembly in _project.Assemblies)
			{
				Assemblies.Add(projectAssembly.FilePath);
			}

			// Load assemblies
			var assemblyLoader = new BuildAssemblyLoader();

			foreach (var projectAssembly in _project.Assemblies)
			{
				var assembly = Assemblies[projectAssembly.FilePath];

				FireTaskStarted(assembly, ProjectBuildTaskType.Load);

				assemblyLoader.Load(assembly, projectAssembly);
				assembly.SaveState();

				if (CancellationPending)
					return;

				FireTaskCompleted(assembly, ProjectBuildTaskType.Load);
			}

			_obfuscateControlFlow = assemblyLoader.ObfuscateControlFlow;
			_renameMembers = assemblyLoader.RenameMembers;
			_encryptIL = assemblyLoader.EncryptIL;
			_obfuscateResources = assemblyLoader.ObfuscateResources;
			_obfuscateStrings = assemblyLoader.ObfuscateStrings;
			_removeUnusedMembers = assemblyLoader.RemoveUnusedMembers;
			_sealTypes = assemblyLoader.SealTypes;
			_devirtualizeMethods = assemblyLoader.DevirtualizeMethods;
			_renamedAssemblyNames = assemblyLoader.RenamedAssemblyNames;
			_renameAssemblies = (_renamedAssemblyNames.Count > 0);

			// Load generated names
			foreach (var assembly in Assemblies)
			{
				_nameGenerator.Load(assembly);
			}

			_nameGenerator.GenerateGlobal();
			MainTypeNamespace = _nameGenerator.MainTypeNamespace;

			Scavenge();
		}

		private void Analyze()
		{
			if (CancellationPending)
				return;

			foreach (var projectAssembly in _project.Assemblies)
			{
				var assembly = Assemblies[projectAssembly.FilePath];

				FireTaskStarted(assembly, ProjectBuildTaskType.Analyze);

				Analyze(assembly, projectAssembly);

				if (CancellationPending)
					return;

				FireTaskCompleted(assembly, ProjectBuildTaskType.Analyze);
			}

			Scavenge();
		}

		private void Analyze(BuildAssembly assembly, ProjectAssembly projectAssembly)
		{
			ConfigurationAnalizer.Analyze(assembly, projectAssembly.RenameConfigurationMembers);

			SerializationAnalizer.Analyze(assembly, projectAssembly.RenameSerializableMembers);

			BindableAttributeAnalizer.Analyze(assembly, projectAssembly.RenameBindableMembers);

			DependencyPropertyAnalizer.Analyze(assembly);

			CodeAnalizer.Analyze(assembly);

			BamlAnalyzer.Analyze(assembly);

			SaveState();
		}

		private void Analyze2()
		{
			if (CancellationPending)
				return;

			foreach (var assembly in Assemblies)
			{
				FireTaskStarted(assembly, ProjectBuildTaskType.Analyze2);

				Analyze2(assembly);

				if (CancellationPending)
					return;

				FireTaskCompleted(assembly, ProjectBuildTaskType.Analyze2);
			}

			Scavenge();
		}

		private void Analyze2(BuildAssembly assembly)
		{
			if (_removeUnusedMembers)
				StripAnalyzer.Analyze(assembly);

			if (_sealTypes)
				TypeSealer.Analyze(assembly);

			if (CancellationPending)
				return;

			if (_devirtualizeMethods)
				MethodDevirtualizer.Analyze(assembly);

			if (_renameMembers)
			{
				MismatchTypeGenericArgumentAnalyzer.Analyze(assembly);
				MemberRenameHelper.FixExportedTypeNames(assembly, _nameGenerator);
			}

			SaveState();
		}

		private void Analyze3()
		{
			if (CancellationPending)
				return;

			foreach (var assembly in Assemblies)
			{
				FireTaskStarted(assembly, ProjectBuildTaskType.Analyze3);

				Analyze3(assembly);

				if (CancellationPending)
					return;

				FireTaskCompleted(assembly, ProjectBuildTaskType.Analyze3);
			}

			Scavenge();
		}

		private void Analyze3(BuildAssembly assembly)
		{
			if (assembly.RemoveUnusedMembers)
				StripUnmarker.Unmark(assembly);

			if (assembly.EncryptIL)
				ILCryptoMethodBuilder.Build(assembly);

			_nameGenerator.Generate(assembly);

			SaveState();
		}

		private void Change()
		{
			if (CancellationPending)
				return;

			foreach (var assembly in Assemblies)
			{
				FireTaskStarted(assembly, ProjectBuildTaskType.Change);

				Change(assembly);

				if (CancellationPending)
					return;

				FireTaskCompleted(assembly, ProjectBuildTaskType.Change);
			}

			CopyAndSign();
		}

		private void Change(BuildAssembly assembly)
		{
			var module = (BuildModule)assembly.Module;
			var strings = new HashList<string>();

			MainType.Create(assembly);

			if (assembly.EncryptIL)
				strings.Add("."); // Dot is at index 0

			if (_evaluationPeriodInDays > 0)
				module.MainType.GenerateCheckForExpiredEvaluation(_evaluationPeriodInDays);

			if (assembly.StripObfuscationAttributeExists)
				ObfuscationAttributeStripper.Strip(assembly);

			if (assembly.SuppressILdasm)
				CA.SuppressIldasmAttribute.AddIfNotExists(assembly.CustomAttributes);

			if (assembly.RenameMembers || assembly.DevirtualizeMethods)
				ExplicitMethodCallBuilder.Build(assembly);

			if (CancellationPending)
				return;

			if (_renameAssemblies)
				MemberRenameHelper.BuildRenamedAssemblyResolver(assembly, _renamedAssemblyNames);

			if (assembly.HasWpfResource && (_renameAssemblies || _renameMembers))
				BamlMemberReferenceMapper.Map(assembly, _log);

			if (assembly.HasWpfResource && _renameAssemblies)
				ResourceHelper.RenameWpfResource(assembly);

			if (_renameAssemblies)
				ResourceHelper.RenameSatelliteAssemblies(assembly);

			if (assembly.ObfuscateResources)
				ResourceObfuscator.Obfuscate(assembly);

			if (_obfuscateResources)
				ResourceResolverGenerator.Generate(assembly);

			if (assembly.ObfuscateStrings)
				StringObfuscator.Obfuscate(assembly, strings);

			if (CancellationPending)
				return;

			if (assembly.SealTypes)
				TypeSealer.Seal(assembly);

			if (assembly.DevirtualizeMethods)
				MethodDevirtualizer.Devirtualize(assembly);

			if (CancellationPending)
				return;

			if (_renameMembers)
				ExplicitMethodOverrideBuilder.Build(assembly, _nameGenerator);

			if (_encryptIL)
				ILCryptoMapper.Map(assembly);

			if (assembly.EncryptIL)
				ILCryptoObfuscator.Obfuscate(assembly, strings);

			if (CancellationPending)
				return;

			if (assembly.RemoveUnusedMembers)
				Stripper.Strip(assembly);

			if (assembly.ObfuscateControlFlow)
				ControlFlowObfuscator.Obfuscate(assembly, true);

			if (strings.Count > 0)
				StringLoaderGenerator.Generate(assembly, strings);

			if (CancellationPending)
				return;

			// From this point no more code can be added.

			MainType.Generate(assembly);
			DelegateTypeGenerator.Generate(assembly);
			GeneratedCodeObfuscator.Obfuscate(assembly, _nameGenerator);

			if (CancellationPending)
				return;

			ILCryptoBlobBuilder.MapMemberReferences(assembly);
			MemberReferenceMapper.Map(assembly);
			MemberNameChanger.Change(assembly);

			if (CancellationPending)
				return;

			assembly.Compile();

			Scavenge();
		}

		#endregion
	}
}
