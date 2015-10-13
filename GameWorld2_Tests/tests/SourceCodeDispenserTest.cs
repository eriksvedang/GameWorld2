using System;
using System.Text;
using NUnit.Framework;
using GameWorld2;
using RelayLib;
using System.IO;
using System.Collections.Generic;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class SourceCodeDispenserTest
	{
		[Test()]
		public void BasicUsage()
		{
			const string saveName = "SourceCodeDispenserTest.json";
			
			{
                RelayTwo relay = new RelayTwo();
				relay.CreateTable(SourceCode.TABLE_NAME);
				
				SourceCodeDispenser sourceCodeDispenser = new SourceCodeDispenser(relay);
				sourceCodeDispenser.LoadSourceCode(WorldTestHelper.INIT_DATA_PATH + "Sprak/helloworld.sprak");
				
				SourceCode s = sourceCodeDispenser.GetSourceCode("helloworld");
				Assert.AreEqual("print(\"Hello World!\")", s.content);
				
				relay.SaveAll(saveName);
			}
			
			{
                RelayTwo relay = new RelayTwo(saveName);
				SourceCodeDispenser sourceCodeDispenser = new SourceCodeDispenser(relay);
				SourceCode s = sourceCodeDispenser.GetSourceCode("helloworld");
				Assert.AreEqual("print(\"Hello World!\")", s.content);
			}
		}
	}
}

