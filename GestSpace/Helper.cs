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
	public class Helper
	{
		public static IObservable<T> PropertyChanged<TTarget, T>(TTarget target, Expression<Func<T>> property) where TTarget : INotifyPropertyChanged
		{
			var propName = ExpressionExtensions.GetPropertyName(property);
			var getFunc = property.Compile();
			return Observable.FromEventPattern<PropertyChangedEventArgs>(target, "PropertyChanged")
				   .Where(p => p.EventArgs.PropertyName == propName)
				   .Select(p => getFunc());
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
