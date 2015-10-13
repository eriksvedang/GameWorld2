using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProgrammingLanguageNr1;
using GameWorld2;
using GameTypes;
using TingTing;

namespace GameWorld2_Tests
{
	[TestFixture()]
	public class TingsWithProgramsInThem
	{
		World _world;
		List<string> output;
        WorldCoordinate testDefaultCoordinate = new WorldCoordinate("Eden", IntPoint.Zero);
		
		[SetUp()]
		public void SetUp()
		{			
			D.onDLog += Console.WriteLine;
			WorldTestHelper.GenerateInitData();
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			_world = new World(i.CreateRelay(WorldTestHelper.INIT_DATA_PATH));
		}
		
		object API_print(object[] args)
		{
			string pretty = ReturnValueConversions.PrettyStringRepresenation(args[0]);
			output.Add(pretty);
			Console.WriteLine("output: " + pretty);
			return VoidType.voidType;
		}
		
		[Test()]
		public void ProgramWithOutputGettingHacked()
		{
			const string saveName = "SimpleHacking.json";
			output = new List<string>();
			FunctionDefinition print = new FunctionDefinition("void", "print", new string[] { "string" }, new string[] { "s" }, API_print, FunctionDocumentation.Default());
			
			{
				SourceCode helloWorldSource = _world.sourceCodeDispenser.GetSourceCode("helloworld");
				
				Program helloWorldProgram = _world.programRunner.CreateProgram(helloWorldSource);
				helloWorldProgram.FunctionDefinitions.Add(print);
			
				Character morgan = _world.tingRunner.CreateTing<Character>("Morgan", testDefaultCoordinate);
				morgan.programs = new Program[] { helloWorldProgram };
				
				helloWorldProgram.Start();
				WorldTestHelper.UpdateWorld(_world, 5f);
				
				Assert.AreEqual(1, output.Count);
				Assert.AreEqual("Hello World!", output[0]);
				
				helloWorldProgram.sourceCodeContent = "print(\"Hello Moon!\")";
				
				_world.Save(saveName);
			}
			
			{
				_world = new World(saveName);
				
				Character morgan = _world.tingRunner.GetTing("Morgan") as Character;
				Assert.AreEqual(1, morgan.programs.Length);	
				
				Program helloWorld = morgan.GetProgram("helloworld");
				Assert.IsNotNull(helloWorld);
				
				helloWorld.FunctionDefinitions.Add(print);
				
				helloWorld.Start();
				WorldTestHelper.UpdateWorld(_world, 5f);
				
				Assert.AreEqual(2, output.Count);
				Assert.AreEqual("Hello Moon!", output[1]);
			}
		}
		
		[Test()]
		public void InteractWithMysticalCubeToRunItsProgram()
		{
			const string saveName = "InteractWithMysticalCubeToRunItsProgram.json";
			{
				MysticalCube cube = _world.tingRunner.CreateTing<MysticalCube>("The Cube", testDefaultCoordinate);
				Assert.AreEqual(0, cube.programs.Length);
				Assert.AreEqual(0, cube.mysteryLevel);
			
				Character bosse = _world.tingRunner.CreateTing<Character>("Bosse", testDefaultCoordinate);
				bosse.InteractWith(cube);
				WorldTestHelper.UpdateWorld(_world, 5f);
				
				Assert.AreEqual(5, cube.mysteryLevel); // uses the default "TheCube.sprak" program
				_world.Save(saveName);
			}
			{
				_world = new World(saveName);
				
				MysticalCube cube = _world.tingRunner.GetTing("The Cube") as MysticalCube;
				Assert.AreEqual(5, cube.mysteryLevel);
				
				Character bosse = _world.tingRunner.GetTing("Bosse") as Character;
				bosse.InteractWith(cube);
				
				WorldTestHelper.UpdateWorld(_world, 5f);
				Assert.AreEqual(10, cube.mysteryLevel);
			}
		}
		
		[Test()]
		public void UsePowerCubeLoadedFromInitData1()
		{
			MysticalCube powerCube = _world.tingRunner.GetTing("PowerCube") as MysticalCube;
			Assert.AreEqual(0, powerCube.programs.Length);
			Assert.AreEqual(0, powerCube.mysteryLevel);
			
			Character erik = _world.tingRunner.CreateTing<Character>("Erik", testDefaultCoordinate);
			erik.InteractWith(powerCube);
			
			WorldTestHelper.UpdateWorld(_world, 5f);
			Assert.AreEqual(100, powerCube.mysteryLevel); // power!
		}
		
		[Test()]
		public void LampMasterProgramTest()
		{
			//Lamp lamp = _world.tingRunner.CreateTing<Lamp>("Streetlight", testDefaultCoordinate);
		}

		void PrintErrors(Program pProgram)
		{
			Console.WriteLine ("Errors in program " + pProgram + ":");
			foreach (var e in pProgram.GetErrors()) {
				Console.WriteLine(e.ToString());
			}
		}

		void PrintOutput(Computer pComputer)
		{
			for (int i = 0; i < pComputer.currentLine; i++) {
				var line = pComputer.consoleOutput[i];
				Console.WriteLine(line);
			}
		}

		[Test()]
		public void SendDataFromComputerToComputer()
		{
			D.onDLog += Console.WriteLine;

			Computer sender = _world.tingRunner.CreateTing<Computer>("Sender", new WorldCoordinate("Eden", 1, 3));
			Computer receiver = _world.tingRunner.CreateTing<Computer>("Receiver", new WorldCoordinate("Eden", 3, 2));

			sender.hasInternetAPI = true;

			sender.masterProgramName = "Sender";
			receiver.masterProgramName = "Receiver";

			receiver.RunProgram(null);
			sender.RunProgram(null);

			WorldTestHelper.UpdateWorld(_world, 1f);

			/*
			Console.WriteLine("Output in sender:");
			PrintOutput(sender);

			Console.WriteLine("Output in receiver:");
			PrintOutput(receiver);
			*/

			Console.WriteLine("Errors in sender:");
			PrintErrors(sender.masterProgram);

			Console.WriteLine("Errors in receiver:");
			PrintErrors(receiver.masterProgram);

			//Assert.AreEqual(1, receiver.consoleOutput.Length);
			Assert.AreEqual("received: hej", receiver.consoleOutput[0]);
		}

		[Test()]
		public void SendNumericDataFromComputerToComputer()
		{
			D.onDLog += Console.WriteLine;
			
			Computer sender = _world.tingRunner.CreateTing<Computer>("Sender", new WorldCoordinate("Eden", 1, 3));
			Computer receiver = _world.tingRunner.CreateTing<Computer>("Receiver", new WorldCoordinate("Eden", 3, 2));
			
			sender.hasInternetAPI = true;
			
			sender.masterProgramName = "Sender4";
			receiver.masterProgramName = "Receiver4";
			
			receiver.RunProgram(null);
			sender.RunProgram(null);
			
			WorldTestHelper.UpdateWorld(_world, 1f);

			Console.WriteLine("Output in sender:");
			PrintOutput(sender);

			Console.WriteLine("Output in receiver:");
			PrintOutput(receiver);

			Assert.IsFalse(sender.containsBrokenPrograms);
			Assert.IsFalse(receiver.containsBrokenPrograms);
			
//			Console.WriteLine("Errors in sender:");
//			PrintErrors(sender.masterProgram);
//			
//			Console.WriteLine("Errors in receiver:");
//			PrintErrors(receiver.masterProgram);

			Assert.AreEqual("received: 5", receiver.consoleOutput[0]);
			Assert.AreEqual("x + x: 10", receiver.consoleOutput[1]);
		}

		[Test()]
		public void GetDataFromComputerToComputer()
		{
			Computer sender = _world.tingRunner.CreateTing<Computer>("Sender", new WorldCoordinate("Eden", 1, 3));
			Computer receiver = _world.tingRunner.CreateTing<Computer>("Receiver", new WorldCoordinate("Eden", 3, 2));

			sender.hasInternetAPI = true;

			sender.masterProgramName = "Sender2";
			receiver.masterProgramName = "Receiver2";

			receiver.RunProgram(null);
			sender.RunProgram(null);

			WorldTestHelper.UpdateWorld(_world, 1f);

			Console.WriteLine("Errors in sender:");
			PrintErrors(sender.masterProgram);

			Console.WriteLine("Errors in receiver:");
			PrintErrors(receiver.masterProgram);

			//Assert.AreEqual(1, receiver.consoleOutput.Length);
			Assert.AreEqual("got back: 42", sender.consoleOutput[0]);
		}

		[Test()]
		public void DontUseVoidRemoteFnCallOnVoidFn()
		{
			D.onDLog += Console.WriteLine;

			Computer sender = _world.tingRunner.CreateTing<Computer>("Sender", new WorldCoordinate("Eden", 1, 3));
			Computer receiver = _world.tingRunner.CreateTing<Computer>("Receiver", new WorldCoordinate("Eden", 3, 2));

			sender.hasInternetAPI = true;

			sender.masterProgramName = "Sender3";
			receiver.masterProgramName = "Receiver3";

			receiver.RunProgram(null);
			sender.RunProgram(null);

			WorldTestHelper.UpdateWorld(_world, 1f);

			Console.WriteLine("Errors in sender:");
			PrintErrors(sender.masterProgram);

			Console.WriteLine("Errors in receiver:");
			PrintErrors(receiver.masterProgram);

			//Assert.AreEqual(1, receiver.consoleOutput.Length);
			Assert.AreEqual("received: svejs", receiver.consoleOutput[0]);
		}

		[Test()]
		public void SendDataFromComputerToComputerWhileSavingAndLoadingInbetween()
		{
			{
				WorldTestHelper.PreloadWorld(_world);
				_world.Update(0.1f);

				Computer sender = _world.tingRunner.CreateTing<Computer>("Sender", new WorldCoordinate("Eden", 1, 3));
				Computer receiver = _world.tingRunner.CreateTing<Computer>("Receiver", new WorldCoordinate("Eden", 3, 2));

				sender.hasInternetAPI = true;

				sender.masterProgramName = "Sender";
				receiver.masterProgramName = "Receiver";

				//Console.WriteLine("sender program state: " + sender.masterProgram.programState);

				receiver.RunProgram(null);
				sender.RunProgram(null);

				//_world.Update(1f);
				//_world.Update(1f);
				//_world.Update(1f);

				_world.Save("InBetween.json");

				Assert.IsTrue(sender.masterProgram.isOn);
				Assert.IsTrue(receiver.masterProgram.isOn);

				Console.WriteLine("sender executions: " + sender.masterProgram.executionCounter);
				Console.WriteLine("sender is waiting for input? " + sender.masterProgram.waitingForInput);
				Console.WriteLine("sender is on? " + sender.masterProgram.isOn);
				//Console.WriteLine("sender program state: " + sender.masterProgram.programState);

				Console.WriteLine("receiver executions: " + receiver.masterProgram.executionCounter);
				Console.WriteLine("receiver is waiting for input? " + receiver.masterProgram.waitingForInput);
				Console.WriteLine("receiver is on? " + receiver.masterProgram.isOn);
				//Console.WriteLine("receiver program state: " + receiver.masterProgram.programState);
			}

			Console.WriteLine(" --- AFTER --- ");

			{
				var world2 = new World("InBetween.json");
				WorldTestHelper.PreloadWorld(world2);

				Computer sender2 = world2.tingRunner.GetTing<Computer>("Sender");
				Computer receiver2 = world2.tingRunner.GetTing<Computer>("Receiver");

				Assert.IsTrue(sender2.masterProgram.isOn);
				Assert.IsTrue(receiver2.masterProgram.isOn);

				WorldTestHelper.UpdateWorld(world2, 5f);

				Console.WriteLine("sender executions: " + sender2.masterProgram.executionCounter);
				Console.WriteLine("sender is waiting for input? " + sender2.masterProgram.waitingForInput);
				Console.WriteLine("sender is on? " + sender2.masterProgram.isOn);
				//Console.WriteLine("sender program state: " + sender2.masterProgram.programState);

				Console.WriteLine("receiver executions: " + receiver2.masterProgram.executionCounter);
				Console.WriteLine("receiver is waiting for input? " + receiver2.masterProgram.waitingForInput);
				Console.WriteLine("receiver is on? " + receiver2.masterProgram.isOn);
				//Console.WriteLine("receiver program state: " + receiver2.masterProgram.programState);

				Assert.AreEqual("bleh", sender2.consoleOutput[0]);
				Assert.AreEqual("received: hej", receiver2.consoleOutput[0]);

				//Assert.IsFalse(sender2.masterProgram.isOn);
				//Assert.IsFalse(receiver2.masterProgram.isOn);
			}
		}

		[Test()]
		public void CallBuiltInFunctionOnAnotherTing()
		{
			D.onDLog += Console.WriteLine;

			Computer sender = _world.tingRunner.CreateTing<Computer>("Sender", new WorldCoordinate("Eden", 1, 3));
			Computer receiver = _world.tingRunner.CreateTing<Computer>("Receiver", new WorldCoordinate("Eden", 3, 2));

			sender.hasInternetAPI = true;

			sender.masterProgramName = "CallMeSender";
			receiver.masterProgramName = "CallMeReceiver";

			receiver.RunProgram(null);
			sender.RunProgram(null);

			WorldTestHelper.UpdateWorld(_world, 1f);

			Console.WriteLine("Output in sender:");
			PrintOutput(sender);

			Console.WriteLine("Output in receiver:");
			PrintOutput(receiver);

			Console.WriteLine("Errors in sender:");
			PrintErrors(sender.masterProgram);

			Console.WriteLine("Errors in receiver:");
			PrintErrors(receiver.masterProgram);

			//Assert.AreEqual(1, receiver.currentLine);
			Assert.AreEqual("hej du", receiver.consoleOutput[0]);

			Assert.AreEqual (false, sender.containsBrokenPrograms);
			Assert.AreEqual (false, receiver.containsBrokenPrograms);
		}

		[Test()]
		public void CallCheckBalanceFunctionOnCreditCardTest()
		{
			var lines = new List<string>();

			CreditCard card = _world.tingRunner.CreateTing<CreditCard> ("Visa", testDefaultCoordinate);
			card.logger.AddListener (o => Console.WriteLine ("Card log: " + o));
			card.AddDataListener ("dialogueLine", (string oldValue, string newValue) => {
				lines.Add(newValue);
				Console.WriteLine ("Card said: " + newValue);
			});
			card.masterProgramName = "EvasCard";

			Computer financeComputer = _world.tingRunner.CreateTing<Computer>("FinanceComputer", testDefaultCoordinate);
			financeComputer.masterProgramName = "FinanceComputer";
			financeComputer.hasMemoryAPI = true;
			financeComputer.masterProgram.Start();

			WorldTestHelper.UpdateWorld (_world, 3f);
			card.RunMakeTransactionFunction (100.0f);
			WorldTestHelper.UpdateWorld (_world, 3f);
			card.PushButton (null);
			WorldTestHelper.UpdateWorld (_world, 3f);

//			object cashAmount = null;
//			bool gotIt = financeComputer.memory.data.TryGetValue("Eva", out cashAmount);
//			Assert.IsTrue(cashAmount);

			Assert.AreEqual(5, lines.Count);
			Assert.AreEqual("Will make transaction: 100", lines[0]);
			Assert.AreEqual("", lines[1]);
			Assert.AreEqual("id = 0", lines[2]);
			Assert.AreEqual("", lines[3]);
			Assert.AreEqual("Balance: 100", lines[4]);

			Assert.AreEqual (false, card.containsBrokenPrograms);
			Assert.AreEqual (false, financeComputer.containsBrokenPrograms);
		}

		[Test()]
		public void CallTransactionFunctionOnCreditCardTest()
		{
			var lines = new List<string>();

			CreditCard card = _world.tingRunner.CreateTing<CreditCard> ("Visa", testDefaultCoordinate);
			//card.logger.AddListener (o => Console.WriteLine ("Card log: " + o));
			card.AddDataListener ("dialogueLine", (string oldValue, string newValue) => {
				lines.Add(newValue);
				Console.WriteLine ("Card said: " + newValue);
			});
			card.masterProgramName = "EvasCard";

			Computer financeComputer = _world.tingRunner.CreateTing<Computer>("FinanceComputer", testDefaultCoordinate);
			financeComputer.masterProgramName = "FinanceComputer";
			financeComputer.hasMemoryAPI = true;
			financeComputer.masterProgram.Start();

			WorldTestHelper.UpdateWorld (_world, 3f);
			card.RunMakeTransactionFunction (-3.5f);
			WorldTestHelper.UpdateWorld (_world, 3f);

			Assert.AreEqual(1, lines.Count);
			Assert.AreEqual("Will make transaction: -3.5", lines[0]);

			Assert.AreEqual (false, card.containsBrokenPrograms);
			Assert.AreEqual (false, financeComputer.containsBrokenPrograms);
		}

		[Test()]
		public void TingWithErrorInProgram()
		{
			var fountain = _world.tingRunner.CreateTing<Fountain> ("Fountain1", testDefaultCoordinate);
			fountain.masterProgram.sourceCodeContent = "sdkfhfhakjhrkajda";
			fountain.logger.AddListener (o => Console.WriteLine ("Fountain log: " + o));

			WorldTestHelper.UpdateWorld (_world, 1f);
			fountain.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.AreEqual (true, fountain.containsBrokenPrograms);
		}

		[Test()]
		public void CallFunctionOnProgramWithInfiniteLoopInGlobalScope()
		{
			Computer computer1 = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer1.masterProgramName = "Computer1";
			computer1.logger.AddListener (o => Console.WriteLine ("Computer1 log: " + o));

			Computer computer2 = _world.tingRunner.CreateTing<Computer>("Computer2", testDefaultCoordinate);
			computer2.masterProgramName = "Computer2";

			computer2.AddDataListener<string[]> ("consoleOutput", (pOldValue, pNewValue) => {
				//Console.WriteLine("Computer2 printed!");
			});

			computer2.masterProgram.Start (); // the infinite loop should start
			WorldTestHelper.UpdateWorld (_world, 1f);

//			Console.WriteLine ("Before:");
//			PrintProgramInfo (computer1);
//			PrintProgramInfo (computer2);

			computer1.masterProgram.Start ();
			WorldTestHelper.UpdateWorld (_world, 1f);

//			Console.WriteLine ("Middle:");
//			PrintProgramInfo (computer1);
//			PrintProgramInfo (computer2);

			computer1.masterProgram.Start ();
			WorldTestHelper.UpdateWorld (_world, 1f);

//			Console.WriteLine ("After:");
//			PrintProgramInfo (computer1);
//			PrintProgramInfo (computer2);

			Assert.AreEqual("LOOP!", computer2.consoleOutput[0]);
			Assert.AreEqual("LOOP!", computer2.consoleOutput[1]);
			Assert.AreEqual("LOOP!", computer2.consoleOutput[2]);
			Assert.AreEqual("LOOP!", computer2.consoleOutput[3]);
			Assert.AreEqual("LOOP!", computer2.consoleOutput[4]);
			Assert.AreEqual("LOOP!", computer2.consoleOutput[5]);

			Assert.AreEqual("hej", computer1.consoleOutput[0]);
			Assert.AreEqual("hej", computer1.consoleOutput[1]);
			Assert.AreEqual("", computer1.consoleOutput[2]);
		}

		[Test()]
		public void CallExternalFunctionOnProgramWithPrintStatementInGlobalScope()
		{
			Computer computer1 = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer1.masterProgramName = "Caller";
			computer1.logger.AddListener (o => Console.WriteLine ("Computer1 log: " + o));
			
			Computer computer2 = _world.tingRunner.CreateTing<Computer>("Computer2", testDefaultCoordinate);
			computer2.masterProgramName = "Stupid";
			
			computer2.AddDataListener<string[]> ("consoleOutput", (pOldValue, pNewValue) => {
				Console.WriteLine("Computer2 printed: ");
				foreach(var s in pNewValue) {
					Console.WriteLine(s);
				}
			});
			
			WorldTestHelper.UpdateWorld (_world, 1f);

			computer1.masterProgram.Start (); // call
			WorldTestHelper.UpdateWorld (_world, 1f);
						
			Assert.AreEqual("hej", computer2.consoleOutput[0]);
			Assert.AreEqual("", computer2.consoleOutput[1]);
			
			Assert.AreEqual (false, computer1.containsBrokenPrograms);
			Assert.AreEqual (false, computer2.containsBrokenPrograms);
		}

		[Test()]
		public void CallFunctionOnProgramWithInputFunctionInGlobalScope()
		{
			Computer computer1 = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer1.masterProgramName = "Computer1";
			computer1.logger.AddListener (o => Console.WriteLine ("Computer1 log: " + o));
			
			Computer computer2 = _world.tingRunner.CreateTing<Computer>("Computer2", testDefaultCoordinate);
			computer2.masterProgramName = "Computer2inputfn";
			
			computer2.AddDataListener<string[]> ("consoleOutput", (pOldValue, pNewValue) => {
				Console.WriteLine("Computer2 printed!");
			});
			
			computer2.masterProgram.Start ();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			//			Console.WriteLine ("Before:");
			//			PrintProgramInfo (computer1);
			//			PrintProgramInfo (computer2);
			
			computer1.masterProgram.Start ();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			//			Console.WriteLine ("Middle:");
			//			PrintProgramInfo (computer1);
			//			PrintProgramInfo (computer2);
			
			computer1.masterProgram.Start ();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			//			Console.WriteLine ("After:");
			//			PrintProgramInfo (computer1);
			//			PrintProgramInfo (computer2);
			
			Assert.AreEqual("Enter number:", computer2.consoleOutput[0]);
			Assert.AreEqual(null, computer2.consoleOutput[1]);
			
			Assert.AreEqual("hej", computer1.consoleOutput[0]);
			Assert.AreEqual("hej", computer1.consoleOutput[1]);
			Assert.AreEqual("", computer1.consoleOutput[2]);
		}

		private void PrintProgramInfo(Computer pComputer) {
			var p = pComputer.masterProgram;
			Console.WriteLine (pComputer + ".masterProgram, isOn = " + p.isOn + ", waitingForInput = " + p.waitingForInput + " ");
		}


		[Test()]
		public void CallVoidFunctionOnOtherObjectUsingDotSyntax()
		{
			Computer computer3 = _world.tingRunner.CreateTing<Computer> ("Computer3", testDefaultCoordinate);
			Computer computer4 = _world.tingRunner.CreateTing<Computer> ("Computer4", testDefaultCoordinate);
				
			computer3.masterProgramName = "Computer3";
			computer4.masterProgramName = "Computer4";

			computer3.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.AreEqual("hej", computer4.consoleOutput[0]);
			Assert.AreEqual (false, computer3.containsBrokenPrograms);
			Assert.AreEqual (false, computer4.containsBrokenPrograms);
		}

		[Test()]
		public void CallBuiltInFunctionOnOtherObjectUsingDotSyntax()
		{
			Computer computer5 = _world.tingRunner.CreateTing<Computer> ("Computer5", testDefaultCoordinate);
			Computer computer6 = _world.tingRunner.CreateTing<Computer> ("Computer6", testDefaultCoordinate);

			computer5.masterProgramName = "Computer5";
			computer6.masterProgramName = "Computer6";

			computer5.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.AreEqual("hejsan", computer6.consoleOutput[0]);
			Assert.AreEqual (false, computer5.containsBrokenPrograms);
			Assert.AreEqual (false, computer6.containsBrokenPrograms);
		}

		[Test()]
		public void CallWeatherComputer()
		{
			Computer humidityCaller = _world.tingRunner.CreateTing<Computer> ("HumidityStation", testDefaultCoordinate);
			Computer weatherServer = _world.tingRunner.CreateTing<Computer> ("WeatherServer", testDefaultCoordinate);
			
			humidityCaller.masterProgramName = "Humidity";
			weatherServer.masterProgramName = "Weather";
			weatherServer.hasWeatherAPI = true;

			_world.dialogueRunner.RunStringAsFunction("RunMasterProgram(WeatherServer)");
			WorldTestHelper.UpdateWorld(_world, 5.0f);

			foreach(var o in weatherServer.consoleOutput) {
				Console.WriteLine("" + o);
			}

			humidityCaller.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.AreEqual (false, humidityCaller.containsBrokenPrograms);
			Assert.AreEqual (false, weatherServer.containsBrokenPrograms);

			Assert.AreEqual("3", humidityCaller.consoleOutput[0]);
			Assert.AreEqual("Humidity sample: 100", humidityCaller.consoleOutput[1]);
			Assert.AreEqual("Humidity sample: 100", humidityCaller.consoleOutput[2]);
			Assert.AreEqual("Humidity sample: 100", humidityCaller.consoleOutput[3]);
			Assert.AreEqual("Average humidity: 100%", humidityCaller.consoleOutput[4]);

			Assert.AreEqual("Set weather (1 - 4): Reading requested, humidity is 100", weatherServer.consoleOutput[0]);
			Assert.AreEqual("Reading requested, humidity is 100", weatherServer.consoleOutput[1]);
			Assert.AreEqual("Reading requested, humidity is 100", weatherServer.consoleOutput[2]);
			Assert.AreEqual(string.Empty, weatherServer.consoleOutput[3]);

			Assert.AreEqual (false, humidityCaller.containsBrokenPrograms);
			Assert.AreEqual (false, weatherServer.containsBrokenPrograms);
		}

		[Test()]
		public void CallNonexistingFunction()
		{
			Computer computer7 = _world.tingRunner.CreateTing<Computer> ("Computer7", testDefaultCoordinate);
			Computer computer8 = _world.tingRunner.CreateTing<Computer> ("Computer8", testDefaultCoordinate);

			computer7.masterProgramName = "Computer7";
			computer8.masterProgramName = "BlankSlate";

			computer7.logger.AddListener(s => Console.WriteLine("Computer7: " + s));

			//Console.WriteLine ("\n\nAsserting programs");
			D.isNull (computer7.masterProgram, "master program is null");
			D.isNull (computer7.floppyBootProgram, "floppy boot program is null");

			//Console.WriteLine ("\n\nPrograms count: " + computer7.programs.Length);
//			foreach (var prog in computer7.programs) {
//				Console.WriteLine (" -" + prog.name + ": " + prog.sourceCodeName);
//			}

			computer7.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.AreEqual (true, computer7.containsBrokenPrograms);
			PrintErrors (computer7.masterProgram);

			Assert.AreEqual (false, computer8.containsBrokenPrograms);
			PrintErrors (computer8.masterProgram);
		}

		[Test()]
		public void CallExternalFunctionWithTooManyArgs()
		{
			Computer computer7 = _world.tingRunner.CreateTing<Computer> ("Computer7", testDefaultCoordinate);
			Computer computer8 = _world.tingRunner.CreateTing<Computer> ("Computer8", testDefaultCoordinate);
			
			computer7.masterProgramName = "Computer7TooManyArgs";
			computer8.masterProgramName = "BlankSlate";
			
			computer7.logger.AddListener(s => Console.WriteLine("Computer7: " + s));
			
			//Console.WriteLine ("\n\nAsserting programs");
			D.isNull (computer7.masterProgram, "master program is null");
			D.isNull (computer7.floppyBootProgram, "floppy boot program is null");

			computer7.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.AreEqual (true, computer7.containsBrokenPrograms);
			PrintErrors (computer7.masterProgram);
			
			Assert.AreEqual (false, computer8.containsBrokenPrograms);
			PrintErrors (computer8.masterProgram);
		}

		[Test()]
		public void CallExternalFunctionWithTooFewArgs()
		{
			Computer computer7 = _world.tingRunner.CreateTing<Computer> ("Computer7", testDefaultCoordinate);
			Computer computer8 = _world.tingRunner.CreateTing<Computer> ("Computer8", testDefaultCoordinate);
			
			computer7.masterProgramName = "Computer7TooFewArgs";
			computer8.masterProgramName = "BlankSlate";
			
			computer7.logger.AddListener(s => Console.WriteLine("Computer7: " + s));
			
			//Console.WriteLine ("\n\nAsserting programs");
			D.isNull (computer7.masterProgram, "master program is null");
			D.isNull (computer7.floppyBootProgram, "floppy boot program is null");
			
			computer7.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.AreEqual (true, computer7.containsBrokenPrograms);
			PrintErrors (computer7.masterProgram);
			
			Assert.AreEqual (false, computer8.containsBrokenPrograms);
			PrintErrors (computer8.masterProgram);
		}

		[Test()]
		public void CallExternalFunctionThatFails()
		{
			Computer computer7 = _world.tingRunner.CreateTing<Computer> ("Computer7", testDefaultCoordinate);
			Computer computer8 = _world.tingRunner.CreateTing<Computer> ("Computer8", testDefaultCoordinate);
			
			computer7.masterProgramName = "Computer7CallFailingFunction";
			computer8.masterProgramName = "Computer8";
			
			computer7.logger.AddListener(s => Console.WriteLine("Computer7: " + s));
			
			//Console.WriteLine ("\n\nAsserting programs");
			D.isNull (computer7.masterProgram, "master program is null");
			D.isNull (computer7.floppyBootProgram, "floppy boot program is null");
			
			computer7.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.AreEqual (true, computer7.containsBrokenPrograms);
			PrintErrors (computer7.masterProgram);
			
			Assert.AreEqual (true, computer8.containsBrokenPrograms);
			PrintErrors (computer8.masterProgram);

			Console.WriteLine("RESTORE TIME");

			// should be able to restore
			computer7.masterProgram.sourceCodeContent = "";
			computer7.RunProgram(null);
			Assert.AreEqual (false, computer7.containsBrokenPrograms);
		}

		[Test()]
		public void ElseHeartBreak()
		{
			Computer heartAnalyzer = _world.tingRunner.CreateTing<Computer> ("HeartAnalyzer", testDefaultCoordinate);
			Computer heart = _world.tingRunner.CreateTing<Computer> ("Heart", testDefaultCoordinate);
			
			heartAnalyzer.masterProgramName = "HeartBreaker";
			heart.masterProgramName = "Heart";
			heart.hasHeartAPI = true;

			heartAnalyzer.logger.AddListener(s => Console.WriteLine("Computer7: " + s));
			
			//Console.WriteLine ("\n\nAsserting programs");
			D.isNull (heartAnalyzer.masterProgram, "master program is null");
			D.isNull (heartAnalyzer.floppyBootProgram, "floppy boot program is null");
			
			heartAnalyzer.RunProgram (null);
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			PrintErrors (heartAnalyzer.masterProgram);
			PrintErrors (heart.masterProgram);

			Assert.AreEqual (true, heartAnalyzer.containsBrokenPrograms);
			Assert.AreEqual (true, heart.containsBrokenPrograms);

			WorldTestHelper.UpdateWorld (_world, 1f);

			heartAnalyzer.masterProgram.sourceCodeContent = "";
			heartAnalyzer.RunProgram(null); // should fix it
			heart.masterProgram.StartAtFunction("Print", new object[] { 42.0f }, null); // shouldn't fix it!

			WorldTestHelper.UpdateWorld (_world, 0.2f);

			Assert.AreEqual (false, heartAnalyzer.containsBrokenPrograms);
			Assert.AreEqual (true, heart.containsBrokenPrograms);

			heart.RunProgram(null);
			WorldTestHelper.UpdateWorld (_world, 1f);
			Assert.AreEqual (true, heart.containsBrokenPrograms);
		}

		[Test()]
		public void StartTheSameProgramOverAndOverAgain()
		{
			Computer computer = _world.tingRunner.CreateTing<Computer> ("Computer", testDefaultCoordinate);
			computer.masterProgramName = "Foo";

			for(int i = 0; i < 20; i++) {
				WorldTestHelper.UpdateWorld (_world, 1f);
				computer.RunProgram(null);
			}

			Assert.AreEqual("10", computer.consoleOutput[0]);
			Assert.AreEqual("10", computer.consoleOutput[1]);
			Assert.AreEqual("10", computer.consoleOutput[10]);
		}

		[Test()]
		public void ChangeSourceCodeOverAndOverAgain()
		{
			Computer computer = _world.tingRunner.CreateTing<Computer> ("Computer", testDefaultCoordinate);
			computer.masterProgramName = "Foo";

			_world.dialogueRunner.RunStringAsFunction("ListActivePrograms()");

			for(int i = 0; i < 200; i++) {
				WorldTestHelper.UpdateWorld (_world, 1f);
				computer.RunProgram(null);
				WorldTestHelper.UpdateWorld (_world, 1f);
				computer.masterProgram.sourceCodeContent = "Print(10)";
			}

			_world.dialogueRunner.RunStringAsFunction("ListActivePrograms()");

			for(int i = 0; i < 200; i++) {
				WorldTestHelper.UpdateWorld (_world, 1f);
				computer.RunProgram(null);
				WorldTestHelper.UpdateWorld (_world, 1f);
				computer.masterProgram.sourceCodeContent = "Print(10)";
			}

			_world.dialogueRunner.RunStringAsFunction("ListActivePrograms()");
			
			Assert.AreEqual("10", computer.consoleOutput[0]);
			Assert.AreEqual("10", computer.consoleOutput[1]);
			Assert.AreEqual("10", computer.consoleOutput[10]);
		}

		[Test()]
		public void ReadDataFromFloppy()
		{
			Computer computer = _world.tingRunner.CreateTing<Computer> ("ComputerWithFloppy", testDefaultCoordinate);
			
			computer.masterProgramName = "FloppyReader";

			Floppy floppy = _world.tingRunner.CreateTing<Floppy> ("Floppy", testDefaultCoordinate);
			floppy.masterProgram.sourceCodeContent = "hejsan";

			WorldTestHelper.UpdateWorld (_world, 0.5f);
			computer.RunProgram (floppy);
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			PrintErrors (computer.masterProgram);
			Assert.AreEqual (false, computer.containsBrokenPrograms);
			
			Assert.AreEqual("hejsan", computer.consoleOutput[0]);
		}

		[Test()]
		public void EnsurePrograms()
		{
			_world.programRunner.CreateProgram ("Bleh", "...", "Whatever");

			Computer computer = _world.tingRunner.CreateTing<Computer> ("Computer", testDefaultCoordinate);
			computer.logger.AddListener (s => Console.WriteLine ("LOGGER: " + s));
			Assert.AreEqual (0, computer.programs.Length);
			computer.EnsureProgram ("Foo1", "BlankSlate");
			D.Log ("---");
			Assert.AreEqual (1, computer.programs.Length);
			computer.EnsureProgram ("Foo2", "BlankSlate");
			Assert.AreEqual (2, computer.programs.Length);
			computer.EnsureProgram ("Foo3", "BlankSlate");
			Assert.AreEqual (3, computer.programs.Length);
		}

		[Test()]
		public void OptionalApiFunction()
		{
			Computer trapComputer = _world.tingRunner.CreateTing<Computer> ("TrapComputer", testDefaultCoordinate);

			_world.tingRunner.CreateTing<Character> ("Eve", trapComputer.position);

			trapComputer.masterProgramName = "TrapComputer";
			trapComputer.hasTrapAPI = true;
			trapComputer.RunProgram (null);

			WorldTestHelper.UpdateWorld (_world, 1f);
			_world.dialogueRunner.EventHappened ("Eve_hack_TrapComputer"); // trigger trap api!

			Assert.AreEqual (false, trapComputer.containsBrokenPrograms);
			PrintErrors (trapComputer.masterProgram);
		}

		[Test()]
		public void CallingFunctionWithTooFewArguments()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer.hasInternetAPI = true;

			computer.masterProgramName = "BlankSlate"; // temp
			computer.masterProgram.sourceCodeContent = "Connect()";

			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.IsTrue (computer.containsBrokenPrograms);
		}

		[Test()]
		public void CallingFunctionWrongType()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer.hasInternetAPI = true;

			computer.masterProgramName = "BlankSlate"; // temp
			computer.masterProgram.sourceCodeContent = "Connect(Print(42))";

			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.IsTrue (computer.containsBrokenPrograms);
		}

		[Test()]
		public void WhatFredrikDid()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer.hasInternetAPI = true;

			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"var a = [10,20,30]
                  Print(a(0))";

			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.IsTrue (computer.containsBrokenPrograms);
		}

		[Test()]
		public void UsingFunctionAsArray()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer.hasInternetAPI = true;

			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"void F()
                  end
                  Print(F[10])";

			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			PrintErrors(computer.masterProgram);

			Assert.IsTrue (computer.containsBrokenPrograms);
		}

		[Test()]
		public void ProblemWithLinesCommand()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer.hasGraphicsAPI = true;
			
			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"Lines([10,20,30,40,50,60])";

			var lines = new List<IntPoint>();
			computer.onLineDrawing = (p1, p2) => {
				lines.Add(p1);
				lines.Add(p2);
			};

			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.IsFalse (computer.containsBrokenPrograms);
			Assert.AreEqual(4, lines.Count);
			Assert.AreEqual(new IntPoint(10,20), lines[1]);
			Assert.AreEqual(new IntPoint(30,40), lines[0]);
			Assert.AreEqual(new IntPoint(30,40), lines[3]);
			Assert.AreEqual(new IntPoint(50,60), lines[2]);
		}

		[Test()]
		public void ProblemWithLinesCommandCrash()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			computer.hasGraphicsAPI = true;
			
			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"Lines(10,20,30,40,50,60)";

			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.IsTrue (computer.containsBrokenPrograms);
		}

		[Test()]
		public void DuplicatedVariableName()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);

			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"var x
                  var x";
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.IsTrue (computer.containsBrokenPrograms);
		}

		[Test()]
		public void ConvertingExpressionBasedOnDeclaration()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			
			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"number x = '10'
                  Print(x + x)";
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.IsFalse (computer.containsBrokenPrograms);
			Assert.AreEqual("20", computer.consoleOutput[0]);
		}

		[Test()]
		public void DefaultValues()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			
			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"
number x
string y
bool z
Print(x)
Print(y)
Print(z)
";
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			Assert.IsFalse (computer.containsBrokenPrograms);
			Assert.AreEqual("0", computer.consoleOutput[0]);
			Assert.AreEqual("", computer.consoleOutput[1]);
			Assert.AreEqual("false", computer.consoleOutput[2]);
		}

		[Test()]
		public void ToLowercase()
		{
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			
			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"
Print(ToLowercase('ErIK!A'))

string ToLowercase(var text)
    string res = ''
    loop c in text
        if IsUppercase(c)
            res += IntToChar(CharToInt(c) + 32)
        else
            res += c
        end
    end
    return res
end

bool IsUppercase(var c)
    return CharToInt(c) >= -32 && CharToInt(c) <= -7
end
";
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			PrintErrors(computer.masterProgram);

			Assert.IsFalse (computer.containsBrokenPrograms);
			Assert.AreEqual("erik!a", computer.consoleOutput[0]);
		}

		[Test()]
		public void PathfindingProgram()
		{
			foreach(string s in _world.Preload()) {}

			var computer = _world.tingRunner.CreateTing<Computer> ("Computer1", testDefaultCoordinate);
			var computer2 = _world.tingRunner.CreateTing<Computer> ("Computer2", testDefaultCoordinate);

			var p = new MimanPathfinder2(_world.tingRunner, _world.roomRunner);
			p.RecreateRoomNetwork();

			computer2.masterProgramName = "BlankSlate";

			computer.hasDoorAPI = true;
			computer.masterProgramName = "BlankSlate";
			computer.masterProgram.sourceCodeContent = 
				@"
					var path = FindPath('Computer1', 'Computer2')
					Print('Path: ' + path)
				";
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 5f); // can take up to 4 secs because of random

			PrintOutput(computer);
			PrintErrors(computer.masterProgram);
			
			Assert.IsFalse (computer.containsBrokenPrograms);
			Assert.AreEqual("Path: []", computer.consoleOutput[0]);
		}

		[Test()]
		public void HackdevAllowFunction()
		{
			MimanPathfinder2.ClearRoomNetwork();
			
			var h = _world.tingRunner.CreateTing<Hackdev> ("Hackdev", testDefaultCoordinate);
			
			h.masterProgramName = "BlankSlate";

			h.masterProgram.sourceCodeContent = 
				@"
bool Allow(string target, number level)
    Log('calling allow, level: ' + level)
    return level == 0
end
				";

			h.PrepareForBeingHacked ();
			h.masterProgram.Compile();

			PrintErrors(h.masterProgram);

			WorldTestHelper.UpdateWorld (_world, 3.0f);

			string msg = null;
			D.onDLog += (pMessage) => msg = pMessage;

			h.masterProgram.StartAtFunctionIfItExists("BLAHAHAHAHA", new object[] { "blah", 10 }, null);

			h.masterProgram.StartAtFunction("Allow", new object[] { "blah", 10 }, null);
			WorldTestHelper.UpdateWorld (_world, 1f);

			Assert.AreEqual("LOG: calling allow, level: 10", msg);
			Assert.IsTrue(h.masterProgram.HasFunction("Allow", true));
		}

		[Test()]
		public void CallPrintWithHugeNegativeNumber()
		{
			MimanPathfinder2.ClearRoomNetwork();
			
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer", testDefaultCoordinate);
			
			computer.masterProgramName = "BlankSlate";
			
			computer.masterProgram.sourceCodeContent = 
				@"
Print(-999999)
Print(-999999999999999999999)
Print(999999999999999999999)
				";
			
			computer.PrepareForBeingHacked ();
			computer.masterProgram.Compile();
			
			PrintErrors(computer.masterProgram);
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			//string msg = null;
			//D.onDLog += (pMessage) => msg = pMessage;

			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);

			PrintErrors (computer.masterProgram);

			Assert.IsFalse(computer.containsBrokenPrograms);
			Assert.AreEqual("-999999", computer.consoleOutput[0]);
		}

		[Test()]
		public void ReturnHugeNegativeNumber()
		{
			MimanPathfinder2.ClearRoomNetwork();
			
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer", testDefaultCoordinate);
			
			computer.masterProgramName = "BlankSlate";
			
			computer.masterProgram.sourceCodeContent = 
				@"
number f()
  return -9634634525242342
end

var x = f()

Print(x)
				";
			
			computer.PrepareForBeingHacked ();
			computer.masterProgram.Compile();
			
			PrintErrors(computer.masterProgram);
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			//string msg = null;
			//D.onDLog += (pMessage) => msg = pMessage;
			
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 1f);
			
			PrintErrors (computer.masterProgram);
			
			Assert.IsFalse(computer.containsBrokenPrograms);
			Assert.AreEqual("-9.634635E+15", computer.consoleOutput[0]);
		}

		[Test()]
		public void MachineProgram1()
		{
			var machine = _world.tingRunner.CreateTing<Machine> ("Machine", testDefaultCoordinate);
			var computer = _world.tingRunner.CreateTing<Computer> ("Computer", testDefaultCoordinate);
			var goods = _world.tingRunner.CreateTing<Goods> ("Goods", testDefaultCoordinate);

			machine.currentGoods = goods;
			goods.minerals = "zzzzazzzbzzzazzz".ToCharArray();
			
			computer.masterProgramName = "BlankSlate";
			
			computer.masterProgram.sourceCodeContent = 
				@"
var machine = Connect('Machine')
string content = machine.Analyze()
Print(content)
				";
			
			computer.PrepareForBeingHacked ();
			computer.masterProgram.Compile();
			
			PrintErrors(computer.masterProgram);
			
			WorldTestHelper.UpdateWorld (_world, 1f);
			
//			string msg = null;
//			D.onDLog += Conso
			
			computer.masterProgram.Start();
			WorldTestHelper.UpdateWorld (_world, 5f);
			
			PrintErrors (computer.masterProgram);

			Assert.IsFalse(computer.masterProgram.waitingForInput);
			Assert.IsFalse(computer.masterProgram.isOn);

			Assert.AreEqual("zzzzazzzbzzzazzz", computer.consoleOutput[0]);
			Assert.AreEqual(0, computer.masterProgram.GetErrors().Length);
		}
	}
}

