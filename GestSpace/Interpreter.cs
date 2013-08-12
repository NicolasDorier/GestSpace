using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace GestSpace
{
	public class CommandBinding
	{
		public string CommandType
		{
			get;
			set;
		}

		public Action<string[]> ValidateParameters
		{
			get;
			set;
		}

		public Action<string[]> Do
		{
			get;
			set;
		}
	}
	public class InterpreterCommand
	{
		public InterpreterCommand()
		{
			Parameters = new List<string>();
		}
		public string CommandType
		{
			get;
			set;
		}
		public List<String> Parameters
		{
			get;
			set;
		}
	}

	[Serializable]
	public class InterpreterException : Exception
	{
		public InterpreterException()
		{
		}
		public InterpreterException(string message)
			: base(message)
		{
		}
		public InterpreterException(string message, Exception inner)
			: base(message, inner)
		{
		}
		protected InterpreterException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
	}

	public class Interpreter
	{
		enum Token
		{
			None,
			Command,
			Parameter,
			EOF
		}
		class InterpreterReader
		{
			private StringReader _Reader;

			public InterpreterReader(StringReader reader)
			{
				this._Reader = reader;
			}

			private Token _CurrentToken;
			public Token CurrentToken
			{
				get
				{
					return _CurrentToken;
				}
			}

			public bool Read()
			{
				var c = _Reader.Read();
				if(c == -1)
				{
					if(_CurrentToken != Token.EOF)
					{
						_CurrentToken = Token.EOF;
						return true;
					}
					else
					{
						return false;
					}
				}
				if(c == '\r')
				{
					c = _Reader.Read();
					if(c == '\n')
					{
						_CurrentToken = Token.EOF;
						return true;
					}
					ThrowInvalidToken();
				}

				if(_CurrentToken == Token.None || _CurrentToken == Token.EOF)
				{
					var result = (char)c + ReadUntil(' ');
					var command = result;
					_CurrentToken = Token.Command;
					Value = command;
					return true;
				}
				else
				{
					var result = (char)c + ReadUntil(',');
					result = TrimQuotes(result);
					_CurrentToken = Token.Parameter;
					Value = result;
					return true;
				}
			}

			private string TrimQuotes(string result)
			{
				if(result.StartsWith("\""))
					result = result.Substring(1, result.Length - 1);
				if(result.EndsWith("\""))
					result = result.Substring(0, result.Length - 1);
				return result;
			}

			private string ReadUntil(char until)
			{
				StringBuilder result = new StringBuilder();
				while(true)
				{
					var c = _Reader.Peek();
					if(c == -1)
						return result.ToString();
					if(c == until)
					{
						_Reader.Read();
						break;
					}
					if(c == '\r')
						break;

					_Reader.Read();
					result.Append((char)c);
				}
				return result.ToString();
			}

			private void ThrowInvalidToken()
			{
				throw new InterpreterException("Invalid token");
			}

			public object Value
			{
				get;
				set;
			}
		}


		private readonly List<CommandBinding> _CommandBindings = new List<CommandBinding>();
		public List<CommandBinding> CommandBindings
		{
			get
			{
				return _CommandBindings;
			}
		}

		public Interpreter()
		{
			AddCommandBinding("PRESS", (args) =>
			{
				var allArgs = args.Select(a => ToVK(a)).ToList();
				if(allArgs.Count == 1)
					InputSimulator.SimulateKeyPress(allArgs[0]);
				else
					foreach(var a in allArgs)
						InputSimulator.SimulateKeyDown(a);
				foreach(var a in allArgs)
					InputSimulator.SimulateKeyUp(a);

			}, ValidateAllKeys);
			AddCommandBinding("UP", (args) =>
			{
				foreach(var arg in args.Select(a => ToVK(a)).ToList())
				{
					InputSimulator.SimulateKeyUp(arg);
				}
			}, ValidateAllKeys);
			AddCommandBinding("DOWN", (args) =>
			{
				foreach(var arg in args.Select(a => ToVK(a)).ToList())
				{
					InputSimulator.SimulateKeyDown(arg);
				}
			}, ValidateAllKeys);
		}

		private void ValidateAllKeys(string[] args)
		{
			args.Select(a => ToVK(a)).ToList();
		}

		Dictionary<string, VirtualKeyCode> _Aliases;

		Dictionary<string, VirtualKeyCode> Aliases
		{
			get
			{
				if(_Aliases == null)
				{
					_Aliases = new object[][]
								{
									new Object[]{"alt", VirtualKeyCode.MENU},
									new Object[]{"win", VirtualKeyCode.LWIN},
									new Object[]{"ctrl", VirtualKeyCode.CONTROL},
									new Object[]{"pageup", VirtualKeyCode.PRIOR},
									new Object[]{"pagedown", VirtualKeyCode.NEXT},
								}.ToDictionary(o => (string)o[0], o => (VirtualKeyCode)o[1]);
					foreach(var val in Enum.GetNames(typeof(VirtualKeyCode)))
					{
						if(val.StartsWith("VK_"))
						{
							var alias = val.Substring(3);
							_Aliases.Add(alias.ToLowerInvariant(), (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), val));
						}
					}
				}
				return _Aliases;
			}
		}


		private VirtualKeyCode ToVK(string key)
		{
			VirtualKeyCode result;
			if(Enum.TryParse<VirtualKeyCode>(key, out result))
				return result;
			if(Aliases.TryGetValue(key.ToLowerInvariant(), out result))
				return result;

			FuzzyCollection<string> acceptedValues = new FuzzyCollection<string>(Metrics.LevenshteinDistance);
			foreach(var val in Enum.GetNames(typeof(VirtualKeyCode)).Concat(Aliases.Select(kv => kv.Key)).Select(a => a.ToUpperInvariant()))
			{
				acceptedValues.Add(val);
			}
			var closest = acceptedValues.GetClosest(key.ToUpperInvariant()).First();
			throw new InterpreterException(key + " is not a valid parameter, do you mean " + closest.Value + " ?");
		}

		void AddCommandBinding(string commandType, Action<string[]> run, Action<string[]> validateParameters)
		{
			CommandBindings.Add(new CommandBinding()
			{
				CommandType = commandType,
				Do = run,
				ValidateParameters = validateParameters
			});
		}

		public void Interpret(string txt)
		{
			Interpret(Parse(txt));
		}

		public void Interpret(IEnumerable<InterpreterCommand> commands)
		{
			foreach(var cmd in commands)
			{
				var binding = Find(cmd.CommandType);
				if(binding == null)
					throw new InterpreterException("Command " + cmd.CommandType + " not found");
				binding.Do(cmd.Parameters.ToArray());
			}
		}

		public void Interpret(params string[] commands)
		{
			Interpret(String.Join("\r\n", commands));
		}

		private CommandBinding Find(string command)
		{
			return CommandBindings.FirstOrDefault(o => o.CommandType == command.ToUpperInvariant());
		}
		public IEnumerable<InterpreterCommand> Parse(params string[] commands)
		{
			return Parse(String.Join("\r\n", commands));
		}
		public IEnumerable<InterpreterCommand> Parse(string txt)
		{
			if(txt == null)
				yield break;
			var reader = new InterpreterReader(new StringReader(txt));
			InterpreterCommand command = null;
			while(reader.Read())
			{
				if(reader.CurrentToken == Token.EOF && command != null)
				{
					var binding = Find(command.CommandType);
					if(binding.ValidateParameters != null)
						binding.ValidateParameters(command.Parameters.ToArray());
					yield return command;
				}
				if(reader.CurrentToken == Token.Command)
				{
					var binding = Find((string)reader.Value);
					if(binding == null)
						throw new InterpreterException("Command " + reader.Value + " does not exists, available commands are : " + PrintCommands());
					command = new InterpreterCommand()
					{
						CommandType = (string)reader.Value
					};
				}
				if(reader.CurrentToken == Token.Parameter)
					command.Parameters.Add((string)reader.Value);
			}

		}

		private string PrintCommands()
		{
			return String.Join(",", CommandBindings.Select(b => b.CommandType).ToArray());
		}
	}
}
