using System.Collections.Generic;
using GrimmLib;
using TingTing;
using GameTypes;
using ProgrammingLanguageNr1;
using RelayLib;
using System.IO;
using System.Linq;
using System;

namespace GameWorld2
{
	public class Computer : MimanTing
	{
		public static new string TABLE_NAME = "Ting_Computers";
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_floppyBootProgramName;
		ValueEntry<string[]> CELL_consoleOutput;
		ValueEntry<int> CELL_nrOfLines;
		ValueEntry<int> CELL_charsOnLine;
		ValueEntry<int> CELL_currentLine;
		ValueEntry<int> CELL_currentTopLine;
		ValueEntry<string> CELL_currentInput;
		ValueEntry<bool> CELL_hasInternetAPI;
		ValueEntry<bool> CELL_hasGraphicsAPI;
		ValueEntry<bool> CELL_hasWeatherAPI;
		ValueEntry<bool> CELL_hasLampAPI;
		ValueEntry<bool> CELL_hasDoorAPI;
		ValueEntry<bool> CELL_hasMemoryAPI;
		ValueEntry<bool> CELL_hasVoiceAPI;
		ValueEntry<bool> CELL_hasElevatorAPI;
		ValueEntry<bool> CELL_hasTingrunnerAPI;
		ValueEntry<bool> CELL_hasTrapAPI;
		ValueEntry<bool> CELL_hasHeartAPI;
		ValueEntry<bool> CELL_hasArcadeMachineAPI;
		ValueEntry<bool> CELL_hasFloppyAPI;
		ValueEntry<int> CELL_mhz;
		ValueEntry<string> CELL_memoryUnitName;
		ValueEntry<float> CELL_maxExecutionTime;
		ValueEntry<int> CELL_screenWidth;
		ValueEntry<int> CELL_screenHeight;
		ValueEntry<string> CELL_floppyInDrive;

		Program _program, _floppyBootProgram;
		Character _user;

		public delegate void OnLineDrawing (IntPoint p1,IntPoint p2);
		public delegate bool IsKeyPressed (string key);
		public delegate void OnClearScreen();
		public delegate void OnSetColor(float r, float g, float b);
		public delegate void OnTextDrawing(int x, int y, string text);

		public OnLineDrawing onLineDrawing;
		public OnLineDrawing onRectDrawing;
		public System.Action onDisplayGraphics;
		public IsKeyPressed isKeyPressed;
		public OnClearScreen onClearScreen;
		public OnSetColor onSetColor;
		public OnTextDrawing onTextDrawing; // not normal printing, this text can be positioned

		protected override void SetupCells ()
		{
			base.SetupCells ();
			CELL_programName = EnsureCell ("masterProgramName", "HelloWorld");
			CELL_floppyBootProgramName = EnsureCell ("floppyBootProgramName", "BlankSlate");

			CELL_nrOfLines = EnsureCell ("nrOfLines", 24);
			CELL_charsOnLine = EnsureCell ("charsOnLine", 100); // used to not exist, will have to work around that now
			CELL_currentLine = EnsureCell ("currentLine", 0);
			CELL_currentTopLine = EnsureCell ("currentTopLine", 0);
			CELL_consoleOutput = EnsureCell ("consoleOutput", new string[64]);

			CELL_currentInput = EnsureCell ("currentInput", "");
		
			CELL_hasInternetAPI = EnsureCell ("hasInternetAPI", true);
			CELL_hasGraphicsAPI = EnsureCell ("hasGraphicsAPI", true);
			CELL_hasWeatherAPI = EnsureCell ("hasWeatherAPI", false);
			CELL_hasLampAPI = EnsureCell ("hasLampAPI", false);
			CELL_hasDoorAPI = EnsureCell ("hasDoorAPI", false);
			CELL_hasMemoryAPI = EnsureCell ("hasMemoryAPI", false);
			CELL_hasVoiceAPI = EnsureCell ("hasVoiceAPI", false);
			CELL_hasElevatorAPI = EnsureCell ("hasElevatorAPI", false);
			CELL_hasTingrunnerAPI = EnsureCell ("hasTingrunnerAPI", false);
			CELL_hasTrapAPI = EnsureCell ("hasTrapAPI", false);
			CELL_hasHeartAPI = EnsureCell("hasHeartAPI", false);
			CELL_hasArcadeMachineAPI = EnsureCell("hasArcadeMachineAPI", false);
			CELL_hasFloppyAPI = EnsureCell("hasFloppyAPI", false);

			CELL_mhz = EnsureCell("mhz", 30);
			CELL_maxExecutionTime = EnsureCell("maxExecutionTime", -1.0f);

			CELL_memoryUnitName = EnsureCell ("memoryUnit", "");
			CELL_floppyInDrive = EnsureCell ("floppyInDrive", "");

			CELL_screenWidth = EnsureCell("screenWidth", 512);
			CELL_screenHeight = EnsureCell("screenHeight", 256);
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public void RemovePrograms() {
			_program = null;
			_floppyBootProgram = null;
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		public override int securityLevel {
			get {
				if(masterProgramName == "CashRegister") {
					return 1;
				}
				else {
					return 0;
				}
			}
		}

		public Program activeProgram {
			get {
				if(floppyBootProgram != null && floppyBootProgram.isOn) {
					return floppyBootProgram;
				}
				else {
					return masterProgram;
				}
			}
		}

		public override void Say (string pLine, string pConversation)
		{
			if(onTextDrawing != null) {
				API_Print(pLine);
			}

			if(name.ToLower().Contains("bandit")) {
				if(user != null && !user.isAvatar) {
					//D.Log(name + " will not say " + pLine + " because " + user + " is not avatar");
					return; // don't talk, screen gets overwhelmed with speech bubbles
				}
				else if(user == null) {
					//D.Log(name + " will not say " + pLine + " because " + user + " is null");
					return; // don't talk, screen gets overwhelmed with speech bubbles
				}
			}

			base.Say (pLine, pConversation);
		}

		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();

			hasInternetAPI = true;

			if (name.Contains ("TriPod")) {
				nrOfLines = 22;
				ReplaceHelloWorldProgram ("TriPodOS");
				hasFloppyAPI = true;
				hasMemoryAPI = true;
			} else if (name.Contains ("MediumSewerComputer")) {
				nrOfLines = 14;
				ReplaceHelloWorldProgram ("MediumSewerComputer");
				hasFloppyAPI = true;
				hasMemoryAPI = true;
			} else if (name.Contains ("MediumComputer1")) {
				// gröna kubliknande datorn
				nrOfLines = 14;
				ReplaceHelloWorldProgram ("Moonlander");
				hasFloppyAPI = true;
			} else if (name.Contains ("LargeComputerL4")) {
				// stor dator med ganska stor skärm
				nrOfLines = 16;
				ReplaceHelloWorldProgram ("Wayfinder");
				hasFloppyAPI = true;
				hasDoorAPI = true;
			} else if (name.Contains ("LargeComputerL3")) {
				// stor dator med lite mindre skärm
				nrOfLines = 16;
				ReplaceHelloWorldProgram ("EmailClient");
				hasFloppyAPI = true;
			} else if (name.Contains ("LargeComputerL2")) {
				// grundläggande dator med stor widescreenskärm etc, finns lite överallt
				ReplaceHelloWorldProgram ("PedestrianOS");
				hasFloppyAPI = true;
				hasDoorAPI = true;
			} else if (name.Contains ("HugeComputer")) {
				// hög och ganska smal dator utan skärm
				ReplaceHelloWorldProgram ("ConnectionServer");
				hasFloppyAPI = true;
				hasMemoryAPI = true;
				hasGraphicsAPI = false; // no graphics
			} else if (name.Contains ("PillarComputer")) {
				// väldig smal och hög dator utan skärm
				ReplaceHelloWorldProgram ("ConnectionServer2");
				hasFloppyAPI = true;
			} else if (name.Contains ("Ministry")) {
				if (name.Contains ("LargeRecorder")) {
					ReplaceHelloWorldProgram ("MinistryLargeRecorder");
					hasTrapAPI = true;
					hasFloppyAPI = true;
				} else if (name.Contains ("ModernComputer") || name.Contains ("NewComputerScreen") || name.Contains ("ComputerTerminal") || name.Contains ("LapTop")) {
					ReplaceHelloWorldProgram ("MinistryOS");
					hasFloppyAPI = true;
					hasMemoryAPI = true;
					nrOfLines = 16;
				} else if (name.Contains ("FacadeComputer")) {
					ReplaceHelloWorldProgram ("MinistryFacadeComputer");
					hasFloppyAPI = true;
				} else if (name.Contains ("Wooper")) {
					ReplaceHelloWorldProgram ("MinistryWooper");
					hasFloppyAPI = true;
				} else if (name.Contains ("Cubbard")) {
					ReplaceHelloWorldProgram ("MinistryInformationBoard");
					hasFloppyAPI = true;
				}
			} else if (name.Contains ("RecorderComputer") || name.Contains ("LargeRecorder")) {
				ReplaceHelloWorldProgram ("WorldRecorderComputer");
				hasTrapAPI = true;
			} else if (name.Contains ("SteeringComputer")) {
				ReplaceHelloWorldProgram ("WorldSteeringComputer");
				hasFloppyAPI = true;
			} else if (name.Contains ("LapTop")) {
				ReplaceHelloWorldProgram ("LapTopOS");
				hasFloppyAPI = true;
			} else if (name.Contains ("GardenBoxComputer")) {
				ReplaceHelloWorldProgram ("WorldGardenBoxComputer");
			} else if (name.Contains ("NewComputerScreen") || name.Contains("FlatScreen")) {
				ReplaceHelloWorldProgram ("WorldNewComputerScreen");
			} else if (name.Contains ("ComputerTerminalBoard")) {
				ReplaceHelloWorldProgram ("ComputerTerminalBoard");
			} else if (name.Contains ("Arcade")) {
				hasArcadeMachineAPI = true;
				hasFloppyAPI = true;
				hasMemoryAPI = true;
			} else if (name.Contains ("CashRegister")) {
				nrOfLines = 6;
				hasFloppyAPI = true;
				hasMemoryAPI = true;
				ReplaceHelloWorldProgram ("CashSystem");
			}
			else if (name.Contains ("Cashier")) {
				hasMemoryAPI = true;
			}	
			else if (name.Contains ("Arcade")) {
				hasFloppyAPI = true;
				hasMemoryAPI = true;
				hasArcadeMachineAPI = true;
				maxExecutionTime = 30;
			}
		}

		void ReplaceHelloWorldProgram(string pNewProgramName) {
			if (masterProgramName == "HelloWorld") {
				masterProgramName = pNewProgramName;
			}
		}
		

		// This does not work! The computer can't be used by the player when this code is enabled
		/*
		public override bool isBeingUsed {
			get {
				var tile = room.GetTile(interactionPoints[0]);
				return tile == null || tile.HasOccupants();
			}
		}
		*/
		
		public override bool canBePickedUp {
			get {
				return false;
			}
		}

		public override string tooltipName {
			get {
				return "computer"; // + (hasFloppyAPI ? " (has floppy drive)" : "");
			}
		}

		public override string verbDescription {
			get {
				return "use";
			}
		}

		[EditableInEditor()]
		public int mhz {
			get {
				return CELL_mhz.data;
			}
			set {
				CELL_mhz.data = value;
			}
		}

		[EditableInEditor()]
		public float maxExecutionTime {
			get {
				return CELL_maxExecutionTime.data;
			}
			set {
				CELL_maxExecutionTime.data = value;
			}
		}

		public void GetUsedBy (Character pUser, Floppy pFloppy)
		{
			//D.Log(name + " is Getting Used By " + pUser);
			_user = pUser;
			RunProgram (pFloppy);
		}

		public void RunProgram (Floppy pFloppy)
		{
			if(floppyBootProgram != null) {
				floppyBootProgram.StopAndReset();
			}

			floppyInDrive = pFloppy;
			masterProgram.executionsPerFrame = mhz;

			if(maxExecutionTime > 0f) {
				masterProgram.maxExecutionTime = maxExecutionTime;
			} else if(maxExecutionTime <= -2f) {
				masterProgram.maxExecutionTime = -2f; // no limit
			} else {
				masterProgram.maxExecutionTime = 60f;
			}

			//D.Log("Starting " + name + ", max execution time: " + masterProgram.maxExecutionTime);

			masterProgram.Start ();
		}
			
#if DEBUG && LOGGING
		[SprakAPI("Log")]
		public void API_Log(string text)
		{
			D.Log("LOG: " + text);
		}
#endif

//		[SprakAPI("Print a profiling report for the program")]
//		public void API_Prof()
//		{
//			var profData = _program.GetProfileData ();
//
//			foreach(var kv in profData.OrderBy(o => o.Value.calls).Reverse()) {
//				int n = 20 - kv.Key.Length;
//				API_Print (kv.Key + ": " + nSpaces (n) + kv.Value.calls + " calls"); //, " + kv.Value.totalTime + " s.");
//			}
//		}

		private string nSpaces(int n) {
			if (n < 0) {
				return "";
			}
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			for (int i = 0; i < n; i++) {
				sb.Append (" ");
			}
			return sb.ToString ();
		}

		[SprakAPI("Get information about the system")]
		public void API_Info()
		{
			API_Print (name);
			API_Print ("Speed: " + mhz + " mhz");
			if(CELL_hasInternetAPI.data) {
				API_Print("Has internet modem");
			}
			if(CELL_hasFloppyAPI.data) {
				API_Print("Has floppy drive");
			}
			if(CELL_hasMemoryAPI.data) {
				API_Print("Has memory unit");
			}
			API_Print ("Screen width " + screenWidth + " pixels");
			API_Print ("Screen height " + screenHeight + " pixels");
		}

		private void Checkbox(string pName, bool pTrue) {
			string xornot = (pTrue ? "X" : " ");
			API_Print ("[" + xornot + "] " + pName);
		}
		
		[SprakAPI("Get a random number between 0.0 and 1.0")]
		public float API_Random ()
		{
			return Randomizer.GetValue (0f, 1f);
		}

		[SprakAPI("Get the name of who is using the computer, if any")]
		public string API_GetUser ()
		{
			if (_user == null) {
				return "";
			} else {
				return _user.name;
			}
		}

		[SprakAPI("Get the name of the computer")]
		public string API_Name ()
		{
			return name;
		}

		[SprakAPI("Get the total time as a float")]
		public float API_Time()
		{
			return _worldSettings.totalWorldTime;
		}

		[SprakAPI("Get the screen width")]
		public float API_Width()
		{
			return screenWidth;
		}

		[SprakAPI("Get the screen height")]
		public float API_Height()
		{
			return screenHeight;
		}

//		[SprakAPI("Does the computer have a monitor?")]
//		public float API_HasMonitor()
//		{
//			return ;
//		}

//		[SprakAPI("Get the current time as a string")]
//		public string API_ClockTime()
//		{
//			return _worldSettings.gameTimeClock.hours + ":" + _worldSettings.gameTimeClock.minutes;
//		}

		[SprakAPI("Get the current hour")]
		public float API_GetHour ()
		{
			return _worldSettings.gameTimeClock.hours;
		}

		[SprakAPI("Get the current minute")]
		public float API_GetMinute ()
		{
			return _worldSettings.gameTimeClock.minutes;
		}
		
		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			//D.Log(this.ToString() + " told to sleep for " + seconds + " seconds");
			activeProgram.sleepTimer = seconds;
		}

		[SprakAPI("Stop the program")]
		public void API_Quit ()
		{
			activeProgram.StopAndReset ();
		}

//		[SprakAPI("TODO: Remove this function!")]
//		public string API_GetRandomComputer ()
//		{
//			return Randomizer.RandNth (_tingRunner.GetTingsOfType<Computer> ()).name;
//		}

		[SprakAPI("Convert a single character to a numeric value, 'a' equals 0")]
		public float API_CharToInt(string character)
		{
			if (character == "") {
				return 0f;
			}

			char c = character [0];
			return ((int)c) - ((int)'a');
		}

		[SprakAPI("Convert a number to a character, 0 equals 'a'")]
		public string API_IntToChar(float number)
		{
			char c = (char)((int)'a' + (int)number);
			return c.ToString ();
		}

		private void InternalPrint(string pText, bool pNewLine) {
		
			#if DEBUG
			if(logger != null) {
				logger.Log("Printing on " + name + ": " + pText);
			}
			#endif

			if (currentLine >= nrOfLines) {
				D.Log ("Error in " + name + ", trying to write to line " + currentLine + " on console with only " + nrOfLines + " lines");
				currentLine = 0;
			}

			//D.Log("Printed on computer " + name + ": " + pText);
						
			string[] cout = consoleOutput;

			int i = 0;

			while (true) {
				int charsLeft = pText.Length - i;
				int nrOfCharsToPrint = Math.Min (charsOnLine, charsLeft);
				cout [currentLine] += pText.Substring(i, nrOfCharsToPrint);
				i += nrOfCharsToPrint;
				if (i < pText.Length) {
					//currentLine++;
					NextLine();
				} else {
					break;
				}
			}

			if(pNewLine) {
				NextLine ();
				cout [currentLine] = "";
			}

			consoleOutput = cout;
		}

		[SprakAPI("Remove all text from the screen")]
		public void API_ClearText ()
		{
			string[] cout = consoleOutput;

			for (int i = 0; i < nrOfLines; i++) {
				cout [i] = "";
			}

			consoleOutput = cout;
			currentLine = 0;
			currentTopLine = 0;
		}

		[SprakAPI("Print text to the screen")]
		public void API_Print (string text)
		{
			InternalPrint(text, true);
		}

		[SprakAPI("Say something through speaker")]
		public void API_Say (string text)
		{
			Say (text, "");
		}

		[SprakAPI("Play a sound")]
		public void API_PlaySound (string soundName)
		{
			PlaySound (soundName);
			audioLoop = false;
		}

		[SprakAPI("Set the pitch of the sound")]
		public void API_Pitch (float pitch)
		{
			this.pitch = pitch;
		}

		[SprakAPI("The sinus function", "x")]
		public float API_Sin(float x)
		{
			return (float)Math.Sin(x);
		}

		[SprakAPI("The cosinus function", "x")]
		public float API_Cos(float x)
		{
			return (float)Math.Cos(x);
		}
		
		public void NextLine ()
		{
			if ((currentLine == (currentTopLine - 1)) ||
			    ((currentLine == (nrOfLines - 1)) && (currentTopLine == 0))) {
				currentTopLine++; // scroll down one line
			}
			currentLine++;
			consoleOutput[currentLine] = ""; // clear the new line
		}

		[SprakAPI("Print text without skipping to a new line afterwards")]
		public void API_PrintS(string text)
		{
			InternalPrint(text, false);
		}

		[SprakAPI("Display a prompt and receive text input from the keyboard")]
		public string API_Input (string prompt)
		{
			API_PrintS(prompt);
			activeProgram.waitingForInput = true;
			return "WAITING_FOR_INPUT";
		}

		public void OnKeyDown (string pKey)
		{
			//D.Log("Key was pressed: '" + pKey + "'");
			
			if ("ABCDEFGHIKJLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ,.:;()_<>+-*/&%#$@!?\"'[]{}=\\".LastIndexOf (pKey) < 0) {
				//D.Log ("Ignoring key press: " + pKey);
				return;
			}
		
			if (activeProgram.waitingForInput) {
				currentInput += pKey;
				string[] cout = consoleOutput;
				cout [currentLine] += pKey;
				consoleOutput = cout;
			}

			activeProgram.executionTime = 0f; // don't turn off automatically yet
		}

		public void OnEnterKey ()
		{
			if (activeProgram.waitingForInput) {
				activeProgram.SwapStackTopValueTo (currentInput);
				activeProgram.waitingForInput = false;
				currentInput = "";

				NextLine ();
				string[] cout = consoleOutput;
				cout [currentLine] = "";
				consoleOutput = cout;
			}

			activeProgram.executionTime = 0f; // don't turn off automatically yet
		}

		public void OnBackspaceKey ()
		{
			if (activeProgram.waitingForInput && currentInput.Length > 0) {
				currentInput = currentInput.Substring (0, currentInput.Length - 1);
				string[] cout = consoleOutput;
				var completeCurrentLine = cout [currentLine];
				cout [currentLine] = completeCurrentLine.Substring (0, completeCurrentLine.Length - 1);
				consoleOutput = cout;
			}

			activeProgram.executionTime = 0f; // don't turn off automatically yet
		}
	
		public void OnDirectionKey (string pKey)
		{
			//D.Log("Direction key pressed: " + pKey);

//			if (hasArcadeMachineAPI) {
//				masterProgram.StartAtFunction ("OnInput", new ReturnValue[] { new ReturnValue (pKey) }, null);
//			}
			//masterProgram.ChangeGlobalVariableInitValue("key", new ReturnValue(pKey));
		}
		
		[EditableInEditor]
		public string masterProgramName {
			get {
				return CELL_programName.data;
			}
			set {
				CELL_programName.data = value;
			}
		}

		[EditableInEditor]
		public string floppyBootProgramName {
			get {
				return CELL_floppyBootProgramName.data;
			}
			set {
				CELL_floppyBootProgramName.data = value;
			}
		}

		[ShowInEditor]
		public string[] consoleOutput {
			get {
				return CELL_consoleOutput.data;
			}
			set {
				CELL_consoleOutput.data = value;
			}
		}

		[ShowInEditor]
		public string programStatus {
			get {
				if(_program == null) {
					return "NO PROGRAM";
				}
				return masterProgram.isOn ? "ON" : "OFF";
			}
		}

		[ShowInEditor]
		public string currentInput {
			get {
				return CELL_currentInput.data;
			}
			set {
				CELL_currentInput.data = value;
			}
		}

		[ShowInEditor]
		public int currentInputXPos {
			get {
				D.isNull(CELL_consoleOutput.data, "consoleOutput.data in " + name + " is null");
				var contentsOfCurrentLine = CELL_consoleOutput.data[CELL_currentLine.data];
				if(contentsOfCurrentLine == null) {
					return 0;
				}
				return CELL_consoleOutput.data[CELL_currentLine.data].Length;
			}
		}

		public override Program masterProgram {
			get {
				if (_program == null) {
					_program = EnsureProgram ("MasterProgram", masterProgramName);
					GenerateProgramAPI (_program);
				}
				return _program;
			}
		}

		public Program floppyBootProgram {
			get {
				if (_floppyBootProgram == null) {
					_floppyBootProgram = EnsureProgram ("FloppyBootProgram", floppyBootProgramName);
					GenerateProgramAPI (_floppyBootProgram);
				}
				return _floppyBootProgram;
			}
		}

		void GenerateProgramAPI(Program pProgram) {
			var defs = new List<FunctionDefinition> (FunctionDefinitionCreator.CreateDefinitions (this, typeof(Computer)));

			if (hasGraphicsAPI) {

				var graphicsApi = new GraphicsAPI (this);

				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (graphicsApi, typeof(GraphicsAPI)));

				var linesFn = new FunctionDefinition (
					"void", 
					"Lines", 
					new string[] { "array" }, 
					new string[] { "points" }, 
					new ExternalFunctionCreator.OnFunctionCall (graphicsApi.Lines), 
					FunctionDocumentation.Default ());

				defs.Add(linesFn);
			}

			if (hasInternetAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new InternetAPI (this, _tingRunner), typeof(InternetAPI)));

				ConnectionAPI_Optimized connectionApi = new ConnectionAPI_Optimized(this, _tingRunner, pProgram);
				defs.Add(new FunctionDefinition(
					"number", 
					"Connect", 
					new string[] {"string"}, 
					new string[] {"name"}, 
					new ExternalFunctionCreator.OnFunctionCall(connectionApi.Connect), 
					FunctionDocumentation.Default()));

				defs.Add(new FunctionDefinition(
					"void",
					"DisconnectAll",
					new string[] {},
					new string[] {},
					new ExternalFunctionCreator.OnFunctionCall(connectionApi.DisconnectAll),
					new FunctionDocumentation("Remove all connections", new string[] {})));

				var rfc = new FunctionDefinition (
					          "number", 
					          "RemoteFunctionCall", 
					          new string[] { "number", "string", "array" }, 
					          new string[] { "receiverIndex", "functionName", "arguments" }, 
					          new ExternalFunctionCreator.OnFunctionCall (connectionApi.RemoteFunctionCall), 
					          FunctionDocumentation.Default ());
				rfc.hideInModifier = true;

				defs.Add(rfc);
			}
			if (hasWeatherAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new WeatherAPI (this, _worldSettings), typeof(WeatherAPI)));
			}
			if (hasLampAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new LampAPI (this, _tingRunner), typeof(LampAPI)));
			}
			if (hasDoorAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new DoorAPI (this, _tingRunner, _roomRunner), typeof(DoorAPI)));
			}
			if (hasMemoryAPI) {

				MemoryAPI memoryApi = new MemoryAPI(this, _tingRunner);
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (memoryApi, typeof(MemoryAPI)));

				// Are these added manually because they can take any kind of argument..?

				defs.Add(new FunctionDefinition(
					"void", 
					"SaveMemory", 
					new string[] {"string", "var"}, 
					new string[] {"key", "value"}, 
					new ExternalFunctionCreator.OnFunctionCall(memoryApi.SaveMemory), 
					FunctionDocumentation.Default()));

				defs.Add(new FunctionDefinition(
					"var", 
					"LoadMemory", 
					new string[] {"string"}, 
					new string[] {"key"}, 
					new ExternalFunctionCreator.OnFunctionCall(memoryApi.LoadMemory), 
					FunctionDocumentation.Default()));
			}
			if (hasVoiceAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new VoiceAPI (this, _tingRunner, _dialogueRunner), typeof(VoiceAPI)));
			}
			if (hasElevatorAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new ElevatorAPI (this, _tingRunner), typeof(ElevatorAPI)));
			}
			if (hasTingrunnerAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new TingrunnerAPI (this, _tingRunner, _roomRunner), typeof(TingrunnerAPI)));
			}
			if (hasTrapAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new TrapAPI (this, _tingRunner, _dialogueRunner), typeof(TrapAPI)));
			}
			if (hasHeartAPI) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new HeartAPI (this, _tingRunner, _dialogueRunner), typeof(HeartAPI)));
			}
			if (true /*hasArcadeMachineAPI*/) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new ArcadeMachineAPI (this), typeof(ArcadeMachineAPI)));
			}
			if (true /*hasFloppyAPI*/) {
				defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new FloppyAPI (this, _tingRunner), typeof(FloppyAPI)));
			}

			pProgram.FunctionDefinitions = defs;
		}

		public override void PrepareForBeingHacked ()
		{
			// Accessing the getter to make sure that a program is generated
			if (masterProgram == null) {
				logger.Log ("There was a problem generating the master program");
			}
			else {
				masterProgram.nameOfOwner = name;
			}
			if (floppyBootProgram == null) {
				logger.Log ("There was a problem generating the floppy boot program");
			}
		}

		public void EnsureMemoryUnit() {
			if (memory == null) {
				string memoryUnitName = name + "_builtInMemoryUnit";
				var exisitingMemoryUnit = _tingRunner.GetTingUnsafe (memoryUnitName);
				if (exisitingMemoryUnit != null) {
					memory = exisitingMemoryUnit as Memory;
				} else {
					Memory newMemoryUnit = _tingRunner.CreateTing<Memory> (memoryUnitName, position, direction, "InvisibleHardDrive");
					memory = newMemoryUnit;
				}

				if (memory == null) {
					throw new Error ("Failed to find/create memory unit");
				}
			}
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
						//localPoint - IntPoint.DirectionToIntPoint (direction) * 2,
						localPoint + IntPoint.DirectionToIntPoint (direction) * 2
					};
			}
		}

		public Character user {
			get {
				return _user;
			}
		}

		public Memory memory {
			get {
				if (CELL_memoryUnitName.data == "") {
					return null;
				} else {
					return _tingRunner.GetTing<Memory> (CELL_memoryUnitName.data);
				}
			}
			set {
				if (value == null) {
					CELL_memoryUnitName.data = "";
				} else {
					CELL_memoryUnitName.data = value.name;
				}
			}
		}

		[EditableInEditor]
		public string memoryUnitName {
			get {
				return CELL_memoryUnitName.data;
			}
			set {
				CELL_memoryUnitName.data = value;
			}
		}

		public Floppy floppyInDrive {
			get {
				if (CELL_floppyInDrive.data == "") {
					return null;
				} else {
					return _tingRunner.GetTing<Floppy> (CELL_floppyInDrive.data);
				}
			}
			set {
				if (value == null) {
					CELL_floppyInDrive.data = "";
				} else {
					CELL_floppyInDrive.data = value.name;
				}
			}
		}

		[EditableInEditor]
		public string floppyInDriveName {
			get {
				return CELL_floppyInDrive.data;
			}
			set {
				CELL_floppyInDrive.data = value;
			}
		}

		[EditableInEditor]
		public int currentLine {
			get {
				return CELL_currentLine.data;
			}
			set {
				CELL_currentLine.data = (value % nrOfLines);
			}
		}

		[EditableInEditor]
		public int nrOfLines {
			get {
				return CELL_nrOfLines.data;
			}
			set {
				CELL_nrOfLines.data = value;
			}
		}

		[EditableInEditor]
		public int charsOnLine {
			get {
				return CELL_charsOnLine.data;
			}
			set {
				CELL_charsOnLine.data = value;
			}
		}

		[EditableInEditor]
		public int currentTopLine {
			get {
				return CELL_currentTopLine.data;
			}
			set {
				CELL_currentTopLine.data = (value % nrOfLines);
			}
		}

		[EditableInEditor]
		public bool hasInternetAPI {
			get {
				return CELL_hasInternetAPI.data;
			}
			set {
				CELL_hasInternetAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasGraphicsAPI {
			get {
				return CELL_hasGraphicsAPI.data;
			}
			set {
				CELL_hasGraphicsAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasWeatherAPI {
			get {
				return CELL_hasWeatherAPI.data;
			}
			set {
				CELL_hasWeatherAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasLampAPI {
			get {
				return CELL_hasLampAPI.data;
			}
			set {
				CELL_hasLampAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasDoorAPI {
			get {
				return CELL_hasDoorAPI.data;
			}
			set {
				CELL_hasDoorAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasMemoryAPI {
			get {
				return CELL_hasMemoryAPI.data;
			}
			set {
				CELL_hasMemoryAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasVoiceAPI {
			get {
				return CELL_hasVoiceAPI.data;
			}
			set {
				CELL_hasVoiceAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasElevatorAPI {
			get {
				return CELL_hasElevatorAPI.data;
			}
			set {
				CELL_hasElevatorAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasTingrunnerAPI {
			get {
				return CELL_hasTingrunnerAPI.data;
			}
			set {
				CELL_hasTingrunnerAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasTrapAPI {
			get {
				return CELL_hasTrapAPI.data;
			}
			set {
				CELL_hasTrapAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasHeartAPI {
			get {
				return CELL_hasHeartAPI.data;
			}
			set {
				CELL_hasHeartAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasArcadeMachineAPI {
			get {
				return CELL_hasArcadeMachineAPI.data;
			}
			set {
				CELL_hasArcadeMachineAPI.data = value;
			}
		}

		[EditableInEditor]
		public bool hasFloppyAPI {
			get {
				return CELL_hasFloppyAPI.data;
			}
			set {
				CELL_hasFloppyAPI.data = value;
			}
		}

		[ShowInEditor]
		public int screenWidth {
			get {
				return CELL_screenWidth.data;
			}
			set {
				CELL_screenWidth.data = value;
			}
		}

		[ShowInEditor]
		public int screenHeight {
			get {
				return CELL_screenHeight.data;
			}
			set {
				CELL_screenHeight.data = value;
			}
		}
	}

	public class GraphicsAPI
	{
		Computer _computer;

		public GraphicsAPI (Computer pComputer)
		{
			_computer = pComputer;
		}

		[SprakAPI("Draw a line on the screen")]
		public void API_Line (float x1, float y1, float x2, float y2)
		{
			if (_computer.onLineDrawing != null) {
				_computer.onLineDrawing (new IntPoint ((int)x1, (int)y1), new IntPoint ((int)x2, (int)y2));
			} else {
				//D.LogError (_computer.name + " doesn't have onLineDrawing set");
			}
		}

		[SprakAPI("Draw text in a specific place", "X position in character coordinates", "Y position in character coordinates", "The text to print")]
		public void API_Text (float x, float y, string text)
		{
			if (_computer.onTextDrawing != null) {
				//D.Log ("API_Text called with args " + x + ", " + y + ", " + text);
				_computer.onTextDrawing ((int)x, (int)y, text);
			} else {
				//D.LogError (_computer.name + " doesn't have onTextDrawing set");
			}
		}
			
		public object Lines (object[] args)
		{
			if (args[0].GetType() != typeof(SortedDictionary<KeyWrapper,object>)) {
				throw new Error ("Must get an array of points");
			}

			if (_computer.onLineDrawing != null) 
			{
				var arr = ReturnValueConversions.SafeUnwrap<SortedDictionary<KeyWrapper,object>>(args, 0);
				float x, y;
				float prevX = 0f;
				float prevY = 0f;
				bool firstPoint = true;
				var i = arr.GetEnumerator ();
				while (i.MoveNext ()) {
					x = (float)i.Current.Value;
					if (!i.MoveNext ()) {
						break;
					}
					y = (float)i.Current.Value;
					if (!firstPoint) {
						_computer.onLineDrawing (new IntPoint ((int)x, (int)y), new IntPoint ((int)prevX, (int)prevY));
					}
					prevX = x;
					prevY = y;
					firstPoint = false;
				}

			} else {
				D.Log (_computer.name + " doesn't have onLineDrawing set");
			}

			return VoidType.voidType;
		}

		[SprakAPI("Draw a rectangle on the screen")]
		public void API_Rect (float x1, float y1, float x2, float y2)
		{
			if (_computer.onRectDrawing != null) {
				_computer.onRectDrawing (new IntPoint ((int)x1, (int)y1), new IntPoint ((int)x2, (int)y2));
			} else {
				//D.LogError (_computer.name + " doesn't have onLineDrawing set");
			}
		}

		[SprakAPI("Clear the screen and display graphical elements")]
		public void API_DisplayGraphics()
		{
			if (_computer.onDisplayGraphics != null) {
				_computer.onDisplayGraphics ();
			} else {
				//D.LogError (_computer.name + " doesn't have the callback onDisplayGraphics set");
			}
		}

		[SprakAPI("Set the color to draw or print text with")]
		public void API_Color(float r, float g, float b)
		{
			if (_computer.onSetColor != null) {
				_computer.onSetColor (r, g, b);
			} else {
				//D.LogError (_computer.name + " doesn't have the callback onSetColor set");
			}
		}

		[SprakAPI("Keep a value between 0 and an upper bound", "Value", "Upper bound")]
		public float API_Repeat(float x, float bound) {
			return x - (float)System.Math.Floor(x / bound) * bound;
		}

		[SprakAPI("Hue, Saturation, Value -> [r, g, b]", "Hue", "Saturation", "Value")]
		public SortedDictionary<KeyWrapper,object> API_HSVtoRGB(float H, float S, float V)
		{
			Float3 white = new Float3 (1f, 1f, 1f);
			if (S == 0)
			{
				white.x = V;
				white.y = V;
				white.z = V;
			}
			else
			{
				if (V == 0)
				{
					white.x = 0;
					white.y = 0;
					white.z = 0;
				}
				else
				{
					white.x = 0;
					white.y = 0;
					white.z = 0;
					float num = H * 6;
					int num2 = (int)System.Math.Floor(num);
					float num3 = num - (float)num2;
					float num4 = V * (1 - S);
					float num5 = V * (1 - S * num3);
					float num6 = V * (1 - S * (1 - num3));
					int num7 = num2;
					switch (num7 + 1)
					{
					case 0:
						white.x = V;
						white.y = num4;
						white.z = num5;
						break;
					case 1:
						white.x = V;
						white.y = num6;
						white.z = num4;
						break;
					case 2:
						white.x = num5;
						white.y = V;
						white.z = num4;
						break;
					case 3:
						white.x = num4;
						white.y = V;
						white.z = num6;
						break;
					case 4:
						white.x = num4;
						white.y = num5;
						white.z = V;
						break;
					case 5:
						white.x = num6;
						white.y = num4;
						white.z = V;
						break;
					case 6:
						white.x = V;
						white.y = num4;
						white.z = num5;
						break;
					case 7:
						white.x = V;
						white.y = num6;
						white.z = num4;
						break;
					}
					white.x = Clamp01 (white.x);
					white.y = Clamp01 (white.y);
					white.z = Clamp01 (white.z);
				}
			}
			return new SortedDictionary<KeyWrapper,object> { 
				{new KeyWrapper(0), white.x}, 
				{new KeyWrapper(1), white.y},
				{new KeyWrapper(2), white.z} };
		}

		float Clamp01(float x) {
			if (x < 0f)
				return 0f;
			else if (x > 1f)
				return 1f;
			return x;
		}

		[SprakAPI("[r, g, b] -> Hue, Saturation, Value", "Red", "Green", "Blue")]
		public SortedDictionary<KeyWrapper,object> API_RGBToHSV(float r, float g, float b)
		{
			float H, S, V;

			if (b > g && b > r)
			{
				RGBToHSVHelper(4, b, r, g, out H, out S, out V);
			}
			else
			{
				if (g > r)
				{
					RGBToHSVHelper(2, g, b, r, out H, out S, out V);
				}
				else
				{
					RGBToHSVHelper(0, r, g, b, out H, out S, out V);
				}
			}

			return new SortedDictionary<KeyWrapper,object> { 
				{new KeyWrapper(0), H}, 
				{new KeyWrapper(1), S},
				{new KeyWrapper(2), V} };
		}

		private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
		{
			V = dominantcolor;
			if (V != 0)
			{
				float num;
				if (colorone > colortwo)
				{
					num = colortwo;
				}
				else
				{
					num = colorone;
				}
				float num2 = V - num;
				if (num2 != 0)
				{
					S = num2 / V;
					H = offset + (colorone - colortwo) / num2;
				}
				else
				{
					S = 0;
					H = offset + (colorone - colortwo);
				}
				H /= 6;
				if (H < 0)
				{
					H += 1;
				}
			}
			else
			{
				S = 0;
				H = 0;
			}
		}

//		[SprakAPI("Wait here until next frame")]
//		public void API_FrameSync ()
//		{
//			_computer.masterProgram.waitForNextFrame = true;
//		}
	}

	public class InternetAPI
	{
		Computer _computer;
		//TingRunner _tingRunner;

		public InternetAPI (Computer pComputer, TingRunner pTingRunner)
		{
			_computer = pComputer;
			//_tingRunner = pTingRunner;
		}

		[SprakAPI("Get a list of all connections (list of names)")]
		public object[] API_GetConnections ()
		{
			return _computer.connectedTings.Select (t => t.name).ToArray ();
		}

		[SprakAPI("Use with caution")]
		public void API_Slurp ()
		{
			if (_computer.user != null) {
				_computer.user.SlurpIntoInternet(_computer);
			} else {
				_computer.API_Print ("No one to slurp");
			}
		}
	}

	public class WeatherAPI
	{
		Computer _computer;
		WorldSettings _worldSettings;

		public WeatherAPI (Computer pComputer, WorldSettings pWorldSettings)
		{
			_computer = pComputer;
			_worldSettings = pWorldSettings;
		}

		[SprakAPI("Set the amount of rain (0 - 250)")]
		public void API_SetRain (float amount)
		{
			if (amount < 0)
				amount = 0;
			else if (amount > 250)
				amount = 250;
			_worldSettings.rainTargetValue = amount;
			_computer.API_Print ("Rain level was set to " + amount);
		}

		[SprakAPI("Get the amount of rain (0 - 250)")]
		public float API_GetRain ()
		{
			float rain = _worldSettings.rain + Randomizer.GetValue (0.1f, 10.0f);
			if (rain < 0f)
				rain = 0f;
			if (rain > 250f)
				rain = 250f;
			return rain;
		}
	}

	public class LampAPI
	{
		Computer _computer;
		TingRunner _tingRunner;

		public LampAPI (Computer pComputer, TingRunner pTingRunner)
		{
			_computer = pComputer;
			_tingRunner = pTingRunner;
		}

		[SprakAPI("Turn off a lamp")]
		public void API_TurnOff (string lampName)
		{
			Lamp lamp = GetLamp (lampName);
			if (lamp != null) {
				lamp.on = false;
				_computer.API_Print ("'" + lampName + "' was turned off");
				//_computer.PlaySound ("Blip 1");
			} else {
				_computer.API_Print ("Can't find a lamp named '" + lampName + "'");
			}
		}

		[SprakAPI("Turn on a lamp")]
		public void API_TurnOn (string lampName)
		{
			Lamp lamp = GetLamp (lampName);
			if (lamp != null) {
				lamp.on = true;
				_computer.API_Print ("'" + lampName + "' was turned on");
				//_computer.PlaySound ("Blip 1");
			} else {
				_computer.API_Print ("Can't find a lamp named '" + lampName + "'");
			}
		}
		
		[SprakAPI("Flip the switch on a lamp")]
		public void API_Switch (string lampName)
		{
			Lamp lamp = GetLamp (lampName);
			if (lamp != null) {
				if (lamp.on) {
					lamp.on = false;
					_computer.API_Print ("'" + lampName + "' was switched off");
					//_computer.PlaySound ("Blip 1");
				} else {
					lamp.on = true;
					_computer.API_Print ("'" + lampName + "' was switched on");
					//_computer.PlaySound ("Blip 1");
				}
			} else {
				_computer.API_Print ("Can't find a lamp named '" + lampName + "'");
			}
		}
		
		private Lamp GetLamp (string lampName)
		{
			return _tingRunner.GetTingUnsafe (lampName) as Lamp;
		}
	}

	public class DoorAPI
	{
		Computer _computer;
		TingRunner _tingRunner;
		RoomRunner _roomRunner;

		public DoorAPI (Computer pComputer, TingRunner pTingRunner, RoomRunner pRoomRunner)
		{
			_computer = pComputer;
			_tingRunner = pTingRunner;
			_roomRunner = pRoomRunner;
		}

		[SprakAPI("Lock a door", "The name of the door to lock")]
		public bool API_Lock (string doorName)
		{
			Ting ting = _tingRunner.GetTingUnsafe (doorName);
			if (ting is Door) {
				(ting as Door).isLocked = true;
				_computer.API_Print ("'" + doorName + "'");
				_computer.API_Print("is now locked.");
				//_computer.API_PlaySound ("Blip 1");
				return true;
			} else {
				_computer.API_Print ("Can't find a door named '" + doorName + "'");
				return false;
			}
		}

		[SprakAPI("Unlock a door", "The name of the door to unlock")]
		public bool API_Unlock (string doorName)
		{
			Ting ting = _tingRunner.GetTingUnsafe (doorName);
			if (ting is Door) {
				(ting as Door).isLocked = false;
				_computer.API_Print ("'" + doorName + "'");
				_computer.API_Print("is now unlocked!");
				//_computer.API_PlaySound ("Blip 1");
				return true;
			} else {
				_computer.API_Print ("Can't find a door named '" + doorName + "'");
				return false;
			}
		}

		[SprakAPI("Find the path between two objects")]
		public object[] API_FindPath (string start, string goal)
		{
			_computer.API_Sleep (Randomizer.GetValue (1f, 4f));

			Ting startT = _tingRunner.GetTingUnsafe(start);
			Ting goalT = _tingRunner.GetTingUnsafe(goal);

			if(startT == null) {
				_computer.API_Print("Can't find start object");
				return new object[] {};
			}
			if(goalT == null) {
				_computer.API_Print("Can't find goal object");
				return new object[] {};
			}
			var pathFinder = new MimanPathfinder2(_tingRunner, _roomRunner);
			MimanPath path = pathFinder.Search(startT, goalT);

			return path.tings.Select(t => t.room.name).ToArray();
		}
	}

	public class MemoryAPI
	{
		Computer _computer;
		TingRunner _tingRunner;

		public MemoryAPI (Computer pComputer, TingRunner pTingRunner)
		{
			_computer = pComputer;
			_tingRunner = pTingRunner;
		}
			
		[SprakAPI("Save something to the active memory unit", "key", "value to save")]
		public object SaveMemory (object[] args)
		{
			var val = args [1];

			if (val.GetType() == typeof(string) ||
			    val.GetType () == typeof(float) ||
			    val.GetType () == typeof(bool)) 
			{
				// ok
			} else {
				throw new Error ("Can't save value of type " + ReturnValueConversions.PrettyObjectType(val.GetType ()) + " in memory");
			}

			_computer.EnsureMemoryUnit ();
			string key = ReturnValueConversions.SafeUnwrap<string>(args, 0);
			_computer.memory [key] = val;
			return VoidType.voidType;
		}

		[SprakAPI("Retreive something from the active memory unit", "key")]
		public object LoadMemory (object[] args)
		{
			_computer.EnsureMemoryUnit ();
			string key = ReturnValueConversions.SafeUnwrap<string>(args, 0);
			object x;
			if (_computer.memory.data.TryGetValue (key, out x)) {
				var t = x.GetType();
				if(t == typeof(float)) {
					return (float)x;
				}
				else if(t == typeof(double)) {
					return (float)(double)x; // the serializer converts values to double :/
				}
				else if(t == typeof(int)) {
					return (float)(int)x;
				}
				else if(t == typeof(string)) {
					return x;
				}
				else if(t == typeof(bool)) {
					return x;
				}
				else {
					throw new Error ("Can't load memory of type " + ReturnValueConversions.PrettyObjectType(t) + " from memory");
				}
			} else {
				//throw new Error ("Can't load memory with key " + key);
				_computer.API_Print("Can't load memory with key '" + key + "'");
				return 0.0f;
			}
		}

		[SprakAPI("Connect to an external memory unit (HD)", "key")]
		public object SetMemoryUnit (object[] args)
		{
			string memoryUnitName = ReturnValueConversions.SafeUnwrap<string>(args, 0);
			Memory memoryUnit = _tingRunner.GetTingUnsafe(memoryUnitName) as Memory;

			if(memoryUnit != null) {
				_computer.memory = memoryUnit;
				_computer.API_Print("Connected to " + memoryUnitName);
			}
			else {
				throw new Error ("Can't connect to external memory unit '" + memoryUnitName + "'");
			}

			return VoidType.voidType;
		}

		[SprakAPI("Get the keys of all entries in the memory unit")]
		public object[] API_GetMemories ()
		{
			_computer.EnsureMemoryUnit ();
			return _computer.memory.data.Keys.ToArray ();
		}

		[SprakAPI("Remove all memories from the memory unit")]
		public void API_ClearMemories ()
		{
			_computer.EnsureMemoryUnit ();
			_computer.memory.data.Clear ();
		}

		[SprakAPI("Remove a specific memory", "The key")]
		public void API_EraseMemory (string key)
		{
			_computer.EnsureMemoryUnit ();
			_computer.memory.data.Remove(key);
		}

		[SprakAPI("Does the computer have the memory with a certain key?", "The key")]
		public bool API_HasMemory (string key)
		{
			_computer.EnsureMemoryUnit ();
			return _computer.memory.data.ContainsKey (key);
		}
	}

	public class ElevatorAPI
	{
		Computer _computer;
		//TingRunner _tingRunner;

		public ElevatorAPI (Computer pComputer, TingRunner pTingRunner)
		{
			_computer = pComputer;
			//_tingRunner = pTingRunner;
		}

		private Door GetElevatorDoor ()
		{
			foreach (Ting t in _computer.connectedTings) {
				if (t is Door) {
					return t as Door;
				}
			}			
			throw new Error ("Can't access elevator door");
		}

		[SprakAPI("Move the elevator to another floor", "the floor nr")]
		public void API_GotoFloor (float floorNr)
		{
			var door = GetElevatorDoor ();
			door.elevatorFloor = (int)floorNr;
		}

		[SprakAPI("Get current floor nr")]
		public float API_GetFloor ()
		{
			return (float)GetElevatorDoor ().elevatorFloor;
		}
	}

	public class VoiceAPI
	{
		Computer _computer;
		//TingRunner _tingRunner;
		DialogueRunner _dialogueRunner;

		public VoiceAPI (Computer pComputer, TingRunner pTingRunner, DialogueRunner pDialogueRunner)
		{
			_computer = pComputer;
			//_tingRunner = pTingRunner;
			_dialogueRunner = pDialogueRunner;
		}

		[SprakAPI("Listen")]
		public string API_Listen ()
		{
			_computer.masterProgram.waitingForInput = true;
			_dialogueRunner.AddOnSomeoneSaidSomethingListener (OnSomeoneSaidSomething);
			return "LISTENING";
		}

		public void OnSomeoneSaidSomething (Speech pSpeech)
		{
			_computer.masterProgram.SwapStackTopValueTo (pSpeech.line);
			_computer.masterProgram.waitingForInput = false;
			_dialogueRunner.RemoveOnSomeoneSaidSomethingListener (OnSomeoneSaidSomething);
		}

		/*[SprakAPI("Wait until the whole sentence has been said")]
		public void WaitForSilence()
		{
			if(_computer.dialogueLine != "") {
				_computer.masterProgram.waitingForInput = true;
				_computer.AddDataListener<string>("dialogueLine", (prevLine, newLine) => {
					if(newLine == "")
					_computer.masterProgram.waitingForInput = false;
					_computer.RemoveDataListener<string>("dialogueLine");
				});
			}
		}*/
	}

	public class TingrunnerAPI
	{
		Computer _computer;
		TingRunner _tingRunner;
		RoomRunner _roomRunner;

		public TingrunnerAPI (Computer pComputer, TingRunner pTingRunner, RoomRunner pRoomRunner)
		{
			_computer = pComputer;
			_tingRunner = pTingRunner;
			_roomRunner = pRoomRunner;
		}

		[SprakAPI("")]
		public object[] API_GetPeople ()
		{
			List<string> names = new List<string> ();
			foreach (var character in _tingRunner.GetTingsOfType<Character>()) {
				names.Add (character.name);
			}
			return names.ToArray ();
		}

		[SprakAPI("")]
		public object[] API_GetThingsOfType (string type)
		{
			Ting[] tings = null;
			string t = type.ToLower();

			if(t == "bed") {
				tings = _tingRunner.GetTingsOfType<Bed>();
			}
			else if(t == "creditcard") {
				tings = _tingRunner.GetTingsOfType<CreditCard>();
			}
			else if(t == "door") {
				tings = _tingRunner.GetTingsOfType<Door>();
			}
			else if(t == "drink") {
				tings = _tingRunner.GetTingsOfType<Drink>();
			}
			else if(t == "extractor") {
				tings = _tingRunner.GetTingsOfType<Extractor>();
			}
			else if(t == "fence") {
				tings = _tingRunner.GetTingsOfType<Fence>();
			}
			else if(t == "floppy") {
				tings = _tingRunner.GetTingsOfType<Floppy>();
			}
			else if(t == "fountain") {
				tings = _tingRunner.GetTingsOfType<Fountain>();
			}
			else if(t == "fryingpan") {
				tings = _tingRunner.GetTingsOfType<FryingPan>();
			}
			else if(t == "fusebox") {
				tings = _tingRunner.GetTingsOfType<FuseBox>();
			}
			else if(t == "goods") {
				tings = _tingRunner.GetTingsOfType<Goods>();
			}
			else if(t == "modifier" || t == "hackdev") {
				tings = _tingRunner.GetTingsOfType<Hackdev>();
			}
			else if(t == "key") {
				tings = _tingRunner.GetTingsOfType<Key>();
			}
			else if(t == "lamp") {
				tings = _tingRunner.GetTingsOfType<Lamp>();
			}
			else if(t == "locker") {
				tings = _tingRunner.GetTingsOfType<Locker>();
			}
			else if(t == "machine") {
				tings = _tingRunner.GetTingsOfType<Machine>();
			}
			else if(t == "map") {
				tings = _tingRunner.GetTingsOfType<Map>();
			}
			else if(t == "memory") {
				tings = _tingRunner.GetTingsOfType<Memory>();
			}
			else if(t == "musicbox") {
				tings = _tingRunner.GetTingsOfType<MusicBox>();
			}
			else if(t == "mysticalcube") {
				tings = _tingRunner.GetTingsOfType<MysticalCube>();
			}
			else if(t == "pawn") {
				tings = _tingRunner.GetTingsOfType<Pawn>();
			}
			else if(t == "point") {
				tings = _tingRunner.GetTingsOfType<Point>();
			}
			else if(t == "portal") {
				tings = _tingRunner.GetTingsOfType<Portal>();
			}
			else if(t == "radio") {
				tings = _tingRunner.GetTingsOfType<Radio>();
			}
			else if(t == "screwdriver") {
				tings = _tingRunner.GetTingsOfType<Screwdriver>();
			}
			else if(t == "seat") {
				tings = _tingRunner.GetTingsOfType<Seat>();
			}
			else if(t == "sendpipe") {
				tings = _tingRunner.GetTingsOfType<SendPipe>();
			}
			else if(t == "sink") {
				tings = _tingRunner.GetTingsOfType<Sink>();
			}
			else if(t == "snus") {
				tings = _tingRunner.GetTingsOfType<Snus>();
			}
			else if(t == "suitcase") {
				tings = _tingRunner.GetTingsOfType<Suitcase>();
			}
			else if(t == "taser") {
				tings = _tingRunner.GetTingsOfType<Taser>();
			}
			else if(t == "teleporter") {
				tings = _tingRunner.GetTingsOfType<Teleporter>();
			}
			else if(t == "tram") {
				tings = _tingRunner.GetTingsOfType<Tram>();
			}
			else if(t == "trashcan") {
				tings = _tingRunner.GetTingsOfType<TrashCan>();
			}
			else if(t == "tv") {
				tings = _tingRunner.GetTingsOfType<Tv>();
			}
			else if(t == "vendingmachine") {
				tings = _tingRunner.GetTingsOfType<VendingMachine>();
			}
			else {
				_computer.API_Print("Can't get tings of type " + type);
				return new object[] {};
			}

			List<string> names = new List<string> ();
			foreach (var ting in tings) {
				names.Add (ting.name);
			}
			return names.ToArray ();
		}

		[SprakAPI("")]
		public string API_GetPosition (string name)
		{
			var ting = _tingRunner.GetTingUnsafe (name);
			if (ting != null) {
				return "Room: " + ting.position.roomName + ", coordinate: " + ting.position.localPosition;
			} else {
				return "Thing not found";
			}
		}

		[SprakAPI("")]
		public string API_GetAction (string name)
		{
			var ting = _tingRunner.GetTingUnsafe (name);
			if (ting != null) {
				return ting.actionName;
			} else {
				return "Thing not found";
			}
		}

		[SprakAPI("")]
		public string API_GetRoom (string name)
		{
			var ting = _tingRunner.GetTingUnsafe (name);
			if (ting != null) {
				return ting.position.roomName;
			} else {
				return "";
			}
		}

		[SprakAPI("")]
		public void API_SetPosition (string name, string targetThing)
		{
			var ting = _tingRunner.GetTingUnsafe (name);
			var target = _tingRunner.GetTingUnsafe(targetThing);

			if (ting == null) {
				_computer.API_Print(name + " not found");
				return;
			}

			if (ting is Character && ting.name != "PlayWife") {
				_computer.API_Print("Impossible to move " + ting.name);
				return;
			}

			if (target == null) {
				_computer.API_Print(targetThing + " not found");
				return;
			}

			try {
				ting.position = target.position;
			}
			catch(Exception) {
				throw new Error("Can't move " + ting.name + " to position of " + target.name);
			}
		}

		[SprakAPI("")]
		public void API_InteractWith (string name, string target)
		{
			var ting = _tingRunner.GetTingUnsafe (name);
			var targetTing = _tingRunner.GetTingUnsafe (target);

			if (ting == null) {
				_computer.API_Print(name + " not found");
				return;
			}

			if (target == null) {
				_computer.API_Print(target + " not found");
				return;
			}

//			if (target.room != ting.room) {
//				_computer.API_Print(target + " is in another room, can't interact");
//				return;
//			}

			var character = ting as Character;

			if(character == null) {
				_computer.API_Print(targetTing + " is not a character");
				return;
			}

			character.WalkToTingAndInteract(targetTing);
		}

		[SprakAPI("")]
		public object[] API_GetThingsInRoom (string roomName)
		{
			var theRoom = _roomRunner.GetRoomUnsafe (roomName);

			if (theRoom == null) {
				_computer.API_Print(roomName + " not found");
				return new string[] {};
			}

			return theRoom.GetTings ().Select (t => t.name).ToArray ();
		}

		[SprakAPI("")]
		public object[] API_GetAllRooms ()
		{
			List<string> roomNames = new List<string>();
			foreach(var room in _roomRunner.rooms) {
				roomNames.Add(room.name);
			}
			return roomNames.ToArray();
		}
	}

	public class TrapAPI
	{
		Computer _computer;
		TingRunner _tingRunner;
		DialogueRunner _dialogueRunner;

		string _computerRoomCache;

		public TrapAPI (Computer pComputer, TingRunner pTingRunner, DialogueRunner pDialogueRunner)
		{
			_computer = pComputer;
			_tingRunner = pTingRunner;
			_dialogueRunner = pDialogueRunner;

			_tingRunner.onTingHasNewRoom += OnTingHasNewRoom;
			_dialogueRunner.AddOnEventListener (OnEvent);

			_computerRoomCache = _computer.room.name;

			//D.Log("Trap API for " + pComputer + " is rigged!");
		}

		~TrapAPI() {
			_tingRunner.onTingHasNewRoom -= OnTingHasNewRoom;
			_dialogueRunner.RemoveOnEventListener (OnEvent);
		}

		void OnTingHasNewRoom(Ting pTing, string pNewRoomName) {
			//D.Log("OnTingHasNewRoom in " + _computer + " happened with " + pTing + " and new room " + pNewRoomName);
			if(pNewRoomName == _computerRoomCache && !_computer.masterProgram.isOn) {
				//D.Log("OnIntruder in " + _computer + " will trigger with " + pTing.name + " in room " + pNewRoomName);
				_computer.masterProgram.maxExecutionTime = 10f;
				_computer.masterProgram.StartAtFunctionIfItExists ("OnIntruder", new object[] { pTing.name }, null);
			}
		}

		void OnEvent(string pEvent) {
			if(pEvent.Contains("_hack_")) {
				string hackerName = pEvent.Substring (0, pEvent.IndexOf ('_'));
				Character hacker = _tingRunner.GetTing<Character> (hackerName);
				if (hacker.room == _computer.room) {
					OnHack (hackerName);
				} else {
					//D.Log (_computer + " ignoring " + pEvent + " because the hacker is in another room");
				}
			}
		}

		void OnHack(string pHackerName) {
			if (!_computer.masterProgram.isOn) {
				_computer.masterProgram.maxExecutionTime = 10f;
				_computer.masterProgram.StartAtFunctionIfItExists ("OnHack", new object[] { pHackerName }, null);
			}
		}

		[SprakAPI("Checks whether a string contains a substring")]
		public bool API_StringContains(string s, string substr)
		{
			return s.Contains(substr);
		}

		[SprakAPI("Move an intruder in the room to the position of another thing")]
		public void API_MovePerson (string name, string target)
		{
			Ting intruder = _tingRunner.GetTingUnsafe (name);
			Ting targetTing = _tingRunner.GetTingUnsafe (target);
			if (intruder == null || intruder.room != _computer.room) {
				throw new Error ("Can't find " + name + " in this room");
			}
			if (targetTing == null) {
				throw new Error ("Can't find " + target + " to send " + name + " to");
			}
			intruder.position = targetTing.position;
		}

		[SprakAPI("Zap an interuder in the room (will make them fall asleep)")]
		public void API_ZapPerson (string name)
		{
			Character intruder = _tingRunner.GetTingUnsafe (name) as Character;
			if (intruder == null || intruder.room != _computer.room) {
				throw new Error ("Can't find " + name + " in this room");
			}
			//D.Log (_computer.name + " will zap " + intruder);
			intruder.StopAction ();
			intruder.GetTased ();
			_computer.PlaySound ("FishAttack");
		}

		[SprakAPI("Resets code to default state")]
		public void API_RestoreCode (string name)
		{
			MimanTing ting = _tingRunner.GetTingUnsafe (name) as MimanTing;
			if (ting == null || ting.room != _computer.room) {
				throw new Error ("Can't find " + name + " in this room");
			}
			foreach (var program in ting.programs) {
				string originalSourceCode = ting.sourceCodeDispenser.GetSourceCode(program.sourceCodeName).content;
				if(program.sprakRunner != null) {
					program.sprakRunner.HardReset();
				}
				program.sourceCodeContent = originalSourceCode;
				program.StopAndReset ();
			}
		}

		[SprakAPI("Broadcast a message. Use with caution.", "The message to be broadcasted")]
		public void API_Broadcast (string pMessage)
		{
			_dialogueRunner.EventHappened (pMessage);
		}
	}

	public class HeartAPI
	{
		Computer _computer;
		TingRunner _tingRunner;
		DialogueRunner _dialogueRunner;

		public HeartAPI (Computer pComputer, TingRunner pTingRunner, DialogueRunner pDialogueRunner)
		{
			_computer = pComputer;
			_tingRunner = pTingRunner;
			_dialogueRunner = pDialogueRunner;
		}
		
		[SprakAPI("Set numeric data on object")]
		public void API_SetNumericData (string objectName, string dataName, float value)
		{
			Ting o = _tingRunner.GetTingUnsafe (objectName);
			if(o == null) {
				_computer.API_Print("Can't find object with name \"" + objectName + "\"");
				return;
			}
			o.table.SetValue<float>(o.objectId, dataName, value);
			_computer.API_Print("Set " + dataName + " to " + value + " on " + objectName);
		}

		[SprakAPI("Get numeric data on object")]
		public float API_GetNumericData (string objectName, string dataName)
		{
			Ting o = _tingRunner.GetTingUnsafe (objectName);
			if(o == null) {
				_computer.API_Print("Can't find object with name \"" + objectName + "\"");
				return 0f;
			}
			float f = o.table.GetValue<float>(o.objectId, dataName);
			return f;
		}

		[SprakAPI("Break")]
		public void API_Break ()
		{
			//_dialogueRunner.EventHappened("HEART_WAS_BROKEN");
			_computer.masterProgram.sourceCodeContent = "a00j ksdhg 245kljshg a sl3434 kjghklj434 651 xsdhgkl";
			_computer.masterProgram.sourceCodeName = "";
			throw new Error("BROKEN", Error.ErrorType.RUNTIME, -1, -1);
		}

		[SprakAPI("")] 
		public void API_ZapPersonGently (string name)
		{
			Character intruder = _tingRunner.GetTingUnsafe (name) as Character;
			if (intruder == null) {
				throw new Error ("Can't find " + name);
			}
			//D.Log (_computer.name + " will zap " + intruder + " gently");
			intruder.StopAction ();
			intruder.GetTasedGently ();
			if(Randomizer.OneIn(3)) {
				intruder.Say (Randomizer.RandNth(zapExclaims), "Misc");
			}
		}

		static string[] zapExclaims = new string[] {
			"Ahhhh!!!",
			"AAAA!!!",
			//"Aj",
			//"Nej!",
			"OOHH",
			"UUUUuuuu",
			"!!!",
		};
	}

	public class FloppyAPI
	{
		Computer _computer;
		//TingRunner _tingRunner;

		public FloppyAPI (Computer pComputer, TingRunner pTingRunner)
		{
			_computer = pComputer;
			//_tingRunner = pTingRunner;
		}

		[SprakAPI("Is there a floppy in the drive?")]
		public bool API_HasFloppy ()
		{
			return _computer.floppyInDrive != null;
		}

//		[SprakAPI("Get the name of the floppy")]
//		public string API_FloppyName ()
//		{
//			var floppy = _computer.floppyInDrive;
//
//			if (floppy == null) {
//				throw new Error ("No floppy in drive, can't get its name");
//			}
//
//			return floppy.name;
//		}

		[SprakAPI("Load data from the floppy, split by lines")]
		public SortedDictionary<KeyWrapper, object> API_LoadData ()
		{
			var floppy = _computer.floppyInDrive;

			if (floppy == null) {
				throw new Error ("No floppy in drive, can't load data");
			}

			//var oarray = new List<object>();
			var dictArray = new SortedDictionary<KeyWrapper, object>();
			int i = 0;

			foreach(var s in floppy.masterProgram.sourceCodeContent.Split (new char[] { '\n' }, 9999)) {
				//oarray.Add(s);
				dictArray.Add(new KeyWrapper((float)i++), s);
			}

			return dictArray;
		}

		[SprakAPI("Clear all data on the floppy")]
		public void API_ClearData ()
		{
			var floppy = _computer.floppyInDrive;

			if (floppy == null) {
				throw new Error ("No floppy in drive, can't clear data");
			}

			floppy.masterProgram.sourceCodeContent = "";
		}

		[SprakAPI("Save data to the floppy by appending a line at the end")]
		public void API_SaveData (string data)
		{
			var floppy = _computer.floppyInDrive;

			if (floppy == null) {
				throw new Error ("No floppy in drive, can't save data");
			}

			floppy.masterProgram.sourceCodeContent += ("\n" + data);
		}

		[SprakAPI("Restart the computer but run the code on the floppy instead")]
		public void API_BootFromFloppy ()
		{
			var floppy = _computer.floppyInDrive;

			if (floppy == null) {
				throw new Error ("No floppy in drive, can't boot from it");
			}

			_computer.masterProgram.StopAndReset ();
			_computer.floppyBootProgram.sourceCodeContent = floppy.masterProgram.sourceCodeContent;
			_computer.floppyBootProgram.Start ();
			_computer.floppyBootProgram.maxExecutionTime = 60f;
		}
	}

	public class ArcadeMachineAPI
	{
		Computer _computer;

		public ArcadeMachineAPI (Computer pComputer)
		{
			_computer = pComputer;
		}

		[SprakAPI("Is a key pressed? (left/right/down/up/space)")]
		public bool API_IsKeyPressed (string key)
		{
			if (_computer.isKeyPressed == null) {
				//D.LogError ("_computer.isKeyPressed is null!");
				return false;
			} else {
				bool b = _computer.isKeyPressed (key.ToLower());
				if (b) {
					_computer.activeProgram.executionTime = 0f; // don't turn off automatically yet
				}
				return b;
			}
		}
	}
}

