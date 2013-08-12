using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class LeapGestureViewModel : GestureViewModel
	{
		Leap.Gesture.GestureType _Type;
		public LeapGestureViewModel(Leap.Gesture.GestureType gestureType)
		{
			_Type = gestureType;
		}
		public override IDisposable Subscribe(ReactiveSpace space)
		{
			return space
					.ReactiveListener
					.Gestures
					.Where(g => g.Key.Type == _Type)
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

						float speed = g.Gestures.Where(gg => gg.Pointables.Count > 0)
										.Average(gg => gg.Pointables[0].TipVelocity.Magnitude);
						
						//Diviser par le nbr de frame a la place de la duration
						var stop = g.Gestures.Last();
						var orientation = new Vector(GetDelta(g.Gestures, p => p.x),
													  GetDelta(g.Gestures, p => p.y),
													 GetDelta(g.Gestures, p => p.z));
						var size = orientation.Magnitude;
						orientation = Normalize(orientation);

						var detected = new LeapGestureViewModel(_Type)
						{
							Duration = TimeSpan.FromSeconds(stop.DurationSeconds),
							Orientation = orientation,
							FingersCount = gs.SelectMany(gg => gg.Gestures).SelectMany(gg => gg.Pointables).Select(i => i.Id).Distinct().Count(),
							Speed = speed,
							Size = size
						};

						if(detected.Speed > MinSpeed)
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

		public float Speed
		{
			get;
			set;
		}

		public float Size
		{
			get;
			set;
		}

		public override string ToString()
		{
			return string.Format("Detected Duration : {0} ms, Orientation : {1}, Fingers : {2}, Speed : {3} unit/frame, Size : {4}",
											(int)Duration.TotalMilliseconds,
											Orientation.NiceToString(2),
											FingersCount,
											Math.Round(Speed, 2),
											Math.Round(Size,2));
		}


		public double MinSpeed
		{
			get;
			set;
		}
	}
}
