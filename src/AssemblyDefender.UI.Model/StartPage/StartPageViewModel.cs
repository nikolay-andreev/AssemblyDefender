using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.StartPage
{
	public class StartPageViewModel : ViewModel
	{
		#region Fields

		private ShellViewModel _shell;
		private ObservableCollection<PropertyAwareObject> _actions;

		#endregion

		#region Ctors

		internal StartPageViewModel(ShellViewModel shell)
			: base(shell)
		{
			_shell = shell;
			_actions = new ObservableCollection<PropertyAwareObject>();
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType LogoHeaderImage
		{
			get { return ImageType.LogoHeader; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ObservableCollection<PropertyAwareObject> Actions
		{
			get { return _actions; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public MRUListViewModel ProjectMRUList
		{
			get { return _shell.ProjectMRUList; }
		}

		#endregion

		#region Methods

		protected override void OnActivate()
		{
			// Set default window caption.
			_shell.WindowTitle = null;

			LoadActions();
		}

		private void LoadActions()
		{
			_actions.Add(
				new ActionViewModel()
				{
					Text = SR.NewProject,
					Image = ImageType.New,
					Command = Commands.New,
				});

			_actions.Add(
				new ActionViewModel()
				{
					Text = SR.OpenProject,
					Image = ImageType.Open,
					Command = Commands.Open,
				});
		}

		#endregion
	}
}
