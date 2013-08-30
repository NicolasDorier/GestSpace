using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace RxPlayground
{
	class Program
	{
		public static void Main(string[] args)
		{
			BugGroupByUntilFromHell();
		}

		public static void BugGroupByUntilFromHell()
		{
			Subject<int> s = new Subject<int>();
			var obs = s.GroupByUntil(i => i % 5, g => g.Throttle(TimeSpan.FromSeconds(1.0)));
			obs.Subscribe((o) =>
			{
				Console.WriteLine("Start1 " + o.Key);
				o.Subscribe(oo =>
				{
					Console.WriteLine(oo);
				},
				() =>
				{
					Console.WriteLine("end1");
				});
			}, () =>
			{

			});

			s.OnNext(1);
			s.OnNext(1);
			Thread.Sleep(1500);
			s.OnNext(1);
			var dos = obs.Subscribe((o) =>
			{
				Console.WriteLine("\tStart2 " + o.Key);
				o.Subscribe(oo =>
				{
					Console.WriteLine("\t" + oo);
				},
				() =>
				{
					Console.WriteLine("\tend2");
				});
			});
			s.OnNext(1);
			s.OnNext(1);
			Thread.Sleep(1500);
			s.OnNext(1);
			s.OnNext(1);
			s.OnNext(1);
			Thread.Sleep(1500);
			s.OnNext(1);
			s.OnNext(1);
			dos.Dispose();
			Console.WriteLine("\tkilled");
			s.OnNext(1);
			s.OnNext(1);
			s.OnNext(1);
			Thread.Sleep(2000);
			s.OnNext(1);
			s.OnNext(1);
			Thread.Sleep(2000);

			//Why the first subscription do not finish ?
		}
	}
}
