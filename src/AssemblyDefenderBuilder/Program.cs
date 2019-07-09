using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AssemblyDefender;
using CmdLine = AssemblyDefender.Common.CommandLine;

namespace AssemblyDefenderBuilder
{
	class Program
	{
		[DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
		private static extern bool SetConsoleCtrlHandler(ConsoleEventHandler handler, bool add);
		private delegate bool ConsoleEventHandler(CtrlType sig);

		private static bool _shell;
		private static bool _quiet;
		private static bool _nologo;
		private static int _currentTaskCount;
		private static int _currentTaskID = -1;
		private static string _projectFilePath;
		private static ProjectBuilder _builder;
		private static ConsoleEventHandler _consoleHandler;

		static void Main(string[] args)
		{
#if (DEBUG != true)
			// Don't handle the exceptions in Debug mode because otherwise the Debugger wouldn't
			// jump into the code when an exception occurs.
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif

			LoadCommandLine(args);

			HookupConsoleHandler();

			if (_shell)
			{
				BeginReadInput();
			}
			else
			{
				if (!_nologo)
				{
					PrintHeader();
				}
			}

			Build();
		}

		private static void Build()
		{
			var project = Project.LoadFile(_projectFilePath, true);

			_builder = new ProjectBuilder(project);
			_builder.TaskStarted += OnTaskStarted;
			_builder.TaskCompleted += OnTaskCompleted;

			_builder.Build();

			switch (_builder.Status)
			{
				case BuildStatus.Completed:
					if (!_shell && !_quiet)
					{
						Console.WriteLine("Build completed successfully");
					}
					break;

				case BuildStatus.Aborted:
					if (!_shell && !_quiet)
					{
						Console.WriteLine("Build aborted");
					}
					break;

				case BuildStatus.Failed:
					{
						if (!_shell)
						{
							PrintException(_builder.Exception);
						}

						Environment.Exit(-1);
					}
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private static void LoadCommandLine(string[] args)
		{
			var helpArg = new CmdLine.BoolArgument();

			var quietArg = new CmdLine.BoolArgument();

			var nologoArg = new CmdLine.BoolArgument();

			var shellArg = new CmdLine.BoolArgument();

			var projectFilePathArg = new CmdLine.StringArgument()
			{
				AtMostOnce = true,
				Required = true,
			};

			var parser = new CmdLine.CommandLineParser();
			parser.AddArgument("help", "?", helpArg);
			parser.AddArgument("quiet", quietArg);
			parser.AddArgument("nologo", nologoArg);
			parser.AddArgument("shell", shellArg);
			parser.DefaultArgument = projectFilePathArg;

			try
			{
				parser.Parse(args);
			}
			catch (Exception)
			{
				PrintHelp();
				Environment.Exit(1);
				return;
			}

			if (helpArg.Value)
			{
				PrintHelp();
				Environment.Exit(0);
				return;
			}

			if (string.IsNullOrEmpty(projectFilePathArg.Value))
			{
				PrintHelp();
				Environment.Exit(1);
				return;
			}

			_quiet = quietArg.Value;
			_nologo = nologoArg.Value;
			_shell = shellArg.Value;
			_projectFilePath = projectFilePathArg.Value;
		}


		private static void PrintHeader()
		{
			Console.WriteLine("Assembly Defender {0}", typeof(Program).Assembly.GetName().Version.ToString());
			Console.WriteLine("Copyright (C) Nikolay Andreev. All rights reserved.");
			Console.WriteLine();
		}

		private static void PrintHelp()
		{
			PrintHeader();

			Console.WriteLine("Usage: ProjectBuilder [Options] <project file>");
			Console.WriteLine();

			CmdLine.CommandLinePrinter.Print(
				new CmdLine.ArgumentUsage[]
					{
						new CmdLine.ArgumentUsage("/help", "Display this usage message (Short form /?)"),
						new CmdLine.ArgumentUsage("/nologo", "Suppress copyright message"),
						new CmdLine.ArgumentUsage("/quiet", "Suppress build messages"),
					});
		}

		private static void HookupConsoleHandler()
		{
			_consoleHandler += new ConsoleEventHandler(ConsoleHandler);
			SetConsoleCtrlHandler(_consoleHandler, true);
		}

		private static void BeginReadInput()
		{
			((Action)ReadInput).BeginInvoke(null, null);
		}

		private static void ReadInput()
		{
			while (true)
			{
				string inputText = Console.ReadLine();
				switch (inputText)
				{
					case "shutdown":
						Abort();
						return;
				}
			}
		}

		private static void Abort()
		{
			_builder.Abort();
		}

		private static bool ConsoleHandler(CtrlType sig)
		{
			Console.Error.WriteLine("Stopping...");

			Abort();

			return true;
		}

		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			PrintException(e.ExceptionObject as Exception);
			Environment.Exit(-1);
		}

		private static void PrintException(Exception exception)
		{
			string message = "Error: ";
			if (IsInternalError(exception))
				message += AssemblyDefender.Common.SR.InternalError;
			else
				message += exception.Message;

			Console.Error.WriteLine(message);
		}

		private static bool IsInternalError(Exception exception)
		{
			if (exception == null)
				return true;

			if (string.IsNullOrEmpty(exception.Message))
				return true;

			if (exception is InvalidOperationException)
				return true;

			return false;
		}

		private static void OnTaskStarted(object sender, ProjectBuildTaskEventArgs e)
		{
			_currentTaskCount++;
		}

		private static void OnTaskCompleted(object sender, ProjectBuildTaskEventArgs e)
		{
			if (_shell)
			{
				Console.WriteLine(_currentTaskCount);
			}
			else
			{
				int taskID = (int)GetTaskID(e.TaskType);
				if (_currentTaskID < taskID)
				{
					_currentTaskID = taskID;
					Console.WriteLine(PrintTaskID(taskID));
				}
			}
		}

		private static int GetTaskID(ProjectBuildTaskType taskType)
		{
			switch (taskType)
			{
				case ProjectBuildTaskType.Load:
					return 0;

				case ProjectBuildTaskType.Analyze:
				case ProjectBuildTaskType.Analyze2:
				case ProjectBuildTaskType.Analyze3:
					return 1;

				case ProjectBuildTaskType.Change:
					return 2;

				default:
					throw new NotImplementedException();
			}
		}

		private static string PrintTaskID(int taskID)
		{
			switch (taskID)
			{
				case 0:
					return "Loading...";

				case 1:
					return "Analyzing...";

				case 2:
					return "Changing...";

				default:
					throw new NotImplementedException();
			}
		}

		private enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6,
		}
	}
}
