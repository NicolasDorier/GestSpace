using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Leap;
using System.Windows.Media;
using System.Threading;

namespace GestSpace
{
	public class CyclePresenterViewModel : PresenterViewModel
	{
		public CyclePresenterViewModel()
		{
			IncrementCount = 6;
		}
		public Action OnClockWise
		{
			get;
			set;
		}

		public Action OnCounterClockWise
		{
			get;
			set;
		}

		public int IncrementCount
		{
			get;
			set;
		}

		private double _Rotation;
		public double Rotation
		{
			get
			{
				return _Rotation;
			}
			set
			{
				if(value != _Rotation)
				{
					double increment = 360 / IncrementCount;
					int oldBin = (int)(_Rotation / increment);
					_Rotation = value;
					if(_Rotation > 360)
						_Rotation -= 360;
					if(_Rotation < 0)
						_Rotation += 360;

					int newBin = (int)(_Rotation / increment);

					if(newBin != oldBin)
					{
						bool goNext = oldBin < newBin || newBin == 0 && oldBin == IncrementCount - 1;
						goNext = goNext && !(newBin == IncrementCount - 1 && oldBin == 0);
						if(goNext && OnClockWise != null)
							OnClockWise();
						if(!goNext && OnCounterClockWise != null)
							OnCounterClockWise();
					}
					OnPropertyChanged(() => this.Rotation);
				}
			}
		}

		
		protected override IDisposable SubscribeCore(ReactiveSpace spaceListener)
		{
			return
				spaceListener
				.ReactiveListener
				.FingersMoves()
				.Concat()
				.ObserveOn(UI)
				.Subscribe(c =>
				{
					var v = c.TipVelocity.To2D();
					//v = new Vector(-c.TipVelocity.x, c.TipVelocity.y, 0);

					var cos = Math.Cos(Helper.DegreeeToRadian(Rotation));
					var sin = Math.Sin(Helper.DegreeeToRadian(Rotation));

					var tan = new Vector((float)sin, (float)cos, 0);

					var man = tan.Dot(v);

					Rotation -= man / 120;

				});
		}
	}
}
