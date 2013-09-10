using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace GestSpace
{
	/// <summary>
	/// Interaction logic for TileConfigView.xaml
	/// </summary>
	public partial class TileConfigView : UserControl
	{
		public TileConfigView()
		{
			InitializeComponent();
			SetBinding(ViewModelProperty, new Binding());
			SetBinding(CanShowProperty, new Binding("Main.ShowConfig"));
			SetBinding(IsSelectedProperty, new Binding("IsSelected"));
			SetBinding(HasExceptionProperty, new Binding("SelectedEvent.Command.HasException"));
			UpdateStates(false);
		}



		public bool HasException
		{
			get
			{
				return (bool)GetValue(HasExceptionProperty);
			}
			set
			{
				SetValue(HasExceptionProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for HasException.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HasExceptionProperty =
			DependencyProperty.Register("HasException", typeof(bool), typeof(TileConfigView), new PropertyMetadata(OnHasExceptionChanged));

		static void OnHasExceptionChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			TileConfigView sender = (TileConfigView)source;
			sender.Show((bool)args.NewValue);
		}

		Subject<bool> _ToolTipShow;
		Subject<bool> ToolTipShow
		{
			get
			{
				if(_ToolTipShow == null)
				{
					_ToolTipShow = new Subject<bool>();
					_ToolTipShow.Throttle(TimeSpan.FromMilliseconds(5000))
								.ObserveOn(SynchronizationContext.Current)
								.Subscribe(o => errorTooltip.IsOpen = false);
					_ToolTipShow
						.ObserveOn(SynchronizationContext.Current)
						.Subscribe(o => errorTooltip.IsOpen = o);
				}
				return _ToolTipShow;
			}
		}
		private void Show(bool value)
		{
			ToolTipShow.OnNext(value);
		}

		public bool IsSelected
		{
			get
			{
				return (bool)GetValue(IsSelectedProperty);
			}
			set
			{
				SetValue(IsSelectedProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register("IsSelected", typeof(bool), typeof(TileConfigView), new PropertyMetadata(false, OnIsSelectedChanged));

		static void OnIsSelectedChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			TileConfigView sender = (TileConfigView)source;
			sender.UpdateStates(true);
		}

		public bool CanShow
		{
			get
			{
				return (bool)GetValue(CanShowProperty);
			}
			set
			{
				SetValue(CanShowProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for IsHidden.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanShowProperty =
			DependencyProperty.Register("CanShow", typeof(bool), typeof(TileConfigView), new PropertyMetadata(true, OnCanShowChanged));

		static void OnCanShowChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			TileConfigView sender = (TileConfigView)source;
			sender.UpdateStates(true);
		}

		private void UpdateStates(bool transition)
		{
			VisualStateManager.GoToState(this, CanShow && IsSelected ? "Showing" : "NotShowing", transition);
		}

		TileViewModel ViewModel
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
		static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register("ViewModel", typeof(TileViewModel), typeof(TileConfigView), new PropertyMetadata(null));



		private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(ViewModel != null)
			{
				ViewModel.Main.ShowConfig = false;
			}
		}
	}
}
