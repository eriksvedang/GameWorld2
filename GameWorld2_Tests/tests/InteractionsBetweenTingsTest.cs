using System;
using NUnit.Framework;
using GameWorld2;
using TingTing;
using GameTypes;

namespace GameWorld2_Tests
{
	[TestFixture]
	public class InteractionsBetweenTingsTest
	{
		const string TEST_ROOM = "Eden";
		const float PRECISION = 0.01f;
		
		World _world;
        
		[SetUp()]
		public void SetUp()
		{			
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
		}
		
		[Test]
		public void CharacterInteractsWithDrink()
		{
            Character lisa = _world.tingRunner.CreateTing<Character>("Lisa", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
            Drink coke = _world.tingRunner.CreateTing<Drink>("Coke", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));			
			Assert.IsTrue(lisa.CanInteractWith(coke));

            WorldTestHelper.UpdateWorld(_world, 0.3f);
			lisa.InteractWith(coke);
			Assert.AreEqual("Drinking", lisa.actionName);
			Assert.AreEqual(0.3f, lisa.actionStartTime, PRECISION);
			Assert.AreEqual(1.8f, lisa.actionTriggerTime, PRECISION);
			Assert.AreEqual(3.6f, lisa.actionEndTime, PRECISION);
			Assert.AreEqual(false, lisa.actionHasFired);
			
			WorldTestHelper.UpdateWorld(_world, 5.0f);
			Assert.AreEqual(90f, coke.amount, PRECISION);
			Assert.AreEqual(true, lisa.actionHasFired);

            WorldTestHelper.UpdateWorld(_world, 5.0f);
			Assert.AreEqual(90f, coke.amount, PRECISION);
			Assert.AreEqual("", lisa.actionName);
		}

		[Test]
		public void CreateDrinksAutomaticallyButWithLimit()
		{
			D.onDLog += Console.WriteLine;

			Character lisa = _world.tingRunner.CreateTing<Character>("Lisa", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
						
			WorldTestHelper.UpdateWorld(_world, 3f);

			Drink lastDrink = null;
			for(int i = 0; i < 100; i++) {
				lastDrink = Behaviour_Sell.CreateTingToSell(lisa, _world.tingRunner, "WellspringSoda", _world.settings) as Drink;
			}			

			WorldTestHelper.UpdateWorld(_world, 3f);

			Assert.AreEqual(21, _world.tingRunner.GetTingsOfType<Drink>().Length);

			lisa.handItem = lastDrink;

			for(int i = 0; i < 100; i++) {
				Behaviour_Sell.CreateTingToSell(lisa, _world.tingRunner, "WellspringSoda", _world.settings);
			}
			
			WorldTestHelper.UpdateWorld(_world, 3f);

			Assert.AreEqual(25, _world.tingRunner.GetTingsOfType<Drink>().Length); // creates one extra drink each time the one Lisa holds is found when trying to create _sale_ drink
		}
		
		[Test]
		public void CharacterUsesKeyOnDoor()
		{
            Character sune = _world.tingRunner.CreateTing<Character>("Sune", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
            Door door1 = _world.tingRunner.CreateTing<Door>("Door1", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
			Door door2 = _world.tingRunner.CreateTing<Door>("Door2", new WorldCoordinate(TEST_ROOM, new IntPoint(2, 2)));
			door1.targetDoorName = "Door2";
			door2.targetDoorName = "Door1";
            Key key = _world.tingRunner.CreateTing<Key>("Key", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
			Assert.IsTrue(sune.CanInteractWith(door1));
			
			sune.InteractWith(door1);
			Assert.AreEqual("WalkingThroughDoor", sune.actionName);
			
			sune.StopAction();
			Assert.AreEqual("", sune.actionName);
			
			sune.PickUp(key);
			Assert.IsNull(sune.handItem);
			Assert.AreEqual("PickingUp", sune.actionName);
			
            WorldTestHelper.UpdateWorld(_world, 2.0f);
			Assert.IsNotNull(sune.handItem);
			Assert.AreEqual("Key", sune.handItem.name);
			
			sune.UseHandItemToInteractWith(door1);
			Assert.AreEqual("UsingDoorWithKey", sune.actionName);
            
			WorldTestHelper.UpdateWorld(_world, 2.0f);
			Assert.AreEqual("", sune.actionName);
		}
		
		[Test]
		public void CharacterInteractsWithBed()
		{
			D.onDLog += Console.WriteLine;

			Character bert = _world.tingRunner.CreateTing<Character>("Bert", new WorldCoordinate(TEST_ROOM, new IntPoint(1, 2)));
			Bed bed = _world.tingRunner.CreateTing<Bed>("Bed", new WorldCoordinate(TEST_ROOM, new IntPoint(3, 2)));
			Assert.IsTrue(bert.CanInteractWith(bed));

			bert.sleepiness = 100.0f;
			bert.InteractWith(bed);
            WorldTestHelper.UpdateWorld(_world, 1.0f);
			Assert.AreEqual("LayingDown", bert.actionName);

            WorldTestHelper.UpdateWorld(_world, 4.0f);
			Assert.IsTrue(bert.laying);
			Assert.AreNotEqual("Sleeping", bert.actionName);
			WorldTestHelper.UpdateWorld(_world, 10f);
			Assert.AreEqual("Sleeping", bert.actionName);
		}
        /*
        [Test]
        public void CharacterHacksHandItem()
        {
            Character sune = _world.tingRunner.CreateTing<Character>("Sune", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
            Key key = _world.tingRunner.CreateTing<Key>("Key", new WorldCoordinate(TEST_ROOM, IntPoint.Zero));
            bool gotEvent = false;
            sune.AddActionListener( (a,b,c) => gotEvent = true, Ting.ActionEvent.START);
            sune.handItem = key;
            sune.Hack(sune.handItem);
            Assert.IsTrue(gotEvent);
        }
		*/
		[Test]
		public void CharacterInteractsWithSeat()
		{
            Character frida = _world.tingRunner.CreateTing<Character>("Frida", new WorldCoordinate(TEST_ROOM, new IntPoint(0, 1)));
            Seat fancyChair = _world.tingRunner.CreateTing<Seat>("Fancy Chair", new WorldCoordinate(TEST_ROOM, new IntPoint(0, 2)));
			Assert.IsTrue(frida.CanInteractWith(fancyChair));

			frida.logger.AddListener(Console.WriteLine);
			
			frida.InteractWith(fancyChair);
            WorldTestHelper.UpdateWorld(_world, 0.5f);
			Assert.AreEqual("GettingSeated", frida.actionName);

            WorldTestHelper.UpdateWorld(_world, 2.5f);
			Assert.AreEqual("GettingSeated", frida.actionName);
		}
		
		[Test]
		public void CharacterInteractsWithHandItem()
		{
            Character adam = _world.tingRunner.GetTing("Adam") as Character;
            MysticalCube powerCube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;			
			Assert.AreEqual(0, powerCube.mysteryLevel);
			
			adam.PickUp(powerCube);
            WorldTestHelper.UpdateWorld(_world, 3.0f);
			Assert.AreEqual(powerCube, adam.handItem);
			
			adam.InteractWith(powerCube);
            WorldTestHelper.UpdateWorld(_world, 3.0f);
			Assert.AreEqual(100, powerCube.mysteryLevel);
			
			adam.InteractWith(powerCube);
            WorldTestHelper.UpdateWorld(_world, 3.0f);
			Assert.AreEqual(200, powerCube.mysteryLevel);
		}
		
		[Test]
		public void CharacterInteractsWithCubeToChangeItsColor()
		{
            Character adam = _world.tingRunner.GetTing("Adam") as Character;
            MysticalCube powerCube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;			
			powerCube.onInteractionSourceCodeName = "CubeColorSetter";
			
			Assert.AreEqual(0f, powerCube.color.x, 0.05f);
			Assert.AreEqual(0f, powerCube.color.y, 0.05f);
			Assert.AreEqual(0f, powerCube.color.z, 0.05f);
			
			adam.PickUp(powerCube);
            WorldTestHelper.UpdateWorld(_world, 3.0f);
			
			adam.InteractWith(powerCube);
            WorldTestHelper.UpdateWorld(_world, 3.0f);
			
			Assert.AreEqual(0.9f, powerCube.color.x, 0.05f);
			Assert.AreEqual(0.2f, powerCube.color.y, 0.05f);
			Assert.AreEqual(0.6f, powerCube.color.z, 0.05f);
		}

		[Test]
		public void CharacterInteractsWithTingThatMoves()
		{
			Character adam = _world.tingRunner.GetTing("Adam") as Character;
			MysticalCube powerCube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;			

			adam.logger.AddListener(Console.WriteLine);

			powerCube.position = new WorldCoordinate("Eden", 4, 0);
			adam.WalkToTingAndInteract(powerCube);
			WorldTestHelper.UpdateWorld(_world, 0.5f);

			// Move before he reaches it!
			powerCube.position = new WorldCoordinate("Eden", 4, 4);
			WorldTestHelper.UpdateWorld(_world, 6.0f);

			Assert.AreEqual(powerCube, adam.handItem);
			Assert.AreEqual(new WorldCoordinate("Eden", 4, 3), adam.position);
		}

		[Test]
		public void CharacterInteractsWithTingThatIsDestroyed()
		{
			Character adam = _world.tingRunner.GetTing("Adam") as Character;
			MysticalCube powerCube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;			
			
			adam.logger.AddListener(Console.WriteLine);
			//CharacterWalkBehaviour.logger.AddListener(Console.WriteLine);

			powerCube.position = new WorldCoordinate("Eden", 4, 0);
			adam.WalkToTingAndInteract(powerCube);
			WorldTestHelper.UpdateWorld(_world, 0.5f);
			
			// Destroy before he reaches it!
			_world.tingRunner.RemoveTingAfterUpdate(powerCube.name);
			WorldTestHelper.UpdateWorld(_world, 6.0f);
			
			Assert.AreEqual(null, adam.handItem);
		}

		[Test]
		public void CharacterInteractsWithTingOnSameTile()
		{
			Character adam = _world.tingRunner.GetTing("Adam") as Character;
			MysticalCube powerCube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;			
			
			adam.logger.AddListener(Console.WriteLine);
			SmartWalkBehaviour.s_logger.AddListener(Console.WriteLine);
			
			powerCube.position = adam.position;
			adam.WalkToTingAndInteract(powerCube);
			WorldTestHelper.UpdateWorld(_world, 5.0f);
			
			Assert.AreEqual(powerCube, adam.handItem);
		}
	}
}

