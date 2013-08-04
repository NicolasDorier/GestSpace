using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GestSpace
{
	public class TileViewModel : NotifyPropertyChangedBase
	{
		private MainViewModel _Main;
		public MainViewModel Main
		{
			get
			{
				return _Main;
			}
			set
			{
				if(value != _Main)
				{
					_Main = value;
					OnPropertyChanged(() => this.Main);
				}
			}
		}
		private Point _Position;
		public Point Position
		{
			get
			{
				return _Position;
			}
			set
			{
				if(value != _Position)
				{
					_Position = value;
					OnPropertyChanged(() => this.Position);
				}
			}
		}

		private bool _IsSelected;

		public bool IsSelected
		{
			get
			{
				return _IsSelected;
			}
			set
			{
				if(value != _IsSelected)
				{
					_IsSelected = value;
					OnPropertyChanged(() => this.IsSelected);
				}
			}
		}

		private ActionViewModel _Action = new ActionViewModel();
		public ActionViewModel Action
		{
			get
			{
				return _Action;
			}
			set
			{
				if(value != _Action)
				{
					_Action = value;
					OnPropertyChanged(() => this.Action);
					OnPropertyChanged(() => this.IsUnused);
				}
			}
		}


		private string _Description;
		public string Description
		{
			get
			{
				return _Description;
			}
			set
			{
				if(value != _Description)
				{
					_Description = value;
					OnPropertyChanged(() => this.Description);
				}
			}
		}

		public bool IsUnused
		{
			get
			{
				return Action is UnusedActionViewModel;
			}
		}

		private bool _IsLocked;
		public bool IsLocked
		{
			get
			{
				return _IsLocked;
			}
			set
			{
				if(value != _IsLocked)
				{
					_IsLocked = value;
					OnPropertyChanged(() => this.IsLocked);
				}
			}
		}
	}
}
