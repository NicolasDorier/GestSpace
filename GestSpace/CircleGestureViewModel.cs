using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class CircleGestureViewModel : GestureViewModel
	{
		public override IDisposable Subscribe(ReactiveSpace space)
		{
			return space
					.ReactiveListener
					.Gestures
					.Where(g => g.Key.Type == Leap.Gesture.GestureType.TYPECIRCLE)
					.SelectMany(g => g.ToList().Select(l => new
					{
						Key = g.Key,
						Gestures = l
					}))
					.BufferUntilCalm(TimeSpan.FromMilliseconds(200))
					.Where(b => b.Count > 0)
					.Subscribe(gs =>
					{
						var g = gs[0];
						var stop = g.Gestures.Last();


						var orientation = new Vector(GetDelta(g.Gestures, p => p.x),
													  GetDelta(g.Gestures, p => p.y),
													 GetDelta(g.Gestures, p => p.z));

						orientation = Normalize(orientation);

						var detected = new CircleGestureViewModel()
						{
							Duration = TimeSpan.FromSeconds(stop.DurationSeconds),
							Orientation = orientation,
							FingersCount = gs.SelectMany(gg => gg.Gestures).SelectMany(gg => gg.Pointables).Select(i => i.Id).Distinct().Count()
						};

						Console.WriteLine(detected.ToString());
						OnMatch(detected, 1.0);
					});
		}

		private Vector Normalize(Vector orientation)
		{
			var magnitude = orientation.Magnitude;
			return new Vector(orientation.x / magnitude, orientation.y / magnitude, orientation.z / magnitude);
		}

		float GetDelta(IEnumerable<Gesture> gestures, Func<Vector, float> selector)
		{
			var gestureSelector = new Func<Gesture, float>(g => selector(g.Pointables[0].TipPosition));
			var validGestures = gestures
					.Where(g => g.Pointables.Count > 0);
			var min = validGestures.Min(gestureSelector);
			var max = validGestures.Max(gestureSelector);
			return max - min;
		}

		public TimeSpan Duration
		{
			get;
			set;
		}

		public Vector Orientation
		{
			get;
			set;
		}

		public int FingersCount
		{
			get;
			set;
		}

		public override string ToString()
		{
			return string.Format("Detected Duration : {0} ms, Orientation : {1}, Fingers : {2}",
											(int)Duration.TotalMilliseconds,
											Orientation.NiceToString(2),
											FingersCount);
		}
	}
}
