using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ClickPresenterViewModel : PresenterViewModel
	{

		public ClickPresenterViewModel(
			Action onClicked = null,
			Action onDown = null,
			Action onUp = null,
			Action onLeft = null,
			Action onRight = null)
		{
			_MinInterval = TimeSpan.FromMilliseconds(500);
			VelocityThreshold = 500;
		}

		private Action Wrap(string side, Action act)
		{
			if(act == null)
				return null;
			return () =>
			{
				LastSide = side;
				act();
			};
		}

		private TimeSpan _MinInterval;
		public TimeSpan MinInterval
		{
			get
			{
				return _MinInterval;
			}
			set
			{
				if(value != _MinInterval)
				{
					_MinInterval = value;
					OnPropertyChanged(() => this.MinInterval);
				}
			}
		}

		public float VelocityThreshold
		{
			get;
			set;
		}

		public override IDisposable Subscribe(ReactiveSpace spaceListener)
		{
			return spaceListener
				.LockedHands
				.SelectMany(l => l)
				.Where(h => Math.Abs(h.PalmVelocity.y) > VelocityThreshold || Math.Abs(h.PalmVelocity.x) > VelocityThreshold)
				.Sample(MinInterval)
				.ObserveOn(UI)
				.Subscribe(h =>
				{
					if(OnClicked != null)
					{
						LastSide = "Center";
						OnClicked();
					}

					if(Math.Abs(h.PalmVelocity.y) > VelocityThreshold)
					{
						var isDown = h.PalmVelocity.y < 0.0;
						var upOrDown = isDown ? OnDown : OnUp;
						var side = isDown ? "Down" : "Up";
						if(upOrDown != null)
						{
							LastSide = side;
							upOrDown();
						}
					}
					else
					{
						var isLeft = h.PalmVelocity.x < 0.0;
						var leftOrRight = isLeft ? OnLeft : OnRight;
						var side = isLeft ? "Left" : "Right";
						if(leftOrRight != null)
						{
							LastSide = side;
							leftOrRight();
						}
					}
				});

		}

		private string _LastSide;
		public string LastSide
		{
			get
			{
				return _LastSide;
			}
			set
			{
				_LastSide = null;
				OnPropertyChanged(() => this.LastSide);
				_LastSide = value;
				OnPropertyChanged(() => this.LastSide);
				Console.WriteLine("click");
			}
		}



		public Action OnUp
		{
			get;
			set;
		}
		public bool CanGoUp
		{
			get
			{
				return OnUp != null;
			}
		}

		public Action OnDown
		{
			get;
			set;
		}
		public bool CanGoDown
		{
			get
			{
				return OnDown != null;
			}
		}

		public Action OnClicked
		{
			get;
			set;
		}
		public bool CanClick
		{
			get
			{
				return OnClicked != null;
			}
		}

		public Action OnRight
		{
			get;
			set;
		}
		public bool CanGoRight
		{
			get
			{
				return OnRight != null;
			}
		}

		public Action OnLeft
		{
			get;
			set;
		}
		public bool CanGoLeft
		{
			get
			{
				return OnLeft != null;
			}
		}
	}
}
