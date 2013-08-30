using Leap;
using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ValuePresenterViewModel : PresenterViewModel
	{
		IObservable<double> getValue;
		Action<double> setValue;
		IDisposable _Subscription;

		public ValuePresenterViewModel(double minValue, double maxValue, Func<double> getValue, Action<double> setValue, TimeSpan? pollInterval = null)
			: this(minValue, maxValue, Create(getValue, pollInterval), setValue)
		{
		}

		private static IObservable<double> Create(Func<double> getValue, TimeSpan? pollInterval)
		{
			if(pollInterval == null)
				pollInterval = TimeSpan.FromSeconds(1.0);
			return Observable.Return(getValue())
					  .Merge(Observable.Interval(pollInterval.Value)
							.Select(v => getValue()));
		}

		public ValuePresenterViewModel(double minValue, double maxValue, IObservable<double> getValue, Action<double> setValue)
		{
			this.setValue = setValue;
			this.getValue = getValue;
			this.MinValue = minValue;
			this.MaxValue = maxValue;
			this._Subscription = getValue
									.Subscribe(d =>
									{
										_Value = Normalize(d);
										OnPropertyChanged(() => this.Value);
									});
		}


		protected override IDisposable SubscribeCore(ReactiveSpace spaceListener)
		{
			return
				spaceListener.LockedHands()
				.ObserveOn(UI)
				.Select(g => new
				{
					Group = g,
					StartValue = Value,
					StartHeight = g.Key.PalmPosition.y
				})
				.SelectMany(g =>
					g
					.Group
					.Select(p => new
					{
						Position = p,
						GroupContext = g
					}))
				.Subscribe((hand) =>
				{
					double heightRange = 150.0;
					var maxHeight = hand.GroupContext.StartHeight + heightRange * ((MaxValue - hand.GroupContext.StartValue) / (MaxValue - MinValue));
					var minHeight = maxHeight - heightRange;
					double height = hand.Position.PalmPosition.y;
					height = Math.Max(minHeight, height);
					height = Math.Min(maxHeight, height);
					Value = Helper.Map(height, minHeight, maxHeight, MinValue, MaxValue);
				});
		}


		private double _Value;
		public double Value
		{
			get
			{
				return _Value;
			}
			set
			{
				if(value != _Value)
				{
					value = Normalize(value);
					_Value = value;
					setValue(value);
					OnPropertyChanged(() => this.Value);
				}
			}
		}

		private double Normalize(double value)
		{
			value = Math.Min(MaxValue, value);
			value = Math.Max(MinValue, value);
			return value;
		}

		private double _MaxValue;
		public double MaxValue
		{
			get
			{
				return _MaxValue;
			}
			set
			{
				if(value != _MaxValue)
				{
					_MaxValue = value;
					OnPropertyChanged(() => this.MaxValue);
				}
			}
		}
		private double _MinValue;
		public double MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
				if(value != _MinValue)
				{
					_MinValue = value;
					OnPropertyChanged(() => this.MinValue);
				}
			}
		}

		
		public override void Dispose()
		{
			_Subscription.Dispose();
		}
	}
}
