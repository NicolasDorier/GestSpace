using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class TileConfigViewModelMock : DynamicViewModel
	{
		dynamic that;
		public TileConfigViewModelMock()
		{
			that = this;
			that.Description = "Volume";

			var unused = new PresenterTemplateViewModel("Not used", () => PresenterViewModel.Unused);
			that.Main = new DynamicViewModel();
			that.Main.PresenterTemplates = new List<PresenterTemplateViewModel>()
			{
				unused
			};
			that.SelectedPresenterTemplate = unused;

		}


	}
}
