using System;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using System.Collections.Generic;
using TingTing;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class BedTests
	{	
		World _world;
		Room _room;

		[SetUp()]
		public void SetUp()
		{			
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
			_room = _world.roomRunner.CreateRoom<Room>("Room");

		}
		
		[Test()]
		public void BedInteractionPointLeft()
		{
			D.onDLog += (pMessage) => Console.WriteLine("DLOG: " + pMessage);

			_room.SetTiles(new List<IntPoint>() {
				new IntPoint( 0, 0),
				new IntPoint(-2, 0),
			});
			_room.ApplyTileData();
			
			_room.GetTile(new IntPoint( 0, 0)).group = 1; // bed
			_room.GetTile(new IntPoint(-2, 0)).group = 2; // interaction point

			var bed = _world.tingRunner.CreateTing<Bed>("Bed", new WorldCoordinate("Room", new IntPoint(0,0)));
			bed.direction = Direction.LEFT;
			bed.MaybeFixGroupIfOutsideIslandOfTiles();

			Assert.AreEqual(2, _room.GetTile(new IntPoint(0, 0)).group); // bed:s tile should change
			Assert.AreEqual(2, _room.GetTile(new IntPoint(-2, 0)).group); // interaction point tile should stay the same (it's connected to the rest of the level)
		}

		[Test()]
		public void BedInteractionPointRight()
		{
			D.onDLog += (pMessage) => Console.WriteLine("DLOG: " + pMessage);
			
			_room.SetTiles(new List<IntPoint>() {
				new IntPoint( 0, 0),
				new IntPoint( 2, 0),
			});
			_room.ApplyTileData();
			
			_room.GetTile(new IntPoint(0, 0)).group = 1; // bed
			_room.GetTile(new IntPoint(2, 0)).group = 2; // interaction point
			
			var bed = _world.tingRunner.CreateTing<Bed>("Bed", new WorldCoordinate("Room", new IntPoint(0,0)));
			bed.direction = Direction.LEFT;
			bed.MaybeFixGroupIfOutsideIslandOfTiles();
			
			Assert.AreEqual(2, _room.GetTile(new IntPoint(0, 0)).group); // bed:s tile should change
			Assert.AreEqual(2, _room.GetTile(new IntPoint(2, 0)).group); // interaction point tile should stay the same (it's connected to the rest of the level)
		}
	}
}

