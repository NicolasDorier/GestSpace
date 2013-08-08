using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GestSpace
{
	public class TileEventViewModel : NotifyPropertyChangedBase
	{
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

		private InterpreterCommandViewModel _Command;
		public InterpreterCommandViewModel Command
		{
			get
			{
				return _Command;
			}
			set
			{
				if(value != _Command)
				{
					_Command = value;
					OnPropertyChanged(() => this.CanModifyScript);
					OnPropertyChanged(() => this.Command);
				}
			}
		}

		
		public bool CanModifyScript
		{
			get
			{
				return Command != null;
			}
		}
	}
}
