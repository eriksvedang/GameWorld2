using System;
using NUnit.Framework;
using GameWorld2;
using System.Collections.Generic;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class MemoryTests
	{
		[Test()]
		public void TrySavingComputerMemory ()
		{
			string saveName = "ComputerMemoryTest.json";
			{
				WorldTestHelper.GenerateInitData ();
				InitialSaveFileCreator i = new InitialSaveFileCreator ();
				World world = new World (i.CreateRelay (WorldTestHelper.INIT_DATA_PATH));
				Memory computerMemory = world.tingRunner.CreateTing<Memory> ("ComputerMemory1", new TingTing.WorldCoordinate ("Eden", 2, 3));
				computerMemory["a"] = 42;
				computerMemory["b"] = 100.5;
				world.Save (saveName);
			}
			{
				World world = new World (saveName);
				Memory computerMemory = world.tingRunner.GetTing<Memory> ("ComputerMemory1");
				Assert.AreEqual (2, computerMemory.data.Keys.Count);
				Assert.AreEqual (42, computerMemory["a"]);
				Assert.AreEqual (100.5, computerMemory["b"]);
			}
		}
	}
}

