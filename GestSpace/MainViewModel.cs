using Leap;
using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WindowsInput;

namespace GestSpace
{
	public class DebugViewModel : NotifyPropertyChangedBase
	{
		private int _FPS;
		public int FPS
		{
			get
			{
				return _FPS;
			}
			set
			{
				if(value != _FPS)
				{
					_FPS = value;
					OnPropertyChanged(() => this.FPS);
				}
			}
		}

		private int _FingerCount;
		private MainViewModel main;

		public DebugViewModel(MainViewModel mainViewModel)
		{
			this.main = mainViewModel;
			Subscribe(main.SpaceListener);
		}

		CompositeDisposable _Subscriptions = new CompositeDisposable();
		private void Subscribe(ReactiveSpace reactiveSpace)
		{
			var fps = reactiveSpace.ReactiveListener.Frames
					.Timestamp()
					.Buffer(2)
					.ObserveOn(main.UI)
					.Subscribe(o =>
					{
						var seconds = (o[1].Timestamp - o[0].Timestamp).TotalSeconds;
						FPS = (int)(1.0 / seconds);
					});

			var fingerCount = reactiveSpace.ReactiveListener.FingersMoves
					.ObserveOn(main.UI)
					.Do(o => FingerCount++)
					.Select(f => f.ObserveOn(main.UI).Subscribe(o =>
					{

					}, () =>
					{
						FingerCount--;
					})).Subscribe();

			_Subscriptions.Add(fingerCount);
			_Subscriptions.Add(fps);
		}
		public int FingerCount
		{
			get
			{
				return _FingerCount;
			}
			set
			{
				if(value != _FingerCount)
				{
					_FingerCount = value;
					OnPropertyChanged(() => this.FingerCount);
				}
			}
		}
	}

	public enum MainViewState
	{
		Locked,
		Minimized,
		Navigating
	}
	public class MainViewModel : NotifyPropertyChangedBase
	{
		InterpreterViewModel Interpreter = new InterpreterViewModel(new Interpreter());
		ForegroundProgramListener _ProgListener;
		int _CurrentPid = Process.GetCurrentProcess().Id;
		public MainViewModel(ReactiveSpace spaceListener)
		{
			_ProgListener = new ForegroundProgramListener();

			_ProgListener.ForegroundProcess
						.ObserveOn(UI)
						.Subscribe(pid =>
						{
							if(pid != _CurrentPid)
							{
								using(var p = Process.GetProcessById(pid))
								{
									CurrentProgram = p.ProcessName;
									var tile = Tiles.FirstOrDefault(t => t.FastContext == p.ProcessName);
									if(tile != null)
										CurrentTile = tile;
								}
							}
						});

			this.State = MainViewState.Navigating;
			this._SpaceListener = spaceListener;
			this._Tiles.CollectionChanged += UpdateFreeTiles;
			Subscribe(spaceListener);

			_GestureTemplates.Add(GestureTemplateViewModel.Empty);
			_GestureTemplates.Add(new LeapGestureTemplateViewModel(Gesture.GestureType.TYPECIRCLE)
			{
				Name = "Circle"
			});
			_GestureTemplates.Add(new LeapGestureTemplateViewModel(Gesture.GestureType.TYPEKEYTAP)
			{
				Name = "KeyTap"
			});
			_GestureTemplates.Add(new LeapGestureTemplateViewModel(Gesture.GestureType.TYPESCREENTAP)
			{
				Name = "ScreenTap"
			});

			_GestureTemplates.Add(new LeapGestureTemplateViewModel(Gesture.GestureType.TYPESWIPE)
			{
				Name = "Swipe",
				MinSpeed = 1500
			});
			_PresenterTemplates.Add(new PresenterTemplateViewModel("Not used", "", () => PresenterViewModel.Unused));
			_PresenterTemplates.Add(new PresenterTemplateViewModel("Switch windows", () => new MovePresenterViewModel()
				{
					OnEnter = Interpreter.Simulate("DOWN ALT"),
					OnMoveUp = Interpreter.Simulate("PRESS TAB"),
					OnMoveDown = Interpreter.Simulate(
											"DOWN SHIFT",
											"PRESS TAB",
											"UP SHIFT"),
					OnRelease = Interpreter.Simulate("UP ALT")
				}
			));
			_PresenterTemplates.Add(new PresenterTemplateViewModel("Volume", () => VolumePresenterViewModel.Create()));
			_PresenterTemplates.Add(new PresenterTemplateViewModel("Dock window", () => new ZonePresenterViewModel()
				{
					Up = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate("PRESS WIN,UP"),
						OnLeave = Interpreter.Simulate("PRESS WIN,DOWN"),
					},
					Down = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate("PRESS WIN,DOWN"),
						OnLeave = Interpreter.Simulate("")
					},
					Right = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate("PRESS WIN,RIGHT"),
						OnLeave = Interpreter.Simulate("PRESS WIN,LEFT"),
					},
					Left = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate("PRESS WIN,LEFT"),
						OnLeave = Interpreter.Simulate("PRESS WIN,RIGHT"),
					},
					Center = new ZoneTransitionViewModel(),

				}));
			_PresenterTemplates.Add(new PresenterTemplateViewModel("Close window", () => new ClickPresenterViewModel()
			{
				OnClicked = Interpreter.Simulate("PRESS ALT,F4")
			}));

			_Debug = new DebugViewModel(this);
			Observable.Interval(TimeSpan.FromSeconds(5.0))
				.ObserveOn(UI)
				.Subscribe(t =>
				{
					new GestSpaceRepository().Save(this);
				});

			new GestSpaceRepository().Load(this);
			//CurrentTile = Tiles.First(t => !t.IsUnused);
		}



		public readonly SynchronizationContext UI = SynchronizationContext.Current;

		CompositeDisposable _Subscriptions = new CompositeDisposable();
		private void Subscribe(ReactiveSpace spaceListener)
		{
			var minimized =
				spaceListener
				.ReactiveListener
				.FingersMoves
				.SelectMany(m => m)
				.Select((m) => false)
				.OnlyTimeout(TimeSpan.FromSeconds(1))
				.Repeat()
				.ObserveOn(UI)
				.Subscribe(b =>
				{
					if(!ShowConfig)
					{
						State = MainViewState.Minimized;
					}
				});

			//var maximize =
			//				spaceListener
			//				.ReactiveListener
			//				.Gestures
			//				.Where(g => g.Key.Type == Gesture.GestureType.TYPECIRCLE)
			//				.SelectMany(g => g.ToList().Select(l => new
			//													{
			//														Key = g.Key,
			//														Values = l
			//													}))
			//				.Buffer(() => spaceListener.ReactiveListener.Gestures.SelectMany(g => g).OnlyTimeout(TimeSpan.FromMilliseconds(500)))
			//				.Where(b => b.Count > 0)
			//				.Take(1)
			//				.Repeat()
			//				.ObserveOn(UI)
			//				.Subscribe(l =>
			//				{
			//					var distinct = l.SelectMany(oo => oo.Values.SelectMany(o => o.Pointables)).Select(p => p.Id).Distinct().Count();
			//					State = MainViewState.Navigating;
			//				});

			var maximize =
						spaceListener
						.LockedHands
						.SelectMany(h => h)
						.ObserveOn(UI)
						.Subscribe(h =>
						{
							if(State == MainViewState.Minimized)
								State = MainViewState.Navigating;
						});

			var lockedSubs =
				spaceListener
				.IsLocked
				.ObserveOn(UI)
				.CombineLatest(Helper.PropertyChanged(this, () => this.State), (l, s) => new
				{
					Locked = l,
					State = s
				})
				.Where(o => o.State != MainViewState.Minimized)
				.Subscribe(o =>
				{
					State = o.Locked ? MainViewState.Locked : MainViewState.Navigating;
				});
			_Subscriptions.Add(lockedSubs);
			_Subscriptions.Add(maximize);
			_Subscriptions.Add(minimized);
		}


		void UpdateFreeTiles(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if(e.NewItems != null)
				foreach(var item in e.NewItems)
				{
					((TileViewModel)item).Main = this;
				}
			UpdateFreeTiles();
		}



		bool guard = false;
		internal void UpdateFreeTiles()
		{
			if(!guard)
			{
				guard = true;
				var neightbours = _Tiles
				.Where(t => !t.IsUnused)
				.Select(t => new
					{
						Tile = t,
						Neightbours = t.GetNeightbours()
					})
				.SelectMany(p => p.Neightbours)
				.Distinct()
				.Where(p => Tiles.All(t => t.Position != p.Position))
				.Select(p => new TileViewModel()
				{
					Position = p.Position,
					Presenter = PresenterViewModel.Unused
				}).ToList();
				foreach(var neightbour in neightbours)
				{
					_Tiles.Add(neightbour);
				}

				RemoveOccupiedUnused();

				var isolatedUnused = _Tiles
										.Where(t => t.IsUnused)
										.Select(t => new
											{
												Tile = t,
												Neightbours = NeightbourOfTile(t)
											})
										.Where(t => t.Neightbours.All(n => n == null || n.IsUnused))
										.Select(t => t.Tile);

				foreach(var tile in isolatedUnused.ToList())
				{
					if(_Tiles.Count != 1)
						_Tiles.Remove(tile);
				}

				guard = false;
			}
		}

		public IEnumerable<TileViewModel> NeightbourOfTile(TileViewModel tile)
		{
			return
				tile.GetNeightbours()
				.Select(n => FindTile(n.Position))
				.Where(t => t != null);
		}

		private TileViewModel FindTile(Point point)
		{
			return _Tiles.FirstOrDefault(t => t.Position == point);
		}

		private void RemoveOccupiedUnused()
		{
			var stacks = _Tiles
				.GroupBy(t=>t.Position);
				

			foreach(var stack in stacks)
			{
				var usedTile = stack.FirstOrDefault(t => !t.IsUnused);
				if(usedTile == null)
					usedTile = stack.LastOrDefault();
				foreach(var tile in stack.Where(t=>t != usedTile))
				{
					_Tiles.Remove(tile);
				}
			}

		}

		private GestureViewModel _LastGesture;
		public GestureViewModel LastGesture
		{
			get
			{
				return _LastGesture;
			}
			set
			{

				_LastGesture = null;
				OnPropertyChanged(() => this.LastGesture);
				_LastGesture = value;
				OnPropertyChanged(() => this.LastGesture);
			}
		}


		private readonly ObservableCollection<GestureTemplateViewModel> _GestureTemplates = new ObservableCollection<GestureTemplateViewModel>();
		public ObservableCollection<GestureTemplateViewModel> GestureTemplates
		{
			get
			{
				return _GestureTemplates;
			}
		}

		private readonly ObservableCollection<PresenterTemplateViewModel> _PresenterTemplates = new ObservableCollection<PresenterTemplateViewModel>();
		public ObservableCollection<PresenterTemplateViewModel> PresenterTemplates
		{
			get
			{
				return _PresenterTemplates;
			}
		}

		private readonly DebugViewModel _Debug;
		public DebugViewModel Debug
		{
			get
			{
				return _Debug;
			}
		}
		private readonly ObservableCollection<TileViewModel> _Tiles = new ObservableCollection<TileViewModel>();
		public ObservableCollection<TileViewModel> Tiles
		{
			get
			{
				return _Tiles;
			}
		}

		private readonly ReactiveSpace _SpaceListener;
		public ReactiveSpace SpaceListener
		{
			get
			{
				return _SpaceListener;
			}
		}


		private TileViewModel _CurrentTile;
		public TileViewModel CurrentTile
		{
			get
			{
				return _CurrentTile;
			}
			set
			{
				if(value != _CurrentTile)
				{
					_CurrentTile = value;
					ShowConfig = false;
					OnPropertyChanged(() => this.CurrentTile);
				}
			}
		}



		internal TileViewModel SelectTile(double angle)
		{
			if(CurrentTile == null)
				CurrentTile = Tiles.FirstOrDefault();
			if(CurrentTile == null)
				return null;

			int anglePart
						 = angle < -120 ? -120 :
						   angle < -60 ? -60 :
						   angle < 0 ? 0 :
						   angle < 60 ? 60 :
						   angle < 120 ? 120 : 180;
			var neightbour = CurrentTile.GetNeightbours().First(a => a.Angle == anglePart);
			return SelectTile(neightbour.Position);
		}


		public TileViewModel SelectTile(Point point)
		{
			var tile = Tiles.FirstOrDefault(t => t.Position == point);
			if(tile != null && !tile.IsUnused)
			{
				CurrentTile = tile;
				return tile;
			}
			else
				return null;
		}

		private bool _ShowConfig;
		public bool ShowConfig
		{
			get
			{
				return _ShowConfig;
			}
			set
			{
				if(value != _ShowConfig)
				{
					_ShowConfig = value;
					OnPropertyChanged(() => this.ShowConfig);
				}
			}
		}


		private MainViewState _State;
		public MainViewState State
		{
			get
			{
				return _State;
			}
			set
			{
				if(value != _State)
				{
					var old = _State;
					_State = value;

					if(old == MainViewState.Minimized)
						LastUnMinimized = DateTime.UtcNow;
					if(CurrentTile != null)
						CurrentTile.IsLockedChanged();

					if(old == MainViewState.Minimized)
						ListenerGestures(false);
					else if(value == MainViewState.Minimized)
						ListenerGestures(true);
					OnPropertyChanged(() => this.State);
				}
			}
		}

		CompositeDisposable _GesturesListeners = null;
		private void ListenerGestures(bool listen)
		{
			if(listen && _GesturesListeners == null)
			{
				_GesturesListeners = new CompositeDisposable();

				var gestures = Tiles.Where(t => !t.IsUnused && t.Gesture != null).Select(t => t.Gesture);
				foreach(var gesture in gestures)
				{
					_GesturesListeners.Add(gesture.Subscribe(SpaceListener));
				}

				_GesturesListeners.Add(SubscribeToGestures(gestures.Select(g => g.GestureMatches)));

			}
			if(!listen && _GesturesListeners != null)
			{
				_GesturesListeners.Dispose();
				_GesturesListeners = null;
			}
		}

		private IDisposable SubscribeToGestures(IEnumerable<IObservable<GestureMatch>> matches)
		{
			return matches.Merge()
				   .ObserveOn(UI)
				   .Subscribe(g =>
				   {
					   var tile = Tiles.FirstOrDefault(t => t.Gesture == g.Sender);
					   if(tile != null)
					   {
						   CurrentTile = tile;
						   State = MainViewState.Navigating;
					   }
				   });
		}

		
		private string _CurrentProgram;
		public string CurrentProgram
		{
			get
			{
				return _CurrentProgram;
			}
			set
			{
				if(value != _CurrentProgram)
				{
					_CurrentProgram = value;
					OnPropertyChanged(() => this.CurrentProgram);
				}
			}
		}

		public DateTime LastUnMinimized
		{
			get;
			private set;
		}

		private Point _SelectionPosition;
		public Point SelectionPosition
		{
			get
			{
				return _SelectionPosition;
			}
			set
			{
				if(value != _SelectionPosition)
				{
					_SelectionPosition = value;
					OnPropertyChanged(() => this.SelectionPosition);
				}
			}
		}

	}
}
