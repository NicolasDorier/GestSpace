using GestSpace.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GestSpace
{
	public static class Extensions
	{
		public static Leap.Vector To2D(this Leap.Vector v)
		{
			return new Leap.Vector(v.x, v.y, 0);
		}

		public static bool GetOnAdornerLayer(DependencyObject obj)
		{
			return (bool)obj.GetValue(OnAdornerLayerProperty);
		}

		public static void SetOnAdornerLayer(DependencyObject obj, bool value)
		{
			obj.SetValue(OnAdornerLayerProperty, value);
		}



	
		// Using a DependencyProperty as the backing store for OnAdornerLayer.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OnAdornerLayerProperty =
			DependencyProperty.RegisterAttached("OnAdornerLayer", typeof(bool), typeof(Extensions), new PropertyMetadata(false, OnOnAdornerLayerChanged));


		static void OnOnAdornerLayerChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
			{
				var uiElement = (Control)source;
				var parent = (Panel)VisualTreeHelper.GetParent(source);
				var layer = AdornerLayer.GetAdornerLayer(parent);
				if(layer != null)
				{
					parent.Children.Remove(uiElement);
					var adorner = new ControlAdorner(parent);
					adorner.Child = uiElement;
					uiElement.SetBinding(Control.DataContextProperty, new Binding("DataContext")
					{
						Source = parent
					});
					layer.Add(adorner);
				}
			}));
		}


		public static IObservable<T> ThrottleWithDefault<T>(this IObservable<T> input, TimeSpan time, T defaultValue = default(T))
		{
			return input.Merge(Observable.Return(defaultValue)).Throttle(time);
		}
		
		public static IList<T> Drain<T>(this IObservable<T> stream)
		{
			List<T> list = new List<T>();
			using(ManualResetEvent wait = new ManualResetEvent(false))
			{
				using(stream.Subscribe(o =>
				{
					list.Add(o);
				}, () =>
				{
					wait.Set();
				}))
				{
					wait.WaitOne();
				}
			}
			return list;
		}
		public static void DrainSubscribe<TKey, T>(this IObservable<IGroupedObservable<TKey, T>> stream, Action<KeyValuePair<TKey,IList<T>>> act)
		{
			stream.Subscribe(g =>
				{
					g.ToList().Subscribe(l =>
					{
						var kv = new KeyValuePair<TKey, IList<T>>(g.Key,l);
						act(kv);
					});
				});
		}
	}
}
