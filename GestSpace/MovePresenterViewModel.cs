﻿using System;
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
		public MovePresenterViewModel(Action onEnter, Action onMoveUp, Action onMoveDown, Action onRelease)
		{
			OnEnter = onEnter ?? Empty;
			OnMoveUp = onMoveUp ?? Empty;
			OnMoveDown = onMoveDown ?? Empty;
			OnRelease = onRelease ?? Empty;
			MinInterval = 50.0;
		}
		int _HandsCount;
		int HandsCount
		{
			get
			{
				return _HandsCount;
			}
			set
			{
				_HandsCount = value;
				if(_HandsCount < 0)
					_HandsCount = 0;
				if(_HandsCount == 0)
					OnRelease();
				if(_HandsCount == 1)
					OnEnter();
			}
		}

		public double MinInterval
		{
			get;
			set;
		}

		private Action OnEnter;
		private Action OnRelease;
		private Action OnMoveUp;
		private Action OnMoveDown;

		public int PreviousBin
		{
			get;
			set;
		}

		public override IDisposable Subscribe(ReactiveSpace spaceListener)
		{
			CompositeDisposable subscriptions = new CompositeDisposable();
			subscriptions.Add(spaceListener
								.LockedHands
								.ObserveOn(UI)
								.Subscribe(o =>
								{
									HandsCount++;
								}));

			subscriptions.Add(spaceListener
								.LockedHands
								.Select(o =>
										o
										.ObserveOn(UI)
										.Subscribe(oo =>
										{
										}, () =>
										{
											HandsCount--;
										}))
								.Subscribe());


			subscriptions.Add(spaceListener
				.LockedHands
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
						OnMoveDown();
					}
					if(bin > PreviousBin)
					{
						OnMoveUp();
					}
					PreviousBin = bin;
				}));

			return subscriptions;
		}

	}
}