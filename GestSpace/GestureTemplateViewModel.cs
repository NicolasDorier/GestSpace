using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class LeapGestureTemplateViewModel : GestureTemplateViewModel
	{
		Leap.Gesture.GestureType _Type;
		public LeapGestureTemplateViewModel(Leap.Gesture.GestureType gesture)
		{
			_Type = gesture;
		}
		public override GestureViewModel CreateGesture()
		{
			return new LeapGestureViewModel(_Type)
			{
				MinSpeed = MinSpeed
			};
		}

		public double MinSpeed
		{
			get;
			set;
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
