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

			var unused = new ActionTemplateViewModel("Not used", () => new UnusedActionViewModel());
			that.Main = new DynamicViewModel();
			that.Main.ActionTemplates = new List<ActionTemplateViewModel>()
			{
				unused
			};
			that.SelectedActionTemplate = unused;

		}


	}
}
