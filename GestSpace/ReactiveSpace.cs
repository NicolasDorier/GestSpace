﻿using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ReactiveSpace
	{
		ReactiveListener listener;

		public ReactiveListener ReactiveListener
		{
			get
			{
				return listener;
			}
		}
		public ReactiveSpace(ReactiveListener listener)
		{
			this.listener = listener;
		}


		IObservable<bool> _IsLocked;
		public IObservable<bool> IsLocked()
		{
			if(_IsLocked == null)
			{
				var replay = listener
							.Frames()
							.SelectMany(f => f.Hands)
							.Where(h => h.Fingers.Count >= 4)
							.Select(s => true)
							.Timeout(TimeSpan.FromMilliseconds(200), Observable.Return(false).Take(1))
							.Repeat()
							.DistinctUntilChanged()
							.Replay(1);
				replay.Connect();
				_IsLocked = replay;
			}
			return _IsLocked;
		}


		public IObservable<IGroupedObservable<Hand, Hand>> LockedHands()
		{
			return listener
					.Frames()
					.SelectMany(f => f.Hands)
					.CombineLatest(IsLocked(), (a, b) => new
					{
						IsLocked = b,
						Group = a
					})
					.Where(l => l.IsLocked)
					.Select(l => l.Group)
					.GroupByUntil(
									h => h,
									g => IsLocked().Where(t => !t),
									AnonymousComparer.Create((Hand hand) => hand.Id)
								);
		}
	}
}

