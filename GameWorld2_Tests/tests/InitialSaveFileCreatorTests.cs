using System;
using System.Reflection;
using NUnit.Framework;
using RelayLib;
using GrimmLib;
using GameTypes;
using TingTing;
using GameWorld2;
using System.IO;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class InitialSaveFileCreatorTests
	{	
		[SetUp()]
		public void Setup()
		{
            WorldTestHelper.GenerateInitData();
		}
		
		[Test()]
		public void CreateAndSaveEmptyRelay()
		{
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			RelayTwo emptyRelay = i.CreateEmptyRelay();
			AssertThatCanFindAllTables(emptyRelay);			
			emptyRelay.SaveAll("empty.json");
            emptyRelay.tables.Clear();
            emptyRelay.LoadAll("empty.json");
			AssertThatCanFindAllTables(emptyRelay);
		}
		
		void AssertThatCanFindAllTables(RelayTwo pRelay)
		{
			Assert.IsNotNull(pRelay.GetTable(Ting.TABLE_NAME));
			Assert.IsNotNull(pRelay.GetTable(Room.TABLE_NAME));
			Assert.IsNotNull(pRelay.GetTable(DialogueNode.TABLE_NAME));
			Assert.IsNotNull(pRelay.GetTable(SourceCode.TABLE_NAME));
			Assert.IsNotNull(pRelay.GetTable(Program.TABLE_NAME));
			Assert.IsNotNull(pRelay.GetTable(WorldSettings.TABLE_NAME));
		}
		
		[Test()]
		public void CreateInitialSaveFromInitData1()
		{			
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			i.CreateSaveFile("../InitData1/", WorldTestHelper.INITIAL_SAVE_1);
			
            RelayTwo relay = new RelayTwo(WorldTestHelper.INITIAL_SAVE_1);
			AssertThatCanFindAllTables(relay);
			
			TableTwo tingTable = relay.GetTable(Character.TABLE_NAME);
			Assert.AreEqual("Adam", tingTable.GetRow(0).Get<string>("name"));
			Assert.AreEqual("Eva", tingTable.GetRow(1).Get<string>("name"));
            Assert.AreEqual("PowerCube", relay.GetTable(MysticalCube.TABLE_NAME).GetRow(0).Get<string>("name"));
			
			TableTwo roomTable = relay.GetTable(Room.TABLE_NAME);
            Assert.AreEqual("Eden", roomTable.GetRow(0).Get<string>("name"));
			Assert.AreEqual("Hallway", roomTable.GetRow(1).Get<string>("name"));
			Assert.AreEqual("Kitchen", roomTable.GetRow(2).Get<string>("name"));
			Assert.AreEqual("Bedroom", roomTable.GetRow(3).Get<string>("name"));
		}
	}
}

