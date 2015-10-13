using System;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using System.Collections.Generic;
using TingTing;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class CharacterKnowledge
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
		public void NoKnowledge()
		{
			Assert.IsFalse(_eva.HasKnowledge("Whatever"));
		}
		
		[Test()]
		public void OneKnowledge()
		{
			_eva.SetKnowledge("Something");
			Assert.IsTrue(_eva.HasKnowledge("Something"));
		}
		
		[Test()]
		public void TwoKnowledges()
		{
			_eva.SetKnowledge("A");
			_eva.SetKnowledge("B");
			Assert.IsTrue(_eva.HasKnowledge("A"));
			Assert.IsTrue(_eva.HasKnowledge("B"));
		}
		
		[Test()]
		public void RepeatedKnowledge()
		{
			_eva.SetKnowledge("A");
			_eva.SetKnowledge("A");
			Assert.AreEqual(1, _eva.knowledge.Length);
		}
	}
}

