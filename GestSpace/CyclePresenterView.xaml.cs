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
	/// Interaction logic for CyclePresenterView.xaml
	/// </summary>
	public partial class CyclePresenterView : UserControl
	{
		public CyclePresenterView()
		{
			InitializeComponent();
		}

		public CyclePresenterViewModel ViewModel
		{
			get
			{
				return this.DataContext as CyclePresenterViewModel;
			}
		}
	}
}
