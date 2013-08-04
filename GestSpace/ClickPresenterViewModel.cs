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
		Action _OnClicked;
		Action _OnUp;
		Action _OnDown;
		Action _OnLeft;
		Action _OnRight;
		public ClickPresenterViewModel(
			Action onClicked = null,
			Action onDown = null,
			Action onUp = null,
			Action onLeft = null,
			Action onRight = null)
		{
			_OnClicked = Wrap("Center", onClicked);
			_OnUp = Wrap("Up", onUp);
			_OnDown = Wrap("Down", onDown);
			_OnRight = Wrap("Right", onRight);
			_OnLeft = Wrap("Left", onLeft);
			_MinInterval = TimeSpan.FromMilliseconds(300);
			VelocityThreshold = 1000;
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
					if(_OnClicked != null)
						_OnClicked();

					if(Math.Abs(h.PalmVelocity.y) > VelocityThreshold)
					{
						var upOrDown = h.PalmVelocity.y < 0.0 ? _OnDown : _OnUp;
						if(upOrDown != null)
							upOrDown();
					}
					else
					{
						var leftOrRight = h.PalmVelocity.x < 0.0 ? _OnLeft : _OnRight;
						if(leftOrRight != null)
							leftOrRight();
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
				if(value != _LastSide)
				{
					_LastSide = value;
					OnPropertyChanged(() => this.LastSide);
				}
			}
		}

		public bool CanGoUp
		{
			get
			{
				return _OnUp != null;
			}
		}

		public bool CanGoDown
		{
			get
			{
				return _OnDown != null;
			}
		}

		public bool CanClick
		{
			get
			{
				return _OnClicked != null;
			}
		}

		public bool CanGoRight
		{
			get
			{
				return _OnRight != null;
			}
		}

		public bool CanGoLeft
		{
			get
			{
				return _OnLeft != null;
			}
		}
	}
}
