using Leap;
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

		MainWindow window;
		public ReactiveListener(MainWindow window)
		{
		
			this.window = window;
			Frames = NewFrameDetected;
			Gestures = Frames
							.SelectMany(f => f.Gestures())
							.GroupByUntil(g => g, gr => gr.Any(g => g.State == Gesture.GestureState.STATESTOP), AnonymousComparer.Create((Gesture g) => g.Id));
			FingersMoves = Frames
							.SelectMany(f => f.Fingers)
							.GroupByUntil(f => f, f => f.OnlyTimeout(TimeSpan.FromMilliseconds(300)), AnonymousComparer.Create((Finger g) => g.Id));
		}

		public IObservable<Frame> Frames
		{
			get;
			private set;
		}

		public IObservable<IGroupedObservable<Finger, Finger>> FingersMoves
		{
			get;
			private set;
		}

		public IObservable<IGroupedObservable<Gesture, Gesture>> Gestures
		{
			get;
			private set;
		}

		public override void OnInit(Controller controller)
		{
			controller.EnableGesture(Gesture.GestureType.TYPECIRCLE);
			controller.EnableGesture(Gesture.GestureType.TYPEKEYTAP);
			controller.EnableGesture(Gesture.GestureType.TYPESCREENTAP);
			controller.EnableGesture(Gesture.GestureType.TYPESWIPE);
		}

		public override void OnFrame(Controller arg0)
		{
			NewFrameDetected.OnNext(arg0.Frame());
		}


		Subject<Frame> NewFrameDetected = new Subject<Frame>();
	}


}
