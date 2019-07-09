using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace AssemblyDefender.UI
{
	/// <summary>
	/// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
	/// </summary>
	public class DelegateCommand : ICommand
	{
		#region Fields

		public event EventHandler CanExecuteChanged;
		private List<Subscription> _subscriptions = new List<Subscription>(2);

		#endregion

		#region Ctors

		public DelegateCommand()
		{
		}

		public DelegateCommand(Action action)
			: this(action, null)
		{
		}

		public DelegateCommand(Action action, Func<bool> filter)
		{
			Subscribe(action, filter);
		}

		#endregion

		#region Methods

		public bool CanExecute()
		{
			if (_subscriptions.Count == 0)
				return false;

			foreach (var subscription in _subscriptions)
			{
				if (subscription.Filter != null && !subscription.Filter())
					return false;
			}

			return true;
		}

		public void Execute(bool runFilter = false)
		{
			foreach (var subscription in _subscriptions)
			{
				if (runFilter)
				{
					if (subscription.Filter != null && !subscription.Filter())
						continue;
				}

				subscription.Action();
			}
		}

		public void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
			{
				CanExecuteChanged(this, EventArgs.Empty);
			}
		}

		public bool IsSubscribed(Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			foreach (var subscription in _subscriptions)
			{
				if (subscription.Action == action)
					return true;
			}

			return false;
		}

		public void Subscribe(Action action, Func<bool> filter = null)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			var subscription =
				new Subscription()
				{
					Action = action,
					Filter = filter,
				};

			foreach (var existing in _subscriptions)
			{
				if (existing.Action == action)
				{
					throw new InvalidOperationException("Already subscribed.");
				}
			}

			_subscriptions.Add(subscription);

			RaiseCanExecuteChanged();
		}

		public void Unsubscribe(Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			for (int i = _subscriptions.Count - 1; i >= 0; i--)
			{
				var subscription = _subscriptions[i];
				if (subscription.Action == action)
				{
					_subscriptions.RemoveAt(i);
					break;
				}
			}

			if (_subscriptions.Count == 0)
			{
				RaiseCanExecuteChanged();
			}
		}

		bool ICommand.CanExecute(object parameter)
		{
			return CanExecute();
		}

		void ICommand.Execute(object parameter)
		{
			Execute();
		}

		#endregion

		#region Nested types

		private struct Subscription
		{
			internal Action Action;
			internal Func<bool> Filter;
		}

		#endregion
	}

	/// <summary>
	/// An <see cref="ICommand"/> whose delegates can be attached for <see cref="Execute"/> and <see cref="CanExecute"/>.
	/// </summary>
	public class DelegateCommand<T> : ICommand
	{
		#region Fields

		public event EventHandler CanExecuteChanged;
		private List<Subscription> _subscriptions = new List<Subscription>(2);

		#endregion

		#region Ctors

		public DelegateCommand()
		{
		}

		public DelegateCommand(Action<T> action)
			: this(action, null)
		{
		}

		public DelegateCommand(Action<T> action, Predicate<T> filter)
		{
			Subscribe(action, filter);
		}

		#endregion

		#region Methods

		public bool CanExecute(T parameter)
		{
			if (_subscriptions.Count == 0)
				return false;

			foreach (var subscription in _subscriptions)
			{
				if (subscription.Filter != null && !subscription.Filter(parameter))
					return false;
			}

			return true;
		}

		public void Execute(T parameter, bool runFilter = false)
		{
			foreach (var subscription in _subscriptions)
			{
				if (runFilter)
				{
					if (subscription.Filter != null && !subscription.Filter(parameter))
						continue;
				}

				subscription.Action(parameter);
			}
		}

		public void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
			{
				CanExecuteChanged(this, EventArgs.Empty);
			}
		}

		public bool IsSubscribed(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			foreach (var subscription in _subscriptions)
			{
				if (subscription.Action == action)
					return true;
			}

			return false;
		}

		public void Subscribe(Action<T> action, Predicate<T> filter = null)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			var subscription =
				new Subscription()
				{
					Action = action,
					Filter = filter,
				};

			foreach (var existing in _subscriptions)
			{
				if (existing.Action == action)
				{
					throw new InvalidOperationException("Already subscribed.");
				}
			}

			_subscriptions.Add(subscription);

			RaiseCanExecuteChanged();
		}

		public void Unsubscribe(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			for (int i = _subscriptions.Count - 1; i >= 0; i--)
			{
				var subscription = _subscriptions[i];
				if (subscription.Action == action)
				{
					_subscriptions.RemoveAt(i);
					break;
				}
			}

			if (_subscriptions.Count == 0)
			{
				RaiseCanExecuteChanged();
			}
		}

		bool ICommand.CanExecute(object parameter)
		{
			return CanExecute(parameter != null ? (T)parameter : default(T));
		}

		void ICommand.Execute(object parameter)
		{
			Execute(parameter != null ? (T)parameter : default(T));
		}

		#endregion

		#region Nested types

		private struct Subscription
		{
			internal Action<T> Action;
			internal Predicate<T> Filter;
		}

		#endregion
	}
}
