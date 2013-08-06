using NicolasDorier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public static class ObservableExtensions
	{
		/// <summary>
		/// Group observable sequence into buffers separated by periods of calm
		/// </summary>
		/// <param name="source">Observable to buffer</param>
		/// <param name="calmDuration">Duration of calm after which to close buffer</param>
		/// <param name="maxCount">Max size to buffer before returning</param>
		/// <param name="maxDuration">Max duration to buffer before returning</param>
		public static IObservable<IList<T>> BufferUntilCalm<T>(this IObservable<T> source, TimeSpan calmDuration, Int32? maxCount = null, TimeSpan? maxDuration = null)
		{
			var closes = source.Throttle(calmDuration);
			if(maxCount != null)
			{
				var overflows = source.Where((x, index) => index + 1 >= maxCount);
				closes = closes.Amb(overflows);
			}
			if(maxDuration != null)
			{
				var ages = source.Delay(maxDuration.Value);
				closes = closes.Amb(ages);
			}
			return source.Window(() => closes).SelectMany(window => window.ToList());
		}
	}

	public static class Helper
	{
		public static IObservable<T> PropertyChanged<TTarget, T>(TTarget target, Expression<Func<T>> property) where TTarget : INotifyPropertyChanged
		{
			var propName = ExpressionExtensions.GetPropertyName(property);
			var getFunc = property.Compile();
			return Observable.FromEventPattern<PropertyChangedEventArgs>(target, "PropertyChanged")
				   .Where(p => p.EventArgs.PropertyName == propName)
				   .Select(p => getFunc());
		}

		public static string NiceToString(this Leap.Vector v)
		{
			return (int)v.x + "," + (int)v.y + "," + (int)v.z;
		}

		public static double RadianToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}
		public static double Map(double value,
							  double istart,
							  double istop,
							  double ostart,
							  double ostop)
		{
			return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
		}

	}
}
