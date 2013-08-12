using GestSpace.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ForegroundProgramListener
	{
		public ForegroundProgramListener()
		{
			ForegroundProcess
				= 
				Observable.Interval(TimeSpan.FromMilliseconds(800))
				.Select(_ =>
				{
					var hwnd = user32.GetForegroundWindow();
					uint pid = 0;
					user32.GetWindowThreadProcessId(hwnd, out pid);
					return (int)pid;
				})
				.DistinctUntilChanged();
		}
		public IObservable<int> ForegroundProcess
		{
			get;
			private set;
		}
	}
}
