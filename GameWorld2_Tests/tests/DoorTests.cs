using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using Pathfinding;
using TingTing;
namespace GameWorld2_Tests
{
    [TestFixture]
    public class DoorTests
    {
        World _world;
        Door _d1, _d2, _d3, _d4;
        Character _adam, _eve;
		
        [SetUp()]
        public void Setup()
        {
			D.onDLog += Console.WriteLine;
			SmartWalkBehaviour.s_logger.AddListener (s => Console.WriteLine ("Walk behaviour: " + s));

            WorldTestHelper.GenerateInitData();
            InitialSaveFileCreator i = new InitialSaveFileCreator();
            _world = new World(i.CreateRelay("../InitData1/"));
            foreach (string s in _world.Preload()) ;
            
			_d1 = _world.tingRunner.CreateTing<Door>("DoorOne", new TingTing.WorldCoordinate("Eden", new IntPoint(4, 4)), Direction.LEFT);
            _d2 = _world.tingRunner.CreateTing<Door>("DoorTwo", new TingTing.WorldCoordinate("Hallway", new IntPoint(0, 0)), Direction.RIGHT);
            _d1.targetDoorName = _d2.name;
            _d2.targetDoorName = _d1.name;
			
			_d3 = _world.tingRunner.CreateTing<Door>("DoorThree", new TingTing.WorldCoordinate("Hallway", new IntPoint(4, 2)), Direction.LEFT);
            _d4 = _world.tingRunner.CreateTing<Door>("DoorFour", new TingTing.WorldCoordinate("Kitchen", new IntPoint(0, 2)), Direction.RIGHT);
            _d3.targetDoorName = _d4.name;
            _d4.targetDoorName = _d3.name;
			
            _world.roomRunner.GetRoom("Hallway").worldPosition = new IntPoint(4, 4);
            _adam = _world.tingRunner.GetTing<Character>("Adam");
            _eve = _world.tingRunner.GetTing<Character>("Eva");
        }

		[Test]
        public void UseDoor()
		{			
			_eve.logger.AddListener(Console.WriteLine);
            _eve.position = new WorldCoordinate("Eden", 0, 0);

			Room r = _eve.room;
			foreach (var t in r.tiles) {
				D.Log (t.ToString () + ": " + string.Join(", ", t.GetOccupants().Select(o => o.ToString()).ToArray()));
			}

			_eve.WalkToTingAndInteract(_d1);			
			WorldTestHelper.UpdateWorld(_world, 10f);
			Assert.AreEqual(new WorldCoordinate("Hallway", 3, 0), _eve.position);
			
			_eve.WalkTo(new WorldCoordinate("Hallway", 1, 1));
			WorldTestHelper.UpdateWorld(_world, 10f);
			Assert.AreEqual(new WorldCoordinate("Hallway", 1, 1), _eve.position); 
			
			_eve.WalkToTingAndInteract(_d2);			
			WorldTestHelper.UpdateWorld(_world, 10f);
			Assert.AreEqual(new WorldCoordinate("Eden", 1, 4), _eve.position);
        }
		
		[Test]
        public void WalkBetweenRoomsUsingDoorOnTheWay()
		{	
			_adam.logger.AddListener(Console.WriteLine);
            _adam.position = new WorldCoordinate("Eden", 0, 4);
			_adam.walkSpeed = 1f; // 1 tile / second

			SmartWalkBehaviour.s_logger.AddListener(Console.WriteLine);
			
			_adam.WalkTo(new WorldCoordinate("Hallway", 0, 3));
			WorldTestHelper.UpdateWorld(_world, 3.2f);
			
			Assert.AreEqual("WalkingThroughDoor", _adam.actionName);
			WorldTestHelper.UpdateWorld(_world, 10f);			
			
			Assert.AreEqual(new WorldCoordinate("Hallway", 0, 3), _adam.position);
        }
		
		[Test]
        public void WalkThroughSeveralRoomsUsingDoorsOnTheWay()
		{			
			MimanPathfinder2.ClearRoomNetwork();

			_adam.logger.AddListener(Console.WriteLine);
            _adam.position = new WorldCoordinate("Eden", 0, 4);
			
			_adam.WalkTo(new WorldCoordinate("Kitchen", 3, 3));
			WorldTestHelper.UpdateWorld(_world, 20f);
			Assert.AreEqual(new WorldCoordinate("Kitchen", 3, 3), _adam.position);
        }
		
		[Test]
        public void WalkThroughSeveralRoomsToInteractWithSomethingAtTheEnd()
		{			
			MimanPathfinder2.ClearRoomNetwork();

			_adam.logger.AddListener(Console.WriteLine);
			SmartWalkBehaviour.s_logger.AddListener(Console.WriteLine);

			/*
			_adam.AddDataListener<Character.WalkMode>("walkMode", ((Character.WalkMode prev, Character.WalkMode newWalkMode) => {
				//throw new Exception("NEW WALK MODE: " + newWalkMode);
				Console.WriteLine("NEW WALK MODE: " + newWalkMode);
			}));
			*/

            _adam.position = new WorldCoordinate("Eden", 0, 4);
			
			MysticalCube cube = _world.tingRunner.GetTing<MysticalCube>("PowerCube");
			cube.position = new WorldCoordinate("Kitchen", 3, 3);
			
			_adam.WalkToTingAndInteract(cube);
			WorldTestHelper.UpdateWorld(_world, 60f);
			
			Assert.AreEqual(cube, _adam.handItem);
        }
    }
}
