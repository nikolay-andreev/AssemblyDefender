using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;
using System.Globalization;

namespace AssemblyDefender
{
	public abstract class Builder
	{
		#region Fields

		private bool _cancellationPending;
		private string _mainTypeNamespace;
		private BuildStatus _status;
		private Exception _exception;
		private Random _random;
		private BuildAssemblyManager _assemblyManager;
		private BuildAssemblyCollection _assemblies;
		private HashList<string> _cultures;
		private object _abortLockObject = new object();

		#endregion

		#region Ctors

		protected Builder()
		{
			_random = new Random();
			_assemblyManager = new BuildAssemblyManager(this);
			_assemblies = new BuildAssemblyCollection(_assemblyManager);
		}

		#endregion

		#region Properties

		public BuildStatus Status
		{
			get { return _status; }
		}

		public Exception Exception
		{
			get { return _exception; }
		}

		protected internal bool CancellationPending
		{
			get { return _cancellationPending; }
		}

		protected internal string MainTypeNamespace
		{
			get { return _mainTypeNamespace; }
			set { _mainTypeNamespace = value; }
		}

		protected internal Random Random
		{
			get { return _random; }
		}

		protected internal BuildAssemblyCollection Assemblies
		{
			get { return _assemblies; }
		}

		protected internal HashList<string> Cultures
		{
			get
			{
				if (_cultures == null)
				{
					LoadCultures();
				}

				return _cultures;
			}
		}

		#endregion

		#region Methods

		public void Build()
		{
			if (_status != BuildStatus.NotStarted)
				throw new InvalidOperationException();

#if DEBUG
			try
			{
				_status = BuildStatus.InProgress;

				OnBuild();

				_status = _cancellationPending ? BuildStatus.Aborted : BuildStatus.Completed;

				_exception = null;
			}
			finally
			{
				Close();
			}
#else
			try
			{
				try
				{
					_status = BuildStatus.InProgress;

					OnBuild();

					_status = _cancellationPending ? BuildStatus.Aborted : BuildStatus.Completed;
				}
				catch (Exception ex)
				{
					_status = BuildStatus.Failed;
					_exception = ex;
				}
			}
			finally
			{
				Close();
			}
#endif
		}

		public void BeginBuild()
		{
			((Action)Build).BeginInvoke(null, null);
		}

		public void Abort()
		{
			if (_status == BuildStatus.InProgress && !_cancellationPending)
			{
				lock (_abortLockObject)
				{
					if (_status == BuildStatus.InProgress && !_cancellationPending)
					{
						_cancellationPending = true;
						Monitor.Wait(_abortLockObject);
					}
				}
			}
		}

		public void BeginAbort()
		{
			if (_status == BuildStatus.InProgress && !_cancellationPending)
			{
				_cancellationPending = true;
			}
		}

		protected abstract void OnBuild();

		protected virtual void Close()
		{
			if (_cancellationPending)
			{
				lock (_abortLockObject)
				{
					if (_cancellationPending)
					{
						_cancellationPending = false;
						Monitor.Pulse(_abortLockObject);
					}
				}
			}

			if (_assemblyManager != null)
			{
				_assemblyManager.Dispose();
				_assemblyManager = null;
			}

			_assemblies = null;
			_random = null;
		}

		protected void Scavenge()
		{
			_assemblyManager.Scavenge();
		}

		protected void SaveState()
		{
			foreach (var assembly in Assemblies)
			{
				assembly.SaveState();
			}
		}

		protected void CopyAndSign()
		{
			foreach (var state in _assemblyManager.BuildStates)
			{
				File.Copy(state.BuildFilePath, state.OutputFilePath, true);

				if (state.SignAssembly)
				{
					Sign(state);
				}
			}
		}

		private void Sign(ModuleState state)
		{
			byte[] keyPair = File.ReadAllBytes(state.StrongNameKeyFilePath);

			if (!string.IsNullOrEmpty(state.StrongNameKeyPassword))
			{
				keyPair = StrongNameUtils.ExtractKeyPairFromPKCS12(keyPair, state.StrongNameKeyPassword);
			}

			foreach (string filePath in state.SignFiles)
			{
				StrongNameUtils.SignAssemblyFromKeyPair(filePath, keyPair, StrongNameGenerationFlags.None);
			}
		}

		private void LoadCultures()
		{
			var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

			_cultures = new HashList<string>(cultures.Length, StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < cultures.Length; i++)
			{
				_cultures.TryAdd(cultures[i].Name);
			}
		}

		#endregion
	}
}
