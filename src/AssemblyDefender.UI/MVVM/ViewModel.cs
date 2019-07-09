using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace AssemblyDefender.UI
{
	/// <summary>
	/// Base class for all ViewModel classes in the application.
	/// It provides support for property change notifications.
	/// This class is abstract.
	/// </summary>
	public abstract class ViewModel : PropertyAwareObject, IDisposable
	{
		#region Fields

		private bool _isActive;
		private ViewModel _parent;

		#endregion

		#region Ctors

		public ViewModel()
		{
		}

		public ViewModel(ViewModel parent)
		{
			_parent = parent;
		}

		#endregion

		#region Properties

		public bool IsActive
		{
			get { return _isActive; }
		}

		public ViewModel Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public T FindParent<T>(bool throwIfMissing = false)
			where T : ViewModel
		{
			return (T)FindParent(n => n is T, throwIfMissing);
		}

		public ViewModel FindParent(Predicate<ViewModel> filter, bool throwIfMissing = false)
		{
			var node = _parent;
			while (node != null)
			{
				if (filter(node))
				{
					return node;
				}

				node = node._parent;
			}

			if (throwIfMissing)
			{
				throw new InvalidOperationException();
			}

			return null;
		}

		/// <summary>
		/// Invoked when this object is being removed from the application
		/// and will be subject to garbage collection.
		/// </summary>
		public void Dispose()
		{
			OnDispose();
		}

		/// <summary>
		/// Child classes can override this method to perform
		/// clean-up logic, such as removing event handlers.
		/// </summary>
		protected virtual void OnDispose()
		{
			Deactivate();
			_parent = null;
		}

		public void Activate()
		{
			if (_isActive)
				return;

			_isActive = true;
			OnPropertyChanged("IsActive");
			OnActivate();
		}

		public void Deactivate()
		{
			if (!_isActive)
				return;

			_isActive = false;
			OnPropertyChanged("IsActive");
			OnDeactivate();
		}

		public virtual bool CanDeactivate()
		{
			return true;
		}

		protected virtual void OnActivate()
		{
		}

		protected virtual void OnDeactivate()
		{
		}

		protected bool Show(ref ViewModel workspace, ViewModel viewModel, string propertyName)
		{
			return Show<ViewModel>(ref workspace, viewModel, propertyName);
		}

		protected bool Show<T>(ref T workspace, T viewModel, string propertyName)
			where T : ViewModel
		{
			if (workspace != null)
			{
				if (object.ReferenceEquals(workspace, viewModel))
					return true;

				if (!workspace.CanDeactivate())
					return false;

				workspace.Deactivate();
			}

			workspace = viewModel;

			if (workspace != null)
			{
				workspace.Activate();
			}

			OnPropertyChanged(propertyName);

			return true;
		}

		#endregion
	}

	public abstract class ViewModel<T> : ViewModel
		where T : ViewModel
	{
		private T _parent;

		public ViewModel()
		{
		}

		public ViewModel(T parent)
			: base(parent)
		{
			_parent = parent;
		}

		public new T Parent
		{
			get { return _parent; }
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			_parent = null;
		}
	}
}
