using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
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

			_ActionTemplates.Add(new ActionTemplateViewModel("Not used", "", () => new UnusedActionViewModel()));
			_ActionTemplates.Add(new ActionTemplateViewModel("Switch windows", () => KeyboardActionViewModel.CreateSwitchWindow()));
			_ActionTemplates.Add(new ActionTemplateViewModel("Volume", () => new VolumeActionViewModel()));
			_ActionTemplates.Add(new ActionTemplateViewModel("Dock window", () => new ActionViewModel()
			{
				Presenter = new ClickPresenterViewModel(
				onUp: () =>
				{
					InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
					InputSimulator.SimulateKeyPress(VirtualKeyCode.UP);
					InputSimulator.SimulateKeyUp(VirtualKeyCode.LWIN);
				},
				onDown: () =>
				{
					InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
					InputSimulator.SimulateKeyPress(VirtualKeyCode.DOWN);
					InputSimulator.SimulateKeyUp(VirtualKeyCode.LWIN);
				},
				onLeft:() =>
				{
					InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
					InputSimulator.SimulateKeyPress(VirtualKeyCode.LEFT);
					InputSimulator.SimulateKeyUp(VirtualKeyCode.LWIN);
				},
				onRight: () =>
				{
					InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
					InputSimulator.SimulateKeyPress(VirtualKeyCode.RIGHT);
					InputSimulator.SimulateKeyUp(VirtualKeyCode.LWIN);
				})
			}));

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
			Console.WriteLine("Angle : " + (int)angle);
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
	}
}
