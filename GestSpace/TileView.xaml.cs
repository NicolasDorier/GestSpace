using GestSpace.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestSpace
{
	/// <summary>
	/// Interaction logic for TileView.xaml
	/// </summary>
	public partial class TileView : UserControl
	{
		public TileView()
		{
			InitializeComponent();
			SetBinding(ViewModelProperty, new Binding());

		}

		SerialDisposable _Subscription = new SerialDisposable();

		public TileViewModel ViewModel
		{
			get
			{
				return (TileViewModel)GetValue(ViewModelProperty);
			}
			set
			{
				SetValue(ViewModelProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register("ViewModel", typeof(TileViewModel), typeof(TileView), new PropertyMetadata(OnViewModelChanged));


		static void OnViewModelChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			TileView sender = (TileView)source;
			sender.UpdateStates(true);
			if(sender.ViewModel == null)
				sender._Subscription.Disposable = null;
			else
				sender._Subscription.Disposable = Observable.FromEventPattern<PropertyChangedEventArgs>(sender.ViewModel, "PropertyChanged")
						  .Subscribe(o => sender.UpdateStates(true));
		}



		private void UpdateStates(bool transition)
		{
			VisualStateManager.GoToState(this, ViewModel.IsUnused ? "Unused" : "Used", transition);
			VisualStateManager.GoToState(this, ViewModel.IsSelected ? "SelectedState" : "NotSelectedState", transition);
			VisualStateManager.GoToState(this, ViewModel.IsLocked ? "Locked" : "NotLocked", transition);
		}

		private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			ViewModel.Main.ShowConfig = true;
		}

		


	}
}
