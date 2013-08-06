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
	/// Interaction logic for ClickPresenterView.xaml
	/// </summary>
	public partial class ClickPresenterView : UserControl
	{
		public ClickPresenterView()
		{
			InitializeComponent();
			SetBinding(LastSideProperty, new Binding("LastSide"));
			SetBinding(ViewModelProperty, new Binding());
		}




		public ClickPresenterViewModel ViewModel
		{
			get
			{
				return (ClickPresenterViewModel)GetValue(ViewModelProperty);
			}
			set
			{
				SetValue(ViewModelProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register("ViewModel", typeof(ClickPresenterViewModel), typeof(ClickPresenterView), new PropertyMetadata(null, OnViewModelChanged));

		static void OnViewModelChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			ClickPresenterView sender = (ClickPresenterView)source;
			sender.HideArrowsRefresh();
		}

		private void HideArrowsRefresh()
		{
			if(ViewModel != null)
			{
				UpPart.Visibility = ToVisibility(ViewModel.CanGoUp);
				DownPart.Visibility = ToVisibility(ViewModel.CanGoDown);
				CenterPart.Visibility = ToVisibility(ViewModel.CanClick);
				RightPart.Visibility = ToVisibility(ViewModel.CanGoRight);
				LeftPart.Visibility = ToVisibility(ViewModel.CanGoLeft);
			}
		}

		private Visibility ToVisibility(bool v)
		{
			return v ? Visibility.Visible : Visibility.Hidden;
		}

		String LastSide
		{
			get
			{
				return (string)GetValue(LastSideProperty);
			}
			set
			{
				SetValue(LastSideProperty, value);
			}
		}

		
		 static readonly DependencyProperty LastSideProperty =
			DependencyProperty.Register("LastSide", typeof(string), typeof(ClickPresenterView), new PropertyMetadata(null, OnLastSideChanged));

		 static void OnLastSideChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		 {
			 ClickPresenterView sender = (ClickPresenterView)source;
			 sender.Animate();
		 }

		 private void Animate()
		 {
			 if(ViewModel.LastSide == null)
				 return;
			 ColorAnimation animation = new ColorAnimation();
			 animation.Duration = ViewModel.MinInterval;
			 animation.To = Colors.Black;
			 animation.FillBehavior = FillBehavior.Stop;
			 animation.AutoReverse = true;

			 var element = (Shape)this.FindName(ViewModel.LastSide + "Part");
			 element.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation);
			 
		 }
	}
}
