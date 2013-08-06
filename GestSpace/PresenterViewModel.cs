using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GestSpace
{
	public class PresenterViewModel : NotifyPropertyChangedBase, IDisposable
	{
		public readonly static PresenterViewModel Unused = new PresenterViewModel();

		public static SynchronizationContext UI = SynchronizationContext.Current;
		public virtual IDisposable Subscribe(ReactiveSpace spaceListener)
		{
			return Disposable.Empty;
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			
		}

		#endregion
	}
}
