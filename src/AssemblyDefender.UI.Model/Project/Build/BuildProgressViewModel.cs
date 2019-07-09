using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
    public class BuildProgressViewModel : ViewModel
    {
        #region Fields

        private bool _isClosing;
        private double _progressPercentage;
        private string _statusMessage;
        private string _exitText;
        private ImageType _statusImage;
        private int _taskRun;
        private int _totalTaskRun;
        private BuildViewModel _buildViewModel;
        private Process _process;
        private ICommand _closeCommand;
        private Action _processExitedAction;
        private Action<string> _outputMessageReceivedAction;
        private object _lockObject = new object();
        private static string _builderFilePath;

        #endregion

        #region Ctors

        static BuildProgressViewModel()
        {
            _builderFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), Constants.ProjectBuilderConsoleName);
        }

        public BuildProgressViewModel(BuildViewModel buildViewModel)
            : base(buildViewModel)
        {
            _buildViewModel = buildViewModel;
            _closeCommand = new DelegateCommand(Close);
            _exitText = SR.Cancel;
        }

        #endregion

        #region Properties

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public bool IsClosing
        {
            get { return _isClosing; }
            set
            {
                _isClosing = value;
                OnPropertyChanged("IsClosing");
            }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public double ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                OnPropertyChanged("ProgressPercentage");
            }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public string ExitText
        {
            get { return _exitText; }
            set
            {
                _exitText = value;
                OnPropertyChanged("ExitText");
            }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged("StatusMessage");
            }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public ImageType StatusImage
        {
            get { return _statusImage; }
            set
            {
                _statusImage = value;
                OnPropertyChanged("StatusImage");
            }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        #endregion

        #region Methods

        protected override void OnActivate()
        {
            if (!File.Exists(_builderFilePath))
            {
                _buildViewModel.ShowError(string.Format(Common.SR.FileNotFound, _builderFilePath), null);
                return;
            }

            _totalTaskRun = _buildViewModel.ProjectNode.Assemblies.Count * ProjectBuilder.StageCount;
            _statusMessage = SR.BuildStatusInProgress;
            _statusImage = ImageType.Compile;
            _processExitedAction = (Action)OnProcessExited;
            _outputMessageReceivedAction = (Action<string>)OnOutputMessageReceived;

            StartProcess();
        }

        protected override void OnDeactivate()
        {
            if (_process == null)
                return;

            _process.CancelOutputRead();
            _process.OutputDataReceived -= OnProcessOutputDataReceived;
            _process.Exited -= OnProcessExited;

            if (!_process.HasExited)
            {
                BeginCloseProcess();

                // Wait for process to exit.
                for (int i = 0; i < 20; i++)
                {
                    if (_process.HasExited)
                        break;

                    Thread.Sleep(100);
                }

                if (!_process.HasExited)
                {
                    _process.Kill();
                }
            }

            _process.Close();
            _process = null;
        }

        private bool BeginCloseProcess()
        {
            if (_process == null || _process.HasExited)
                return false;

            if (!_process.Responding)
            {
                _process.Kill();
                return false;
            }

            try
            {
                var standardInput = _process.StandardInput;
                standardInput.WriteLine("shutdown");
                standardInput.Flush();
            }
            catch (Exception)
            {
                _process.Kill();
                return false;
            }

            return true;
        }

        private void StartProcess()
        {
            _process = new Process();
            _process.EnableRaisingEvents = true;
            _process.OutputDataReceived += OnProcessOutputDataReceived;
            _process.Exited += OnProcessExited;

            var startInfo = _process.StartInfo;
            startInfo.FileName = _builderFilePath;
            startInfo.Arguments = string.Format("/shell \"{0}\"", _buildViewModel.ProjectFilePath);
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            _process.Start();
            _process.BeginOutputReadLine();
        }

        private void Close()
        {
            if (_isClosing)
                return;

            if (BeginCloseProcess())
            {
                IsClosing = true;
                StatusMessage = SR.BuildStatusStopping;
            }
            else
            {
                _buildViewModel.Close(true);
            }
        }

        private void OnProcessExited()
        {
            if (_isClosing)
            {
                _buildViewModel.DialogResult = true;
                return;
            }

            if (_process != null && _process.ExitCode == 0)
            {
                ProgressPercentage = 100;
                ExitText = SR.Close;
                StatusMessage = SR.BuildStatusCompleted;
                StatusImage = ImageType.Check;
            }
            else
            {
                _buildViewModel.ShowBuildError();
            }
        }

        private void OnOutputMessageReceived(string message)
        {
            _taskRun++;
            double progress = (double)(100 / ((float)_totalTaskRun / _taskRun));
            if ((int)progress > (int)_progressPercentage)
            {
                ProgressPercentage = progress;
            }
        }

        private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_isClosing)
                return;

            string message = e.Data;
            if (string.IsNullOrEmpty(message))
                return;

            AppService.UI.Invoke(
                _outputMessageReceivedAction,
                new object[] { message });
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            AppService.UI.Invoke(_processExitedAction);
        }

        #endregion
    }
}
