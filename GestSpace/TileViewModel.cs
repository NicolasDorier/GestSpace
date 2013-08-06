using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GestSpace
{
	public class TileNeighbour
	{
		public int Angle
		{
			get;
			set;
		}
		public Point Position
		{
			get;
			set;
		}
	}
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
					_SelectedActionTemplate = _Main.ActionTemplates.First();
					OnPropertyChanged(() => this.Main);
				}
			}
		}
		public TileViewModel()
		{
			this.PropertyChanged += TileViewModel_PropertyChanged;
			this.TakeSuggestedName = true;
		}

		void TileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "IsUnused" && Main != null)
				Main.UpdateFreeTiles();
		}

		private bool _TakeSuggestedName;
		public bool TakeSuggestedName
		{
			get
			{
				return _TakeSuggestedName;
			}
			set
			{
				if(value != _TakeSuggestedName)
				{
					_TakeSuggestedName = value;
					OnPropertyChanged(() => this.TakeSuggestedName);
				}
			}
		}

		private ActionTemplateViewModel _SelectedActionTemplate;
		public ActionTemplateViewModel SelectedActionTemplate
		{
			get
			{
				return _SelectedActionTemplate;
			}
			set
			{
				if(value != _SelectedActionTemplate)
				{
					_SelectedActionTemplate = value;
					Action = _SelectedActionTemplate.CreateAction();
					if(TakeSuggestedName || _SelectedActionTemplate.Sample is UnusedActionViewModel)
					{
						Description = _SelectedActionTemplate.SuggestedName;
						TakeSuggestedName = true;
					}
					OnPropertyChanged(() => this.SelectedActionTemplate);
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

		SerialDisposable _PresenterSubscription = new SerialDisposable();

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
					UpdateListener();
					OnPropertyChanged(() => this.IsLocked);
					OnPropertyChanged(() => this.IsSelected);
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

		public IEnumerable<TileNeighbour> GetNeightbours()
		{
			bool isPair = Position.Y % 2 == 0;
			;
			foreach(var a in _Angles)
			{
				yield return new TileNeighbour()
				{
					Position = Position + (Vector)_NeightbourTable[Tuple.Create(a, isPair)],
					Angle = a
				};
			}
		}
		MultipleAssignmentDisposable _TileListener = new MultipleAssignmentDisposable();
		SynchronizationContext UI = SynchronizationContext.Current;

		internal void UpdateListener()
		{
			if(!_IsSelected || Main.State == MainViewState.Minimized)
			{
				_TileListener.Disposable = null;
				_PresenterSubscription.Disposable = null;
			}
			else
			{
				_TileListener.Disposable = Subscribe(Main.SpaceListener);
				_PresenterSubscription.Disposable = Action.Presenter.Subscribe(Main.SpaceListener);
			}
		}

		private IDisposable Subscribe(ReactiveSpace reactiveSpace)
		{
			return null;
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
					UpdateListener();
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
					TakeSuggestedName = false;
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

		public bool IsLocked
		{
			get
			{
				return Main.State == MainViewState.Locked && _IsSelected;
			}
		}

		internal void IsLockedChanged()
		{
			OnPropertyChanged(() => IsLocked);
		}
	}
}
