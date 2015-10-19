using System;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using System.Collections.Generic;
using TingTing;
using System.Threading;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class CharactersWalking
	{
		World _world;
		Character _eva, _adam;
		
		[SetUp()]
		public void SetUp()
		{			
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
			_adam = _world.tingRunner.GetTing("Adam") as Character;
			_eva = _world.tingRunner.GetTing("Eva") as Character;
			D.onDLog += (pMessage) => Console.WriteLine(pMessage);
		}

		World CreateWorldWithSomeRooms ()
		{
			InitialSaveFileCreator isc = new InitialSaveFileCreator();
			World world = new World(isc.CreateEmptyRelay());
			
			SimpleRoomBuilder srb = new SimpleRoomBuilder(world.roomRunner);
			srb.CreateRoomWithSize("Eden", 10, 10);
			var middleRoom = srb.CreateRoomWithSize("MiddleRoom", 10, 10);
			var wrongRoom = srb.CreateRoomWithSize("WrongRoom", 10, 10);
			var distantRoom = srb.CreateRoomWithSize("DistantRoom", 10, 10);
			
			middleRoom.worldPosition = new IntPoint(10, 0);
			wrongRoom.worldPosition = new IntPoint(30, 0);
			distantRoom.worldPosition = new IntPoint(50, 0);
						
			// Add doors
			var edenDoor = world.tingRunner.CreateTing<Door>("edenDoor", new WorldCoordinate("Eden", new IntPoint(4, 4)));
			var middleRoomDoor1 = world.tingRunner.CreateTing<Door>("middleRoomDoor1", new WorldCoordinate("MiddleRoom", new IntPoint(2, 4)));
			var middleRoomDoor2 = world.tingRunner.CreateTing<Door>("middleRoomDoor2", new WorldCoordinate("MiddleRoom", new IntPoint(7, 4)));
			var middleRoomDoor3 = world.tingRunner.CreateTing<Door>("middleRoomDoor3", new WorldCoordinate("MiddleRoom", new IntPoint(4, 4)));
			var wrongRoomDoor = world.tingRunner.CreateTing<Door>("wrongRoomDoor", new WorldCoordinate("WrongRoom", new IntPoint(4, 4)));
			var distantRoomDoor = world.tingRunner.CreateTing<Door>("distantRoomDoor", new WorldCoordinate("DistantRoom", new IntPoint(2, 4)));
			
			edenDoor.targetDoorName = "middleRoomDoor1";
			middleRoomDoor1.targetDoorName = "edenDoor";
			middleRoomDoor2.targetDoorName = "distantRoomDoor";
			middleRoomDoor3.targetDoorName = "wrongRoomDoor";
			wrongRoomDoor.targetDoorName = "middleRoomDoor3";
			distantRoomDoor.targetDoorName = "middleRoomDoor2";

			var distantTile = new PointTileNode(new IntPoint(200, 200), wrongRoom);
			distantTile.group = 1;
			wrongRoom.AddTile(distantTile);

			var wrongRoomUnattachedDoor = world.tingRunner.CreateTing<Door>("wrongRoomUnattachedDoor", new WorldCoordinate("WrongRoom", new IntPoint(200, 200)));
			wrongRoomUnattachedDoor.targetDoorName = "edenDoor";
			
			world.sourceCodeDispenser.CreateSourceCodeFromString ("OnDoorUsed", "");

			if(!world.isReadyToPlay) {
				foreach (string s in world.Preload()) {}
			}

			return world;
		}

		[Test()]
		public void BuildRoomNetwork() {

			var world = CreateWorldWithSomeRooms();
			var pathfinder = new MimanPathfinder2(world.tingRunner, world.roomRunner);

			var network = pathfinder.RecreateRoomNetwork();
			Console.WriteLine(network.ToString());

			var eden = world.roomRunner.GetRoom("Eden");
			var mid = world.roomRunner.GetRoom("MiddleRoom");
			var wrong = world.roomRunner.GetRoom("WrongRoom");
			var dist = world.roomRunner.GetRoom("DistantRoom");

			var edenDoor = world.tingRunner.GetTing<Door>("edenDoor");
			var middleRoomDoor1 = world.tingRunner.GetTing<Door>("middleRoomDoor1");
			var middleRoomDoor2 = world.tingRunner.GetTing<Door>("middleRoomDoor2");
			var middleRoomDoor3 = world.tingRunner.GetTing<Door>("middleRoomDoor3");
			var wrongRoomDoor = world.tingRunner.GetTing<Door>("wrongRoomDoor");
			var wrongRoomUnattachedDoor = world.tingRunner.GetTing<Door>("wrongRoomUnattachedDoor");
			var distantRoomDoor = world.tingRunner.GetTing<Door>("distantRoomDoor");

			Assert.AreEqual(new Dictionary<RoomGroup,Ting>() {{new RoomGroup(mid, 0), edenDoor}}, network.linkedRoomGroups[new RoomGroup(eden, 0)]);
			Assert.AreEqual(new Dictionary<RoomGroup,Ting>() 
				{
				{new RoomGroup(eden, 0), middleRoomDoor1}, 
					{new RoomGroup(wrong, 0), middleRoomDoor3}, // ORDERED?! wtf
					{new RoomGroup(dist, 0), middleRoomDoor2}, 
				
				}, 
				network.linkedRoomGroups[new RoomGroup(mid, 0)]);
			Assert.AreEqual(new Dictionary<RoomGroup,Ting>() {{new RoomGroup(mid, 0), wrongRoomDoor}}, network.linkedRoomGroups[new RoomGroup(wrong, 0)]);
			Assert.AreEqual(new Dictionary<RoomGroup,Ting>() {{new RoomGroup(eden, 0), wrongRoomUnattachedDoor}}, network.linkedRoomGroups[new RoomGroup(wrong, 1)]);
			Assert.AreEqual(new Dictionary<RoomGroup,Ting>() {{new RoomGroup(mid, 0), distantRoomDoor}}, network.linkedRoomGroups[new RoomGroup(dist, 0)]);
		}

		[Test()]
		public void SimplePathfinding()
		{
			var world = CreateWorldWithSomeRooms();
									
			var pathfinder = new MimanPathfinder2(world.tingRunner, world.roomRunner);

			var network = pathfinder.RecreateRoomNetwork();
			Console.WriteLine(network.ToString());

			var p1 = world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("Eden", new IntPoint(2, 4)));
			var p2 = world.tingRunner.CreateTing<Point>("p2", new WorldCoordinate("DistantRoom", new IntPoint(4, 4)));
			
			var result = pathfinder.Search(p1, p2);
			Console.WriteLine("Result: " + result);

			Assert.AreEqual(MimanPathStatus.FOUND_GOAL, result.status);
			Assert.AreEqual(2, result.tings.Length);
			Assert.AreEqual("edenDoor", result.tings[0].name);
			Assert.AreEqual("middleRoomDoor2", result.tings[1].name);
		}

		[Test()]
		public void LoopedDoorsPathfinding()
		{
			var world = CreateWorldWithSomeRooms();

			var edenDoor = world.tingRunner.GetTing<Door>("edenDoor");
			var middleRoomDoor1 = world.tingRunner.GetTing<Door>("middleRoomDoor1");
			var middleRoomDoor2 = world.tingRunner.GetTing<Door>("middleRoomDoor2");
			var middleRoomDoor3 = world.tingRunner.GetTing<Door>("middleRoomDoor3");
			var wrongRoomDoor = world.tingRunner.GetTing<Door>("wrongRoomDoor");
			var distantRoomDoor = world.tingRunner.GetTing<Door>("distantRoomDoor");

			//         EDEN <-> MIDDLE 1    MIDDLE 2 <-> WRONG
			//  DISTANT      <- MIDDLE 3

			edenDoor.targetDoorName = middleRoomDoor1.name;
			middleRoomDoor1.targetDoorName = middleRoomDoor2.name;
			middleRoomDoor2.targetDoorName = wrongRoomDoor.name;
			middleRoomDoor3.targetDoorName = distantRoomDoor.name;
			wrongRoomDoor.targetDoorName = edenDoor.name;

			var eden = world.roomRunner.GetRoom("Eden");
			var mid = world.roomRunner.GetRoom("MiddleRoom");
			var wrong = world.roomRunner.GetRoom("WrongRoom");
			var dist = world.roomRunner.GetRoom("DistantRoom");

			eden.worldPosition = new IntPoint(0,0);
			mid.worldPosition = new IntPoint(10,0);
			wrong.worldPosition = new IntPoint(10,0);
			dist.worldPosition = new IntPoint(-2000,0);

			var pathfinder = new MimanPathfinder2(world.tingRunner, world.roomRunner);
			var network = pathfinder.RecreateRoomNetwork();
			Console.WriteLine(network.ToString());
						
			var p1 = world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("Eden", new IntPoint(2, 4)));
			var p2 = world.tingRunner.CreateTing<Point>("p2", new WorldCoordinate("DistantRoom", new IntPoint(4, 4)));
			
			var result = pathfinder.Search(p1, p2);
			Console.WriteLine("Result: " + result);
			
			Assert.AreEqual(MimanPathStatus.FOUND_GOAL, result.status);
			Assert.AreEqual(2, result.tings.Length);
			Assert.AreEqual("edenDoor", result.tings[0].name);
			Assert.AreEqual("middleRoomDoor3", result.tings[1].name);
		}
		
		[Test()]
		public void AdamWalksThroughHallway()
		{   
            _adam.position = new WorldCoordinate( "Hallway", new IntPoint(0, 0));
			_adam.WalkTo(new WorldCoordinate("Hallway", new IntPoint(4, 4)));

			WorldTestHelper.UpdateWorld(_world, 1f);
			Assert.AreEqual("Walking", _adam.actionName);

			//Thread.Sleep(200);
			WorldTestHelper.UpdateWorld(_world, 10f);
			//Thread.Sleep(500);

			Assert.AreEqual("", _adam.actionName);
			Assert.AreEqual(new IntPoint(4, 4), _adam.localPoint);
		}
		
		[Test()]
		public void AdamWalksAndPicksUpDrink()
		{

            Drink drink = _world.tingRunner.CreateTing<Drink>("Cola", new TingTing.WorldCoordinate("Hallway", new IntPoint(3, 3)));
		
			_adam.position = new WorldCoordinate("Hallway", IntPoint.Zero);
			_adam.WalkToTingAndInteract(drink);
			WorldTestHelper.UpdateWorld(_world, 1f);
			Assert.AreEqual("Walking", _adam.actionName);

			WorldTestHelper.UpdateWorld(_world, 10f);
			Assert.AreEqual("", _adam.actionName);
			Assert.AreEqual(drink, _adam.handItem);
			Assert.IsTrue(drink.isBeingHeld);
		}  

		[Test()]
		public void LoopedDoorsPathfindingLoopInStartRoom()
		{
			var world = CreateWorldWithSomeRooms();
			
			var edenDoor = world.tingRunner.GetTing<Door>("edenDoor");
			var middleRoomDoor1 = world.tingRunner.GetTing<Door>("middleRoomDoor1");
			var middleRoomDoor2 = world.tingRunner.GetTing<Door>("middleRoomDoor2");
			var middleRoomDoor3 = world.tingRunner.GetTing<Door>("middleRoomDoor3");
			var wrongRoomDoor = world.tingRunner.GetTing<Door>("wrongRoomDoor");
			//var distantRoomDoor = world.tingRunner.GetTing<Door>("distantRoomDoor");
		
			
			middleRoomDoor1.targetDoorName = middleRoomDoor2.name;
			middleRoomDoor2.targetDoorName = wrongRoomDoor.name;
			middleRoomDoor3.targetDoorName = middleRoomDoor2.name;
			wrongRoomDoor.targetDoorName = edenDoor.name;
			
			var eden = world.roomRunner.GetRoom("Eden");
			var mid = world.roomRunner.GetRoom("MiddleRoom");
			var wrong = world.roomRunner.GetRoom("WrongRoom");
			var dist = world.roomRunner.GetRoom("DistantRoom");
			
			mid.worldPosition = new IntPoint(0,0);
			wrong.worldPosition = new IntPoint(100,0);
			eden.worldPosition = new IntPoint(-200,0);
			dist.worldPosition = new IntPoint(200,0);
			
			var pathfinder = new MimanPathfinder2(world.tingRunner, world.roomRunner);
			var network = pathfinder.RecreateRoomNetwork();
			Console.WriteLine(network.ToString());
			
			var p1 = world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("MiddleRoom", new IntPoint(2, 4)));
			var p2 = world.tingRunner.CreateTing<Point>("p2", new WorldCoordinate("Eden", new IntPoint(4, 4)));
			
			var result = pathfinder.Search(p1, p2);
			Console.WriteLine("Result: " + result);
			
			Assert.AreEqual(MimanPathStatus.FOUND_GOAL, result.status);
			Assert.AreEqual(2, result.tings.Length);
			Assert.AreEqual("middleRoomDoor2", result.tings[0].name);
			Assert.AreEqual("wrongRoomDoor", result.tings[1].name);
		}

		[Test()]
		public void EvaWalksWithSaveInTheMiddle()
		{
			const string saveName = "EvaWalksWithSaveInTheMiddle.json";
			
			// These variables are for checking that everything stays the same after loading the save
			IntPoint midPoint = new IntPoint(-1, -1);
			float walkTimerWhenSaving;
			int walkIteratorWhenSaving;
			
			{
                _eva.position = new WorldCoordinate("Hallway", IntPoint.Zero);
				_eva.walkSpeed = 1.0f; // 1 tile per second
				_eva.WalkTo(new WorldCoordinate("Hallway", new IntPoint(4, 4)));

				WorldTestHelper.UpdateWorld(_world, 1f);
				Assert.AreEqual("Walking", _eva.actionName);
				
				WorldTestHelper.UpdateWorld(_world, 2.5f);
				
				midPoint = _eva.localPoint;
				walkTimerWhenSaving = _eva.walkTimer;
				walkIteratorWhenSaving = _eva.walkIterator;
				
				_world.Save(saveName);
				_world = null;
			}
			
			{
				World freshWorld = new World(saveName);
				
				_eva = freshWorld.tingRunner.GetTing("Eva") as Character;
				Assert.AreEqual(midPoint, _eva.localPoint);
				Assert.AreEqual(walkTimerWhenSaving, _eva.walkTimer);
				Assert.AreEqual(walkIteratorWhenSaving, _eva.walkIterator);

				WorldTestHelper.UpdateWorld(freshWorld, 5.0f);
				Assert.AreEqual(new IntPoint(4, 4), _eva.localPoint);
				Assert.AreEqual("", _eva.actionName);
			}
		}
		
		[Test()]
		public void ReachingTheRightPoint()
		{
			_eva.position = new WorldCoordinate("Kitchen", new IntPoint(9, 4)); // lower right corner
			_eva.direction = Direction.UP;
			
			_adam.position = new WorldCoordinate("Kitchen", new IntPoint(2, 1)); // in the upper left corner
			_adam.direction = Direction.LEFT;
			
			_eva.WalkToTingAndInteract(_adam);
			WorldTestHelper.UpdateWorld(_world, 10f);
			
			Assert.AreEqual(new IntPoint(2, 4), _eva.position.localPosition); // outer left side of the room
			Assert.AreEqual(new IntPoint(2, 1), _adam.position.localPosition); // still there

			//Assert.AreEqual(Direction.LEFT, _adam.direction); // should change or not?
		}
		
		[Test()]
		public void NoValidPath()
		{
			const string unreachableRoomName = "UnreachableRoom";
			
			SimpleRoomBuilder srb = new SimpleRoomBuilder(_world.roomRunner);
			srb.CreateRoomWithSize(unreachableRoomName, 10, 10);
			
			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path
			_eva.position = new WorldCoordinate("Eden", new IntPoint(2, 3));
			_eva.WalkTo(new WorldCoordinate(unreachableRoomName, new IntPoint(5, 7)));
			
			WorldTestHelper.UpdateWorld(_world, 1f);
			Assert.AreEqual("", _eva.actionName);
			Assert.AreEqual(new WorldCoordinate("Eden", new IntPoint(2, 3)), _eva.position);
		}

		[Test()]
		public void WalkFromInvalidPointInSameRoom()
		{
			foreach(var s in _world.Preload()) {}

			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path
			_eva.logger.AddListener(Console.WriteLine);
			_eva.position = new WorldCoordinate("Eden", new IntPoint(-200, 300));
			_eva.WalkTo(new WorldCoordinate("Eden", new IntPoint(1, 2)));

			// Since the starting tile is invalid (and null) the character will try to walk from the closest tile which should be in the lower left corner
			// The character seems to move to the next tile (0, 3) immediately though so that's where it should (?) be found
			WorldTestHelper.UpdateWorld(_world, 0.25f);


			// TODO !!! This test doesn't work anymore since the strategy to just choose the closest tile wasn't very good and now a random technique is used :(
			//Assert.AreEqual(new WorldCoordinate("Eden", new IntPoint(0, 3)), _eva.position);

			var room = _world.roomRunner.GetRoom("Eden");
			Console.WriteLine(room.GetTile(new IntPoint(-200, 300)));
			Console.WriteLine(room.GetTile(new IntPoint(1, 2)));

			WorldTestHelper.UpdateWorld(_world, 5f);
			Assert.AreEqual(new WorldCoordinate("Eden", new IntPoint(1, 2)), _eva.position);
		}

		[Test()]
		public void WalkToInvalidPointInSameRoom()
		{
			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path
			_eva.logger.AddListener(Console.WriteLine);
			_eva.position = new WorldCoordinate("Eden", new IntPoint(2, 4));
			_eva.WalkTo(new WorldCoordinate("Eden", new IntPoint(300, 300)));
			
			WorldTestHelper.UpdateWorld(_world, 5f);
			Assert.AreEqual("", _eva.actionName);
			Assert.AreEqual(new WorldCoordinate("Eden", new IntPoint(4, 4)), _eva.position);
		}

		/*
		[Test()]
		public void LimitNumberOfPathFindingSearches()
		{
			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path

			int nrOfFailedSearches = 0;

			Room unreachableRoom = _world.roomRunner.CreateRoom<Room> ("UnreachableRoom");
			unreachableRoom.AddTile(new PointTileNode(new IntPoint(0, 0), unreachableRoom));
			
			_eva.logger.AddListener(Console.WriteLine);
			OLD_CharacterWalkBehaviour.logger.AddListener(s => {
				if(s.Contains("DESTINATION_UNREACHABLE")) {
					nrOfFailedSearches++;
				}
			});

			_eva.position = new WorldCoordinate("Eden", new IntPoint(2, 4));

			WorldTestHelper.PreloadWorld(_world);

			float dt = 0.25f; 
			for(int i = 0; i < 160; i++) {
				// Tries to go to invalid point four times each second, for ten seconds
				_eva.WalkTo(new WorldCoordinate("UnreachableRoom", new IntPoint(0, 0)));
				_world.Update(dt);
			}

			Assert.AreEqual(4, nrOfFailedSearches); // tries to pathfind again every third second
		}
		*/

		[Test()]
		public void KeepTryingToGoToLocation()
		{
			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path

			_eva.logger.AddListener(Console.WriteLine);
			_eva.position = new WorldCoordinate("Eden", new IntPoint(2, 4));

			WorldTestHelper.PreloadWorld(_world);

			float dt = 0.25f; 
			for(int i = 0; i < 40; i++) {
				// She will reach the corner destination in the room (4, 4) eventually
				_eva.WalkTo(new WorldCoordinate("Eden", new IntPoint(200, 300)));
				_world.Update(dt);
			}
		}
		
		[Test()]
		public void PathfindToPointYoureAlreadyOn()
		{
			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path
			_eva.logger.AddListener(Console.WriteLine);
			
			var p1 = _world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("Eden", new IntPoint(2, 4)));
			
			_eva.position = p1.position;
			_eva.WalkToTingAndInteract(p1);
			
			WorldTestHelper.UpdateWorld(_world, 2f);
			
			// Don't know how to test this but it shouldn't generate a ton of output at least...
		}

		[Test()]
		public void PathfindToOtherRoom()
		{
			MimanPathfinder2.ClearRoomNetwork();

			//D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path
			_eva.logger.AddListener(Console.WriteLine);
			
			SimpleRoomBuilder srb = new SimpleRoomBuilder(_world.roomRunner);
			srb.CreateRoomWithSize("DistantRoom", 10, 10);
			
			var p1 = _world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("Eden", new IntPoint(2, 4)));
			var p2 = _world.tingRunner.CreateTing<Point>("p2", new WorldCoordinate("DistantRoom", new IntPoint(4, 4)));

			// Add doors
			var door1 = _world.tingRunner.CreateTing<Door>("Door1", new WorldCoordinate("Eden", new IntPoint(4, 4)));
			_world.tingRunner.CreateTing<Door>("Door2", new WorldCoordinate("DistantRoom", new IntPoint(3, 4)));
			door1.targetDoorName = "Door2";

			var pathfinder = new MimanPathfinder2(_world.tingRunner, _world.roomRunner);

			var result = pathfinder.Search(p1, p2);
			Assert.AreEqual(MimanPathStatus.FOUND_GOAL, result.status);
			Assert.AreEqual(1, result.tings.Length);
			Assert.AreEqual(door1, result.tings[0]);

			Console.WriteLine("RESULT: " + result.ToString());

			//var result2 = pathfinder.Search(p1, p2);
			//Assert.AreEqual(MimanPathStatus.FOUND_GOAL, result2.status);
		}

		[Test()]
		public void PathfindThroughElevator()
		{
			_eva.logger.AddListener(Console.WriteLine);

			SimpleRoomBuilder srb = new SimpleRoomBuilder(_world.roomRunner);
			srb.CreateRoomWithSize("DistantRoom", 10, 10);
			srb.CreateRoomWithSize("ElevatorRoom", 10, 10);

			// Add doors
			var elevatorDoor1 = _world.tingRunner.CreateTing<Door>("ElevatorDoor1", new WorldCoordinate("Eden", new IntPoint(4, 4)));
			var elevatorDoor2 = _world.tingRunner.CreateTing<Door>("ElevatorDoor2", new WorldCoordinate("DistantRoom", new IntPoint(3, 4)));
			var elevatorRoomDoor = _world.tingRunner.CreateTing<Door>("ElevatorRoomDoor", new WorldCoordinate("ElevatorRoom", new IntPoint(4, 4)));

			var p1 = _world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("Eden", new IntPoint(2, 4)));
			var p2 = _world.tingRunner.CreateTing<Point>("p2", new WorldCoordinate("DistantRoom", new IntPoint(4, 4)));

			var pathfinder = new MimanPathfinder2(_world.tingRunner, _world.roomRunner);
			var result = pathfinder.Search(p1, p2);

			Console.WriteLine("Network at first:");
			var network = pathfinder.RecreateRoomNetwork();
			Console.WriteLine(network.ToString());

			Assert.AreEqual(MimanPathStatus.NO_PATH_FOUND, result.status);

			elevatorDoor1.targetDoorName = "ElevatorRoomDoor";
			elevatorDoor2.targetDoorName = "ElevatorRoomDoor";

			elevatorRoomDoor.elevatorAlternatives = new string[] { "ElevatorDoor1", "ElevatorDoor2" };
			elevatorRoomDoor.elevatorFloor = 0;

			Console.WriteLine("Network after adding elevator alternatives:");
			var network2 = pathfinder.RecreateRoomNetwork();
			Console.WriteLine(network2.ToString());

			Console.WriteLine(" - second search - ");
			var result2 = pathfinder.Search(p1, p2);

			Assert.AreEqual(MimanPathStatus.FOUND_GOAL, result2.status);
		}
	}
}

