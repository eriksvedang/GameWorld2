using System;
using NUnit.Framework;
using GameWorld2;
using GameTypes;
using System.Collections.Generic;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class CharactersAndDialogue
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
		}
		
		[Test()]
		public void EvaTalks()
		{
			Assert.AreEqual("", _eva.dialogueLine);
			_world.dialogueRunner.StartConversation("EvaTalks");
			Assert.AreEqual("Hi!", _eva.dialogueLine);
			WorldTestHelper.UpdateWorld(_world, 15.0f);
			Assert.AreEqual("", _eva.dialogueLine);
		}
		
		[Test()]
		public void EvaTalksToAdam()
		{
			_world.dialogueRunner.StartConversation("EvaTalksToAdam");
			Assert.AreEqual("", _adam.dialogueLine);
			Assert.IsTrue(_eva.CanInteractWith(_adam));
			_eva.InteractWith(_adam);
			Assert.AreEqual("Huh?", _adam.dialogueLine);
		}

		[Test()]
		public void CreateCharacterApi()
		{
			_world.dialogueRunner.StartConversation("CreatingSomeCharacters");
			WorldTestHelper.UpdateWorld(_world, 3f);
			Assert.IsNotNull(_world.tingRunner.GetTing<Character>("Johannes"));
		}

		[Test()]
		public void LoadAndSaveDialogueDuplicationBug()
		{
			string saveName = "../Saves/step1_save.json";
			var outputDialogue = new List<string>();

			Console.WriteLine("STEP 1");

			// Step 1
			{
				InitialSaveFileCreator i = new InitialSaveFileCreator();
				var world1 = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
				WorldTestHelper.PreloadWorld(world1);

				GameTypes.D.onDLog += Console.WriteLine;
				world1.dialogueRunner.logger.AddListener(Console.WriteLine);

				world1.dialogueRunner.AddOnSomeoneSaidSomethingListener(o => {
					Console.WriteLine(o.speaker + ": " + o.line);
					outputDialogue.Add(o.line);
				});
				
				WorldTestHelper.UpdateWorld(world1, 1f);
				PrintActiveStuff(world1);
				world1.dialogueRunner.RunStringAsFunction("Story(BeforeFirst)");
				WorldTestHelper.UpdateWorld(world1, 1f);
				PrintActiveStuff(world1);
				
				
				world1.dialogueRunner.RunStringAsFunction("Story(First)");
				WorldTestHelper.UpdateWorld(world1, 0.5f);
				PrintActiveStuff(world1);
				
				
				world1.dialogueRunner.EventHappened("DO_IT_REAL_GOOD");
				PrintActiveStuff(world1);
				WorldTestHelper.UpdateWorld(world1, 3f);
				

				world1.Save(saveName);

				GameTypes.D.onDLog -= Console.WriteLine;
			}

			Console.WriteLine("\n\nSTEP 2");

			// Step 2
			{
				var world2 = new World(saveName);
				WorldTestHelper.PreloadWorld(world2);
				
				world2.dialogueRunner.logger.AddListener(Console.WriteLine);
				GameTypes.D.onDLog += Console.WriteLine;

				world2.dialogueRunner.AddOnSomeoneSaidSomethingListener(o => {
					Console.WriteLine(o.speaker + ": " + o.line);
					outputDialogue.Add(o.line);
				});
				
				PrintActiveStuff(world2);
				WorldTestHelper.UpdateWorld(world2, 3f);
				PrintActiveStuff(world2);
				
				world2.dialogueRunner.RunStringAsFunction("Story(Second)");
				WorldTestHelper.UpdateWorld(world2, 3.0f);
				
				PrintActiveStuff(world2);
				
				
				world2.dialogueRunner.EventHappened("DO_IT_REAL_GOOD");
				WorldTestHelper.UpdateWorld(world2, 3f);
				
				
			}
		}

		public void PrintActiveStuff (World pWorld)
		{
			var activeConversations = pWorld.dialogueRunner.GetActiveConversations();
			var activeConversationsList = new List<string>();
			foreach(var s in activeConversations) {
				activeConversationsList.Add(s);
			}
			Console.WriteLine("Active conversations: " + string.Join(", ", activeConversationsList.ToArray()));
			
			var registeredNodes = pWorld.dialogueRunner.GetRegisteredDialogueNodes();
			var registeredNodeNames = new List<string>();
			foreach(var node in registeredNodes) {
				registeredNodeNames.Add("[" + node.name + " in conversation" + node.conversation + " " + (node.isListening ? "LISTENING" : "NOT listening") + "]");
			}
			Console.WriteLine("Registered nodes: " + string.Join(", ", registeredNodeNames.ToArray()));
		}

		[Test()]
		public void ProblemsWithWaitCommand()
		{
			_world.dialogueRunner.AddOnSomeoneSaidSomethingListener(o => {
				Console.WriteLine(o.speaker + ": " + o.line);
			});

			var key = _world.tingRunner.CreateTing<Key>("Key", _adam.position);
			var bo =  _world.tingRunner.CreateTing<Character>("Bo", _adam.position);
			bo.handItem = key;

			bo.logger.AddListener(Console.WriteLine);

			bo.SetTimetableRunner(_world.timetableRunner);
			bo.timetableName = "Bo";
			bo.timetableTimer = 0.5f;

			WorldTestHelper.UpdateWorld(_world, 1.0f);

			_world.dialogueRunner.StartConversation("ProblemWithTiming");
			WorldTestHelper.UpdateWorld(_world, 1.0f);

			Assert.AreEqual("A", bo.dialogueLine);
			Assert.AreEqual("Key", bo.handItemName);

			bo.timetableTimer = 0.5f;
			WorldTestHelper.UpdateWorld(_world, 1.0f);
			bo.timetableTimer = 0.5f;
			WorldTestHelper.UpdateWorld(_world, 1.0f);

			Assert.AreEqual("", bo.handItemName);
			Assert.AreEqual("B", bo.dialogueLine);
		}

	}
}

