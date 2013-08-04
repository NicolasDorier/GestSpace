using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ActionTemplateViewModel
	{
		public ActionTemplateViewModel(string description, Func<ActionViewModel> createAction)
			: this(description, null, createAction)
		{
		}
		public ActionTemplateViewModel(string description, string suggestedName, Func<ActionViewModel> createAction)
		{
			Description = description;
			this._CreateAction = createAction;
			this.SuggestedName = suggestedName;
		}
		public string Description
		{
			get;
			set;
		}

		Func<ActionViewModel> _CreateAction;
		public ActionViewModel CreateAction()
		{
			return _CreateAction();
		}

		ActionViewModel _Sample;
		public ActionViewModel Sample
		{
			get
			{
				if(_Sample == null)
				{
					_Sample = CreateAction();
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
