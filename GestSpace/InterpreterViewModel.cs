﻿using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class InterpreterCommandViewModel : NotifyPropertyChangedBase
	{
		private InterpreterViewModel interpreterViewModel;
		

		private string[] _Commands;
		public string[] Commands
		{
			get
			{
				return _Commands;
			}
			set
			{
				if(value != _Commands)
				{
					_Commands = value;
					Parse();
					OnPropertyChanged(() => this.Commands);
				}
			}
		}

		private void Parse()
		{
			_ParsedCommands = null;
			Exception = null;
			try
			{
				_ParsedCommands = interpreterViewModel._Interpreter.Parse(Commands);
			}
			catch(InterpreterException ex)
			{
				Exception = ex;
			}
		}

		private InterpreterException _Exception;
		public InterpreterException Exception
		{
			get
			{
				return _Exception;
			}
			set
			{
				if(value != _Exception)
				{
					_Exception = value;
					OnPropertyChanged(() => this.Exception);
				}
			}
		}

		IEnumerable<InterpreterCommand> _ParsedCommands;


		public InterpreterCommandViewModel(InterpreterViewModel interpreterViewModel, string[] commands)
		{
			this.interpreterViewModel = interpreterViewModel;
			this.Commands = commands;
		}

		public void Execute()
		{
			if(_ParsedCommands != null)
				try
				{
					interpreterViewModel._Interpreter.Interpret(_ParsedCommands);
				}
				catch(InterpreterException ex)
				{
					Exception = ex;
				}
		}
	}
	public class InterpreterViewModel
	{
		internal Interpreter _Interpreter;
		public InterpreterViewModel(Interpreter interpreter)
		{
			_Interpreter = interpreter;
		}


		public Action Simulate(params string[] commands)
		{
			var command = new InterpreterCommandViewModel(this, commands);
			return command.Execute;
		}
	}
}