using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;
using System.Reflection;

namespace AssemblyDefender.UI.Model
{
	public static class Commands
	{
		private static ICommand[] _commands = new ICommand[(int)CommandID.Last + 1];

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand New
		{
			get { return Get(CommandID.New); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand Open
		{
			get { return Get(CommandID.Open); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand<bool> Save
		{
			get { return Get<bool>(CommandID.Save); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand Exit
		{
			get { return Get(CommandID.Exit); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand Refresh
		{
			get { return Get(CommandID.Refresh); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand ViewSearch
		{
			get { return Get(CommandID.ViewSearch); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand ViewDecodeStackTrace
		{
			get { return Get(CommandID.ViewDecodeStackTrace); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand ViewStartPage
		{
			get { return Get(CommandID.ViewStartPage); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand AddAssembly
		{
			get { return Get(CommandID.AddAssembly); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand RemoveAssembly
		{
			get { return Get(CommandID.RemoveAssembly); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand Build
		{
			get { return Get(CommandID.Build); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand<string> GoWeb
		{
			get { return Get<string>(CommandID.GoWeb); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand RegisterProduct
		{
			get { return Get(CommandID.RegisterProduct); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand About
		{
			get { return Get(CommandID.About); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand GoBack
		{
			get { return Get(CommandID.GoBack); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand GoForward
		{
			get { return Get(CommandID.GoForward); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand ExpandAll
		{
			get { return Get(CommandID.ExpandAll); }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static DelegateCommand CollapseAll
		{
			get { return Get(CommandID.CollapseAll); }
		}

		private static DelegateCommand Get(CommandID commandID)
		{
			int index = (int)commandID;
			var command = _commands[index] as DelegateCommand;
			if (command == null)
			{
				command = new DelegateCommand();
				_commands[index] = command;
			}

			return command;
		}

		private static DelegateCommand<T> Get<T>(CommandID commandID)
		{
			int index = (int)commandID;
			var command = _commands[index] as DelegateCommand<T>;
			if (command == null)
			{
				command = new DelegateCommand<T>();
				_commands[index] = command;
			}

			return command;
		}

		private enum CommandID
		{
			New,
			Open,
			Save,
			Exit,
			Refresh,
			ViewSearch,
			ViewDecodeStackTrace,
			ViewStartPage,
			AddAssembly,
			RemoveAssembly,
			Build,
			GoWeb,
			RegisterProduct,
			ReportBug,
			About,
			GoBack,
			GoForward,
			ExpandAll,
			CollapseAll,
			Last = CollapseAll,
		}
	}
}
