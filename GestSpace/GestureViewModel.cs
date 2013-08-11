using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class GestureMatch
	{
		public GestureViewModel Sender
		{
			get;
			set;
		}
		public double Distance
		{
			get;
			set;
		}
		public GestureViewModel Detected
		{
			get;
			set;
		}
	}
	public class GestureViewModel
	{
		public virtual IDisposable Subscribe(ReactiveSpace space)
		{
			return Disposable.Empty;
		}
		Subject<GestureMatch> _Matches = new Subject<GestureMatch>();
		protected void OnMatch(GestureViewModel gesture, double distance)
		{
			_Matches.OnNext(new GestureMatch()
			{
				Sender = this,
				Distance = distance,
				Detected = gesture
			});
		}
		public IObservable<GestureMatch> GestureMatches
		{
			get
			{
				return _Matches;
			}
		}
	}
}
