using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class PresenterTemplateViewModel
	{
		public PresenterTemplateViewModel(string description, Func<PresenterViewModel> createAction)
			: this(description, null, createAction)
		{
		}
		public PresenterTemplateViewModel(string description, string suggestedName, Func<PresenterViewModel> createPresenter)
		{
			Description = description;
			this._CreatePresenter = createPresenter;
			this.SuggestedName = suggestedName;
		}
		public string Description
		{
			get;
			set;
		}

		Func<PresenterViewModel> _CreatePresenter;
		public PresenterViewModel CreatePresenter()
		{
			return _CreatePresenter();
		}

		PresenterViewModel _Sample;
		public PresenterViewModel Sample
		{
			get
			{
				if(_Sample == null)
				{
					_Sample = CreatePresenter();
				}
				return _Sample;
			}
		}

		string _SuggestedName;
		public string SuggestedName
		{
			get
			{
				return _SuggestedName ?? Description;
			}
			set
			{
				_SuggestedName = value;
			}
		}
	}
}
