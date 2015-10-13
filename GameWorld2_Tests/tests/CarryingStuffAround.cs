using System;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using System.Collections.Generic;
using TingTing;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class CarryingStuffAround
	{
		World _world;
		Character _eva, _adam;
		MysticalCube _cube;
		
		[SetUp()]
		public void SetUp()
		{			
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
			_adam = _world.tingRunner.GetTing("Adam") as Character;
			_eva = _world.tingRunner.GetTing("Eva") as Character;
			_cube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;
		}
		
		[Test()]
		public void MoveMysticalCube()
		{
			D.onDLog += Console.WriteLine;
			SmartWalkBehaviour.s_logger.AddListener(Console.WriteLine);
			_eva.logger.AddListener(Console.WriteLine);

			_eva.position = new WorldCoordinate("Kitchen", new IntPoint(0, 0));
			_cube.position = new WorldCoordinate("Kitchen", new IntPoint(8, 0));
			
			// First, go and pick up the cube
			_eva.WalkToTingAndInteract(_cube);
			WorldTestHelper.UpdateWorld(_world, 15f);
			Assert.AreEqual(_cube, _eva.handItem);
			Assert.AreEqual(new IntPoint(7, 0), _eva.localPoint);
			Assert.AreEqual(Direction.RIGHT, _eva.direction);
			
			// Then walk over to the left side again
			_eva.WalkTo(new WorldCoordinate("Kitchen", new IntPoint(1, 0)));
			WorldTestHelper.UpdateWorld(_world, 15f);
			Assert.AreEqual(new IntPoint(1, 0), _eva.localPoint);
			Assert.AreEqual(Direction.LEFT, _eva.direction);
			
			// Drop the cube
			_eva.DropHandItem();
			WorldTestHelper.UpdateWorld(_world, 3f);
			Assert.AreEqual(null, _eva.handItem);
			Assert.AreEqual(new IntPoint(0, 0), _cube.localPoint);
		}
		
		[Test()]
		public void PutMysticalCubeIntoInventory()
		{
			_world.roomRunner.CreateRoom<Room> ("Internet");

			_adam.position = new WorldCoordinate("Kitchen", new IntPoint(4, 3));
			_cube.position = new WorldCoordinate("Kitchen", new IntPoint(5, 3));
			
			// Hold the cube in hand
			_adam.PickUp(_cube);
			WorldTestHelper.UpdateWorld(_world, 3f);
			Assert.AreEqual(_cube, _adam.handItem);
			Assert.IsTrue(_cube.isBeingHeld);
			
			// Put it into the bag!
			_adam.PutHandItemIntoInventory();
			WorldTestHelper.UpdateWorld(_world, 3f);
			Assert.AreEqual(null, _adam.handItem);
			Assert.IsFalse(_cube.isBeingHeld);
			Assert.AreEqual(new WorldCoordinate(_adam.inventoryRoomName, IntPoint.Zero), _cube.position);
			
			// Look into the bag
			Ting[] inventoryItems = _adam.inventoryItems;
			Assert.AreEqual(1, inventoryItems.Length);
			Assert.AreEqual(_cube, inventoryItems[0]);
			
			// Take it out again
			_adam.TakeOutInventoryItem(_cube);
			WorldTestHelper.UpdateWorld(_world, 3f);
			Assert.AreEqual(_cube, _adam.handItem);
			Assert.IsTrue(_cube.isBeingHeld);
			Assert.AreEqual(_adam.position, _cube.position);
		}
		
		[Test()]
		public void PickUpTingWhenHoldingAnotherTing()
		{
			_adam.position = new WorldCoordinate("Kitchen", new IntPoint(4, 3));
			_cube.position = new WorldCoordinate("Kitchen", new IntPoint(5, 3));
			
			Drink drink = _world.tingRunner.CreateTing<Drink>("Coffee", new WorldCoordinate("Kitchen", new IntPoint(4, 4)));
			
			// Pick up the cube
			_adam.PickUp(_cube);
			WorldTestHelper.UpdateWorld(_world, 3f);
			
			// Pick up the drink
			_adam.PickUp(drink);
			WorldTestHelper.UpdateWorld(_world, 3f);
			
			Assert.AreEqual(_adam.handItem, drink);
			Assert.AreEqual(_cube.position, new WorldCoordinate(_adam.inventoryRoomName, new IntPoint(0, 0)));
			Assert.AreEqual(1, _adam.inventoryItems.Length);
		}
	}
}

