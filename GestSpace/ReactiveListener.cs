﻿using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ReactiveListener : Listener
	{


		public ReactiveListener()
		{
			
			

		}

		public IObservable<Frame> Frames()
		{
			return NewFrameDetected;
		}

		public IObservable<IGroupedObservable<Finger, Finger>> FingersMoves()
		{
			return Frames()
							.SelectMany(f => f.Fingers)
							.GroupByUntil(f => f, 
							f => f.ThrottleWithDefault(TimeSpan.FromMilliseconds(300)).Take(1), 
							AnonymousComparer.Create((Finger g) => g.Id));
		}

		public IObservable<IGroupedObservable<Hand, Hand>> HandsMoves()
		{
			return GetHandsMoves(h => false);
		}

		public IObservable<IGroupedObservable<Hand, Hand>> GetHandsMoves(Func<Hand, bool> until)
		{
			return Frames()
							.SelectMany(f => f.Hands)
							.GroupByUntil(f => f, f => f.ThrottleWithDefault(TimeSpan.FromMilliseconds(300))
														.Amb(f.Where(until))
														.Take(1), AnonymousComparer.Create((Hand hand) => hand.Id));
		}

		
		

		public IObservable<IGroupedObservable<Gesture, Gesture>> Gestures()
		{
			return Frames()
					.SelectMany(f => f.Gestures())
					.GroupByUntil(g => g, 
					gr => gr.Where(g => g.State == Gesture.GestureState.STATESTOP).Take(1),
					AnonymousComparer.Create((Gesture g) => g.Id));
		}

		public override void OnInit(Controller controller)
		{
			controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
			controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
			controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
			controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
			controller.SetPolicyFlags(Controller.PolicyFlag.POLICYBACKGROUNDFRAMES);
		}

		public override void OnFrame(Controller arg0)
		{
			NewFrameDetected.OnNext(arg0.Frame());
		}

		Subject<Frame> NewFrameDetected = new Subject<Frame>();
	}


}
