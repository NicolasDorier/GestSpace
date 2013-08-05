using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public enum InterpreterCommandType
	{
		Press,
	}
	public class InterpreterCommand
	{
		public InterpreterCommand()
		{
			Parameters = new List<string>();
		}
		public InterpreterCommandType CommandType
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
					var command = (InterpreterCommandType)Enum.Parse(typeof(InterpreterCommandType), result, true);
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
				throw new FormatException("Invalid token");
			}

			public object Value
			{
				get;
				set;
			}
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
					yield return command;
				if(reader.CurrentToken == Token.Command)
					command = new InterpreterCommand()
					{
						CommandType = (InterpreterCommandType)reader.Value
					};
				if(reader.CurrentToken == Token.Parameter)
					command.Parameters.Add((string)reader.Value);
			}

		}
	}
}
