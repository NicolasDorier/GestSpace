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

namespace GestSpace.Controls
{
	/// <summary>
	/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
	///
	/// Step 1a) Using this custom control in a XAML file that exists in the current project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:GestSpace.Controls"
	///
	///
	/// Step 1b) Using this custom control in a XAML file that exists in a different project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:GestSpace.Controls;assembly=GestSpace.Controls"
	///
	/// You will also need to add a project reference from the project where the XAML file lives
	/// to this project and Rebuild to avoid compilation errors:
	///
	///     Right click on the target project in the Solution Explorer and
	///     "Add Reference"->"Projects"->[Browse to and select this project]
	///
	///
	/// Step 2)
	/// Go ahead and use your control in the XAML file.
	///
	///     <MyNamespace:HexCanvas/>
	///
	/// </summary>
	public class HexCanvas : Panel
	{
		static HexCanvas()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HexCanvas), new FrameworkPropertyMetadata(typeof(HexCanvas)));
		}



		public static int GetLeftHex(DependencyObject obj)
		{
			return (int)obj.GetValue(LeftHexProperty);
		}

		public static void SetLeftHex(DependencyObject obj, int value)
		{
			obj.SetValue(LeftHexProperty, value);
		}

		// Using a DependencyProperty as the backing store for LeftHex.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LeftHexProperty =
			DependencyProperty.RegisterAttached("LeftHex", typeof(int), typeof(HexCanvas), new PropertyMetadata(0));




		public static int GetTopHex(DependencyObject obj)
		{
			return (int)obj.GetValue(TopHexProperty);
		}

		public static void SetTopHex(DependencyObject obj, int value)
		{
			obj.SetValue(TopHexProperty, value);
		}

		// Using a DependencyProperty as the backing store for TopHex.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TopHexProperty =
			DependencyProperty.RegisterAttached("TopHex", typeof(int), typeof(HexCanvas), new PropertyMetadata(0));

		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			if(visualAdded != null && visualAdded is Hex)
			{
				((FrameworkElement)visualAdded).Width = Width;
			}
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
		}




		public double GutterSize
		{
			get
			{
				return (double)GetValue(GutterSizeProperty);
			}
			set
			{
				SetValue(GutterSizeProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for GuttenSize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GutterSizeProperty =
			DependencyProperty.Register("GutterSize", typeof(double), typeof(HexCanvas), new PropertyMetadata(2.0, OnGuttenSizeChanged));


		static void OnGuttenSizeChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			HexCanvas sender = (HexCanvas)source;
			sender.InvalidateArrange();
		}

		protected override Size MeasureOverride(Size constraint)
		{
			Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
			foreach(UIElement uIElement in base.InternalChildren)
			{
				if(uIElement != null)
				{
					uIElement.Measure(availableSize);
				}
			}
			return default(Size);
		}


		protected override Size ArrangeOverride(Size arrangeSize)
		{
			foreach(UIElement uIElement in base.InternalChildren)
			{
				if(uIElement != null)
				{
					var width = Width + GutterSize;
					double isOdd = HexCanvas.GetTopHex(uIElement) % 2 == 1 ? 1.0 : 0.0;
					double x = 0.0;
					double y = 0.0;
					double left = HexCanvas.GetLeftHex(uIElement);
					x = left * width + left * 1 / 2 * width + (width - width * 1.0 / 4.0) * isOdd;

					double top = HexCanvas.GetTopHex(uIElement);
					y = top * width * (Math.Sqrt(3) / 4);

					uIElement.Arrange(new Rect(new Point(x, y), uIElement.DesiredSize));
				}
			}
			return arrangeSize;
		}
	}
}
