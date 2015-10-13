using System;
using System.Collections.Generic;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using TingTing;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class TimetableTest
	{
		World _world;
		Character _eva;

		[SetUp()]
		public void SetUp()
		{			
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
			_eva = _world.tingRunner.GetTing("Eva") as Character;
		}

		[Test()]
		public void FillTimeTable()
		{
			WorldCoordinate positionA = new WorldCoordinate("Eden", new IntPoint(2, 3));
			WorldCoordinate positionB = new WorldCoordinate("Eden", new IntPoint(4, 1));
			WorldCoordinate positionC = new WorldCoordinate("Eden", new IntPoint(1, 2));

			Timetable timetable = _world.timetableRunner.CreateTimetable("NewTimetable", "");

			timetable.CreateTimetableSpanInternal( new GameTime(03, 00), new GameTime(08, 00), new Behaviour_BeAtPosition(positionA));
			timetable.CreateTimetableSpanInternal( new GameTime(08, 00), new GameTime(21, 00), new Behaviour_BeAtPosition(positionB));
			timetable.CreateTimetableSpanInternal( new GameTime(21, 00), new GameTime(03, 00), new Behaviour_BeAtPosition(positionC));

			Console.WriteLine("TimetableSpans: \n" + timetable.TimetableSpansToString());

			_eva.timetableName = "NewTimetable";
			_eva.logger.AddListener(Console.WriteLine);

			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(1, 05, 00, 00));
			Assert.AreEqual(positionA, _eva.position);

			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(1, 12, 00, 00));
			Assert.AreEqual(positionB, _eva.position);

			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(2, 02, 00, 00));
			Assert.AreEqual(positionC, _eva.position);
		}

		[Test()]
		public void LoadTimeTablesFromFile()
		{
			_eva.timetableName = "Timetable1";
			Assert.AreEqual("Timetable1", _eva.timetable.name);
		}

		[Test()]
		public void LoadTimeTablesFromSave()
		{
			string saveName = "TimeTableSaveTest.json";

			_eva.timetableName = "Timetable1";
			_world.Save(saveName);

			World newWorld = new World(saveName);
			foreach(string s in newWorld.Preload()) {}

			Character newEva = newWorld.tingRunner.GetTing<Character>("Eva");
			Timetable newTimetable = newWorld.timetableRunner.GetTimetable("Timetable1");

			Assert.IsNotNull(newTimetable);
			Assert.AreEqual(newTimetable, newEva.timetable);
			Assert.AreEqual(2, newEva.timetable.timetableSpans.Length);
		}

		[Test()]
		public void GetCorrectTimetableSpan()
		{
			Timetable t = _world.timetableRunner.GetTimetable("Timetable2");

			//Console.WriteLine("TimetableSpans: \n" + t.TimetableSpansToString());

			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(01, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(02, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(03, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(04, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(05, 30)).behaviour.GetType());

			Assert.AreEqual(typeof(Behaviour_BeAtTing), t.GetCurrentSpan(new GameTime(06, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_BeAtTing), t.GetCurrentSpan(new GameTime(07, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_BeAtTing), t.GetCurrentSpan(new GameTime(08, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_BeAtTing), t.GetCurrentSpan(new GameTime(09, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_BeAtTing), t.GetCurrentSpan(new GameTime(10, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_BeAtTing), t.GetCurrentSpan(new GameTime(11, 30)).behaviour.GetType());

			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(12, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(13, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(14, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(15, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(16, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(17, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(18, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sit), t.GetCurrentSpan(new GameTime(19, 30)).behaviour.GetType());

			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(20, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(21, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(22, 30)).behaviour.GetType());
			Assert.AreEqual(typeof(Behaviour_Sleep), t.GetCurrentSpan(new GameTime(23, 30)).behaviour.GetType());
		}

		[Test()]
		public void SleepDrinkAndSitBehaviours()
		{
			_eva.logger.AddListener(Console.WriteLine);
			_eva.timetableName = "Timetable2";
			_eva.sleepiness = 100.0f;

			WorldCoordinate positionA = new WorldCoordinate("Eden", new IntPoint(1, 1));
			WorldCoordinate positionB = new WorldCoordinate("Eden", new IntPoint(4, 2));
			WorldCoordinate positionC = new WorldCoordinate("Eden", new IntPoint(3, 3));

			_world.tingRunner.CreateTing<Bed>("Bed1", positionA);
			Drink drink = _world.tingRunner.CreateTing<Drink>("Drink1", positionB);
			drink.amount = 100f;

			_world.tingRunner.CreateTing<Seat>("Chair1", positionC);

			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(0, 21, 00, 00));
			Assert.AreEqual("Sleeping", _eva.actionName);

			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(1, 10, 00, 00));
			Assert.AreEqual(_eva.position, drink.position);
		
			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(1, 19, 00, 00));
			Console.WriteLine("Time: " + _world.settings.gameTimeClock);
			Assert.IsNull(_eva.bed);

			_eva.sleepiness = 200f;

			WorldTestHelper.UpdateWorldUntilGameTime(_world, new GameTime(2, 21, 00, 00));
			Assert.AreEqual("Sleeping", _eva.actionName);
			Assert.IsNull(_eva.seat);
		}

		[Test()]
		public void MultipleDefinitionsInOneFile()
		{
			_eva.logger.AddListener(Console.WriteLine);
			_eva.timetableName = "Timetable3a";
			Assert.AreEqual(2, _eva.timetable.timetableSpans.Length);
			_eva.timetableName = "Timetable3b";
			Assert.AreEqual(3, _eva.timetable.timetableSpans.Length);
			_eva.timetableName = "Timetable3c";
			Assert.AreEqual(4, _eva.timetable.timetableSpans.Length);
		}
	}
}

