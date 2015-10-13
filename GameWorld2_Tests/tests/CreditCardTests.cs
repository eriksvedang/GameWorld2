using System;
using NUnit.Framework;
using GameWorld2;
using System.Collections.Generic;
using GameTypes;
using TingTing;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class CreditCardTests
	{
		[Test()]
		public void CheckBalance ()
		{
			WorldCoordinate testDefaultCoordinate = new WorldCoordinate("Eden", IntPoint.Zero);

			WorldTestHelper.GenerateInitData ();
			InitialSaveFileCreator i = new InitialSaveFileCreator ();
			World world = new World (i.CreateRelay (WorldTestHelper.INIT_DATA_PATH));
			
			D.onDLog += o => Console.WriteLine("D.Log: " + o);
			world.dialogueRunner.AddOnSomeoneSaidSomethingListener(o => Console.WriteLine("Dialogue: " + o));

			CreditCard card = world.tingRunner.CreateTing<CreditCard> ("Visa", testDefaultCoordinate);
			card.logger.AddListener (o => Console.WriteLine ("Card log: " + o));
			card.AddDataListener ("dialogueLine", (string oldValue, string newValue) => {
				Console.WriteLine ("Card said: " + newValue);
			});
			card.nameOfOwner = "Eva";
			card.masterProgramName = "EvasCard";
			
			Computer financeComputer = world.tingRunner.CreateTing<Computer>("FinanceComputer", testDefaultCoordinate);
			financeComputer.masterProgramName = "FinanceComputer";
			financeComputer.hasMemoryAPI = true;
			financeComputer.masterProgram.Start();

			WorldTestHelper.UpdateWorld (world, 3f);
			card.RunMakeTransactionFunction (100.0f);
			WorldTestHelper.UpdateWorld (world, 3f);

			Console.WriteLine ("Evas card errors: ");
			foreach (var error in card.masterProgram.GetErrors()) {
				Console.WriteLine (error.ToString ());
			}
			
			Console.WriteLine ("Finance computer errors: ");
			foreach (var error in financeComputer.masterProgram.GetErrors()) {
				Console.WriteLine (error.ToString ());
			}
	
			Console.WriteLine ("Finance computer output: ");
			PrintOutput(financeComputer);

			Assert.IsNotNull(financeComputer.memory);
			Assert.IsNotNull(financeComputer.memory.data);

			object cashAmount = null;
			bool gotIt = financeComputer.memory.data.TryGetValue("Eva", out cashAmount);

			Assert.IsTrue(gotIt);
			Assert.AreEqual(typeof(float), cashAmount.GetType());
			Assert.AreEqual(100.0f, (float)cashAmount);			
			
			card.RunMakeTransactionFunction(-20.0f);
			WorldTestHelper.UpdateWorld (world, 1.0f);

			Assert.IsNotNull(financeComputer.memory);
			Assert.IsNotNull(financeComputer.memory.data);

			object cashAmount2 = null;
			bool gotIt2 = financeComputer.memory.data.TryGetValue("Eva", out cashAmount2);

			Assert.IsTrue(gotIt2);
			Assert.AreEqual(80.0f, (float)cashAmount2);

		}

		void PrintOutput(Computer pComputer)
		{
			for (int i = 0; i < pComputer.currentLine; i++) {
				var line = pComputer.consoleOutput[i];
				Console.WriteLine(line);
			}
		}
	}
}

