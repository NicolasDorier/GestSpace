using GestSpace.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace GestSpace.Tests
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class AltTab : Window
	{
		DispatcherTimer _Timer = new DispatcherTimer();
		public AltTab()
		{
			InitializeComponent();
			_Timer.Interval = TimeSpan.FromSeconds(1);
			_Timer.Tick += _Timer_Tick;
			_Timer.Start();
		}

		void _Timer_Tick(object sender, EventArgs e)
		{
			DataContext = user32.GetAltTabWindows();
		}

		

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(e.AddedItems != null && e.AddedItems.Count > 0)
			{
				var item = (TabWindow)e.AddedItems[0];
				user32.SetForeground(item.Handle);
			}
		}


	}
}
