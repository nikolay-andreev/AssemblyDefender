using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public class FileWatcher : IDisposable
	{
		#region Events

		/// <summary>
		/// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is changed.
		/// </summary>
		public event FileSystemEventHandler Changed;

		/// <summary>
		/// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is created.
		/// </summary>
		public event FileSystemEventHandler Created;

		/// <summary>
		/// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is deleted.
		/// </summary>
		public event FileSystemEventHandler Deleted;

		/// <summary>
		/// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is renamed.
		/// </summary>
		public event RenamedEventHandler Renamed;

		#endregion

		#region Fields

		private Dictionary<string, FileSystemWatcher> _watchedFiles = new Dictionary<string, FileSystemWatcher>();
		private object _lockObject = new object();
		private bool _disposed;

		#endregion

		#region Ctors

		public FileWatcher()
		{
		}

		#endregion

		#region Methods

		public bool IsWatched(string filePath)
		{
			lock (_lockObject)
			{
				return _watchedFiles.ContainsKey(CanonicalizeFilePath(filePath));
			}
		}

		public void StartWatching(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullOrEmptyException("filePath");

			var fileInfo = new FileInfo(filePath);
			filePath = fileInfo.FullName.ToUpper(CultureInfo.CurrentCulture);

			lock (_lockObject)
			{
				if (!_watchedFiles.ContainsKey(filePath))
				{
					var fsw = new FileSystemWatcher()
					{
						Path = fileInfo.DirectoryName,
						Filter = fileInfo.Name,
						EnableRaisingEvents = true,
					};

					WireUpEvents(fsw);

					_watchedFiles.Add(filePath, fsw);
				}
			}
		}

		public void StopWatching(string filePath)
		{
			filePath = CanonicalizeFilePath(filePath);

			lock (_lockObject)
			{
				FileSystemWatcher fsw;
				if (_watchedFiles.TryGetValue(filePath, out fsw))
				{
					UnwireEvents(fsw);
					fsw.Dispose();
					_watchedFiles.Remove(filePath);
				}
			}
		}

		private string CanonicalizeFilePath(string anyFileName)
		{
			var fileInfo = new FileInfo(anyFileName);
			string fullPath = fileInfo.FullName.ToUpper(CultureInfo.CurrentCulture);

			return fullPath;
		}

		private void WireUpEvents(FileSystemWatcher fsw)
		{
			fsw.Changed += OnChanged;
			fsw.Created += OnCreated;
			fsw.Deleted += OnDeleted;
			fsw.Renamed += OnRenamed;
		}

		private void UnwireEvents(FileSystemWatcher fsw)
		{
			fsw.Changed -= OnChanged;
			fsw.Created -= OnCreated;
			fsw.Deleted -= OnDeleted;
			fsw.Renamed -= OnRenamed;
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			if (Changed != null)
			{
				Changed(this, e);
			}
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			if (Created != null)
			{
				Created(this, e);
			}
		}

		private void OnDeleted(object sender, FileSystemEventArgs e)
		{
			if (Deleted != null)
			{
				Deleted(this, e);
			}
		}

		private void OnRenamed(object sender, RenamedEventArgs e)
		{
			if (Renamed != null)
			{
				Renamed(this, e);
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			// Don't dispose more than once
			if (_disposed)
				return;

			_disposed = true;

			foreach (string filePath in _watchedFiles.Keys)
			{
				StopWatching(filePath);
			}

			_watchedFiles.Clear();
		}

		#endregion
	}
}
