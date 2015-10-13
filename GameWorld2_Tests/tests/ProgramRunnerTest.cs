using System;
using NUnit.Framework;
using RelayLib;
using GameWorld2;
namespace GameWorld2_Tests
{
	[TestFixture()]
	public class ProgramRunnerTest
	{
        RelayTwo _relay;
		ProgramRunner _programRunner;
		
		[SetUp()]
		public void SetUp()
		{
            _relay = new RelayTwo();
			_relay.CreateTable(Program.TABLE_NAME);
			_programRunner = new ProgramRunner(_relay);
		}
		
		[Test()]
		public void SaveAndLoadAProgram()
		{
			const string saveName = "SaveAndLoadAProgramTest.json";
			const string programName = "PrintMagicNumber";
			const string sourceCode = "print(42)";
			
			int programObjectId;
			
			{
				Program p = _programRunner.CreateProgram(programName, sourceCode, "unknown");
				programObjectId = p.objectId;
				_relay.SaveAll(saveName);
			}
			
			{
                RelayTwo relay = new RelayTwo(saveName);
				ProgramRunner programRunner = new ProgramRunner(relay);
				Program p = programRunner.GetProgram(programObjectId);
				Assert.AreEqual(programName, p.name);
				Assert.AreEqual(sourceCode, p.sourceCodeContent);
			}
		}
	}
}

