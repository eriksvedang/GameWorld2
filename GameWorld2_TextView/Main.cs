//#define LOAD_QUICKSAVE
//#define ONLY_STARTUP

using System;
using GameWorld2;
using GameTypes;
using TingTing;
using System.Reflection;
using GrimmLib;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;

namespace GameWorld2_TextView
{
    class MainClass
    {
		const float FPS = 30.0f;
        const float NORMAL_DELTA_TIME = 1.0f / FPS;
		static ShowInEditor SHOW_IN_EDITOR = new ShowInEditor();
		static EditableInEditor EDITABLE_IN_EDITOR = new EditableInEditor();

        static World _world;
		static bool _run = true;
		static string _focusedConversation = "";
		static List<string> _macro = new List<string>();
		static int _macroPos = 0;
		static bool _autoAnswer = false;
		static bool _autoPilot = false;
		static float _timeLimit = float.MaxValue;
		
        public static void Main(string[] args)
        {
			TextView (args);
			//SimplePathfinding();
			//ProfileMimanPathfinding ();
        }

		private static void SimplePathfinding() 
		{
			D.onDLog += Console.WriteLine; // take care of the D.Log message from invalid path

			InitialSaveFileCreator isc = new InitialSaveFileCreator();
			World world = new World(isc.CreateEmptyRelay());

			SimpleRoomBuilder srb = new SimpleRoomBuilder(world.roomRunner);
			srb.CreateRoomWithSize("Eden", 10, 10);
			var middleRoom = srb.CreateRoomWithSize("MiddleRoom", 10, 10);
			var wrongRoom = srb.CreateRoomWithSize("WrongRoom", 10, 10);
			var distantRoom = srb.CreateRoomWithSize("DistantRoom", 10, 10);

			middleRoom.worldPosition = new IntPoint(10, 0);
			wrongRoom.worldPosition = new IntPoint(30, 0);
			distantRoom.worldPosition = new IntPoint(50, 0);
			
			var p1 = world.tingRunner.CreateTing<Point>("p1", new WorldCoordinate("Eden", new IntPoint(2, 4)));
			var p2 = world.tingRunner.CreateTing<Point>("p2", new WorldCoordinate("DistantRoom", new IntPoint(4, 4)));
			
			// Add doors
			var edenDoor = world.tingRunner.CreateTing<Door>("edenDoor", new WorldCoordinate("Eden", new IntPoint(4, 4)));
			var middleRoomDoor1 = world.tingRunner.CreateTing<Door>("middleRoomDoor1", new WorldCoordinate("MiddleRoom", new IntPoint(2, 4)));
			var middleRoomDoor2 = world.tingRunner.CreateTing<Door>("middleRoomDoor2", new WorldCoordinate("MiddleRoom", new IntPoint(7, 4)));
			var middleRoomDoor3 = world.tingRunner.CreateTing<Door>("middleRoomDoor3", new WorldCoordinate("MiddleRoom", new IntPoint(4, 4)));
			var wrongRoomDoor = world.tingRunner.CreateTing<Door>("wrongRoomDoor", new WorldCoordinate("WrongRoom", new IntPoint(4, 4)));
			var distantRoomDoor = world.tingRunner.CreateTing<Door>("distantRoomDoor", new WorldCoordinate("DistantRoom", new IntPoint(2, 4)));

			edenDoor.targetDoorName = "middleRoomDoor1";
			middleRoomDoor1.targetDoorName = "edenDoor";
			middleRoomDoor2.targetDoorName = "distantRoomDoor";
			middleRoomDoor3.targetDoorName = "wrongRoomDoor";
			wrongRoomDoor.targetDoorName = "middleRoomDoor3";
			distantRoomDoor.targetDoorName = "middleRoomDoor2";

			world.sourceCodeDispenser.CreateSourceCodeFromString ("OnDoorUsed", "");
			
			if(!world.isReadyToPlay) {
				foreach (string s in world.Preload()) {}
			}

			var pathfinder = new MimanPathfinder2(world.tingRunner, world.roomRunner);
			
			var result = pathfinder.Search(p1, p2);
			Console.WriteLine("Result: " + result);
		}

		private static void ProfileMimanPathfinding () {
			for(int i = 3; i < 11; i++) {
				D.Log("WORLD SIZE " + i);
				ProfileMimanPathfinding(i);
			}
		}

		private static void ProfileMimanPathfinding(int worldSize)
		{
			D.onDLog += Console.WriteLine;

			InitialSaveFileCreator isc = new InitialSaveFileCreator();
			World world = new World(isc.CreateEmptyRelay());

			SimpleRoomBuilder srb = new SimpleRoomBuilder(world.roomRunner);

			int worldWidth = worldSize;
			int worldHeight = worldSize;
			int roomSize = 10;

			int tingCounter = 0;

			for (int x = 0; x <= worldWidth; x++) {
				for (int y = 0; y <= worldHeight; y++) {
					Room r = srb.CreateRoomWithSize("Room_" + x + "_" + y, roomSize, roomSize);

					Door door1 = world.tingRunner.CreateTing<Door> (r.name + "_Door1", new WorldCoordinate (r.name, new IntPoint (0, 0)));
					Door door2 = world.tingRunner.CreateTing<Door> (r.name + "_Door2", new WorldCoordinate (r.name, new IntPoint (roomSize - 1, roomSize - 1)));
					door1.direction = Direction.DOWN;
					door2.direction = Direction.UP;

					// Randomizer.GetIntValue (0, roomSize);

					int rx1 = (x + 1) % worldWidth;
					int ry1 = y;
					door1.targetDoorName = "Room_" + rx1 + "_" + ry1 + "_Door2";

					int rx2 = x;
					int ry2 = (y + 1) % worldHeight;
					door2.targetDoorName = "Room_" + rx2 + "_" + ry2 + "_Door1";

					world.tingRunner.CreateTing<Point> ("Point_" + (tingCounter++), new WorldCoordinate (r.name, new IntPoint (roomSize / 2, roomSize / 2)));
				
					r.worldPosition = new IntPoint(x * roomSize + Randomizer.GetIntValue(-3, 3), 
					                               y * roomSize + Randomizer.GetIntValue(-3, 3));
				}
			}

			world.sourceCodeDispenser.CreateSourceCodeFromString ("OnDoorUsed", "");

			if(!world.isReadyToPlay) {
				foreach (string s in world.Preload())
				{
					//Console.WriteLine(s);
				}
			}

			Console.WriteLine ("Room runner stats: " + world.roomRunner.ToString ());

			var pathfinder = new MimanPathfinder2 (world.tingRunner, world.roomRunner);

			for (int i = 0; i < 100; i++) {
				Point start = world.tingRunner.GetTing<Point> ("Point_" + Randomizer.GetIntValue (0, tingCounter));
				Point goal = world.tingRunner.GetTing<Point> ("Point_" + Randomizer.GetIntValue (0, tingCounter));
				var result = pathfinder.Search (start, goal);
				Console.WriteLine (result.status);
			}
		}

		private static void TextView(string[] args) {
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			DateTime startTime = DateTime.Now;
			D.onDLog += (pMessage) => Console.WriteLine ("" + pMessage);

			string macroFileName = "macro1.txt";
			string saveFileName = "";

			for (int i = 0; i < args.Length; i++) {
				string s = args [i];
				if (s == "-autopilot") {
					_autoPilot = true;
					_autoAnswer = true;
					_timeLimit = Convert.ToSingle (args [++i]);
				} else if (s == "-macro") {
					macroFileName = args [++i];
				} else if (s == "-load") {
					saveFileName = args [++i];
				}
			}

			StreamReader macroFile = File.OpenText ("../" + macroFileName);
			while (!macroFile.EndOfStream) {
				string line = macroFile.ReadLine ();
				_macro.Add (line);
			}

			// Instantiate
			IEnumerable<float> loader = null;
			InitialSaveFileCreator saveCreator = new InitialSaveFileCreator ();

			if(saveFileName != "") {
				//IEnumerable<float> loader = saveCreator.LoadFromFile("../../../../../assembla/MimanUnity2/Saves/Quicksave.json");
				loader = saveCreator.LoadFromFile(saveFileName);
			} else {
				loader = saveCreator.LoadRelayFromDirectory ("../../../../../assembla/MimanUnity2/InitData/");
			}

			// Load files
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			foreach (float f in loader) {
				Console.Write (".");
			}
			Console.Write ("\n");

			// Setup world and its runners
			_world = new World (saveCreator.GetLoadedRelay ());
			SetupWorldListeners ();

			SmartWalkBehaviour.s_logger.AddListener (Console.WriteLine);

			LoadTranslationFiles();

			// Preload
			foreach (string s in _world.Preload()) {
				//Console.WriteLine(s);
			}

			Console.WriteLine ("Loading took " + (DateTime.Now - startTime).TotalSeconds + " seconds");
			Console.ForegroundColor = ConsoleColor.White;

			#if ONLY_STARTUP
			return;
			#endif

			// Run a few frames
			for (int i = 0; i < 30; i++) {
				_world.Update (NORMAL_DELTA_TIME);
			}

			if(saveFileName == "") {
				_world.dialogueRunner.StartConversation ("StoryStart");
			}

			// Run
			while (_run) {
				try {
					PrintBranchingNode ();

					if (_autoPilot) {
						if (_macroPos < _macro.Count) {
							Eval (new string[] { "macro" });
						} else {
							Eval (new string[] { "tick" });
						}
						if (_world.settings.totalWorldTime > _timeLimit) {
							Eval (new string[] { "clock" });
							Console.WriteLine ("Stopped by time limit (" + _timeLimit + " s.)");
							break;
						}
					} else {
						Console.ForegroundColor = ConsoleColor.White;
						//Console.Write("\n> ");
						Console.Write ("\n" + _world.settings.gameTimeClock + " => ");
						string command = Console.ReadLine ();
						Eval (Split (command));
					}
				} catch (Exception e) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine ("Error in World: " + e.Message + ", stack trace:\n" + e.StackTrace);
					Console.ForegroundColor = ConsoleColor.White;
					if (_autoPilot) {
						_run = false;
					}
				}
			}
		}

		static void LoadTranslationFiles ()
		{
			_world.translator.LoadTranslationFiles ("../../../../../assembla/MimanUnity2/InitData/Translations");
		}

		static void PrintBranchingNode()
		{
			BranchingDialogueNode branchingNode = _world.dialogueRunner.GetActiveBranchingDialogueNode(_focusedConversation);
			if (branchingNode != null) {
				int i = 1;
				Console.Write("\n");
				ConsoleColor previousColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				foreach (string optionNodeName in branchingNode.nextNodes) {
					TimedDialogueNode optionNode = _world.dialogueRunner.GetDialogueNode(_focusedConversation, optionNodeName) as TimedDialogueNode;
					Console.WriteLine("   " + i++ + ". " + _world.translator.Get(optionNode.line, _focusedConversation));
				}
				Console.ForegroundColor = previousColor;
				Console.Write("\n");
				if (_autoAnswer) {
					ChooseOption(Randomizer.GetIntValue(0, branchingNode.nextNodes.Length));
				}
			}
		}

		static void SetupWorldListeners()
		{
			_world.dialogueRunner.logger.AddListener((pMessage) => {
				var previousColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("[Grimm] " + pMessage);
				Console.ForegroundColor = previousColor;
			});
			_world.dialogueRunner.AddOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			_world.dialogueRunner.AddFocusConversationListener(OnConversationFocus);
			_world.dialogueRunner.AddDefocusConversationListener(OnConversationDefocus);
		}
		
		private static void Reset()
		{
			_world.dialogueRunner.RemoveOnSomeoneSaidSomethingListener(OnSomeoneSaidSomething);
			_world.dialogueRunner.RemoveDefocusConversationListener(OnConversationFocus);
			_world.dialogueRunner.RemoveFocusConversationListener(OnConversationDefocus);

			_focusedConversation = "";
		}
		
		private static string[] Split (string pString)
		{
			if (pString == null) {
				return new string[] { "t" };
			}
			return pString.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
		}

		private static void ChooseOption(int pOptionNr)
		{
			BranchingDialogueNode branchingNode = _world.dialogueRunner.GetActiveBranchingDialogueNode(_focusedConversation);

			if (branchingNode != null) {
				if (pOptionNr < 0 || pOptionNr > branchingNode.nextNodes.Length - 1) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(pOptionNr.ToString() + " is not a valid option on this branching node with " + branchingNode.nextNodes.Length.ToString() + " options");
				} else {
					branchingNode.Choose(pOptionNr);
				}
			}
			else {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Is not at a branching node");
			}

			Eval(new string[] {"ff", "7"});
			Eval(new string[] {"ff", "7"});
		}
     
        private static void Eval(string[] pCommand)
        {
			Console.ForegroundColor = ConsoleColor.White;

            if (pCommand.Length == 0) {
				pCommand = new string[] { "m" };
            }
         
            int nrOfUpdates = 1;
         
            switch (pCommand[0]) {
            case "q":
            case "quit":
                _run = false;
                break;

			case "1":
				ChooseOption(0);
				break;

			case "2":
				ChooseOption(1);
				break;

			case "3":
				ChooseOption(2);
				break;

			case "4":
				ChooseOption(3);
				break;

			case "5":
				ChooseOption(4);
				break;

			case "6":
				ChooseOption(5);
				break;

			case "7":
				ChooseOption(6);
				break;

			case "PATH":
				var pathfinder = new MimanPathfinder2 (_world.tingRunner, _world.roomRunner);
				Ting sebastian = _world.tingRunner.GetTing ("Sebastian");
				Ting wellspringer = _world.tingRunner.GetTing ("Wellspringer");
				for (int i = 0; i < 100; i++) {
					D.Log (" - " + i + " - ");
					D.Log(pathfinder.Search (sebastian, wellspringer).ToString());
				}
				D.Log ("Did 100 searches.");
				break;
				
			case "clock":
				PrintClock();
                break;

			case "bn":
				BranchingDialogueNode branchingNode = _world.dialogueRunner.GetActiveBranchingDialogueNode(_focusedConversation);
				if (branchingNode != null) {
					Console.WriteLine("On branching node " + branchingNode.name + " in conversation " + branchingNode.conversation);
				} else {
					Console.WriteLine("Not on a branching node");
				}
				break;

			case "occupants":
				Room r = _world.roomRunner.GetRoom (pCommand [1]);
				foreach (var tile in r.tiles) {
					D.Log (tile.ToString () + ": " + tile.GetOccupantsAsString());
				}
				break;

            case "t":
            case "tick":
				nrOfUpdates = 1;
                if (pCommand.Length > 1) {
                    nrOfUpdates = Convert.ToInt32(pCommand[1]);
                }
                for (int i = 0; i < nrOfUpdates; i++) {
                    _world.Update(NORMAL_DELTA_TIME);
					if (!_autoPilot) {
						PrintTotalWorldTime();
					}
                }
                break;
				
			case "ff": // fast forward
				DateTime startTime = DateTime.Now;
				nrOfUpdates = (int)FPS;
				if (pCommand.Length > 1) {
					nrOfUpdates = Convert.ToInt32(pCommand[1]) * (int)FPS;
				}
				for (int i = 0; i < nrOfUpdates; i++) {
					_world.Update(NORMAL_DELTA_TIME);
				}
                PrintTotalWorldTime();
				double totalLength = (DateTime.Now - startTime).TotalSeconds;
				float secsPerUpdate = (float)(totalLength / (double)nrOfUpdates);
				float updatesPerSec = 1f / secsPerUpdate;
				Console.WriteLine("ff took " + (float)totalLength + "s. (" + secsPerUpdate + " s/update, " + updatesPerSec + " updates/s)");
                break;

			case "->": // smart fast forward
				SmartFastForward(pCommand);
				break;
				
			case "!":
			case "run": // run Grimm script
				RunGrimmScript(pCommand, 1);
                break;
				
            case "dump":
                Console.WriteLine(_world.tingRunner.ToString());
                Console.WriteLine (_world.roomRunner.ToString());
                Console.WriteLine(_world.dialogueRunner.ToString());
                Console.WriteLine(_world.programRunner.ToString());
                Console.WriteLine(_world.sourceCodeDispenser.ToString());
				Console.WriteLine(_world.timetableRunner.ToString());
                break;

			case "book":
				// turn all the dialogue into a file called book.txt
				string text = _world.dialogueRunner.GetAllDialogueAsString();
				var file = File.CreateText("book.txt");
				file.Write(text);
				file.Close();
				Console.WriteLine("Saved all dialogue to book.txt");
				break;
				
			case "i":
            case "info":
				string tingName = "Sebastian";
                if (pCommand.Length > 1) {
                    tingName = pCommand[1];
                }
				MimanTing ting = _world.tingRunner.GetTing<MimanTing>(tingName);
                PrintTingInfo(ting);
                break;

			case "pp":
				string tingName2 = "Sebastian";
				if (pCommand.Length > 1) {
					tingName2 = pCommand [1];
				}
				Character character = _world.tingRunner.GetTing<Character> (tingName2);
				if (character != null) {
					Console.WriteLine (character.PrettyPrintableInfo ());
				} else {
					Console.WriteLine ("Not a character");
				}
				break;

			case "ppall":
				foreach(var c in _world.tingRunner.GetTingsOfType<Character>()) {
					Console.WriteLine(c.name + "\t" + c.PrettyPrintableInfo());
				}
				break;
				
			case "m":
			case "macro":
				ExecuteMacro();
				break;

			case "progs":
				foreach(var prog in _world.programRunner.GetAllPrograms()) {
					Console.WriteLine ("- " + prog);
				}
				break;

			case "h":
			case "help":
				Console.WriteLine("q / quit \ni / info [TING] \ndump (prints runners) \n! / run [GRIMM COMMANDO] \nclock (prints in game clock) \nt / tick [N] (ticks N steps) \nff [N] (fast forwards N seconds, does not print time stamp) \nauto_answer [ON | OFF] (choose answers automatically in conversations) \nload/save \nlog [TING]\nconvos (list all active conversations) \nbook save a book.txt file with all dialogue \nroom [TING (optional)] list all chracters in the same room as the ting, no arg means avatar \n@ [TING] translates to avatarName.Interact(TING) and ff until the interaction event\npp [character] pretty print info about character\nppall pretty print info about all characters\n\nprogs print all program names");
				break;
				
			case "save":
				_world.Save("save.json");
				break;
				
			case "load":
				Reset();
				_world = new World("save.json");
				LoadTranslationFiles();
				foreach (string s in _world.Preload()) {
					//Console.WriteLine(s);
				}
				SetupWorldListeners();
				break;

			case "log":
				Ting t = _world.tingRunner.GetTing(pCommand[1]);
				t.logger.AddListener(Console.WriteLine);
				break;

			case "convos":
				var activeConversations = _world.dialogueRunner.GetActiveConversations();
				foreach (var convo in activeConversations) {
					Console.WriteLine(" " + convo);
				}
				break;

			case "room":
				PrintRoomTings(pCommand);
				break;

			case "@":
				FastInteract(pCommand);
				break;
				
			case "auto_answer":
				_autoAnswer = true;
				if (pCommand.Length > 1) {
                    if(pCommand[1].ToString().ToLower() == "off") {
						_autoAnswer = false;
					}
                }
				Console.WriteLine("Auto answer: " + (_autoAnswer ? "ON" : "OFF"));
				break;

			case "stop":
				Console.WriteLine("Stopped by command 'stop'");
				_run = false;
				break;
				
            default:
                //Console.WriteLine("Can't understand command '" + pCommand[0] + "'");
				RunGrimmScript(pCommand, 0);

				break;
            }
        }

		static void SmartFastForward(string[] pCommand)
		{
			string avatarName = _world.settings.avatarName;
			var avatar = _world.tingRunner.GetTing<Character>(avatarName);
			int maxCounter = 5000;
			if (pCommand.Length == 1 || pCommand[1] == "idle") {
				_world.Update(NORMAL_DELTA_TIME);
				while (!avatar.IsIdle()) {
					PrintBranchingNode();
					_world.Update(NORMAL_DELTA_TIME);
					maxCounter--;
					if (maxCounter <= 0) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Hit smart counter limit, cancels smart fast forward");
						break;
					}
				}
			}
		}

		static void FastInteract(string[] pCommand)
		{
			string avatarName = _world.settings.avatarName;
			var avatar = _world.tingRunner.GetTing<Character>(avatarName);
			string targetName = pCommand[1];
			var target = _world.tingRunner.GetTing<Character>(targetName);
			avatar.WalkToTingAndInteract(target);
			bool thereYet = false;
			GrimmLib.DialogueRunner.OnEvent test = (eventName => {
				if (eventName == (avatarName + "_talk_" + targetName)) {
					thereYet = true;
				}
			});
			_world.dialogueRunner.AddOnEventListener(test);
			int maxCounter = 5000;
			while (!thereYet) {
				_world.Update(NORMAL_DELTA_TIME);
				maxCounter--;
				if (maxCounter <= 0) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Seems hard for " + avatarName + " to get to " + targetName + ", cancels ff");
					break;
				}
			}
			_world.dialogueRunner.RemoveOnEventListener(test);
		}

		static void PrintRoomTings(string[] pCommand)
		{
			string tingName = "";
			if (pCommand.Length == 1) {
				tingName = _world.settings.avatarName;
			} else {
				tingName = pCommand[1];
			}
			var tingToFind = _world.tingRunner.GetTing(tingName);
			string roomName = tingToFind.room.name;
			var tingsInRoom = _world.tingRunner.GetTingsInRoom (roomName); //GetTingsOfTypeInRoom<Character>(roomName);
			Console.WriteLine("Tings in room " + roomName + ": ");
			foreach(var tingInRoom in tingsInRoom) {
				Console.WriteLine(" - " + tingInRoom);
			}
		}

		static void PrintClock()
		{
			Console.WriteLine(_world.settings.gameTimeClock);
		}

		static void RunGrimmScript(string[] pCommand, int pStartIndex) {
			try {
				string grimmCommand = "";
				for (int i = pStartIndex; i < pCommand.Length; i++) {
					grimmCommand += pCommand[i] + " ";
				}
				_world.dialogueRunner.RunStringAsFunction(grimmCommand);
			}
			catch (GrimmLib.GrimmException e) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Grimm result: " + e.Message);
			}
		}
		
		static void PrintTingInfo(MimanTing pTing)
		{
			foreach(PropertyInfo p in pTing.GetType().GetProperties())
			{
				foreach(Attribute a in p.GetCustomAttributes(true)) 
				{
					if(a.Match(EDITABLE_IN_EDITOR) || a.Match(SHOW_IN_EDITOR)) 
					{
						object o = p.GetValue(pTing, null);
						
						string line = "";
						if(o == null) line = "null";
						else if(o is string) line = "\"" + o.ToString() + "\"";
						else line = o.ToString();
						
						Console.WriteLine(p.Name + ": " + line);
					}
				}
			}
		}

		static void OnSomeoneSaidSomething(Speech pSpeech)
		{
			if(pSpeech.line != "") { 
				ConsoleColor previousColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(pSpeech.speaker + ": " + _world.translator.Get(pSpeech.line, pSpeech.conversation));
				Console.ForegroundColor = previousColor;
			}
		}

        static void PrintTotalWorldTime()
        {
            Console.WriteLine("Total world time: " + _world.settings.totalWorldTime + " s");
        }
		
		static void OnConversationFocus(string pConversation)
		{
			Console.WriteLine(" --- FOCUSED ON " + pConversation + " --- ");
			_focusedConversation = pConversation;
		}

		static void OnConversationDefocus(string pConversation)
		{
			if (_focusedConversation == pConversation) {
				Console.WriteLine (" --- DEFOCUSED FROM " + pConversation + " --- ");
				_focusedConversation = "";
				_world.tingRunner.GetTing<Character> (_world.settings.avatarName).StopTalking ();
			} else {
				//Console.WriteLine ("Got OnConversationDefocus '" + pConversation + "' but isn't focused on that dia so will ignore it.");
			}
		}

		static void ExecuteMacro()
		{
			if(_macroPos < _macro.Count) {
				string line = _macro[_macroPos++];
				var prevColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.DarkMagenta;
				Console.WriteLine("Executing macro \"" + line + "\"");
				Console.ForegroundColor = prevColor;
				Eval(Split(line));
			}
			else {
				Console.WriteLine("No more lines in macro");
			}
		}
    }
}
