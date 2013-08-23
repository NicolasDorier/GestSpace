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
using System.Windows.Input;

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
					if(_SelectedPresenterTemplate == null)
						SelectedPresenterTemplate = _Main.PresenterTemplates.First();
					if(_SelectedGestureTemplate == null)
						SelectedGestureTemplate = _Main.GestureTemplates.First();
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
			if(e.PropertyName == "IsLocked")
				UpdateListener();
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

		private ICommand _SetToCurrentProgram;
		public ICommand SetToCurrentProgram
		{
			get
			{
				if(_SetToCurrentProgram == null)
				{
					_SetToCurrentProgram = new DelegateCommand(o =>
					{
						FastContext = Main.CurrentProgram;
					}, o => true);
				}
				return _SetToCurrentProgram;
			}
		}
		private PresenterTemplateViewModel _SelectedPresenterTemplate;
		public PresenterTemplateViewModel SelectedPresenterTemplate
		{
			get
			{
				return _SelectedPresenterTemplate;
			}
			set
			{
				if(value != _SelectedPresenterTemplate)
				{
					_SelectedPresenterTemplate = value;
					if(_SelectedPresenterTemplate != null)
					{
						Presenter = _SelectedPresenterTemplate.CreatePresenter();
						if(TakeSuggestedName || _SelectedPresenterTemplate.Sample == PresenterViewModel.Unused)
						{
							Description = _SelectedPresenterTemplate.SuggestedName;
							TakeSuggestedName = true;
						}
					}
					else
					{
						Presenter = null;
					}
					OnPropertyChanged(() => this.SelectedPresenterTemplate);
				}
			}
		}

		private GestureTemplateViewModel _SelectedGestureTemplate;
		public GestureTemplateViewModel SelectedGestureTemplate
		{
			get
			{
				return _SelectedGestureTemplate;
			}
			set
			{
				if(value != _SelectedGestureTemplate)
				{
					_SelectedGestureTemplate = value;
					if(_SelectedGestureTemplate != null)
						Gesture = _SelectedGestureTemplate.CreateGesture();
					else
						Gesture = null;
					OnPropertyChanged(() => this.SelectedGestureTemplate);
				}
			}
		}


		ICommand _ClearAllEvents;
		public ICommand ClearAllEvents
		{
			get
			{
				if(_ClearAllEvents == null)
				{
					_ClearAllEvents = new DelegateCommand(o =>
					{
						foreach(var evt in Events)
						{
							if(evt.CanModifyScript)
								evt.Command.Script = "";
						}
					}, o => true);
				}
				return _ClearAllEvents;
			}
		}

		private GestureViewModel _Gesture;
		public GestureViewModel Gesture
		{
			get
			{
				return _Gesture;
			}
			set
			{
				if(value != _Gesture)
				{
					_Gesture = value;
					OnPropertyChanged(() => this.Gesture);
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
					Main.UI.Post(new SendOrPostCallback(o =>
					{
						OnPropertyChanged(() => this.IsUnused);
					}), null);
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
			if(!IsLocked)
			{
				_TileListener.Disposable = null;
				_PresenterSubscription.Disposable = null;
			}
			else
			{
				_TileListener.Disposable = Subscribe(Main.SpaceListener);
				_PresenterSubscription.Disposable = Presenter.Subscribe(Main.SpaceListener);
			}
		}

		private IDisposable Subscribe(ReactiveSpace reactiveSpace)
		{
			return null;
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
				return Presenter == PresenterViewModel.Unused && !IsSelected;
			}
		}

		public bool IsLocked
		{
			get
			{
				if(Main == null)
					return false;
				return Main.State == MainViewState.Locked && _IsSelected;// && DateTime.UtcNow - Main.LastUnMinimized > TimeSpan.FromSeconds(0.5);
			}
		}


		private List<TileEventViewModel> _Events;
		public List<TileEventViewModel> Events
		{
			get
			{
				if(_Events == null)
					return new List<TileEventViewModel>();
				return _Events;
			}
			set
			{
				if(value != _Events)
				{
					_Events = value;
					OnPropertyChanged(() => this.Events);
				}
			}
		}

		private PresenterViewModel _Presenter;
		public PresenterViewModel Presenter
		{
			get
			{
				return _Presenter;
			}
			set
			{
				if(value != _Presenter)
				{
					_Presenter = value;
					UpdateEvents();
					UpdateListener();
					OnPropertyChanged(() => this.IsUnused);
					OnPropertyChanged(() => this.Presenter);
				}
			}
		}

		private TileEventViewModel _SelectedEvent;
		public TileEventViewModel SelectedEvent
		{
			get
			{
				return _SelectedEvent;
			}
			set
			{
				if(value != _SelectedEvent)
				{
					_SelectedEvent = value;
					OnPropertyChanged(() => this.SelectedEvent);
				}
			}
		}

		private bool _HasEvents;
		public bool HasEvents
		{
			get
			{
				return _HasEvents;
			}
			set
			{
				if(value != _HasEvents)
				{
					_HasEvents = value;
					OnPropertyChanged(() => this.HasEvents);
				}
			}
		}

		private void UpdateEvents()
		{
			var events = new List<TileEventViewModel>();
			if(Presenter != null)
				Presenter.AddEvents(events);
			Events = events;
			SelectedEvent = Events.FirstOrDefault();
			HasEvents = Events.Count != 0;
		}

		internal void IsLockedChanged()
		{
			OnPropertyChanged(() => IsLocked);
		}

		private string _FastContext;
		public string FastContext
		{
			get
			{
				return _FastContext;
			}
			set
			{
				if(value != _FastContext)
				{
					_FastContext = value;
					OnPropertyChanged(() => this.FastContext);
				}
			}
		}
	}
}
