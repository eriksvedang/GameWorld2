//#define LOG_TILE_GROUP_FIX
#define CACHING

using System;
using TingTing;
using System.Collections.Generic;
using GameTypes;
using System.Linq;
using ProgrammingLanguageNr1;
using RelayLib;
using GrimmLib;

namespace GameWorld2
{
    public class MimanTingConcrete : MimanTing { 
		public override Program masterProgram {
			get {
				return null;
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}
	}

	public abstract class MimanTing : Ting
	{
		protected ProgramRunner _programRunner;
		protected SourceCodeDispenser _sourceCodeDispenser;
		protected DialogueRunner _dialogueRunner;
		protected WorldSettings _worldSettings;

		private ValueEntry<int[]> CELL_programObjectIds;
		private ValueEntry<string[]> CELL_connectedTings;
		private ValueEntry<bool> CELL_emitsSmoke;
		
		ValueEntry<bool> CELL_isPlaying;
		ValueEntry<string> CELL_soundName;
		ValueEntry<float> CELL_pitch;
		ValueEntry<float> CELL_audioTime;
		ValueEntry<float> CELL_audioTotalLength;
		ValueEntry<bool> CELL_audioLoop;
		
		ValueEntry<float> CELL_messageTimer;
		ValueEntry<string> CELL_userDefinedLabel;
		
		public delegate void OnPlaySound(string pKey);
		public OnPlaySound onPlaySound;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programObjectIds = EnsureCell("programs", new int[] {});
			CELL_connectedTings = EnsureCell("connections", new string[] {});
			CELL_emitsSmoke = EnsureCell("emitsSmoke", false);
			
			CELL_isPlaying = EnsureCell("isPlaying", false);
			CELL_pitch = EnsureCell("pitch", 1f);
			CELL_soundName = EnsureCell("soundName", "");
			CELL_audioTime = EnsureCell("audioTime", 0.0f);
			CELL_audioTotalLength = EnsureCell("audioTotalLength", 60.0f);
			CELL_audioLoop = EnsureCell("audioLoop", false);
			
			CELL_messageTimer = EnsureCell("messageTimer", 0f);

			CELL_userDefinedLabel = EnsureCell("userDefinedLabel", "");
		}

		internal void SetMimanRunners(ProgramRunner pProgramRunner, SourceCodeDispenser pSourceCodeDispenser, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings) 
        {
			_programRunner = pProgramRunner;
			_sourceCodeDispenser = pSourceCodeDispenser;
            _dialogueRunner = pDialogueRunner;
			_worldSettings = pWorldSettings;
		}

		public virtual void MaybeFixGroupIfOutsideIslandOfTiles() {}

		protected void FixGroupIfOutsideIslandOfTiles() {

			if(this.tile == null) {
				var newTile = new PointTileNode(localPoint, room);
				room.AddTile(newTile);
				#if CACHING
				this.SetCachedTile (); // the caching doesn't account for adding new tiles under an existing Ting
				#endif
				#if LOG_TILE_GROUP_FIX
				D.Log("Created tile at local point for " + name);
				#endif
			}

			for(int i = 0; i < interactionPoints.Length; i++) {
				PointTileNode tileAtInteractionPoint = room.GetTile(interactionPoints[i]);
				
				if(tileAtInteractionPoint == null) {
					#if LOG_TILE_GROUP_FIX
					D.Log("Tile at interaction point of " + name + " is null!");
					#endif
					continue;
				}
								
				if(this.tile.group == -1 && tileAtInteractionPoint.group == -1) {
					throw new Exception("Both tile at position and tile at interaction point belong to group -1 for " + name);
				}
				
				if(this.tile.group != tileAtInteractionPoint.group) {
					this.tile.group = tileAtInteractionPoint.group;
					#if LOG_TILE_GROUP_FIX
					D.Log("Fixed the group of tile under " + name + " to " + tileAtInteractionPoint.group + ", got group from tile " + tileAtInteractionPoint + 
					      "(interaction point " + i + ") which has occupants: " + tileAtInteractionPoint.GetOccupantsAsString());
					#endif
					break; // BREAK OUT, WE'RE DONE! 
				}
			}
		}

		public override string UseTingOnTingDescription (Ting pOtherTing)
		{
			if(pOtherTing is Locker && this.CanInteractWith(pOtherTing)) {
				return "put " + this.tooltipName + " into locker";
			}
			else if(pOtherTing is SendPipe && this.CanInteractWith(pOtherTing)) {
				return "put " + this.tooltipName + " into pipe";
			}
			else if(pOtherTing is TrashCan && this.CanInteractWith(pOtherTing)) {
				return "throw " + this.tooltipName + " into trash can";
			}

			return base.UseTingOnTingDescription (pOtherTing);
		}

		public MimanTing[] connectedTings {
			get {
				string[] tingNames = CELL_connectedTings.data;
				int nrOfConnections = tingNames.Length;
				MimanTing[] tingArray = new MimanTing[nrOfConnections];
				int i = 0;
				foreach (string tingName in tingNames) {
					//D.Log("Finding program with object id " + programObjectId);
					MimanTing mimanTing = _tingRunner.GetTing (tingName) as MimanTing;
#if DEBUG
					D.isNull (mimanTing, "Miman ting is null (the cast failed)");
#endif
					tingArray [i++] = mimanTing;
				}
				return tingArray;
			}
			set {
				string[] tingNames = new string[value.Length];
				int i = 0;
				foreach(MimanTing mimanTing in value) {
					tingNames[i++] = mimanTing.name;
				}
				CELL_connectedTings.data = tingNames;
			}
		}

		public float AddConnectionToTing (Ting pTing)
		{
			if (pTing == this) {
				//return "Can't connect to self"; // can't connect to self
				return -1f;
			}

			D.isNull (pTing, "Can't connect to null");
			string tingName = pTing.name;

			string[] oldTingNames = CELL_connectedTings.data;
			int index = 0;
			foreach (string name in oldTingNames) {
				if (tingName == name) {
					//return "Already connected to " + name; // already has a connection to this ting
					return (float)index;
				}
				index++;
			}
			
			string[] newTingNames = new string[oldTingNames.Length + 1];
			int i = 0;
			foreach(string name in oldTingNames) {
				newTingNames[i++] = name;
			}
			newTingNames[i] = tingName;
			CELL_connectedTings.data = newTingNames;

			//return "Connected successfully to " + tingName;
			return (float)i;
		}
		
		public virtual void Say(string pLine, string pConversation) {
			lastConversation = pConversation;
			if(dialogueLine != "") {
				dialogueLine = ""; // sends an event with "" as the parameter
			}
			messageTimer = _worldSettings.totalWorldTime;
			dialogueLine = pLine;
			_tingRunner.Register(this);
		}

		protected void UpdateBubbleTimer() {
			if (!_dialogueLineIsEmpty_Cache && (_worldSettings.totalWorldTime - messageTimer) > 3.0f) {
				// This is the upper limit of how long a dialogue line can be shown for!!!
				//D.Log("Time to clear dialogueLine for " + name);
				Say ("", "");
				if(autoUnregisterFromUpdate) {
					_tingRunner.Unregister(this);
				}
			}
		}

		public virtual bool autoUnregisterFromUpdate {
			get {
				return true;
			}
		}

		[ShowInEditor()]
		public bool containsBrokenPrograms
		{
			get {
				foreach(Program p in programs) {
					if(p.ContainsErrors()) {
						return true;
					}
				}
				return false;
			}
		}

		public Program[] programs
        {
			get {
#if DEBUG
				D.isNull(_programRunner, "ProgramRunner must be set");
				D.isNull(CELL_programObjectIds, "CELL_programObjectIds is null");
#endif
				int[] programObjectIds = CELL_programObjectIds.data;
				if(CELL_programObjectIds.data == null) {
					D.Log("CELL_programObjectIds.data is null");
					return new Program[] {};
				}
				int nrOfPrograms = programObjectIds.Length;

				//D.Log(name + " has " + nrOfPrograms + " programs, now it's going to find them in ProgramRunner");
				List<Program> listOfPrograms = new List<Program>();
				foreach(int programObjectId in programObjectIds) {
					//D.Log("Finding program with object id " + programObjectId);
					Program p = _programRunner.GetProgramUnsafe(programObjectId);
					if (p != null) {
						listOfPrograms.Add (p);
					} else {
						throw new Exception ("Can't get program with id " + programObjectId);
					}
				}
				return listOfPrograms.ToArray();
			}
			set {
				int[] programObjectIds = new int[value.Length];
				int i = 0;
				//Console.WriteLine("Setting programs array: ");
				foreach(Program p in value) {
					programObjectIds[i++] = p.objectId;
					//Console.WriteLine ("Program object id: " + p.objectId); // + ": " + _programRunner.GetProgram(p.objectId).name);
				}
				//Console.WriteLine("end");
				CELL_programObjectIds.data = programObjectIds;
			}
		}

		[ShowInEditor]
		public string programNames {
			get {
				Program[] cachedProgramsArray = programs;
				int nrOfPrograms = cachedProgramsArray.Length;
				if(nrOfPrograms == 0) {
					return "[] or not generated yet";
				}
				string[] programNames = new string[nrOfPrograms];
				int counter = 0;
				foreach(Program program in cachedProgramsArray) {
					programNames[counter] = cachedProgramsArray[counter].name;
					counter++;
				}
				return "[" + string.Join(",", programNames) + "]";
			}
		}
		
		public void AddProgramToProgramsArray(Program pNewProgram)
		{
#if DEBUG
			foreach(Program p in programs) {
				if(p == pNewProgram) {
					throw new Exception("Adding a program (" + pNewProgram.name + ") to programs array that is already in there");
				}
			}
#endif			
			Program[] originalArray = programs;
			Program[] newArray = new Program[originalArray.Length + 1];
			int i = 0;
			foreach(Program p in originalArray) {
				newArray[i++] = p;
			}
			newArray[i] = pNewProgram;
			programs = newArray;
		}
		
		public Error[] ChangeAndRecompileProgram (string pProgramName, string pNewSourceCodeContent)
		{
			Program p = GetProgram (pProgramName);
			if (p == null) {
				throw new Exception ("Can't find program '" + pProgramName + "' to change and recompile");
			}
			p.sourceCodeContent = pNewSourceCodeContent;

			Error[] errors = p.Compile ();
			return errors;
		}
		
		/// <returns>
		/// Returns null if the program does not exist in the MimanTing
		/// </returns>
		public Program GetProgram(string pProgramName)
		{
			foreach(Program p in programs) {
				if(p.name == pProgramName) {
					return p;
				}
			}
			return null;
		}
		
		public Program EnsureProgram(string pProgramName, string pNameOfSourceCodeToUseIfProgramDoesNotExist)
		{
			logger.Log("Ensuring program with name '" + pProgramName + "'...");
			Program p = GetProgram(pProgramName);
			if(p != null) 
			{
				logger.Log("Program already existed");
				return p;
			}
			else
			{
				SourceCode source = _sourceCodeDispenser.GetSourceCode(pNameOfSourceCodeToUseIfProgramDoesNotExist);
				Program newProgram = _programRunner.CreateProgram(pProgramName, source.content, pNameOfSourceCodeToUseIfProgramDoesNotExist);
				logger.Log ("Created a new program with id " + newProgram.objectId + " and name " + newProgram.name + " from source " + source.name); // + " with content: " + source.content);
				AddProgramToProgramsArray(newProgram);
				return newProgram;
			}
		}

		public abstract Program masterProgram {
			get;
		}
		
		/// <summary>
        /// Override to get a chance to "ensure" all programs (since programs usually aren't generated until they are needed)
        /// </summary>
		public virtual void PrepareForBeingHacked() {}
        
        /// <summary>
        /// This is called when all runners and cells has been set for all tings
        /// </summary>
        public virtual void Init() 
		{
			ConnectToCurrentTile();
		}

		public void StartMasterProgramIfItIsOn() {
			if(masterProgram != null && masterProgram.isOn) {
				D.Log("(RE)STARTING ALREADY RUNNING PROGRAM IN " + name);
				PrepareForBeingHacked();
				masterProgram.Start();
			}
		}

		public abstract bool DoesMasterProgramExist();
		
		public void TurnDegrees(int pDegrees)
		{
			int degrees = (int)IntPoint.DirectionToIntPoint(direction).Degrees();
			degrees -= pDegrees;	
			direction = GridMath.DegreesToDirection((int)degrees);
		}
		
		public void PlaySound(string pKey)
		{
			if(onPlaySound != null) {
				onPlaySound(pKey);
			}
			else {
#if DEBUG
				//D.Log("onPlaySound has got no listener for " + name);
#endif
			}
		}

		public virtual void OnPutDown() {
			// do nothing
		}

		[EditableInEditor]
		public bool emitsSmoke {
			get {
				return CELL_emitsSmoke.data;
			}
			set {
				CELL_emitsSmoke.data = value;

//				if(value) {
//					System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
//					D.Log(name + " should emit smoke, stacktrace: " + t.ToString());
//				}
			}
		}

		public virtual int securityLevel {
			get {
				return 0;
			}
		}
		
		[EditableInEditor]
		public float audioTime
		{
			get {
				return CELL_audioTime.data;
			}
			set {
				CELL_audioTime.data = value;
			}
		}
		
		[EditableInEditor]
		public string soundName
		{
			get {
				return CELL_soundName.data;
			}
			set {
				CELL_soundName.data = value;
			}
		}
		
		[EditableInEditor()]
		public bool isPlaying
		{
			get {
				return CELL_isPlaying.data;
			}
			set {
				CELL_isPlaying.data = value;
			}
		}
		
		[EditableInEditor()]
		public float audioTotalLength
		{
			get {
				return CELL_audioTotalLength.data;
			}
			set {
				CELL_audioTotalLength.data = value;
			}
		}

		[EditableInEditor()]
		public bool audioLoop
		{
			get {
				return CELL_audioLoop.data;
			}
			set {
				CELL_audioLoop.data = value;
			}
		}
		
		public float pitch
		{
			get {
				return CELL_pitch.data;
			}
			set {
				CELL_pitch.data = value;
			}
		}
		
		[EditableInEditor()]
		public float messageTimer
		{
			get {
				return CELL_messageTimer.data;
			}
			set {
				CELL_messageTimer.data = value;
			}
		}

		public string userDefinedLabel
		{
			get {
				return CELL_userDefinedLabel.data;
			}
			set {
				CELL_userDefinedLabel.data = value;
			}
		}

		public SourceCodeDispenser sourceCodeDispenser {
			get {
				return _sourceCodeDispenser;
			}
		}
    }
}

