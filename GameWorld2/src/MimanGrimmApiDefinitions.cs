//#define DEEP

using System;
using System.Collections.Generic;
using GrimmLib;
using TingTing;
using GameTypes;
using Pathfinding;
using ProgrammingLanguageNr1;
using System.Reflection;
using System.Text;
using System.Linq;

namespace GameWorld2
{
	public class MimanGrimmApiDefinitions
	{
		World _world;
		
		static ShowInEditor SHOW_IN_EDITOR = new ShowInEditor();
		static EditableInEditor EDITABLE_IN_EDITOR = new EditableInEditor();

		public static string cheat = "";
		
		public MimanGrimmApiDefinitions(World pWorld)
		{
			_world = pWorld;
		}
		
		#region FUNCTIONS
		
		public void RegisterFunctions()
		{
			_world.dialogueRunner.AddFunction("Hint", Hint);
			_world.dialogueRunner.AddFunction("Story", Story);
			_world.dialogueRunner.AddFunction("GetActiveNodes", GetActiveNodes);
			_world.dialogueRunner.AddFunction("SetFocus", SetFocus);
			_world.dialogueRunner.AddFunction("WP", WP);
			_world.dialogueRunner.AddFunction("Log", Log);
			_world.dialogueRunner.AddFunction("Path", Path);
			_world.dialogueRunner.AddFunction("RebuildRoomNetwork", RebuildRoomNetwork);
			_world.dialogueRunner.AddFunction("GetRoomExits", GetRoomExits);
			_world.dialogueRunner.AddFunction("TilePath", TilePath);
			_world.dialogueRunner.AddFunction("StartLogging", StartLogging);
			_world.dialogueRunner.AddFunction("Kill", Kill);
			_world.dialogueRunner.AddFunction("DetachFromNavNode", DetachFromNavNode);
			_world.dialogueRunner.AddFunction("Pos", Pos);
			_world.dialogueRunner.AddFunction("WorldPos", WorldPos);
			_world.dialogueRunner.AddFunction("CarefulPos", CarefulPos);
			_world.dialogueRunner.AddFunction("CarefulLayInBed", CarefulLayInBed);
			_world.dialogueRunner.AddFunction("Dir", Dir);
			_world.dialogueRunner.AddFunction("Load", Load);
			_world.dialogueRunner.AddFunction("StartAction", StartAction);
			_world.dialogueRunner.AddFunction("StopAction", StopAction);
			_world.dialogueRunner.AddFunction("Interact", Interact);
			_world.dialogueRunner.AddFunction("GetUpAndInteract", GetUpAndInteract);
			_world.dialogueRunner.AddFunction("InteractUsingHandItem", InteractUsingHandItem);
			_world.dialogueRunner.AddFunction("UseHandItem", UseHandItem);
			_world.dialogueRunner.AddFunction("TakeOutItem", TakeOutItem);
			_world.dialogueRunner.AddFunction("ClearHandItem", ClearHandItem);
			_world.dialogueRunner.AddFunction("PutIntoInventory", PutIntoInventory);
			_world.dialogueRunner.AddFunction("Tase", Tase);
			_world.dialogueRunner.AddFunction("GetTasedGently", GetTasedGently);
			_world.dialogueRunner.AddFunction("PickUp", PickUp);
			_world.dialogueRunner.AddFunction("Walk", Walk);
			_world.dialogueRunner.AddFunction("GetUpFromSeat", GetUpFromSeat);
			_world.dialogueRunner.AddFunction("CancelWalking", CancelWalking);
			_world.dialogueRunner.AddFunction("TurnLeft", TurnLeft);
			_world.dialogueRunner.AddFunction("Give", Give);
			_world.dialogueRunner.AddFunction("SetClockSpeed", SetClockSpeed);
			_world.dialogueRunner.AddFunction("God", God);
			_world.dialogueRunner.AddFunction("SetAvatar", SetAvatar);
			_world.dialogueRunner.AddFunction("FindAvatar", FindAvatar);
			_world.dialogueRunner.AddFunction("LockDoor", LockDoor);
			_world.dialogueRunner.AddFunction("UnlockDoor", UnlockDoor);
			_world.dialogueRunner.AddFunction("UseForRoomPathfinding", UseForRoomPathfinding);
			_world.dialogueRunner.AddFunction("MuteNotifications", MuteNotifications);
			_world.dialogueRunner.AddFunction("StartRinging", StartRinging);
			_world.dialogueRunner.AddFunction("StartAllRadios", StartAllRadios);
			_world.dialogueRunner.AddFunction("StartAllFuseboxes", StartAllFuseboxes);
			_world.dialogueRunner.AddFunction("SetDoorTarget", SetDoorTarget);
			_world.dialogueRunner.AddFunction("Sleep", Sleep);
			_world.dialogueRunner.AddFunction("SleepUntil", SleepUntil);
			//_world.dialogueRunner.AddFunction("KnockDown", KnockDown);
			_world.dialogueRunner.AddFunction("BeBored", BeBored);
			_world.dialogueRunner.AddFunction("WakeUp", WakeUp);
			_world.dialogueRunner.AddFunction("SetClock", SetClock);
			_world.dialogueRunner.AddFunction("SetHour", SetHour);
			_world.dialogueRunner.AddFunction("StartMusic", StartMusic);
			_world.dialogueRunner.AddFunction("StopMusic", StopMusic);
			_world.dialogueRunner.AddFunction("SetChannelOnRadio", SetChannelOnRadio);
			_world.dialogueRunner.AddFunction("SetFriendLevel", SetFriendLevel);
			_world.dialogueRunner.AddFunction("SetCorruption", SetCorruption);
			_world.dialogueRunner.AddFunction("StartTalking", StartTalking);
			_world.dialogueRunner.AddFunction("StopTalking", StopTalking);
			_world.dialogueRunner.AddFunction("StartWaitForGift", StartWaitForGift);
			_world.dialogueRunner.AddFunction("StopWaitForGift", StopWaitForGift);
			_world.dialogueRunner.AddFunction("SetKnowledge", SetKnowledge);
			_world.dialogueRunner.AddFunction("SetCameraAutoRotateSpeed", SetCameraAutoRotateSpeed);
			_world.dialogueRunner.AddFunction("SetTimetable", SetTimetable);
			_world.dialogueRunner.AddFunction("SetTimetableTimer", SetTimetableTimer);
			_world.dialogueRunner.AddFunction("SetNeverGetsTired", SetNeverGetsTired);
			_world.dialogueRunner.AddFunction("CreateCharacter", CreateCharacter);
			_world.dialogueRunner.AddFunction("CreateBeerInHand", CreateBeerInHand);
			_world.dialogueRunner.AddFunction("CreateDrinkInHand", CreateDrinkInHand);
			_world.dialogueRunner.AddFunction("CreateCoffeeInHand", CreateCoffeeInHand);
			_world.dialogueRunner.AddFunction("CreateCigarette", CreateCigarette);
			_world.dialogueRunner.AddFunction("SetHandItem", SetHandItem);
			_world.dialogueRunner.AddFunction("DropHandItem", DropHandItem);
			_world.dialogueRunner.AddFunction("PutAwayHandItem", PutAwayHandItem);
			_world.dialogueRunner.AddFunction("SetRain", SetRain);
			_world.dialogueRunner.AddFunction("SetFieldToStringArray", SetFieldToStringArray);
			_world.dialogueRunner.AddFunction("SetFieldToFloat", SetFieldToFloat);
			_world.dialogueRunner.AddFunction("GetMasterProgramStatus", GetMasterProgramStatus);
			_world.dialogueRunner.AddFunction("RunMasterProgram", RunMasterProgram);
			_world.dialogueRunner.AddFunction("RunMasterProgramOnAllComputersInRoom", RunMasterProgramOnAllComputersInRoom);
			_world.dialogueRunner.AddFunction("SetCode", SetCode);
			_world.dialogueRunner.AddFunction("SetCodeAndRun", SetCodeAndRun);
			_world.dialogueRunner.AddFunction("SetResettableCode", SetResettableCode);
			_world.dialogueRunner.AddFunction("SetMemory", SetMemory);
			_world.dialogueRunner.AddFunction("RunFunctionOnComputer", RunFunctionOnComputer);
			_world.dialogueRunner.AddFunction("Throw", o => { throw new Exception(o[0]); });
			_world.dialogueRunner.AddFunction("RunMakeTransactionFunctionOnCreditCard", RunMakeTransactionFunctionOnCreditCard);
			_world.dialogueRunner.AddFunction("CharacterTakesMoneyFromCreditCard", CharacterTakesMoneyFromCreditCard);
			_world.dialogueRunner.AddFunction("CheckMoney", CheckMoney);
			_world.dialogueRunner.AddFunction("Info", Info);
			_world.dialogueRunner.AddFunction("Bugtalk", Bugtalk);
			_world.dialogueRunner.AddFunction("GetOutput", GetOutput);
			_world.dialogueRunner.AddFunction("GetSource", GetSource);
			_world.dialogueRunner.AddFunction("GetProgramErrors", GetProgramErrors);
			_world.dialogueRunner.AddFunction("RemoveDanglingDialogueOptions", RemoveDanglingDialogueOptions);
			_world.dialogueRunner.AddFunction("Hack", Hack);
			_world.dialogueRunner.AddFunction("PrintGroupOfTile", PrintGroupOfTile);
			_world.dialogueRunner.AddFunction("CheckNavNodeChain", CheckNavNodeChain);
			_world.dialogueRunner.AddFunction("RunPathfindingTests", RunPathfindingTests);
			_world.dialogueRunner.AddFunction("MoveAllTingsToTing", MoveAllTingsToTing);
			_world.dialogueRunner.AddFunction("TypeIntoComputer", TypeIntoComputer);
			_world.dialogueRunner.AddFunction("Explode", Explode);
			_world.dialogueRunner.AddFunction("SetRunning", SetRunning);
			_world.dialogueRunner.AddFunction("RunLevelIntegrityTests", RunLevelIntegrityTests);
			_world.dialogueRunner.AddFunction("SetLanguage", SetLanguage);
			_world.dialogueRunner.AddFunction("ListTingsOfType", ListTingsOfType);
			_world.dialogueRunner.AddFunction("ListDigitalTrash", ListDigitalTrash);
			_world.dialogueRunner.AddFunction("ListActiveNodes", ListActiveNodes);
			_world.dialogueRunner.AddFunction("ListActivePrograms", ListActivePrograms);
			_world.dialogueRunner.AddFunction("ListAllPrograms", ListAllPrograms);
			_world.dialogueRunner.AddFunction("KillAllPrograms", KillAllPrograms);
			_world.dialogueRunner.AddFunction("TurnOnTv", TurnOnTv);
			_world.dialogueRunner.AddFunction("StoryItemsSanityTests", StoryItemsSanityTests);
			_world.dialogueRunner.AddFunction("SetCameraTarget", SetCameraTarget);
			_world.dialogueRunner.AddFunction("Beat", Beat);
			_world.dialogueRunner.AddFunction("HeartIsBroken", HeartIsBroken);
			_world.dialogueRunner.AddFunction("SetAllComputersToRunProgram", SetAllComputersToRunProgram);
			_world.dialogueRunner.AddFunction("GetAngryAtComputer", GetAngryAtComputer);
			_world.dialogueRunner.AddFunction("StopSimulation", StopSimulation);
		}

		public Action<string> onRemoveDanglingDialogueOptions;

		private void StopSimulation(string [] args) {
			_world.paused = true;
		}

		// RemoveDanglingDialogueOptions(string conversationName)
		public void RemoveDanglingDialogueOptions(string[] args)
		{
			if (onRemoveDanglingDialogueOptions != null) {
				string conversationName = args[0];
				D.Log("Will remove dangling dialogue options");
				var activeBranchingNode = _world.dialogueRunner.GetActiveBranchingDialogueNode(conversationName);
				if (activeBranchingNode != null) {
					activeBranchingNode.Stop();
				}
				onRemoveDanglingDialogueOptions(conversationName);
			} else {
				D.Log("onRemoveDanglingDialogueOptions in MimanGrimmApiDefinitions is null!");
			}
		}

		// PrintGroupOfTile(string pRoom, int x, int y) OR PrintGroupOfTile(Ting ting)
		private void PrintGroupOfTile(string[] args) {
			if(args.Length == 3) {
				D.Log(_world.roomRunner.GetRoom(args[0]).GetTile(Convert.ToInt32(args[1]), Convert.ToInt32(args[2])).group.ToString());
			} else {
				D.Log(_world.tingRunner.GetTing(args[0]).tile.group.ToString());
			}
		}

		private void ListTingsOfType(string[] args) {
			Type t = null;

			switch (args[0]) {
			case "Radio": t = typeof(Radio); break;
			case "Tv": t = typeof(Tv); break;
			case "Character": t = typeof(Character); break;
			case "Drink": t = typeof(Drink); break;
			case "MysticalCube": t = typeof(MysticalCube); break;
			case "Key": t = typeof(Key); break;
			case "Map": t = typeof(Map); break;
			case "Snus": t = typeof(Snus); break;
			case "Drug": t = typeof(Drug); break;
			case "Lamp": t = typeof(Lamp); break;
			case "Hackdev": t = typeof(Hackdev); break;
			case "MusicBox": t = typeof(MusicBox); break;
			case "Door": t = typeof(Door); break;
			case "CreditCard": t = typeof(CreditCard); break;
			case "Computer": t = typeof(Computer); break;
			case "Locker": t = typeof(Locker); break;
			case "Robot": t = typeof(Robot); break;
			case "Seat": t = typeof(Seat); break;
			case "Sink": t = typeof(Sink); break;
			case "FuseBox": t = typeof(FuseBox); break;
			case "Portal": t = typeof(Portal); break;
			default:
				throw new Exception ("No support for type " + args [0]);				
			}

			foreach (Ting ting in _world.tingRunner.GetTings()) {
				if (ting.GetType() == t) {
					D.Log (ting.name + "\t" + ting.room.name + "\t | Prefab: " + ting.prefab);
				}
			}
		}

		void ListDigitalTrash(string [] args) {
			D.Log("List digital trash: ");
			for(int i = 0; i < 70; i++) {
				string name = "DigitalTrash" + i;
//				var digitalTrashes = _world.tingRunner.GetTings().Select(t => {
//					return (t is Floppy) && ((t as Floppy).masterProgramName == name);
//				});
				var digitalTrashes = from floppy in _world.tingRunner.GetTings()
									 where ((floppy is Floppy) && (floppy as Floppy).masterProgramName == name)
						             select floppy;
				string stars = "";
				for(int j = 0; j < digitalTrashes.Count(); j++) {
					stars += "*";
				}
				D.Log("Nr of '" + name + "': " + digitalTrashes.Count() + " " + stars);
			}
			D.Log("-------------------------------------------------");
		}

		void ListActiveNodes(string [] args) {
			string conversation = args[0];

			var active = from node in _world.dialogueRunner.GetAllNodes()
				         where (node.conversation == conversation && node.isOn)
				         select node;

			D.Log("Active nodes in '" + conversation + "':");
			foreach(var node in active) {
				D.Log(node.name + " " + node.ToString());
			}

			D.Log("-------------------------------------------------");
		}

		void ListActivePrograms(string [] args) {

			D.Log("-------------------------------------------------");
			D.Log("Active programs:");

			foreach(var prog in _world.programRunner.GetAllPrograms()) {
				if(prog.isOn) {
					D.Log(prog.ToString() + " (" + prog.executionsPerFrame + " mhz)");
				}
			}
			
			D.Log("-------------------------------------------------");

			PrintProgramsReport ();

		}

		void ListAllPrograms(string [] args) {
			
			D.Log("-------------------------------------------------");
			D.Log("All programs:");
			
			foreach(var prog in _world.programRunner.GetAllPrograms()) {
				D.Log(prog.ToString() + " (" + prog.executionsPerFrame + " mhz)");
			}
			
			D.Log("-------------------------------------------------");
			
			PrintProgramsReport ();

			D.Log("Total nr of programs: " + _world.programRunner.GetAllPrograms().Length);
		}

		void PrintProgramsReport ()
		{
			D.Log ("Total nr of programs = " + _world.programRunner.GetAllPrograms ().Count ());
			D.Log ("Nr of memory spaces = " + MemorySpace.nrOfMemorySpacesInMemory);
			D.Log ("Nr of scopes = " + Scope.nrOfScopesInMemory);
			D.Log ("Nr of sprak runners = " + SprakRunner.nrOfSprakRunnersInMemory);
			D.Log ("Nr of interpreters = " + InterpreterTwo.nrOfInterpreters);
			D.Log ("Nr of AST:s = " + AST.nrOfASTsInMemory);
			D.Log("COLLECT");
			GC.Collect ();
			GC.WaitForPendingFinalizers();
			D.Log ("Total nr of programs = " + _world.programRunner.GetAllPrograms ().Count ());
			D.Log ("Nr of memory spaces = " + MemorySpace.nrOfMemorySpacesInMemory);
			D.Log ("Nr of scopes = " + Scope.nrOfScopesInMemory);
			D.Log ("Nr of sprak runners = " + SprakRunner.nrOfSprakRunnersInMemory);
			D.Log ("Nr of interpreters = " + InterpreterTwo.nrOfInterpreters);
			D.Log ("Nr of AST:s = " + AST.nrOfASTsInMemory);
		}

		void KillAllPrograms(string [] args) {
			foreach(var prog in _world.programRunner.GetAllPrograms()) {
				//prog.SetSprakRunnerToNull();
				//prog.StopAndReset();
//				if(prog.sprakRunner != null) {
//					prog.sprakRunner.HardReset();
//				}
			}
			PrintProgramsReport();
		}

		// StoryItemsSanityTests()
		void StoryItemsSanityTests (string[] args)
		{
			// Finance computer
			var financeComputer = _world.tingRunner.GetTing<Computer> ("FinanceComputer");
			D.assert (financeComputer.masterProgramName == "FinanceComputer", "FinanceComputer program");

			// Hotel
			var hotelLobbyComputer = _world.tingRunner.GetTing<Computer> ("Hotel_Lobby_ComputerCashier");
			D.assert (hotelLobbyComputer.masterProgramName == "HotelLobbyComputer", "Hotel_Lobby_ComputerCashier program");

			var doorToBasement = _world.tingRunner.GetTing<Door> ("Hotel_Office_DoorToBasement");
			D.assert (doorToBasement.isLocked, "Hotel_Office_DoorToBasement not locked");

			// Trials
			var lodgeComputer1 = _world.tingRunner.GetTing<Computer> ("Computer1");
			D.assert (lodgeComputer1.masterProgramName == "Lodge_Room1_Computer1", "Computer 1 program");

			var lodgeComputer2 = _world.tingRunner.GetTing<Computer> ("Computer2");
			D.assert (lodgeComputer2.masterProgramName == "Lodge_Room1_Computer2", "Computer 2 program");

			var lodgeComputer3 = _world.tingRunner.GetTing<Computer> ("Computer3");
			D.assert (lodgeComputer3.masterProgramName == "Lodge_Room1_Computer3", "Computer 3 program");

			var lodgeComputer5 = _world.tingRunner.GetTing<Computer> ("Computer5");
			D.assert (lodgeComputer5.masterProgramName == "Lodge_Room1_Computer5", "Computer 5 program");

			var lodgeComputer6 = _world.tingRunner.GetTing<Computer> ("Computer6");
			D.assert (lodgeComputer6.masterProgramName == "Lodge_Room1_Computer6", "Computer 6 program");

			var lodgeComputer7 = _world.tingRunner.GetTing<Computer> ("Computer7");
			D.assert (lodgeComputer7.masterProgramName == "Lodge_Room1_Computer7", "Computer 7 program");

			var lodgeDoorToRoom2 = _world.tingRunner.GetTing<Door> ("Lodge_Room1_DoorToRoom2");
			D.assert (lodgeDoorToRoom2.isLocked, "lodge door to room2 locked");
			D.assert (lodgeDoorToRoom2.targetDoorName == "Lodge_Room2_DoorToRoom1", "lodge door to room2 target");

			var fishGame = _world.tingRunner.GetTing<Button> ("FishGameStartButton");
			D.assert (fishGame.masterProgramName == "FishGameButton", "FishGameStartButton program");


			// Factory mission
			var floppy100 = _world.tingRunner.GetTing<Floppy> ("Floppy_100");
			D.assert (floppy100.masterProgramName == "StrangeDataFloppy", "StrangeDataFloppy program");

			var factoryTrap = _world.tingRunner.GetTing<Computer> ("FactoryLobbyTrap");
			D.assert (factoryTrap.masterProgramName == "FactoryLobbyTrap", "FactoryLobbyTrap program");
			D.assert (factoryTrap.hasTrapAPI == true, "FactoryLobbyTrap trap API");

			var factoryServerDoor = _world.tingRunner.GetTing<Computer> ("FactoryServerDoor");
			D.assert (factoryServerDoor.masterProgramName == "FactoryServerDoor", "FactoryServerDoor program");

			var factoryAccessComputer = _world.tingRunner.GetTing<Computer> ("FactoryAccessComputer");
			D.assert (factoryAccessComputer.masterProgramName == "FactoryAccessComputer", "FactoryAccessComputer program");

			// Longson wife
			var playStation = _world.tingRunner.GetTing<Computer> ("LongsonPlaystation");
			D.assert (playStation.masterProgramName == "LongsonPlaystation", "LongsonPlaystation program");

			var doorToLongson = _world.tingRunner.GetTing<Door> ("OutsideFelixApartment_Door_2");
			D.assert (doorToLongson.isLocked, "OutsideFelixApartment_Door_2 not locked");

			// Soda storage
			var doorToStorage = _world.tingRunner.GetTing<Door> ("HarborWest_DoorToSodaStorage");
			D.assert (doorToStorage.isLocked, "HarborWest_DoorToSodaStorage not locked");

			var sodaStorageComputer = _world.tingRunner.GetTing<Computer> ("HarborWest_SodaStorageComputer");
			D.assert (sodaStorageComputer.masterProgramName == "SodaStorageDoor", "HarborWest_SodaStorageComputer program");

			// Trams

			var tram1computer = _world.tingRunner.GetTing<Computer> ("TramComputer1");
			D.assert (tram1computer.masterProgramName == "TramNr1", "tram1 computer program");

			var tram2computer = _world.tingRunner.GetTing<Computer> ("TramComputer2");
			D.assert (tram2computer.masterProgramName == "TramNr1b", "tram2 computer program");

			var tram3computer = _world.tingRunner.GetTing<Computer> ("TramComputer3");
			D.assert (tram3computer.masterProgramName == "TramNr1c", "tram3 computer program");

			var tram4computer = _world.tingRunner.GetTing<Computer> ("TramComputer4");
			D.assert (tram4computer.masterProgramName == "TramNr1d", "tram4 computer program");


			// Heart stuff
			var monadDoor = _world.tingRunner.GetTing<Door> ("Plaza_DoorToMonadsApartment");
			D.assert (monadDoor.targetDoorName == "MonadsApartment_DoorToPlaza" , "monadDoor has invalid target");
			D.assert (monadDoor.isLocked, "monadDoor lock");
			D.assert (monadDoor.code == 38984312, "monadDoor code");

			var bookDoor = _world.tingRunner.GetTing<Door> ("SICP");
			D.assert (bookDoor.targetDoorName == "Internet_Door_13" , "bookDoor has invalid target");

			var theHeart = _world.tingRunner.GetTing<Computer> ("Heart");
			D.assert (theHeart.masterProgramName == "TheHeart", "The heart program");

			var heartAnalyzer = _world.tingRunner.GetTing<Computer> ("Internet_Heart_Analyzer");
			D.assert (heartAnalyzer.masterProgramName == "HeartAnalyzer", "The Internet_Heart_Analyzer program");



			Console.ForegroundColor = ConsoleColor.Green;
			D.Log ("All StoryItemsSantiyTests ran");
			Console.ForegroundColor = ConsoleColor.White;
		}

		// RunLevelIntegrityTests()
		void RunLevelIntegrityTests (string[] args)
		{
			Tram tram1 = _world.tingRunner.GetTing<Tram> ("Tram1");
			Tram tram1b = _world.tingRunner.GetTing<Tram> ("Tram1b");
			Tram tram1c = _world.tingRunner.GetTing<Tram> ("Tram1c");
			Tram tram1d = _world.tingRunner.GetTing<Tram> ("Tram1d");

			D.isNull (tram1.movingDoor, "missing door tram1");
			D.isNull (tram1b.movingDoor, "missing door tram1b");
			D.isNull (tram1c.movingDoor, "missing door tram1c");
			D.isNull (tram1d.movingDoor, "missing door tram1d");

			D.isNull (tram1.movingDoor.targetDoor, "tram1 moving door missing target");
			D.isNull (tram1b.movingDoor.targetDoor, "tram1b moving door missing target");
			D.isNull (tram1c.movingDoor.targetDoor, "tram1c moving door missing target");
			D.isNull (tram1d.movingDoor.targetDoor, "tram1d moving door missing target");

			D.isNull (tram1.currentNavNode, "missing current nav node tram1");
			D.isNull (tram1b.currentNavNode, "missing current nav node tram1b");
			D.isNull (tram1c.currentNavNode, "missing current nav node tram1c");
			D.isNull (tram1d.currentNavNode, "missing current nav node tram1d");

			var channel7 = _world.tingRunner.GetTing<MusicBox>("RadioStation_Channel7");
			D.isNull (channel7, "channel 7");
			D.assert(channel7.isPlaying, "channel 7 not playing");
			D.assert(channel7.soundName == "Station7", "channel 7 wrong sound");

			var channel100 = _world.tingRunner.GetTing<MusicBox>("RadioStation_Channel100");
			D.isNull (channel100, "channel 100");
			D.assert(channel100.isPlaying, "channel 100 not playing");
			D.assert(channel100.soundName == "ExperimentStation", "channel 100 wrong sound");

			Door hotelDoor1 = _world.tingRunner.GetTing<Door> ("Hotel_Corridor_Door1");
			D.assert(hotelDoor1.isLocked, "hotel door 1 lock");
			Door hotelDoor2 = _world.tingRunner.GetTing<Door> ("Hotel_Corridor_Door2");
			D.assert(hotelDoor2.isLocked, "hotel door 2 lock");
			Door hotelDoor3 = _world.tingRunner.GetTing<Door> ("Hotel_Corridor_Door3");
			D.assert(hotelDoor3.isLocked, "hotel door 3 lock");
			Door hotelDoor4 = _world.tingRunner.GetTing<Door> ("Hotel_Corridor_Door4");
			D.assert(hotelDoor4.isLocked, "hotel door 4 lock");
			Door hotelDoor5 = _world.tingRunner.GetTing<Door> ("Hotel_Corridor_Door5");
			D.assert(hotelDoor5.isLocked, "hotel door 5 lock");

			Door ministryElevatorDoor1 = _world.tingRunner.GetTing<Door> ("Ministry_Elevator1_Door1");
			Door ministryElevatorDoor2 = _world.tingRunner.GetTing<Door> ("Ministry_Elevator2_Door1");
			Door ministryElevatorDoor3 = _world.tingRunner.GetTing<Door> ("Ministry_Elevator3_Door1");
			Door ministryElevatorDoor4 = _world.tingRunner.GetTing<Door> ("Ministry_Elevator4_Door1");

			D.assert(ministryElevatorDoor1.targetDoorName == "Ministry_Lobby_MinistryElevatorDoor_1", "ministry elevator target 1");
			D.assert(ministryElevatorDoor2.targetDoorName == "Ministry_Lobby_MinistryElevatorDoor_2", "ministry elevator target 2");
			D.assert(ministryElevatorDoor3.targetDoorName == "Ministry_Lobby_MinistryElevatorDoor_3", "ministry elevator target 3");
			D.assert(ministryElevatorDoor4.targetDoorName == "Ministry_Lobby_MinistryElevatorDoor_4", "ministry elevator target 4");

			D.assert(3 == ministryElevatorDoor1.elevatorAlternatives.Length, "ministry elevator 1 alternatives");
			D.assert(4 == ministryElevatorDoor2.elevatorAlternatives.Length, "ministry elevator 2 alternatives");
			D.assert(4 == ministryElevatorDoor3.elevatorAlternatives.Length, "ministry elevator 3 alternatives");
			D.assert(3 == ministryElevatorDoor4.elevatorAlternatives.Length, "ministry elevator 4 alternatives");

			CountNrOfUnitializedPrograms<Floppy> ("BlankSlate", floppy => floppy.masterProgramName);
			CountNrOfUnitializedPrograms<Computer> ("HelloWorld", computer => computer.masterProgramName);

			//D.Log ("\n\nLOOKING THROUGH DOORS");
			int doorsWithNullTarget = 0;
			var doors = _world.tingRunner.GetTingsOfType<Door> ();
			foreach (var door in doors) {
				if (door.targetDoor == null && !IsInTestingRoom(door)) {
					doorsWithNullTarget++;
					D.Log (door.name + " has target null");
				}
			}
			D.Log ("There are " + doorsWithNullTarget + " doors with null targets");

			D.Log ("Total nr of doors: " + doors.Length);
			D.Log ("Total nr of portals: " + _world.tingRunner.GetTingsOfType<Portal> ().Length);

			/*
			D.Log("Absurdely long names on tings:");
			foreach(var t in _world.tingRunner.GetTings()) {
				if(t.name.Length > 75) {
					D.Log("In " + t.room.name + ": " + t.name);
				}
			}
			*/

//			foreach (var room in _world.roomRunner.rooms) {
//				if (room.HasTinyTileGroup (3)) {
//					D.Log ("Tiny tile group detected in " + room.name + "! This is often a sign of places where you can get stuck.");
//				}
//			}
		}

		bool IsInTestingRoom (Ting door)
		{
			return door.room.name.Contains("Nicke") || door.room.name.Contains("Test");
		}

		void CountNrOfUnitializedPrograms<T>(string pDefaultProgramName, Func<T, string> pGetter) where T : Ting {
			int totalCount = 0;
			int tingsWithDefaultProgram = 0;
			foreach (var ting in _world.tingRunner.GetTingsOfType<T>()) {
				totalCount++;
				if (pGetter(ting) == pDefaultProgramName) {
					D.Log ("USING DEFAULT PROGRAM: " + ting.name + " in " + ting.room.name);
					tingsWithDefaultProgram++;
				}
			}
			D.Log ("# There are " + tingsWithDefaultProgram + " " + typeof(T).ToString() + ":s with default programs (total count " + totalCount + ")");

			int roomCount = 0;
			int tilePointCount = 0;

			foreach (var room in _world.roomRunner.rooms) {
				roomCount++;
				tilePointCount += room.points.Length;
			}

			D.Log ("# There are " + roomCount + " rooms and " + tilePointCount + " tile points in the world");
		}

		// RunPathfindingTests()
		void RunPathfindingTests (string[] args)
		{
			var tingRunner = _world.tingRunner;
			
			MimanPathfinder2 pathfinder = new MimanPathfinder2(tingRunner, _world.roomRunner);
			
			var startsAndGoals = new List<string[]>() {
				new string[] {"Factory_Floor1_Point1", "Cafe_Room1_Point"},
				new string[] {"Cafe_Room1_Point", "FelixApartment_Point1"},
				new string[] {"FelixApartment_Point1", "Mines_ClubDot_Point"},
				new string[] {"Mines_ClubDot_Point", "Casino_Floor2_Point"},
				new string[] {"MonadsApartment_Point1", "Casino_Floor2_Point"},
				new string[] {"Casino_Floor2_Point", "Factory_Lobby_Point1"},
				new string[] {"Factory_Lobby_Point1", "Lodge_Underwater_Point2"},
				new string[] {"Lodge_Underwater_Point2", "Hotel_Bathroom_Point1"},
				new string[] {"Hotel_Bathroom_Point1", "Factory_Floor1_Point1"},
				new string[] {"Casino_Floor2_Point", "Factory_Lobby_Point2"},
				new string[] {"Factory_Lobby_Point2", "HarborSouth_Trigger_1"},
				new string[] {"HarborSouth_Trigger_1", "PoorDesolateBuilding1_Corridor_Point"},
				new string[] {"PoorDesolateBuilding1_Corridor_Point", "Factory_Floor1_Point1"},
				new string[] {"Factory_Floor1_Point1", "Cafe_Room1_Point"},
				new string[] {"Factory_Office_Point1", "SodaStorage_Point"},
				new string[] {"SodaStorage_Point", "FancyHouse1_Point"},
				new string[] {"FancyHouse1_Point", "PixiesApartment_Point"},
				new string[] {"PixiesApartment_Point", "PetrasApartmentPoint1"},
				new string[] {"PetrasApartmentPoint1", "Cafe_Room1_Point"},
				new string[] {"Cafe_Room1_Point", "SodaStorage_Point"},
				new string[] {"SodaStorage_Point", "MonadsApartment_Point1"},
				new string[] {"BureucratApartment1_Trigger_1", "Ministry_Offices1_Trigger_1"},
				new string[] {"NiniApartment_Trigger_1", "EmmaApartment_Trigger_1"},
				new string[] {"AmandaApartment_Trigger_1", "Ministry_Offices3_Trigger_1"},
				new string[] {"Mines_ClubDot_Point", "IvanApartment_Trigger_1"},
				new string[] {"Plaza_Point", "FancyHouse1BedRoom_BedL_Right_1"},
				new string[] {"Plaza_Point", "GlennApartment_Testing_Poor_Bed_Poor_Bed_6_1"},
				new string[] {"Plaza_Point", "FancyHouse2BedRoom_BedL_Right_1"},
				new string[] {"Plaza_Point", "EmmaBedRoom_Testing_Poor_Bed_Poor_Bed_7_1"},
				new string[] {"Plaza_Point", "AmandasBed"},
				new string[] {"Plaza_Point", "TinyBarn_Poor_Bed_Poor_Bed_1"},
				new string[] {"Casino_Floor2_Point", "TouristGirlApartment_BedL_Right_1"},
				new string[] {"Casino_Floor2_Point", "Casino_Floor1_SecurityPoint"},
				new string[] {"Factory_Office_Point1", "MonadsApartment_BedL_Right_1"},
				new string[] {"Casino_Floor2_Point", "MonadsApartment_BedL_Left_1"},
				new string[] {"Casino_Floor2_Point", "LongsonApartment_Testing_Poor_Bed_Poor_Bed_7_1"},
				new string[] {"Casino_Floor2_Point", "BobSchack_BedL_Left_1"},
				new string[] {"Casino_Floor2_Point", "PandaApartment_Poor_Bed_Poor_Bed_1"},
				new string[] {"Casino_Floor2_Point", "PandaApartment_Poor_Bed_Poor_Bed_2"},
				new string[] {"Casino_Floor2_Point", "PandaApartment_BedL_Left_1"},
				new string[] {"Hotel_Lobby_Point", "Hotel_Corridor_Door1"},
			};

			int errors = 0;

			foreach(string[] pair in startsAndGoals) {
				var start = tingRunner.GetTing(pair[0]);
				var goal = tingRunner.GetTing(pair[1]);
				var result = pathfinder.Search(start, goal);
				if(result.status != MimanPathStatus.FOUND_GOAL) {
					Console.ForegroundColor = ConsoleColor.Red;
					D.LogError("Failed pathfinding from " + start + " to " + goal + ": " + result);
					errors++;
					Console.ForegroundColor = ConsoleColor.White;
					//break;
				}
				//D.Log("Pathfinding from " + start + " to " + goal + ": " + result + " was successful");
			}

			if(errors == 0) {
				Console.ForegroundColor = ConsoleColor.Green;
				D.Log("All pathfinding tests ran without error!");
				Console.ForegroundColor = ConsoleColor.White;
			} else {
				Console.ForegroundColor = ConsoleColor.Red;
				D.Log(errors + " PATHFINDING ERRORS!");
				Console.ForegroundColor = ConsoleColor.White;
			}
		}

		// Use this to find out if things have got their prefab in a Resource map, if not it will crash when run from Unity
		// MoveAllTingsToTing(Ting ting)
		private void MoveAllTingsToTing(string[] args) {
			var target = _world.tingRunner.GetTingUnsafe(args[0]);

			foreach(var t in _world.tingRunner.GetTings()) {
				if(t.canBePickedUp) {
					try {
						t.position = target.position;
					}
					catch (Exception e) {
						//D.Log(e);
						D.Log("Failed to move prefab " + t.prefab + ": " + e);
					}
				}
			}

			D.Log("Done moving all tings!");
		}
			
		private void TypeIntoComputer(string[] args) {

			if (args.Length < 2) {
				D.Log ("Too few args to TypeIntoComputer");
				return;
			}

			var character = _world.tingRunner.GetTingUnsafe(args[0]) as Character;
			string text = args [1];

			if (character == null) {
				D.Log (args[0] + " can't type");
				return;
			}

			var computer = character.actionOtherObject as Computer;

			if (computer == null) {
				D.Log (character + " is not interacting with a computer");
				return;
			}

			if(text == "ENTER") {
				computer.OnEnterKey();
			}
			else {
				foreach(char c in text) {
					computer.OnKeyDown (c.ToString());
				}
			}
		}

		// Ting.Explode()
		private void Explode(string [] args) {
			var ting = _world.tingRunner.GetTingUnsafe (args [0]) as MimanTing;
			ting.StartAction ("Explode", null, 0.1f, 5f);
		}
			
		// Character.SetRunning(bool on)
		private void SetRunning(string [] args) {
			var character = _world.tingRunner.GetTing<Character>(args [0]);
			character.running = (args [1] == "true");
		}

		public void SetLanguage (string [] args) {
			_world.settings.translationLanguage = args [0];
			_world.RefreshTranslationLanguage ();
		}

		// CheckNavNodeChain(string navNodeName)
		private void CheckNavNodeChain(string [] args) {
			var start = _world.tingRunner.GetTingUnsafe(args[0]) as NavNode;
			D.Log("Starting at " + start);
			var current = start.mainTrack;
			while(true) {
				Console.ReadLine();
				D.Log("Visiting " + current + (current.isStation ? " STATION" : ""));
				if(current.mainTrack == null) {
					D.Log("Next node is null! Stopping.");
					break;
				}
				else if(current.mainTrack == start) {
					D.Log("Back at start, stopping.");
					break;
				}
				else {
					current = current.mainTrack;
				}
			}
		}

		private void Hint(string[] args) {
			_world.settings.Hint(args[0]);
		}

		// Story(string story)
		// Takes a partial name of a story (for example 'Arrival')
		// and starts all stories that contain that name. (for example 'Babcia_Arrival')
		// Any character AFFECTED by this gets its other stories stopped (TODO: should be paused instead)
		// A character that doesn't have a story for the new story keeps using whatever story it was currently at
		private void Story(string[] args)
		{
			string partialName = "_" + args[0];
			
#if DEBUG
			D.Log (" --- STORY " + partialName + " --- ");
			//System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			//Console.WriteLine("Stack trace: " + t.ToString());
#endif

			string[] storiesThatWillBeStarted = _world.dialogueRunner.GetNamesOfAllStoppedConversationsWithNameContaining (partialName);

			foreach (var storyToBeStarted in storiesThatWillBeStarted) {
				int index = storyToBeStarted.IndexOf (partialName);
				if(index > -1) {
					string characterName = storyToBeStarted.Substring (0, index);
					if(characterName != "") {
#if DEBUG && DEEP
						D.Log ("Stopping other stories for character " + characterName);
#endif
						_world.dialogueRunner.StopAllConversationsContaining (characterName + "_");
					}
				}
			}

			_world.dialogueRunner.StartAllConversationsContaining (partialName);
			_world.settings.LogStoryEvent("Story(" + partialName + ")");
		}

		private void GetActiveNodes(string[] args)
		{
			string diaName = args[0];
			
			var activeNodes = _world.dialogueRunner.GetActiveNodes(diaName);

			D.Log("Active nodes: ");
			foreach (var node in activeNodes) {
				D.Log(node.ToString());
			}
			D.Log("---");
		}

		private void SetFocus(string[] args)
		{
			string diaName = args[0];
			_world.dialogueRunner.FocusConversation(diaName);
		}

		// Log(string s)
		private void Log(string[] args)
		{
			D.Log(args[0]);
		}

		// RebuildRoomNetwork(bool log)
		private void RebuildRoomNetwork(string[] args) {
			var pathFinder = new MimanPathfinder2(_world.tingRunner, _world.roomRunner);
			var roomNetwork = pathFinder.RecreateRoomNetwork();
			if(args.Length > 0 && args[0] == "true") {
				D.Log("ROOM NETWORK: " + roomNetwork);
			}
		}

		// GetRoomExits(Room room)
		private void GetRoomExits(string[] args) {
			string roomName = args[0];
			var room = _world.roomRunner.GetRoom(roomName);

			var network = MimanPathfinder2.roomNetwork;
			if(network == null) {
				D.Log("network is null");
			}

			for(int i = 0; i < 20; i++) {
				var group = new RoomGroup(room, i);
				if(!network.linkedRoomGroups.ContainsKey(group)) {
					continue;
				}
				var linked = network.linkedRoomGroups[group];
				if(linked.Count > 0) {
					D.Log("In " + group + ": ");
					foreach(var l in linked.Keys) {
						D.Log(" " + linked[l] + " => " + l);
					}
				}
			}
		}

		// Path(Ting start, Ting goal)
		private void Path(string[] args)
		{
			Ting start = _world.tingRunner.GetTingUnsafe(args[0]);
			Ting goal = _world.tingRunner.GetTingUnsafe(args[1]);
			if(start == null) {
				D.Log("start is null");
				return;
			}
			if(goal == null) {
				D.Log("goal is null");
				return;
			}
			var pathFinder = new MimanPathfinder2(_world.tingRunner, _world.roomRunner);
			MimanPath path = pathFinder.Search(start, goal);
			D.Log(path.ToString());
		}

		// TilePath(Ting start, Ting goal)
		private void TilePath(string[] args)
		{
			Ting start = _world.tingRunner.GetTingUnsafe(args[0]);
			Ting goal = _world.tingRunner.GetTingUnsafe(args[1]);
			if(start == null) {
				D.Log("start is null");
				return;
			}
			if(goal == null) {
				D.Log("goal is null");
				return;
			}
			var pathFinder = new PathSolver();
			var path = pathFinder.FindPath(start.tile, goal.tile, _world.roomRunner, true);
			D.Log(path.ToString());
		}
		
		// StartLogging(string tingName)
		private void StartLogging(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			ting.logger.AddListener(D.Log);
		}

		private void WP(string[] args) {
			Ting ting = _world.tingRunner.GetTing(args[0]);
			D.Log(ting.name + "'s world pos: " + ting.worldPoint.ToString());
		}

		// Get info about a Ting
		private void Info(string[] args) {
			Ting ting = _world.tingRunner.GetTing(args[0]);
			StringBuilder sb = new StringBuilder();
			
			foreach(PropertyInfo p in ting.GetType().GetProperties())
			{
				foreach(Attribute a in p.GetCustomAttributes(true)) 
				{
					if(a.Match(EDITABLE_IN_EDITOR) || a.Match(SHOW_IN_EDITOR)) 
					{
						object o = p.GetValue(ting, null);
						
						string line = "";
						if(o == null) line = "null";
						else if(o is string) line = "\"" + o.ToString() + "\"";
						else line = o.ToString();
						
						sb.Append(p.Name + ": " + line + "\n");
					}
				}
			}
			D.Log(ting.name + " info: " + sb.ToString());
		}

		void Bugtalk(string [] args) {
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			character.Say("Timetable: " + character.timetable.name + ", finalTargetPosition: " + character.finalTargetPosition + ", talking: " + character.talking + " sleeping: " + character.sleeping, "Bugtalk");
		}
		
		// Kill(Ting ting)
		private void Kill(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			_world.tingRunner.RemoveTing(ting.name);
		}

		private void DetachFromNavNode(string[] args)
		{
			Vehicle ting = _world.tingRunner.GetTing<Vehicle>(args[0]);
			ting.currentNavNode = null;
			ting.nextNavNode = null;
			ting.speed = 0.0f;
			ting.StopAction();
			ting.masterProgram.StopAndReset();
		}
		
		// SetFieldToStringArray(Ting ting, string fieldName, s0, s1, s2...)
		private void SetFieldToStringArray(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string fieldName = args[1];
			string[] newStringArray = new string[args.Length - 2];
			for(int i = 2; i < args.Length; i++) {
				newStringArray[i - 2] = args[i];
			}
			ting.table.SetValue<string[]>(ting.objectId, fieldName, newStringArray);
		}
		
		// SetFieldToFloat(Ting ting, string fieldName, float value)
		private void SetFieldToFloat(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string fieldName = args[1];
			float val = Convert.ToSingle(args[2]);
			
			ting.table.SetValue<float>(ting.objectId, fieldName, val);
		}

		// Pos(Ting ting, Ting otherTing)
		private void Pos(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string roomOrOtherTing = args[1];
			
			if(_world.tingRunner.HasTing(roomOrOtherTing)) {
				Ting targetTing = _world.tingRunner.GetTing(roomOrOtherTing);
				ting.position = targetTing.position;
				ting.direction = targetTing.direction;
				ClearStuffIfItIsACharacter(ting);
				if(targetTing is Seat && ting is Character) {
					//D.Log("Setting position of " + ting + " to a Seat, will set sitting = true");
					(ting as Character).Sit(targetTing as Seat);
				}
				else if(targetTing is Bed && ting is Character) {
					//D.Log("Setting position of " + ting + " to a Bed, will set laying = true");
					(ting as Character).LayInBed(targetTing as Bed);
				}
			}
			else {
				D.Log("Can't find Ting with name '" + roomOrOtherTing + "'");
			}
		}

		private void WorldPos(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string roomName = args[1];
			int x = Int32.Parse(args[2]);
			int y = Int32.Parse(args[3]);

			ting.position = new WorldCoordinate(roomName, x, y);
		}

		// CarefulPos(Ting ting, Ting otherTing) // doesn't clear stuff
		private void CarefulPos(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string roomOrOtherTing = args[1];
			
			if(_world.tingRunner.HasTing(roomOrOtherTing)) {
				Ting targetTing = _world.tingRunner.GetTing(roomOrOtherTing);
				ting.position = targetTing.position;
				ting.direction = targetTing.direction;
			}
			else {
				D.Log("Can't find Room or Ting with name '" + roomOrOtherTing + "'");
			}
		}
			
		// CarefulLayInBed(Character c, Bed b)
		public void CarefulLayInBed(string[] args) {
			Character character = _world.tingRunner.GetTing<Character> (args[0]);
			Bed bed = _world.tingRunner.GetTing<Bed>(args[1]);
			bed.exitPoint = 0;
			character.LayInBed(bed);
		}

		static void ClearStuffIfItIsACharacter(Ting pTing)
		{
			if (pTing is Character) {
				var character = pTing as Character;
				character.ClearState();
			}
		}
		
		// Dir(Ting ting, string dir)
		private void Dir(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			Direction d = Direction.RIGHT;
			switch(args[1].ToLower()) {
			case "up":
				d = Direction.UP;
				break;
			case "down":
				d = Direction.DOWN;
				break;
			case "left":
				d = Direction.LEFT;
				break;
			case "right":
				d = Direction.RIGHT;
				break;
			}
			ting.direction = (Direction)d;
			ClearStuffIfItIsACharacter(ting);
		}
		
		// Interact(Character interacter, Ting target)
		private void Interact(string[] args)
		{
			Character interacter = _world.tingRunner.GetTing<Character>(args[0]);			
			Ting target = _world.tingRunner.GetTing(args[1]);			
			if(interacter.CanInteractWith(target)) {
				interacter.WalkToTingAndInteract(target);
			}
			else {
				D.Log(interacter.name + " can't interact with " + target.name);
			}
		}

		private void GetUpAndInteract(string [] args) {
			Character interacter = _world.tingRunner.GetTing<Character>(args[0]);
			if(interacter.sitting) {
				interacter.GetUpFromSeat();
			}
			Interact(args);
		}

		private void InteractUsingHandItem(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			if (character.handItem == null) {
				D.Log(character + " can't InteractUsingHandItem since she/he has no hand item");
			}
			Ting target = _world.tingRunner.GetTing(args[1]);			
			if(character.handItem.CanInteractWith(target)) {
				character.WalkToTingAndUseHandItem(target);
			}
			else {
				D.Log(character.name + " can't interact (using hand item) with " + target.name);
			}
		}

		// Hack(Character character, Ting target)
		private void Hack(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			MimanTing target = _world.tingRunner.GetTing<MimanTing>(args[1]);			
			character.WalkToTingAndHack(target);
		}

		// Interact(Character interacter, Ting target)
		private void UseHandItem(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			if(character.handItem != null) {
				character.InteractWith(character.handItem);
			} else {
				D.Log("No hand item to interact with");	
			}
		}
		
		// TakeOutItem(Character interacter, Ting item)
		private void TakeOutItem(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			MimanTing ting = _world.tingRunner.GetTing<MimanTing>(args[1]);

			if (character.handItem == ting) {
				D.Log (character + " is already holding " + ting + ", no need to take it out of inventory");
				return;
			}

			if(character.handItem != null) {
				character.MoveHandItemToInventory();
			}
			//character.handItem = ting;
			character.TakeOutInventoryItem(ting);
		}
		
		// PutIntoInventory(Character character, Ting item)
		private void PutIntoInventory(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			MimanTing ting = _world.tingRunner.GetTingUnsafe(args[1]) as MimanTing;
			if(ting == null) {
				D.Log("Can't find " + args[1]);
				return;
			}
			ting.position = new WorldCoordinate(character.inventoryRoomName, 0, 0);
		}
		
		// StartWaitForGift(Character character)
		private void StartWaitForGift(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			character.waitForGift = true;
		}
		
		// StopWaitForGift(Character character)
		private void StopWaitForGift(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			character.waitForGift = false;
		}
		
		// Tase(Character interacter, Character target)
		private void Tase(string[] args)
		{
			Character target = _world.tingRunner.GetTing<Character>(args[0]);
			target.GetTased ();
		}

		// Tase(Character interacter, Character target)
		private void GetTasedGently(string[] args)
		{
			Character target = _world.tingRunner.GetTing<Character>(args[0]);
			target.GetTasedGently ();
		}

		// PickUp(Character picker, Ting target)
		private void PickUp(string[] args)
		{
			Character picker = _world.tingRunner.GetTing(args[0]) as Character;			
			if(picker == null) {
				D.Log(args[0] + " is not a Character");
			}
			Ting target = _world.tingRunner.GetTing(args[1]);			
			if(target.canBePickedUp) {
				picker.PickUp(target);
			}
			else {
				D.Log(picker.name + " can't pick up " + target.name);
			}
		}
		
		// Walk(Character character, string room, int x, int y)
		private void Walk(string[] args)
		{
			Character character = GetCharacterFromFirstArg(args);
			
			string roomOrOtherTing = args[1];
			
			if(_world.roomRunner.HasRoom(roomOrOtherTing)) {
				// it's a room!
				int x = Convert.ToInt32(args[2]);
				int y = Convert.ToInt32(args[3]);
            	character.WalkTo(new WorldCoordinate(roomOrOtherTing, x, y));
			}
			else if(_world.tingRunner.HasTing(roomOrOtherTing)) {
				Ting t = _world.tingRunner.GetTing(roomOrOtherTing);
				character.WalkToTingAndInteract(t);
			}
			else {
				D.Log("Can't find Room or Ting with name '" + roomOrOtherTing + "'");
			}
		}
		
		// GetUpFromSeat(Character character)
		private void GetUpFromSeat(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.GetUpFromSeat();
		}
			
		// TurnLeft(Character character)
		private void TurnLeft(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.TurnLeft();
		}

		private void GetAngryAtComputer(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.GetAngryAtComputer();
		}

		// Give(Character character)
		private void Give(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.GiveHandItemToPerson();
		}
		
		// Load(Room room)
		private void Load(string[] args)
		{
			_world.settings.activeRoom = args[0];
		}
		
		
		// SetClockSpeed(float speedMultiplier)
		private void SetClockSpeed(string[] args)
		{
			_world.settings.gameTimeSpeed = (float)Convert.ToDouble(args[0]);
		}
		
		// SetClock(int day, int hour, int minute, int second)
		private void SetClock(string[] args)
		{
			int days = Convert.ToInt32(args[0]);
			int hours = Convert.ToInt32(args[1]);
			int minutes = Convert.ToInt32(args[2]);
			int seconds = Convert.ToInt32(args[3]);			
			
			GameTime t = new GameTime(days, hours, minutes, (float)seconds);
			_world.settings.gameTimeSeconds = t.totalSeconds;
		}

		private void SetHour(string [] args)
		{
			int days = 0;
			int hours = Convert.ToInt32(args[0]);
			int minutes = 0;
			int seconds = 0;

			GameTime t = new GameTime(days, hours, minutes, (float)seconds);
			_world.settings.gameTimeSeconds = t.totalSeconds;
		}
		
		// God()
		private void God(string[] args)
		{
			Character sebastian = _world.tingRunner.GetTing("Sebastian") as Character;
			
			Teleporter teleporter = _world.tingRunner.CreateTing<Teleporter>("GodModeTeleporter", new WorldCoordinate(sebastian.inventoryRoomName, 0, 0));			
			teleporter.prefab = "Teleporter";
			
			Extractor extractor = _world.tingRunner.CreateTing<Extractor>("GodModeExtractor", new WorldCoordinate(sebastian.inventoryRoomName, 0, 0));
			extractor.prefab = "Extractor";
			
			Radio radio = _world.tingRunner.CreateTing<Radio>("GodModeRadio", new WorldCoordinate(sebastian.inventoryRoomName, 0, 0));
			radio.prefab = "Radio";
			
			MusicBox musicBox = _world.tingRunner.CreateTing<MusicBox>("GodModeMusicBox", new WorldCoordinate(sebastian.inventoryRoomName, 0, 0));
			musicBox.prefab = "MusicBox";
			
			Drink beer = _world.tingRunner.CreateTing<Drink>("EmergencyBeer", new WorldCoordinate(sebastian.inventoryRoomName, 0, 0));
			beer.prefab = "Beer";
		}
		
		// SetAvatar(Ting target)
		private void SetAvatar(string[] args)
		{
			_world.settings.avatarName = args[0];
		}
		
		// Set the active room to where the avatar is
		private void FindAvatar(string[] args)
		{
			Ting avatar = _world.tingRunner.GetTing(_world.settings.avatarName);
			_world.settings.activeRoom = avatar.room.name;
		}
		
		// Lock door
		private void LockDoor(string[] args)
		{
			Door door = _world.tingRunner.GetTing<Door>(args[0]);
			door.isLocked = true;
		}
		
		// Unlock door
		private void UnlockDoor(string[] args)
		{
			Door door = _world.tingRunner.GetTing<Door>(args[0]);
			door.isLocked = false;
			door.autoLockTimer = 0f;
		}

		// UseForRoomPathfinding(Door door, bool on)
		private void UseForRoomPathfinding(string[] args)
		{
			Door door = _world.tingRunner.GetTing<Door>(args[0]);
			door.useForRoomPathfinding = bool.Parse (args [1]);
		}

		// MuteNotifications(bool on)
		private void MuteNotifications(string[] args)
		{
			_world.settings.muteNotifications = bool.Parse (args [0]);
		}

		// StartRinging(string phoneName)
		private void StartRinging(string[] args)
		{
			Telephone phone = _world.tingRunner.GetTing<Telephone>(args[0]);
			phone.ringing = true;
		}

		// StartAllRadios()
		private void StartAllRadios(string[] args)
		{
			var radios = _world.tingRunner.GetTingsOfType <Radio> ();
			foreach (var r in radios) {
				r.masterProgram.Start ();
			}
		}

		// StartAllRadios()
		private void StartAllFuseboxes(string[] args)
		{
			var fuseboxes = _world.tingRunner.GetTingsOfType <FuseBox> ();
			foreach (var f in fuseboxes) {
				f.masterProgram.Start ();
			}
		}

		private void StartMusic(string[] args)
		{
			var musicBox = _world.tingRunner.GetTing<MusicBox>(args[0]);
			musicBox.isPlaying = true;
		}

		private void StopMusic(string[] args)
		{
			var musicBox = _world.tingRunner.GetTing<MusicBox>(args[0]);
			musicBox.isPlaying = false;
		}

		private void SetChannelOnRadio(string[] args)
		{
			var radio = _world.tingRunner.GetTing<Radio>(args[0]);
			radio.API_SetChannel(int.Parse(args[1]));
		}
		
		// Set door target
		private void SetDoorTarget(string[] args)
		{
			Door door = _world.tingRunner.GetTing<Door>(args[0]);
			if(args[1] != "") {
				Door targetDoor = _world.tingRunner.GetTing<Door>(args[1]);
				door.targetDoorName = targetDoor.name;
			} else {
				door.targetDoorName = "";
			}
			door.SetSourceCodeFromDoorTarget ();
		}
		
		// Sleep(Character character, int nrOfHours)
		private void Sleep(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			int hours = Convert.ToInt32(args[1]);
			character.FallAsleepFromStanding(hours);
		}
		
		// BeBored(Character character)
		private void BeBored(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.BeBored();
		}

		// WakeUp(Character character)
		private void WakeUp(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.StopAction();
		}
		
		// SleepUntil(Character character, int hour)
		private void SleepUntil(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			
			int hours = 0;
			
			int targetHour = Convert.ToInt32(args[1]);
			int currentHour = _world.settings.gameTimeClock.hours;
			
			if(targetHour > currentHour) {
				// same day
				hours = targetHour - currentHour;
			} else {
				// next day
				hours = (24 - currentHour) + targetHour;
			}
			
			character.FallAsleepFromStanding(hours);
		}

		// StartAction(Character character, string actionName)
		private void StartAction(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.StartAction(args[1], null, 0f, 1f);
		}

		// StopAction(Character character)
		private void StopAction(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.StopAction();
		}

		// CancelWalking(Character character)
		private void CancelWalking(string[] args)
		{
			GetCharacterFromFirstArg(args).CancelWalking();
		}
		
		// SetFriendLevel(Character character, int nrOfHours)
		private void SetFriendLevel(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			int level = Convert.ToInt32(args[1]);
			character.friendLevel = level;
		}

		// SetCorruption(Character character, float level)
		private void SetCorruption(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			float level = Convert.ToSingle(args[1]);
			character.corruption = level;
		}

		private Character GetCharacterFromFirstArg (string[] args)
		{
			Character character = _world.tingRunner.GetTing (args [0]) as Character;			
			if (character == null) {
				D.Log (args [0] + " is not a Character");
			}
			return character;
		}
		
		// StartTalking(Character character)
		private void StartTalking(string[] args)
		{
			GetCharacterFromFirstArg(args).StartTalking();
		}
		
		// StopTalking(Character character)
		private void StopTalking(string[] args)
		{
			GetCharacterFromFirstArg(args).StopTalking();
		}
		
		// SetKnowledge(Character character, string knowledge)
		private void SetKnowledge(string[] args)
		{
			GetCharacterFromFirstArg(args).SetKnowledge(args[1]);
		}

		// SetCameraAutoRotateSpeed(float speed)
		private void SetCameraAutoRotateSpeed(string[] args)
		{
			_world.settings.cameraAutoRotateSpeed = Convert.ToSingle(args[0]);
		}

		private void SetCameraTarget(string[] args)
		{
			if (_world.settings.onCameraTarget != null) {
				_world.settings.onCameraTarget (args [0], args[1]);
			}
		}

		private void Beat(string[] args)
		{
			_world.settings.beaten = true;
		}

		private void HeartIsBroken(string[] args)
		{
			_world.settings.heartIsBroken = true;
		}

		private void SetAllComputersToRunProgram(string [] args) 
		{
			var src = _world.sourceCodeDispenser.GetSourceCode (args [0]);
			Console.WriteLine ("SET ALL COMPUTERS TO RUN PROGRAM: " + src.content);

			var computers = _world.tingRunner.GetTingsOfType<Computer> ();
			foreach (var c in computers) {
				c.hasInternetAPI = true;
				c.masterProgram.sourceCodeContent = src.content;
				c.masterProgram.StopAndReset ();
				c.RunProgram (null);
			}
		}

		// SetTimetable(Character character, Timetable timetable)
		private void SetTimetable(string [] args)
		{
			if (args.Length != 2) {
				D.LogError ("The number of args to SetTimetable is incorrect: " + args.Length);
			}
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			/*Timetable timetable = _world.timetableRunner.GetTimetable();
			if(timetable == null) {
				D.Log(args[1] + " is not a Timetable");
			}*/
			character.timetableName = args[1];
			character.timetableTimer = 0.5f;
		}

		// SetTimetableTimer(Character character, float time)
		private void SetTimetableTimer(string [] args)
		{
			if (args.Length != 2) {
				D.LogError ("The number of args to SetTimetableTimer is incorrect: " + args.Length);
			}

			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}

			character.timetableTimer = float.Parse(args[1]);
		}

		// SetNeverGetsTired(Character character, bool value)
		private void SetNeverGetsTired(string [] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			character.neverGetsTired = bool.Parse(args[1]);
		}

		// CreateCharacter(string characterName, Ting position)
		private void CreateCharacter(string [] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[1]);
			/*Character character = */_world.tingRunner.CreateTing<Character>(args[0], ting.position, Direction.DOWN, "Sebastian");
		}

		// CreateCigarette(Character character, string cigaretteName)
		private void CreateCigarette(string [] args)
		{
			var character = _world.tingRunner.GetTing<Character>(args[0]);
			var cigarette = _world.tingRunner.CreateTing<Cigarette>(args[1], new WorldCoordinate(character.inventoryRoomName, IntPoint.Zero), Direction.DOWN, "Tagg_Cigarrette");
			cigarette.masterProgramName = "Cigarette";
			cigarette.drugType = "cigarette";
		}
		
		// CreateBeerInHand(string characterName, Ting position)
		private void CreateBeerInHand(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);			
			var name = "Beer_" + holder.name + "_" + _world.settings.tickNr;			
			var drink = _world.tingRunner.CreateTing<Drink>(name, holder.position, Direction.DOWN, "Beer");
			drink.liquidType = "beer";
			holder.handItem = drink;
		}

		// CreateDrinkInHand(string characterName, string drinkPrefabName)
		private void CreateDrinkInHand(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);
			string prefabName = args [1];
			var drink = Behaviour_Sell.CreateTingToSell (holder, _world.tingRunner, prefabName, _world.settings);
			holder.handItem = drink;
		}

		private void CreateCoffeeInHand(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);			
			var name = "Coffee_" + holder.name + "_" + _world.settings.tickNr;			
			var drink = _world.tingRunner.CreateTingAfterUpdate<Drink>(name, holder.position, Direction.DOWN, "CupOfCoffee");
			drink.masterProgramName = "CafeCoffee";
			drink.liquidType = "coffee";
			drink.PrepareForBeingHacked();
			holder.handItem = drink;
		}

		// SetHandItem(string characterName, Ting tingName)
		private void SetHandItem(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);					
			var ting = _world.tingRunner.GetTing<MimanTing>(args[1]);
			if(ting != null) {
				holder.handItem = ting;
			}
			else {
				D.Log("Can't find ting " + args[1] + " to put into hand of " + args[0]);
			}
		}

		// DropHandItem(string characterName)
		private void DropHandItem(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);					
			holder.DropHandItem();
		}
		
		// ClearHandItem(string characterName)
		private void ClearHandItem(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);					
			holder.handItem = null;
		}

		// PutAwayHandItem(string characterName)
		private void PutAwayHandItem(string [] args)
		{
			var holder = _world.tingRunner.GetTing<Character>(args[0]);					
			holder.PutHandItemIntoInventory();
		}
			
		// SetRain(float amount)
		private void SetRain(string [] args)
		{
			float amount = Convert.ToSingle (args [0]);
			_world.settings.rainTargetValue = amount;
		}

		private void GetMasterProgramStatus(string [] args)
		{
			var ting = _world.tingRunner.GetTing(args[0]) as Computer;

			D.Log("Information for program " + ting.masterProgram.name);
			D.Log("is on: " + ting.masterProgram.isOn);
			D.Log("waitingForInput: " + ting.masterProgram.waitingForInput);
			D.Log("compilationTurnedOn: " + ting.masterProgram.compilationTurnedOn);
			D.Log("Sleep timer: " + ting.masterProgram.sleepTimer);
			D.Log("Max execution time: " + ting.masterProgram.maxExecutionTime);
			D.Log("ContainsErrors: " + ting.masterProgram.ContainsErrors());
			D.Log("Execution time: " + ting.masterProgram.executionTime);
		}

		// RunMasterProgram(string computerName)
		private void RunMasterProgram(string [] args)
		{
			var ting = _world.tingRunner.GetTing(args[0]) as MimanTing;

			if (ting is Computer) {
				(ting as Computer).RunProgram (null);
			} else if (ting is Radio) {
				(ting as Radio).masterProgram.Start ();
			} else if (ting is Pawn) {
				(ting as Pawn).masterProgram.Start ();
			} else {
				D.LogError ("Can't run master program on " + args [0]);
			}
		}

		private void RunMasterProgramOnAllComputersInRoom(string [] args)
		{
			var room = _world.roomRunner.GetRoom(args[0]);

			foreach(var computer in room.GetTingsOfType<Computer>()) {
				computer.RunProgram(null);
			}
		}

		private void TurnOnTv(string [] args)
		{
			var tv = _world.tingRunner.GetTing(args[0]) as Tv;
			tv.on = true;
			tv.masterProgram.Start ();
		}

		// SetMemory(Ting ting, string key, string value)
		private void SetMemory(string[] args)
		{
			var memory = _world.tingRunner.GetTing<Memory>(args[0]);
			memory.data.Add(args[1], args[2]);
		}

		// SetCode(Ting ting, CodeFile codeFile)
		private void SetCode(string[] args)
		{
			var ting = _world.tingRunner.GetTing<MimanTing>(args[0]);
			var sourceCode = _world.sourceCodeDispenser.GetSourceCode(args[1]);
			ting.masterProgram.sourceCodeContent = sourceCode.content;
		}

		// SetCodeAndRun(Ting ting, CodeFile codeFile)
		private void SetCodeAndRun(string[] args)
		{
			var ting = _world.tingRunner.GetTing<MimanTing>(args[0]);
			var sourceCode = _world.sourceCodeDispenser.GetSourceCode(args[1]);
			ting.masterProgram.sourceCodeContent = sourceCode.content;
			ting.masterProgram.Start();
		}
			
		// SetResettableCode(Ting ting, CodeFile codeFile)
		private void SetResettableCode(string[] args)
		{
			var ting = _world.tingRunner.GetTing<MimanTing>(args[0]);
			ting.masterProgram.sourceCodeName = args [1];
		}

		// GetOutput(Computer pComputer)
		private void GetOutput(string[] args)
		{
			var computer = _world.tingRunner.GetTing<Computer>(args[0]);
			Console.ForegroundColor = ConsoleColor.Yellow;
			foreach (string s in computer.consoleOutput) {
				Console.WriteLine(s);
			}
			Console.ForegroundColor = ConsoleColor.White;
		}

		private void GetProgramErrors(string[] args)
		{
			var computer = _world.tingRunner.GetTing<Computer>(args[0]);
			Console.ForegroundColor = ConsoleColor.Red;
			int i = 0;
			foreach (var e in computer.masterProgram.GetErrors()) {
				D.Log((i++) + ": " + e);
			}
			Console.ForegroundColor = ConsoleColor.White;
		}

		// GetSource(Ting pTing)
		private void GetSource(string[] args)
		{
			var ting = _world.tingRunner.GetTing(args[0]);
			Console.ForegroundColor = ConsoleColor.Yellow;
			if (ting is CreditCard) {
				Console.WriteLine ((ting as CreditCard).masterProgram.sourceCodeContent);
			} else if (ting is Computer) {
				Console.WriteLine ((ting as Computer).masterProgram.sourceCodeContent);
			} else if (ting is Hackdev) {
				Console.WriteLine ((ting as Hackdev).masterProgram.sourceCodeContent);
			} else if (ting is Door) {
				Console.WriteLine ((ting as Door).masterProgram.sourceCodeContent);
			} else {
				D.Log ("Can't print source of tings with type " + ting.GetType ());
			}
			Console.ForegroundColor = ConsoleColor.White;
		}

		// RunFunctionOnComputer(string computerName, string functionName, arg0, arg1, arg2 ...)
		private void RunFunctionOnComputer(string [] args)
		{
			Computer computer = _world.tingRunner.GetTing(args[0]) as Computer;
			List<object> functionArgs = new List<object> ();
			for (int i = 2; i < args.Length; i++) {
				functionArgs.Add (args [i]);
			}

			if(computer.masterProgram != null) {
				computer.masterProgram.StopAndReset();
			}

			computer.masterProgram.maxExecutionTime = 10.0f;
			computer.masterProgram.StartAtFunction(args[1], functionArgs.ToArray(), null); //, o => D.Log("Got return value from " + args[1] + ": " + o.ToString()));
		}

		// RunMakeTransactionFunctionOnCreditCard(string creditCardName, float amount)
		private void RunMakeTransactionFunctionOnCreditCard(string [] args)
		{
			CreditCard creditCard = _world.tingRunner.GetTing<CreditCard> (args [0]);
			if(creditCard == null) {
				D.Log(args[0] + " is not a CreditCard");
				return;
			}
			float amount = Convert.ToSingle (args [1]);

			creditCard.RunMakeTransactionFunction (amount);
		}

		private void CharacterTakesMoneyFromCreditCard(string [] args) {

			var person = _world.tingRunner.GetTing<Character>(args[0]);

			if(person.handItem == null) {
				D.Log("Can't take money from credit card, " + person + " has no hand item!");
			}

			float amount = Convert.ToSingle (args [1]);

			D.Log(person + " will take $" + amount + " from the credit card " + person.handItem);

			person.creditCardUsageAmount = amount;

			person.InteractWith(person.handItem);

		}

		private void CheckMoney(string [] args) {
			object cashAmount = 0f;
			Computer financeComputer = _world.tingRunner.GetTing("FinanceComputer") as Computer;

			if(financeComputer == null) {
				D.Log("No finance computer");
				return;
			}

			bool found = financeComputer.memory.data.TryGetValue(args[0], out cashAmount);

			if(!found) {
				D.Log("No entry found");
				return;
			}
			
			if(cashAmount == null) {
				D.Log ("cashAmount is null");
				return;
			}

			D.Log("Cash amount: " + cashAmount + ", of type " + cashAmount.GetType());
		}

		#endregion
		
		#region EXPRESSIONS
		
		public void RegisterExpressions()
		{
			_world.dialogueRunner.AddExpression("InSameRoom", InSameRoom);
			_world.dialogueRunner.AddExpression("NotInSameRoom", NotInSameRoom);
			_world.dialogueRunner.AddExpression("IsInRoom", IsInRoom);
			_world.dialogueRunner.AddExpression("RoomIsEmpty", RoomIsEmpty);
			_world.dialogueRunner.AddExpression("IsInEitherRoom", IsInEitherRoom);
			_world.dialogueRunner.AddExpression("IsNotInRoom", IsNotInRoom);
			_world.dialogueRunner.AddExpression("IsNotSlurped", IsNotSlurped);
			_world.dialogueRunner.AddExpression("IsIdle", IsIdle);
			_world.dialogueRunner.AddExpression("IsSitting", IsSitting);
			_world.dialogueRunner.AddExpression("IsLaying", IsLaying);
			_world.dialogueRunner.AddExpression("IsSleepingOnGround", IsSleepingOnGround);
			_world.dialogueRunner.AddExpression("IsStanding", IsStanding);
			_world.dialogueRunner.AddExpression("IsAtPosition", IsAtPosition);
			_world.dialogueRunner.AddExpression("IsAtTing", IsAtTing);
			_world.dialogueRunner.AddExpression("IsHour", IsHour);
			_world.dialogueRunner.AddExpression("IsOverHour", IsOverHour);
			_world.dialogueRunner.AddExpression("IsBeforeHour", IsBeforeHour);
			_world.dialogueRunner.AddExpression("IsBetweenHours", IsBetweenHours);
			_world.dialogueRunner.AddExpression("IsDay", IsDay);
			_world.dialogueRunner.AddExpression("IsNight", IsNight);
			_world.dialogueRunner.AddExpression("IsAwake", IsAwake);
			_world.dialogueRunner.AddExpression("IsSleeping", IsSleeping);
			_world.dialogueRunner.AddExpression("IsHacking", IsHacking);
			_world.dialogueRunner.AddExpression("IsUsingComputer", IsUsingComputer);
			_world.dialogueRunner.AddExpression("Is", Is);
			_world.dialogueRunner.AddExpression("OneIn", OneIn);
			_world.dialogueRunner.AddExpression("IsDrunk", IsDrunk);
			_world.dialogueRunner.AddExpression("HasFriendLevel", HasFriendLevel);
			_world.dialogueRunner.AddExpression("False", ((string[] args) => false));
			_world.dialogueRunner.AddExpression("NotTalking", NotTalking);
			_world.dialogueRunner.AddExpression("NotTalkingButIgnore", NotTalkingButIgnore);
			_world.dialogueRunner.AddExpression("HasConversationTarget", HasConversationTarget);
			_world.dialogueRunner.AddExpression("HasKnowledge", HasKnowledge);
			_world.dialogueRunner.AddExpression("IsAtTask", IsAtTask);
			_world.dialogueRunner.AddExpression("IsNotAtTask", IsNotAtTask);
			_world.dialogueRunner.AddExpression("IsWithinDistance", IsWithinDistance);
			_world.dialogueRunner.AddExpression("IsOutsideDistance", IsOutsideDistance);
			_world.dialogueRunner.AddExpression("IsFieldGreaterThan", IsFieldGreaterThan);
			_world.dialogueRunner.AddExpression("IsFieldLessThan", IsFieldLessThan);
			_world.dialogueRunner.AddExpression("HasSoda", HasSoda);
			_world.dialogueRunner.AddExpression("HasHandItemWithName", HasHandItemWithName);
			_world.dialogueRunner.AddExpression("HasHandItemOfType", HasHandItemOfType);
			_world.dialogueRunner.AddExpression("HasHandItemOfDrinkType", HasHandItemOfDrinkType);
			_world.dialogueRunner.AddExpression("HasAnyHandItem", HasAnyHandItem);
			_world.dialogueRunner.AddExpression("HasNoHandItem", HasNoHandItem);
			_world.dialogueRunner.AddExpression("HasItemOfType", HasItemOfType);
			_world.dialogueRunner.AddExpression("HasDrinkOfType", HasDrinkOfType);
			_world.dialogueRunner.AddExpression("IsPawnAlive", IsPawnAlive);
			_world.dialogueRunner.AddExpression("IsPawnDead", IsPawnDead);
			_world.dialogueRunner.AddExpression("HasMadeMoreMovesThan", HasMadeMoreMovesThan);
			_world.dialogueRunner.AddExpression("HasNoErrors", HasNoErrors);
			_world.dialogueRunner.AddExpression("HasErrors", HasErrors);
			_world.dialogueRunner.AddExpression("IsCubeGlowing", IsCubeGlowing);
			_world.dialogueRunner.AddExpression("IsDebug", IsDebug);
			_world.dialogueRunner.AddExpression("IsNotDebug", IsNotDebug);
			_world.dialogueRunner.AddExpression("BoolIsTrue", BoolIsTrue);
			_world.dialogueRunner.AddExpression("IsUnlocked", IsUnlocked);
			_world.dialogueRunner.AddExpression("IsTvOff", IsTvOff);
			_world.dialogueRunner.AddExpression("IsBeaten", IsBeaten);
			_world.dialogueRunner.AddExpression("IsProgramStopped", IsProgramStopped);
			_world.dialogueRunner.AddExpression("DrinkInHandIsAlmostEmpty", DrinkInHandIsAlmostEmpty);
			_world.dialogueRunner.AddExpression("CharacterHasLessMoneyThan", CharacterHasLessMoneyThan);
			_world.dialogueRunner.AddExpression("Cheat", Cheat);
		}

		private bool Cheat(string [] args) {
			if(args.Length == 0) {
				return cheat != "";
			}
			else {
				return cheat == args[0];
			}
		}

		private bool CharacterHasLessMoneyThan(string [] args)
		{
			float amount = Convert.ToSingle (args [1]);

			Computer financeComputer = _world.tingRunner.GetTingUnsafe("FinanceComputer") as Computer;
			
			object cashAmount = 0f;
			bool foundBankEntryForCharacter = false;
			
			if(financeComputer != null) {
				foundBankEntryForCharacter = financeComputer.memory.data.TryGetValue(args[0], out cashAmount);
			}

			D.Log("Checking if " + args[0] + " has less money than " + args[1] + ", he/she has: $" + cashAmount);

			if(foundBankEntryForCharacter) {
				//float inBank = Convert.ToSingle (cashAmount);
				return (float)cashAmount < amount;
			}
			else {
				D.Log("Can't find bank entry for " + args[0]);
				return true;
			}
		}

		private bool IsDebug(string [] args) {
#if DEBUG
			return true;
#else
			return false;
#endif
		}

		private bool IsNotDebug(string [] args) {
			#if DEBUG
			return false;
			#else
			return true;
			#endif
		}

		// IsUnlocked(Door door)
		private bool IsUnlocked(string[] args)
		{
			Door door = _world.tingRunner.GetTing(args[0]) as Door;
			return !door.isLocked;
		}

		// IsTvOff(Tv tv)
		private bool IsTvOff(string[] args)
		{
			Tv tv = _world.tingRunner.GetTing(args[0]) as Tv;
			return !tv.on;
		}

		// IsBeaten()
		private bool IsBeaten(string[] args)
		{
			return _world.settings.beaten;
		}

		// IsProgramStopped()
		private bool IsProgramStopped(string [] args) {
			var ting = _world.tingRunner.GetTing(args[0]) as MimanTing;
			var program = ting.masterProgram;
			return !program.isOn;
		}

		// BoolIsTrue(Ting ting, string boolFieldName)
		private bool BoolIsTrue(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			return ting.table.GetValue<bool>(ting.objectId, args[1]);
		}
		
		// InSameRoom(Ting ting1, Ting ting2)
		private bool InSameRoom(string[] args)
		{
			Ting ting1 = _world.tingRunner.GetTing(args[0]);
			Ting ting2 = _world.tingRunner.GetTing(args[1]);
			return ting1.room == ting2.room;
		}

		private bool NotInSameRoom(string[] args)
		{
			Ting ting1 = _world.tingRunner.GetTing(args[0]);
			Ting ting2 = _world.tingRunner.GetTing(args[1]);
			return ting1.room != ting2.room;
		}

		// IsInRoom(Ting ting, Room room)
		private bool IsInRoom(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string room = args[1];
			return ting.room.name == room;
		}

		private bool RoomIsEmpty(string[] args)
		{
			var characters = _world.roomRunner.GetRoom(args[0]).GetTingsOfType<Character>();
			return characters.Count == 0;
		}
		
		private bool IsInEitherRoom(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			
			string tingRoomName = ting.room.name;
			
			for(int i = 0; i < args.Length; i++) {
				string room = args[i];
				if(tingRoomName == room) {
					return true;	
				}
			}
			
			return false;
		}

		// IsNotSlurped(Ting ting)
		private bool IsNotSlurped(string[] args)
		{
			var character = _world.tingRunner.GetTing(args[0]) as Character;
			return character.actionName != "InsideComputer";
		}

		// IsNotInRoom(Ting ting, Room room)
		private bool IsNotInRoom(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string room = args[1];
			return ting.room.name != room;
		}
		
		// IsIdle(Ting ting)
		private bool IsIdle(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			return character.IsIdle();
		}

		// IsSitting(Character pCharacter)
		private bool IsSitting(string[] args)
		{
			var character = _world.tingRunner.GetTing<Character>(args[0]);
			return character.sitting;
		}

		// IsLaying(Character pCharacter)
		private bool IsLaying(string[] args)
		{
			var character = _world.tingRunner.GetTing<Character>(args[0]);
			return character.laying;
		}

		// IsAwake(Character character)
		private bool IsAwake(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			//D.Log("Testing awakeness for Character " + c.name + ", it is doing action: " + c.actionName);
			return c.actionName != "Sleeping";
		}
		
		// IsSleeping(Character character)
		private bool IsSleeping(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName == "Sleeping";
		}

		// IsSleepingOnGround(Character character)
		private bool IsSleepingOnGround(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName == "Sleeping" && c.bed == null;
		}

		// IsStanding(Character character)
		private bool IsStanding(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return !c.sitting && !c.laying;
		}

		// IsHacking(Character character)
		private bool IsHacking(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName == "Hacking";
		}

		private bool IsUsingComputer(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName == "UsingComputer";
		}
		
		// Is(Character character, string actionName)
		private bool Is(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName == args[1];
		}
		
		// IsDrunk(Character character, int limit)
		private bool IsDrunk(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			//D.Log("Testing awakeness for Character " + c.name + ", it is doing action: " + c.actionName);
			int limit = Convert.ToInt32(args[1]);
			return c.drunkenness >= limit;
		}
		
		// IsAtPosition(Ting ting, int x, int y)
		private bool IsAtPosition(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			int x = Convert.ToInt32(args[1]);
			int y = Convert.ToInt32(args[2]);
			IntPoint pos = new IntPoint(x, y);
			return (ting.localPoint == pos);
		}

		// IsAtTing(Ting ting, Ting target)
		private bool IsAtTing(string[] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			Ting target = _world.tingRunner.GetTing(args[1]);
			return ting.position == target.position;
		}
		
		// IsHour(int hour)
		private bool IsHour(string[] args)
		{
			int hour = Convert.ToInt32(args[0]);
			return _world.settings.gameTimeClock.hours == hour;
		}

		// IsOverHour(int hour)
		private bool IsOverHour(string[] args)
		{
			int hour = Convert.ToInt32(args[0]);
			return _world.settings.gameTimeClock.hours >= hour;
		}

		// IsBeforeHour(int hour)
		private bool IsBeforeHour(string[] args)
		{
			int hour = Convert.ToInt32(args[0]);
			return _world.settings.gameTimeClock.hours < hour;
		}

		private bool IsBetweenHours(string[] args)
		{
			int hourLow  = Convert.ToInt32(args[0]);
			int hourHigh = Convert.ToInt32(args[1]);
			return _world.settings.gameTimeClock.hours >= hourLow && _world.settings.gameTimeClock.hours < hourHigh;
		}

		// IsDay()
		private bool IsDay(string[] args)
		{
			int h = _world.settings.gameTimeClock.hours;
			return h >= 6 && h <= 22;
		}
		
		// IsNight()
		private bool IsNight(string[] args)
		{
			return !IsDay(args);
		}

		static Random r = new Random();
		
		// OneIn(int howMany)
		private bool OneIn(string[] args)
		{
			int howMany = Convert.ToInt32(args[0]);
			return (r.Next() % howMany) == 0;
		}
		
		// HasFriendLevel(Character character, int level)
		private bool HasFriendLevel(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			int level = Convert.ToInt32(args[1]);
			return c.friendLevel == level;
		}		
		
		// NotTalking(Character character)
		private bool NotTalking(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName != "Talking" && c.conversationTarget == null;
		}

		// NotTalking(Character character, Character ignoreThisOne)
		private bool NotTalkingButIgnore(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			return c.actionName != "Talking" && (c.conversationTarget == null || c.conversationTarget.name == args[1]);
		}

		// HasConversationTarget(Character character, Character target)
		private bool HasConversationTarget(string[] args)
		{
			Character c = _world.tingRunner.GetTing(args[0]) as Character;
			Character target = _world.tingRunner.GetTing(args[1]) as Character;
			return c.conversationTarget == target;
		}

		// HasKnowledge(Character character, string knowledge)
		public bool HasKnowledge(string[] args)
		{
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			return character.HasKnowledge(args[1]);
		}

		// IsAtTask(Character character, TimetableTaskName task)
		private bool IsAtTask(string[] args)
		{
			if(args.Length != 2) {
				D.LogError("Wrong nr of args to IsAtTask: " + args.Length);
			}
			Character character = _world.tingRunner.GetTing(args[0]) as Character;			
			if(character == null) {
				D.Log(args[0] + " is not a Character");
			}
			string taskName = args[1];
			bool atTask = character.IsAtTimetableTask(taskName);			
			return atTask;
		}

		private bool IsNotAtTask(string[] args)
		{
			return !IsAtTask(args);
		}

		// IsWithinDistance(Ting ting1, Ting ting2, int distance)
		private bool IsWithinDistance(string[] args)
		{
			Ting t1 = _world.tingRunner.GetTing(args[0]);
			Ting t2 = _world.tingRunner.GetTing(args[1]);
			int distance = Convert.ToInt32(args[2]);
			return AreTingsWithinDistance(t1, t2, distance);
		}

		private bool IsOutsideDistance(string[] args)
		{
			Ting t1 = _world.tingRunner.GetTing(args[0]);
			Ting t2 = _world.tingRunner.GetTing(args[1]);
			int distance = Convert.ToInt32(args[2]);
			return !AreTingsWithinDistance(t1, t2, distance);
		}

		public static bool AreTingsWithinDistance(Ting t1, Ting t2, int pDistance) {
			int distance = pDistance;
			if(t1.room != t2.room) {
				return false;
			}
			int dx = t1.localPoint.x - t2.localPoint.x;
			int dy = t1.localPoint.y - t2.localPoint.y;
			return ((dx * dx) + (dy * dy)) < (distance * distance);
		}
		
		// IsFieldGreaterThan(Ting ting, string field, float threshold)
		private bool IsFieldGreaterThan(string [] args)
		{
			Ting ting = _world.tingRunner.GetTing(args[0]);
			string fieldName = args[1];
			float val = ting.table.GetValue<float>(ting.objectId, fieldName);
			float threshold = Convert.ToSingle(args[2]);
			//D.Log("Comparing value of field " + fieldName + ": " + val + " with threshold: " + threshold);
			return val > threshold;
		}

		// IsFieldLessThan(Ting ting, string field, float threshold)
		private bool IsFieldLessThan(string [] args)
		{
			return !IsFieldGreaterThan(args);
		}

		private bool HasSoda(string [] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			if (character.handItem is Drink && (character.handItem as Drink).liquidType == "soda") {
				return true;
			}
			Drink[] drinksInInventory = _world.tingRunner.GetTingsOfTypeInRoom<Drink> (character.inventoryRoomName);
			foreach (var drink in drinksInInventory) {
				if (drink.liquidType == "soda") {
					return true;
				}
			}
			return false;
		}
		
		// HasHandItemWithName(Character character, Ting ting)
		private bool HasHandItemWithName(string [] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			if(character.handItem == null) {
				return false;
			}
			return character.handItem.name == args[1];
		}
		
		// HasHandItemOfType(Character character, Ting ting)
		private bool HasHandItemOfType(string [] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			if(character.handItem == null) {
				return false;
			}
			string name = character.handItem.GetType().Name.ToString();
			//D.Log(name);
			return name == args[1];
		}

		// HasHandItemOfDrinkType(Character character, string drinkType)
		private bool HasHandItemOfDrinkType(string [] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			Drink drink = character.handItem as Drink;
			if (drink == null) {
				return false;
			}
			string drinkType = args [1].ToLower ();
			return drink.liquidType.ToLower().Contains(drinkType);
		}

		// HasAnyHandItem(Character character)
		private bool HasAnyHandItem(string [] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			return character.handItem != null;
		}

		// HasNoHandItem(Character character)
		private bool HasNoHandItem(string [] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			return character.handItem == null;
		}

		// HasItemOfType(Character character, string tingTypeName)
		bool HasItemOfType(string[] args)
		{
			if(HasHandItemOfType(args)) {
				return true;
			}

			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			return character.HasInventoryItemOfType(args[1]);
		}

		// DrinkInHandIsAlmostEmpty(Character character)
		bool DrinkInHandIsAlmostEmpty(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			if(character.handItem is Drink) {
				var drink = character.handItem as Drink;
				return drink.amount < 10f;
			}
			else {
				D.Log("DrinkInHandIsAlmostEmpty doesn't work because " + character.name + " isn't holding a drink.");
				return false;
			}
		}

		// HasDrinkOfType(Character character, string drinkTypeName)
		bool HasDrinkOfType(string[] args)
		{
			Character character = _world.tingRunner.GetTing<Character>(args[0]);
			string drinkType = args [1].ToLower ();

			Drink drink = character.handItem as Drink;
			if (drink != null) {
				if (drink.liquidType.ToLower ().Contains (drinkType)) {
					return true;
				}
			}

			foreach (var item in character.inventoryItems) {
				Drink drink2 = item as Drink;
				if (drink2 != null) {
					if (drink2.liquidType.ToLower ().Contains (drinkType)) {
						return true;
					}
				}
			}

			return false;
		}
			
		// IsPawnDead(Pawn pawn)
		bool IsPawnDead(string [] args)
		{
			var pawn = _world.tingRunner.GetTing<Pawn>(args[0]);
			D.Log("Checking if " + pawn.name + " is dead: " + pawn.dead);
			return pawn.dead;
		}
		
		// IsPawnAlive(Pawn pawn)
		bool IsPawnAlive(string [] args)
		{
			var pawn = _world.tingRunner.GetTing<Pawn>(args[0]);
			return !pawn.dead;
		}
		
		// HasMadeMoreMovesThan(Pawn pawn, int nrOfMoves)
		bool HasMadeMoreMovesThan(string [] args) 
		{
			var pawn = _world.tingRunner.GetTing<Pawn>(args[0]);
			int moves = Convert.ToInt32(args[1]);
			return pawn.moveNr > moves;
		}

		// HasErrors(MimanTing ting)
		bool HasErrors(string [] args) 
		{
			var ting = _world.tingRunner.GetTing<MimanTing>(args[0]);
			return ting.containsBrokenPrograms;
		}

		// HasNoErrors(MimanTing ting)
		bool HasNoErrors(string [] args) 
		{
			var ting = _world.tingRunner.GetTing<MimanTing>(args[0]);
			return !ting.containsBrokenPrograms;
		}

		// IsCubeGlowing(Cube cube)
		bool IsCubeGlowing(string [] args) {
			var cube = _world.tingRunner.GetTing<MysticalCube>(args[0]);
			return cube.color.x > 0f || cube.color.y > 0f || cube.color.z > 0f;
		}

		#endregion
	}
}

