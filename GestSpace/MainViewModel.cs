using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GestSpace
{
	public class MainViewModel : NotifyPropertyChangedBase
	{
		public MainViewModel()
		{
			for(int x = 0 ; x < 4 ; x++)
			{
				for(int y = 0 ; y < 5 ; y++)
				{
					_Tiles.Add(new TileViewModel()
					{
						Position = new Point(x,y)
					});
				}
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
					OnPropertyChanged(() => this.CurrentTile);
				}
			}
		}
	}
}
