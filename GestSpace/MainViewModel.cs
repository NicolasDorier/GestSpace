using Leap;
using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		public MainViewModel(ReactiveSpace spaceListener)
		{

			this.State = MainViewState.Navigating;
			this._SpaceListener = spaceListener;
			this._Tiles.CollectionChanged += UpdateFreeTiles;
			Subscribe(spaceListener);


			_ActionTemplates.Add(new ActionTemplateViewModel("Not used", "", () => new UnusedActionViewModel()));
			_ActionTemplates.Add(new ActionTemplateViewModel("Switch windows", () => new ActionViewModel()
			{
				Presenter = new MovePresenterViewModel
				(
				onEnter: Interpreter.Simulate("DOWN LWIN"),
				onMoveUp: Interpreter.Simulate("PRESS TAB"),
				onMoveDown: Interpreter.Simulate(
										"DOWN SHIFT",
										"PRESS TAB",
										"UP SHIFT"),
				onRelease: Interpreter.Simulate("UP LWIN")
				)
			}));
			_ActionTemplates.Add(new ActionTemplateViewModel("Volume", () => new VolumeActionViewModel()));
			_ActionTemplates.Add(new ActionTemplateViewModel("Dock window", () => new ActionViewModel()
			{
				Presenter = new ZonePresenterViewModel()
				{
					Up = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS UP",
									"UP LWIN"),
						OnLeave = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS DOWN",
									"UP LWIN"),
					},
					Down = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS DOWN",
									"UP LWIN"),
					},
					Right = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS RIGHT",
									"UP LWIN"),
						OnLeave = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS LEFT",
									"UP LWIN"),
					},
					Left = new ZoneTransitionViewModel()
					{
						OnEnter = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS LEFT",
									"UP LWIN"),
						OnLeave = Interpreter.Simulate(
									"DOWN LWIN",
									"PRESS RIGHT",
									"UP LWIN"),
					},
					Center = new ZoneTransitionViewModel()
				}
			}));

			//_Tiles.Add(new TileViewModel()
			//{
			//	Position = new Point(0, 1),
			//	Action = KeyboardActionViewModel.CreateSwitchWindow(),
			//	Description = "Switch windows"
			//});
			//_Tiles.Add(new TileViewModel()
			//{
			//	Position = new Point(1, 0),
			//	Action = new VolumeActionViewModel(),
			//	Description = "Volume"
			//});


			//SelectTile(new Point(0, 1));
			this._Debug = new DebugViewModel(this);

			this.Tiles.Add(new TileViewModel()
			{
				Action = new UnusedActionViewModel()
			});

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
					Action = new UnusedActionViewModel()
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
			var unuseds = _Tiles
				.Select(t => new
				{
					Original = t,
					Duplicates = _Tiles
						.Where(t2 => t2.IsUnused)
						.Where(t2 => t != t2 && t.Position == t2.Position)
				})
				.SelectMany(d => d.Duplicates).ToList();

			foreach(var unused in unuseds)
			{
				_Tiles.Remove(unused);
			}

		}

		private readonly ObservableCollection<ActionTemplateViewModel> _ActionTemplates = new ObservableCollection<ActionTemplateViewModel>();
		public ObservableCollection<ActionTemplateViewModel> ActionTemplates
		{
			get
			{
				return _ActionTemplates;
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


		private TileViewModel SelectTile(Point point)
		{
			var tile = Tiles.FirstOrDefault(t => t.Position == point);
			if(tile != null && !tile.IsUnused)
				CurrentTile = tile;
			return tile;
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
					_State = value;
					if(CurrentTile != null)
						CurrentTile.IsLockedChanged();
					OnPropertyChanged(() => this.State);
				}
			}
		}
	}
}
