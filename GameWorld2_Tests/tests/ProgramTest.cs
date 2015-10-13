using System;
using System.Text;
using NUnit.Framework;
using GameWorld2;
using RelayLib;
using System.IO;
using System.Collections.Generic;
using ProgrammingLanguageNr1;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class ProgramTest
	{
		[Test()]
		public void Compile()
		{
            RelayTwo relay = new RelayTwo();
			TableTwo programsTable = relay.CreateTable(Program.TABLE_NAME);
			
			Program p1 = new Program();
			p1.CreateNewRelayEntry(programsTable, "Program");
			p1.sourceCodeContent = "var x = 5";
			p1.Compile();
			Assert.AreEqual(0, p1.GetErrors().Length);
		}
		
		[Test()]
		public void CompileWithError()
		{
            RelayTwo relay = new RelayTwo();
            TableTwo programsTable = relay.CreateTable(Program.TABLE_NAME);
			
			Program p1 = new Program();
			p1.CreateNewRelayEntry(programsTable, "Program");
			p1.sourceCodeContent = "var x = ";
			p1.Compile();
			Assert.AreEqual(1, p1.GetErrors().Length);
		}		
		
		List<string> output;
		
		[Test()]
		public void CompileAndRun()
		{
			GameTypes.D.onDLog += Console.WriteLine;

			output = new List<string>();

            RelayTwo relay = new RelayTwo();
			TableTwo programsTable = relay.CreateTable(Program.TABLE_NAME);
			
			FunctionDefinition print = new FunctionDefinition("void", "print", new string[] { "string" }, new string[] { "s" }, API_print, FunctionDocumentation.Default());

			Program p1 = new Program();
			p1.CreateNewRelayEntry(programsTable, "Program");
			p1.Init(new ProgramRunner(relay));
			p1.sourceCodeContent = "print(42)";
			p1.FunctionDefinitions.Add(print);
			p1.Compile();
			Assert.AreEqual(0, p1.GetErrors().Length);
			
			for(int i = 0; i < 100; i++) {
				if(p1.sprakRunner.interpreter != null) {
					p1.Update(0.1f);
				}
			}
			
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("42", output[0]);
		}
		
		object API_print(object[] args)
		{
			string pretty = ReturnValueConversions.PrettyStringRepresenation(args[0]);
			output.Add(pretty);
			Console.WriteLine("output: " + pretty);
			return VoidType.voidType;
		}
	}
}

