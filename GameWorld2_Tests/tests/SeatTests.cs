using System;
using NUnit.Framework;
using GameWorld2;
using TingTing;
using GameTypes;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class SeatTests
	{
		World _world;
		Character _adam;
		Seat _seat;
		
        [SetUp()]
        public void Setup()
        {
			D.onDLog += Console.WriteLine;
            WorldTestHelper.GenerateInitData();
            InitialSaveFileCreator i = new InitialSaveFileCreator();
            _world = new World(i.CreateRelay("../InitData1/"));
            _adam = _world.tingRunner.GetTing<Character>("Adam");
			_seat = _world.tingRunner.CreateTing<Seat>("Chair", new WorldCoordinate("Eden", 0, 3));
        }
		
		[Test()]
		public void SitDown()
		{
			_adam.position = new WorldCoordinate("Eden", 0, 0);
			_adam.WalkToTingAndInteract(_seat);
			WorldTestHelper.UpdateWorld(_world, 30f);
			
			Assert.AreEqual("", _adam.actionName);
			Assert.AreEqual(true, _adam.sitting);
		}
	}
}

