using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ActionViewModel : NotifyPropertyChangedBase
	{
		public ActionViewModel()
		{

		}
		private PresenterViewModel _Presenter;
		public PresenterViewModel Presenter
		{
			get
			{
				return _Presenter;
			}
			set
			{
				if(value != _Presenter)
				{
					_Presenter = value;
					OnPropertyChanged(() => this.Presenter);
				}
			}
		}

	}
}
