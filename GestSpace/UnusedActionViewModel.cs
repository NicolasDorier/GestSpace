using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class UnusedPresenterViewModel : PresenterViewModel
	{
	}
	public class UnusedActionViewModel : ActionViewModel
	{
		public UnusedActionViewModel()
		{
			this.Presenter = new UnusedPresenterViewModel();
		}
	}
}
