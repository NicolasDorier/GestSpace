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
using System.Windows.Markup;
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
	///     xmlns:MyNamespace="clr-namespace:AOControl.Controls"
	///
	///
	/// Step 1b) Using this custom control in a XAML file that exists in a different project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:AOControl.Controls;assembly=AOControl.Controls"
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
	///     <MyNamespace:AutoArrangeGrid/>
	///
	/// </summary>
	[ContentProperty("NotifiableChildren")]
	public class AutoArrangeGrid : Grid
	{
		public class NotifiableUIElementCollection : UIElementCollection
		{
			private AutoArrangeGrid parent;

			public NotifiableUIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
				: base(visualParent, logicalParent)
			{
				parent = (AutoArrangeGrid)logicalParent;
			}

			public override int Add(System.Windows.UIElement element)
			{
				var ret = parent.AddChild(element);
				parent.ChildAdded(element);
				if(parent != null)
					return ret;
				return -1;
			}

			public override void Remove(UIElement element)
			{
				if(parent != null)
					parent.RemoveChild(element);

				parent.ChildRemoved(element);
			}
		}


		int count = 1;

		private NotifiableUIElementCollection notifiableChildren;

		public NotifiableUIElementCollection NotifiableChildren
		{
			get
			{
				return notifiableChildren;
			}
		}

		public AutoArrangeGrid()
		{
			// Initialize the NotifiableUIElementCollection
			notifiableChildren = new NotifiableUIElementCollection(this, this);
		}

		#region INotifiableParent Members

		internal int AddChild(System.Windows.UIElement child)
		{
			// Add your custom code here
			TextBlock tb = child as TextBlock;

			if(tb != null)
			{
				tb.Text = count++.ToString();
			}

			// Add the child to the InternalChildren
			return this.Children.Add(child);
		}

		internal void RemoveChild(System.Windows.UIElement child)
		{
			this.Children.Remove(child);
		}

		#endregion


		internal void ChildAdded(UIElement element)
		{
			var index = this.Children.IndexOf(element);
			var line = index / ColumnDefinitions.Count;
			var column = index % ColumnDefinitions.Count;
			Grid.SetRow(element, line);
			Grid.SetColumn(element, column);
			ArrangeRows();
		}

		private void ArrangeRows()
		{
			var rowCount = (this.Children.Count / ColumnDefinitions.Count) + 1;
			while(RowDefinitions.Count < rowCount)
				RowDefinitions.Add(new RowDefinition());
		}

		internal void ChildRemoved(UIElement element)
		{
			foreach(UIElement child in Children)
				ChildAdded(child);
		}
	}
}
