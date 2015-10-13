using System;
using NUnit.Framework;
using TingTing;
using GameWorld2;
using GameTypes;
namespace GameWorld2_Tests
{
	[TestFixture()]
	public class MimanTingTest
	{
		World _world;
		
		[SetUp]
		public void SetUp()
		{
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
		}

		[Test]
		public void GetTingBasedOnProgramName()
		{
			Computer computer = _world.tingRunner.CreateTing<Computer>("Miman Ting", new WorldCoordinate("Kitchen", IntPoint.Zero));
			computer.masterProgramName = "BlankSlate";

			foreach(var s in  _world.Preload()) {}

			computer.PrepareForBeingHacked();
			Assert.IsNotNull(computer.masterProgram);

			var fooTing = ConnectionAPI_Optimized.GetTingFromNameOrSourceCodeName(_world.tingRunner, "BlankSlate");
			Assert.AreEqual(computer, fooTing);
		}
		
		[Test]
		public void AddProgramsToProgramsArray()
		{
            MimanTing mimanTing = _world.tingRunner.CreateTing<MimanTingConcrete>("Miman Ting", new WorldCoordinate("Kitchen", IntPoint.Zero));
			SourceCode dummyCode = new SourceCode();
			dummyCode.CreateNewRelayEntry(_world.relay.GetTable(SourceCode.TABLE_NAME), typeof(SourceCode).Name);

			WorldTestHelper.PreloadWorld (_world);

			Program p1 = _world.programRunner.CreateProgram(dummyCode);
			Program p2 = _world.programRunner.CreateProgram(dummyCode);
			_world.Update (0.1f); // the programs are added to the program runner at the end of the frame

			Assert.AreEqual(0, mimanTing.programs.Length);
			mimanTing.AddProgramToProgramsArray(p1);
			Assert.AreEqual(1, mimanTing.programs.Length);
			mimanTing.AddProgramToProgramsArray(p2);
			Assert.AreEqual(2, mimanTing.programs.Length);
		}

		[Test]
		public void RegexTest()
		{
			Assert.IsTrue (System.Text.RegularExpressions.Regex.IsMatch("_", "[0-9A-Za-z\\_]"));
		}
	}
}

