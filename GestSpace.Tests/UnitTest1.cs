using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GestSpace.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void ShowLastWindows()
		{
			AltTab win = new AltTab();
			win.ShowDialog();
		}


		[TestMethod]
		public void CanInterpret()
		{
			Interpreter interpreter = new Interpreter();
			var commands = interpreter.Parse("PRESS A,B,C").ToList();
			Assert.AreEqual(1, commands.Count);
			var result = commands.First();
			Assert.AreEqual("PRESS", result.CommandType);
			Assert.AreEqual(3, result.Parameters.Count);

			Assert.AreEqual(0, interpreter.Parse("").Count());
			Assert.AreEqual(0, interpreter.Parse(null as string).Count());
		}

		[TestMethod]
		public void StripQuotesFromParameters()
		{
			Interpreter interpreter = new Interpreter();
			var commands = interpreter.Parse("PRESS A,\"B\",C").ToList();
			Assert.AreEqual(1, commands.Count);
			var result = commands.First();
			Assert.AreEqual("PRESS", result.CommandType);
			Assert.AreEqual(3, result.Parameters.Count);
			Assert.AreEqual("B", result.Parameters[1]);
		}

		[TestMethod]
		public void CanInterpretTwoSentences()
		{
			Interpreter interpreter = new Interpreter();
			var commands = interpreter.Parse("PRESS A,\"B\",C\r\nPRESS A,B").ToList();
			Assert.AreEqual(2, commands.Count);
			Assert.AreEqual(2, commands[1].Parameters.Count);
		}
	}
}
