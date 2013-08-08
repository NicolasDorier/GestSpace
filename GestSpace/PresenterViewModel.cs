using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
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

		protected List<TileEventViewModel> CreateEventsFrom(object obj)
		{
			var events = new List<TileEventViewModel>();
			if(obj == null)
				return events;
			foreach(var prop in obj.GetType()
								.GetProperties(BindingFlags.Instance | BindingFlags.Public)
								.Where(p => p.PropertyType == typeof(Action)))
			{
				var action = prop.GetValue(obj) as Action;
				if(action != null)
				{
					var command = action.Target as InterpreterCommandViewModel;
					if(command != null)
					{
						TileEventViewModel evt = new TileEventViewModel();
						evt.Name = prop.Name;
						evt.Command = command;
						events.Add(evt);
					}
				}
			}
			return events;
		}

		internal virtual void AddEvents(List<TileEventViewModel> events)
		{
			events.AddRange(CreateEventsFrom(this));
		}
	}
}
