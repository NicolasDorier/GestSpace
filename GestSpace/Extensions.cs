using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GestSpace
{
	public static class Extensions
	{
		public static IObservable<bool> OnlyTimeout<T>(this IObservable<T> obs, TimeSpan time)
		{
			return obs.Select((o) => false).Timeout(time, Observable.Return(true)).Where((o) => o);
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
