using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GestSpace
{
	public class PresenterViewModel : NotifyPropertyChangedBase, IDisposable
	{
		public readonly static PresenterViewModel Unused = new PresenterViewModel();

		public static SynchronizationContext UI = SynchronizationContext.Current;
		protected virtual IDisposable SubscribeCore(ReactiveSpace spaceListener)
		{
			return Disposable.Empty;
		}

		#region IDisposable Members

		public virtual void Dispose()
		{

		}


		public Action OnEnter
		{
			get;
			set;
		}
		public Action OnRelease
		{
			get;
			set;
		}

		public IDisposable Subscribe(ReactiveSpace spaceListener)
		{
			CompositeDisposable subscriptions = new CompositeDisposable();
			subscriptions.Add(spaceListener
								.LockedHands()
								.ObserveOn(UI)
								.Subscribe(o =>
								{
									HandsCount++;
								}));

			subscriptions.Add(spaceListener
								.LockedHands()
								.Select(o =>
										o
										.ObserveOn(UI)
										.Subscribe(oo =>
										{
										}, () =>
										{
											HandsCount--;
										}))
								.Subscribe());
			subscriptions.Add(SubscribeCore(spaceListener));
			subscriptions.Add(Disposable.Create(()=>HandsCount = 0));
			return subscriptions;
		}

		int _HandsCount;
		int HandsCount
		{
			get
			{
				return _HandsCount;
			}
			set
			{
				if(_HandsCount != value && value >= 0)
				{
					_HandsCount = value;
					if(_HandsCount < 0)
						_HandsCount = 0;
					if(_HandsCount == 0)
						if(OnRelease != null)
							OnRelease();
					if(_HandsCount == 1)
						if(OnEnter != null)
							OnEnter();
				}
			}
		}

		#endregion

		protected List<TileEventViewModel> CreateEventsFrom(object obj)
		{
			var events = new List<TileEventViewModel>();
			if(obj == null)
				return events;
			foreach(var prop in obj.GetType()
								.GetProperties(BindingFlags.Instance | BindingFlags.Public)
								.Where(p => p.PropertyType == typeof(Action)))
			{
				var action = prop.GetValue(obj) as Action;
				if(action != null)
				{
					var command = action.Target as InterpreterCommandViewModel;
					if(command != null)
					{
						TileEventViewModel evt = new TileEventViewModel();
						evt.Name = prop.Name;
						evt.Command = command;
						events.Add(evt);
					}
				}
			}
			return events;
		}

		internal virtual void AddEvents(List<TileEventViewModel> events)
		{
			events.AddRange(CreateEventsFrom(this));
		}
	}
}
