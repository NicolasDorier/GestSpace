using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class MovePresenterViewModel : PresenterViewModel
	{
		void Empty()
		{
		}
		public MovePresenterViewModel()
		{
			MinInterval = 50.0;
		}
		

		public double MinInterval
		{
			get;
			set;
		}

		
		public Action OnMoveUp
		{
			get;
			set;
		}
		public Action OnMoveDown
		{
			get;
			set;
		}

		
		public int PreviousBin
		{
			get;
			set;
		}

		protected override IDisposable SubscribeCore(ReactiveSpace spaceListener)
		{
			CompositeDisposable subscriptions = new CompositeDisposable();
			

			subscriptions.Add(spaceListener
				.LockedHands()
				.ObserveOn(UI)
				.SelectMany(h => h
								.Select(hh => new
								{
									Group = h,
									Hand = hh
								}))
				.Subscribe(h =>
				{
					var diff = 1000 + (h.Hand.PalmPosition.y - h.Group.Key.PalmPosition.y);
					var bin = (int)(diff / MinInterval);
					if(bin < PreviousBin)
					{
						if(OnMoveDown != null)
							OnMoveDown();
					}
					if(bin > PreviousBin)
					{
						if(OnMoveUp != null)
							OnMoveUp();
					}
					PreviousBin = bin;
				}));

			return subscriptions;
		}

	}
}
