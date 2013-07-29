using GestSpace.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace GestSpace
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			this.Loaded += MainWindow_Loaded;
		}

		
		private void Minimize()
		{
			var animation = CreateDoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(0.5)));
			this.BeginAnimation(OpacityProperty, animation);
		}

		void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			Maximize();
			Center(hex2);
		}

		private void Maximize()
		{
			WindowState = System.Windows.WindowState.Maximized;
			var animation = CreateDoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(0.5)));
			this.BeginAnimation(OpacityProperty, animation);
		}


		private void Center(Hex hex)
		{
			var centerPoint = center.TranslatePoint(new Point(0, 0), root);
			var hexPoint = hex.TranslatePoint(new Point(hex.ActualWidth / 2.0, hex.ActualHeight / 2.0), root);
			var transform = canvas.RenderTransform as TranslateTransform; // = new TranslateTransform(centerPoint.X - hexPoint.X, centerPoint.Y - hexPoint.Y);
			if(transform == null)
			{
				transform = new TranslateTransform();
				canvas.RenderTransform = transform;
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
			Center((Hex)sender);
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
