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
			SetBinding(IsSelectedProperty, new Binding("IsSelected"));
		}


		bool IsSelected
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
		static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register("IsSelected", typeof(bool), typeof(TileView), new PropertyMetadata(false,OnIsSelectedChanged));

		static void OnIsSelectedChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			TileView sender = (TileView)source;
			sender.UpdateStates(true);
		}

		private void UpdateStates(bool transition)
		{
			if(IsSelected)
				VisualStateManager.GoToState(this, "SelectedState", transition);
			else
				VisualStateManager.GoToState(this, "NotSelectedState", transition);
		}


	}
}
