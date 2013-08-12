using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GestSpace
{
	public class ZoneTransitionViewModel : NotifyPropertyChangedBase
	{
		public Action OnEnter
		{
			get;
			set;
		}
		public Action OnLeave
		{
			get;
			set;
		}
		private bool _Activated;
		public bool Activated
		{
			get
			{
				return _Activated;
			}
			set
			{
				if(value != _Activated)
				{
					_Activated = value;
					OnPropertyChanged(() => this.Activated);
				}
			}
		}
	}
	public class ZonePresenterViewModel : PresenterViewModel
	{
		public ZonePresenterViewModel()
		{
			ZoneHeight = 100.0;
		}


		public double ZoneHeight
		{
			get;
			set;
		}

		public override IDisposable Subscribe(ReactiveSpace spaceListener)
		{
			var deselectWhenUnlocked = 
				       spaceListener.IsLocked
						.ObserveOn(UI)
						.Subscribe(isLocked =>
						{
							if(!isLocked)
								if(Current != null)
								{
									Current.Activated = false;
									_Current = null;
									OnPropertyChanged(() => this.Current);
								}
						});
			var updatePosition = spaceListener
						.LockedHands
						.ObserveOn(UI)
						.Select(g => new
						{
							Group = g,
							CenterPosition = g.Key.PalmPosition
						})
						.SelectMany(g =>
							g
							.Group
							.Select(p => new
							{
								Position = p,
								GroupContext = g
							}))
						.Subscribe((hand) =>
						{
							var offset = hand.Position.PalmPosition.To2D() - hand.GroupContext.CenterPosition.To2D();
							if(-ZoneHeight/2f <  offset.y && 
								offset.y < ZoneHeight / 2f &&
							   -ZoneHeight/2f < offset.x &&
								offset.x < ZoneHeight / 2f)
								GoTo(Center);

							if(-ZoneHeight / 2f < offset.y &&
								offset.y < ZoneHeight / 2f)
							{
								if(-ZoneHeight / 2f >= offset.x)
								{
									GoTo(Left);
								}
								if(offset.x >= ZoneHeight / 2f)
								{
									GoTo(Right);
								}
							}

							if(-ZoneHeight / 2f < offset.x &&
								offset.x < ZoneHeight / 2f)
							{
								if(-ZoneHeight / 2f >= offset.y)
								{
									GoTo(Down);
								}
								if(offset.y >= ZoneHeight / 2f)
								{
									GoTo(Up);
								}
							}
						});

			CompositeDisposable subscriptions = new CompositeDisposable();
			subscriptions.Add(deselectWhenUnlocked);
			subscriptions.Add(updatePosition);
			return subscriptions;
		}

		private void GoTo(ZoneTransitionViewModel zone)
		{
			Current = zone;
		}




		private ZoneTransitionViewModel _Current;
		public ZoneTransitionViewModel Current
		{
			get
			{
				return _Current;
			}
			set
			{
				if(value != _Current)
				{
					if(_Current != null)
					{
						_Current.Activated = false;
						if(_Current.OnLeave != null)
							_Current.OnLeave();
					}
					_Current = value;
					var current = _Current;
					if(current != null)
					{
						current.Activated = true;
						if(current.OnEnter != null)
							current.OnEnter();
					}
					OnPropertyChanged(() => this.Current);
				}
			}
		}

		public ZoneTransitionViewModel Up
		{
			get;
			set;
		}
		public ZoneTransitionViewModel Down
		{
			get;
			set;
		}
		public ZoneTransitionViewModel Center
		{
			get;
			set;
		}
		public ZoneTransitionViewModel Right
		{
			get;
			set;
		}
		public ZoneTransitionViewModel Left
		{
			get;
			set;
		}

		internal override void AddEvents(List<TileEventViewModel> events)
		{
			AddEvents(Up, "Up", events);
			AddEvents(Down, "Down", events);
			AddEvents(Center, "Center", events);
			AddEvents(Right, "Right", events);
			AddEvents(Left, "Left", events);
		}

		private void AddEvents(ZoneTransitionViewModel zone, string nameSuffix, List<TileEventViewModel> events)
		{
			foreach(var evt in CreateEventsFrom(zone))
			{
				evt.Name = nameSuffix + "-" + evt.Name;
				events.Add(evt);
			}
		}
	}
}
