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
	/// Interaction logic for ZoneView.xaml
	/// </summary>
	public partial class ZoneView : UserControl
	{
		public ZoneView()
		{
			InitializeComponent();
			SetBinding(IsActivatedProperty, new Binding("Activated"));
		}



		public bool IsActivated
		{
			get
			{
				return (bool)GetValue(IsActivatedProperty);
			}
			set
			{
				SetValue(IsActivatedProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for IsActivated.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsActivatedProperty =
			DependencyProperty.Register("IsActivated", typeof(bool), typeof(ZoneView), new PropertyMetadata(false, OnIsActivatedChanged));

		static void OnIsActivatedChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			ZoneView sender = (ZoneView)source;
			sender.UpdateStates(true);
		}

		private void UpdateStates(bool transitions)
		{
			VisualStateManager.GoToState(this, IsActivated ? "Activated" : "NotActivated", transitions);
		}
	}
}
