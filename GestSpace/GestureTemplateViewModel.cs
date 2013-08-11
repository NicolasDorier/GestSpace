using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class GestureTemplateViewModel<T> : GestureTemplateViewModel where T : GestureViewModel, new()
	{
		public override GestureViewModel CreateGesture()
		{
			return new T();
		}
	} 
	public abstract class GestureTemplateViewModel : NotifyPropertyChangedBase
	{
		class EmptyGestureTemplateViewModel : GestureTemplateViewModel
		{
			class EmptyGesture : GestureViewModel
			{
				public readonly static EmptyGesture Instance = new EmptyGesture();
			}
			public EmptyGestureTemplateViewModel()
			{
				this.Name = "None";
			}
			
			public override GestureViewModel CreateGesture()
			{
				return EmptyGesture.Instance;
			}
		}
		public static readonly GestureTemplateViewModel Empty = new EmptyGestureTemplateViewModel();
		public abstract GestureViewModel CreateGesture();

		private string _Name;
		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				if(value != _Name)
				{
					_Name = value;
					OnPropertyChanged(() => this.Name);
				}
			}
		}
	}
}
