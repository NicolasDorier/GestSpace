using CoreAudioApi;
using GestSpace.Controls;
using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;

namespace GestSpace
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		Controller controller;
		public MainWindow()
		{
			InitializeComponent();

			listener = new ReactiveListener();
			var spaceListener = new ReactiveSpace(listener);

			controller = new Controller(listener);

			ViewModel = new MainViewModel(spaceListener);
			root.SetBinding(Grid.DataContextProperty, new Binding()
			{
				Source = this,
				Path = new PropertyPath("ViewModel")
			});
			SetBinding(CurrentTileProperty, new Binding()
			{
				Source = this,
				Path = new PropertyPath("ViewModel.CurrentTile")
			});
			this.Loaded += MainWindow_Loaded;
			this.Closed += MainWindow_Closed;
		}




		TileViewModel CurrentTile
		{
			get
			{
				return (TileViewModel)GetValue(CurrentTileProperty);
			}
			set
			{
				SetValue(CurrentTileProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for CurrentTile.  This enables animation, styling, binding, etc...
		static readonly DependencyProperty CurrentTileProperty =
			DependencyProperty.Register("CurrentTile", typeof(TileViewModel), typeof(MainWindow), new PropertyMetadata(null, OnCurrentTileChanged));

		static void OnCurrentTileChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			MainWindow sender = (MainWindow)source;
			sender.Center();
		}

		private void Center()
		{
			Center(CurrentTile);
		}

		private void Center(TileViewModel tile)
		{
			if(tile == null && ViewModel != null)
				tile = ViewModel.Tiles.FirstOrDefault();

			if(tile == null)
				return;

			var container = list.ItemContainerGenerator.ContainerFromItem(tile);
			Center((FrameworkElement)container);
		}

		public MainViewModel ViewModel
		{
			get
			{
				return (MainViewModel)GetValue(ViewModelProperty);
			}
			set
			{
				SetValue(ViewModelProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register("ViewModel", typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));


		volatile bool _Maximized;
		private void Minimize()
		{
			if(!_Maximized)
				return;
			_Maximized = false;
			var animation = CreateDoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(0.5)));
			this.BeginAnimation(OpacityProperty, animation);
		}
		private void Maximize()
		{
			if(_Maximized)
				return;
			_Maximized = true;
			WindowState = System.Windows.WindowState.Maximized;
			var animation = CreateDoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(0.5)));
			this.BeginAnimation(OpacityProperty, animation);
		}
	
		ReactiveListener listener;
		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			//var a = GestSpace.Interop.user32.CountVisibleWindows();
			//InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
			//InputSimulator.SimulateKeyDown(VirtualKeyCode.TAB);

			var ui = SynchronizationContext.Current;

			listener.Frames
					.Timestamp()
					.Buffer(2)
					.ObserveOn(ui)
					.Subscribe(o =>
					{
						var seconds = (o[1].Timestamp - o[0].Timestamp).TotalSeconds;
						ViewModel.Debug.FPS = (int)(1.0 / seconds);
					});

			listener.FingersMoves
					.ObserveOn(ui)
					.Do(o => ViewModel.Debug.FingerCount++)
					.Select(f => f.ObserveOn(ui).Subscribe(o =>
					{

					}, () =>
					{
						ViewModel.Debug.FingerCount--;
					})).Subscribe();

			listener
				.FingersMoves
				.SelectMany(m => m)
				.Select((m) => false)
				.OnlyTimeout(TimeSpan.FromSeconds(3))
				.Repeat()
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(b =>
				{
					//Minimize();
				});



			ViewModel.SpaceListener
				.IsLocked
				.ObserveOn(ui)
				.Subscribe(locked =>
				{
					ViewModel.CurrentTile.IsLocked = locked;
				});

			

			var centers =
				listener
				.FingersMoves
				.SelectMany(f => f)
				.Buffer(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(100))
				.Where(b => b.Count > 0)
				.Select(v => new Leap.Vector(v.Average(m => m.TipPosition.x), v.Average(m => m.TipPosition.y), 0.0f));


			//new System.Windows.Vector(v.Average(m => m.Direction.x), v.Average(m => m.Direction.y))
			listener
				.FingersMoves
				.SelectMany(f => f)
				.Select(v => v.TipPosition)
				.CombineLatest(centers, ViewModel.SpaceListener.IsLocked, (p, center, locked) => new
				{
					Center = center,
					Position = To2d(p),
					Move = To2d(p) - center,
					Locked = locked
				})
				.Where(o => o.Move.Magnitude >= 50.0)
				.Sample(TimeSpan.FromMilliseconds(500))
				.Where(p=>!p.Locked)
				.ObserveOn(ui)
				.Subscribe(o =>
				{
					if(ViewModel.Debug.FingerCount <= 2)
					{
						var angle = Helper.RadianToDegree(Math.Atan2(o.Move.y, o.Move.x));
						ViewModel.SelectTile(angle);
					}
				});

			//.Where((i) => _Maximized)
			//	.SelectMany(f => f)
			//	.SkipUntil(Observable.Interval(TimeSpan.FromMilliseconds(600)))
			//		.Buffer(20)
			//		.Select(v => new System.Windows.Vector(v.Average(m => m.Direction.x), v.Average(m => m.Direction.y)))
			//		.Where(v => v.Length > 0.4)
			//		.Take(1)
			//.Repeat()
			//.ObserveOn(SynchronizationContext.Current)
			//.Subscribe(vector =>
			//{
			//	var angle = RadianToDegree(Math.Atan2(vector.Y, vector.X));
			//	ViewModel.SelectTile(angle);

			//});

			var gesturesById = listener
			.Gestures
			.Where((o) => !false)
				.Where(g => g.Key.Type == Gesture.GestureType.TYPECIRCLE)
				.SelectMany(g => g.ToList().Select(l => new
													{
														Key = g.Key,
														Values = l
													}))
				//.SkipUntil(Observable.Interval(TimeSpan.FromMilliseconds(600)))
				.Buffer(() => listener.Gestures.OnlyTimeout(TimeSpan.FromMilliseconds(700)))
				.Where(b => b.Count > 0)
				.Take(1)
			.Repeat()
			.ObserveOn(SynchronizationContext.Current)
			.Subscribe(l =>
			{

				var distinct = l.SelectMany(oo => oo.Values.SelectMany(o => o.Pointables)).Select(p => p.Id).Distinct().Count();
				Console.WriteLine("Gesture " + distinct);
				Maximize();
			});


			Maximize();
			Center();
		}

		Leap.Vector To2d(Leap.Vector vector)
		{
			Leap.Vector v = new Leap.Vector(vector.x, vector.y, 0);
			return v;
		}


		void MainWindow_Closed(object sender, EventArgs e)
		{
			controller.Dispose();
		}



		private void Center(FrameworkElement hex)
		{
			if(hex == null)
				return;
			var centerPoint = center.TranslatePoint(new Point(0, 0), root);
			var hexPoint = hex.TranslatePoint(new Point(hex.ActualWidth / 2.0, hex.ActualHeight / 2.0), root);
			var transform = list.RenderTransform as TranslateTransform; // = new TranslateTransform(centerPoint.X - hexPoint.X, centerPoint.Y - hexPoint.Y);
			if(transform == null)
			{
				transform = new TranslateTransform();
				list.RenderTransform = transform;
			}
			Duration duration = new Duration(TimeSpan.FromSeconds(0.3));
			var xAnimation = CreateDoubleAnimation(transform.X + centerPoint.X - hexPoint.X, duration);
			var yAnimation = CreateDoubleAnimation(transform.Y + centerPoint.Y - hexPoint.Y, duration);
			transform.BeginAnimation(TranslateTransform.XProperty, xAnimation);
			transform.BeginAnimation(TranslateTransform.YProperty, yAnimation);

		}

		private DoubleAnimation CreateDoubleAnimation(double to, Duration duration)
		{
			var animation = new DoubleAnimation(to, duration);
			animation.EasingFunction = new ExponentialEase()
			{
				EasingMode = EasingMode.EaseInOut
			};
			return animation;
		}

		private void Hex_MouseDown(object sender, MouseButtonEventArgs e)
		{
			//Center((Hex)sender);
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Escape)
			{
				Minimize();
				e.Handled = true;
			}
		}


	}
}
