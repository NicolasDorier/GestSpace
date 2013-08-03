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
			for(int x = 0 ; x < 1 ; x++)
			{
				for(int y = 0 ; y < 2 ; y++)
				{
					_Tiles.Add(new TileViewModel()
					{
						Position = new Point(x, y),
						Action = y % 2 == 0 ? (ActionViewModel)new UnusedActionViewModel() : KeyboardActionViewModel.CreateSwitchWindow(),
						Description = y % 2 == 0 ? "" : "Volume",
					});
				}
			}

			_Tiles.Add(new TileViewModel()
			{
				Position = new Point(0,1),
				Action = KeyboardActionViewModel.CreateSwitchWindow(),
				Description = "Switch windows"
			});
			_Tiles.Add(new TileViewModel()
			{
				Position = new Point(0, 0),
				Action = new UnusedActionViewModel(),
				Description = "New"
			});
			_Tiles.Add(new TileViewModel()
			{
				Position = new Point(1, 0),
				Action = new VolumeActionViewModel(),
				Description = "Volume"
			});
			SelectTile(new Point(0, 1));
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
					if(_CurrentTile != null)
						_CurrentTile.IsSelected = false;
					_CurrentTile = value;
					if(_CurrentTile != null)
						_CurrentTile.IsSelected = true;
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

		internal void SelectTile(double angle)
		{
			Console.WriteLine("Angle : " + (int)angle);
			if(CurrentTile == null)
				CurrentTile = Tiles.FirstOrDefault();
			if(CurrentTile == null)
				return;

			bool isPair = CurrentTile.Position.Y % 2 == 0;
			int anglePart
						 = angle < -120 ? -120 :
						   angle < -60 ? -60 :
						   angle < 0 ? 0 :
						   angle < 60 ? 60 :
						   angle < 120 ? 120 : 180;

			var point = _NeightbourTable[Tuple.Create(anglePart, isPair)];
			SelectTile(new Point(CurrentTile.Position.X + point.X, CurrentTile.Position.Y + point.Y));
		}

		private void SelectTile(Point point)
		{
			var tile = Tiles.FirstOrDefault(t => t.Position == point);
			if(tile != null && !tile.IsUnused)
				CurrentTile = tile;
		}
	}
}
