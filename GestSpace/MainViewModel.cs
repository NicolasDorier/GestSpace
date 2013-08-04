using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
	public class MainViewModel : NotifyPropertyChangedBase
	{
		public MainViewModel(ReactiveSpace spaceListener)
		{
			this._SpaceListener = spaceListener;
			this._Tiles.CollectionChanged += UpdateFreeTiles;

			_Tiles.Add(new TileViewModel()
			{
				Position = new Point(0, 1),
				Action = KeyboardActionViewModel.CreateSwitchWindow(),
				Description = "Switch windows"
			});
			_Tiles.Add(new TileViewModel()
			{
				Position = new Point(1, 0),
				Action = new VolumeActionViewModel(),
				Description = "Volume"
			});
			SelectTile(new Point(0, 1));
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
		private void UpdateFreeTiles()
		{
			if(!guard)
			{
				guard = true;
				var neightbours = _Tiles
				.Where(t => !t.IsUnused)
				.Select(t => new
					{
						Tile = t,
						Neightbours = _Angles
										.Select(a => t.Position + (Vector)_NeightbourTable[Tuple.Create(a, IsPair(t))])
					})
				.SelectMany(p => p.Neightbours)
				.Distinct()
				.Where(p => Tiles.All(t => t.Position != p))
				.Select(p => new TileViewModel()
				{
					Position = p,
					Action = new UnusedActionViewModel()
				}).ToList();
				foreach(var neightbour in neightbours)
				{
					_Tiles.Add(neightbour);
				}
				RemoveOccupiedUnused();
				guard = false;
			}
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

		private readonly DebugViewModel _Debug = new DebugViewModel();
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

		SerialDisposable _PresenterSubscription = new SerialDisposable();

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
					if(_CurrentTile != null)
					{
						_PresenterSubscription.Disposable = _CurrentTile.Action.Presenter.Subscribe(SpaceListener);
					}
					else
					{
						_PresenterSubscription.Disposable = null;
					}
					OnPropertyChanged(() => this.CurrentTile);
				}
			}
		}

		int[] _Angles = new[] { 120, -60, -120, 180, 0, 60 };
		Dictionary<Tuple<int, bool>, Point> _NeightbourTable =
			new object[][]
			{
				new object[]{ 120, true, 0, -2 },
				new object[]{ 120, false, 0, -2 },
				new object[]{ -60, true, 0, 2 },
				new object[]{ -60, false, 0, 2 },

				new object[]{ 60, true, 0, -1 },
				new object[]{ 60, false, 1, -1 },
				new object[]{ -120, true, -1, 1 },
				new object[]{ -120, false, 0, 1 },

				new object[]{ 180, true, -1, -1 },
				new object[]{ 180, false, 0, -1 },
				new object[]{ 0, true, 0, 1 },
				new object[]{ 0, false, 1, 1 },

			}.ToDictionary(row => Tuple.Create((int)row[0], (bool)row[1]), row => new Point((int)row[2], (int)row[3]));

		internal TileViewModel SelectTile(double angle)
		{
			Console.WriteLine("Angle : " + (int)angle);
			if(CurrentTile == null)
				CurrentTile = Tiles.FirstOrDefault();
			if(CurrentTile == null)
				return null;

			bool isPair = IsPair(CurrentTile);
			int anglePart
						 = angle < -120 ? -120 :
						   angle < -60 ? -60 :
						   angle < 0 ? 0 :
						   angle < 60 ? 60 :
						   angle < 120 ? 120 : 180;
			var point = _NeightbourTable[Tuple.Create(anglePart, isPair)];
			return SelectTile(CurrentTile.Position + (Vector)point);
		}

		private bool IsPair(TileViewModel tile)
		{
			return tile.Position.Y % 2 == 0;
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
	}
}
