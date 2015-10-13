using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;
using Pathfinding;

namespace GameWorld2
{
	public class Door : MimanTing, IExit
	{
        public static new string TABLE_NAME = "Ting_Doors";
		ValueEntry<bool> CELL_isLocked;
		ValueEntry<string> CELL_targetDoorName;
		ValueEntry<string> CELL_programName;
		ValueEntry<string[]> CELL_elevatorAlternatives;
		ValueEntry<int> CELL_elevatorFloor;
		ValueEntry<bool> CELL_isElevatorEntrance; // a door where you walk into the elevator room
		ValueEntry<int> CELL_code;
		ValueEntry<float> CELL_autoLockTimer;
		ValueEntry<bool> CELL_isMoveable;
		ValueEntry<bool> CELL_hasDoorKnob; // if it has a doorknob it is opened by touching it, otherwise you just walk through
		ValueEntry<bool> CELL_useForRoomPathfinding;

		Program _program;
		Character _user;
		int _attempts = 0;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_isLocked = EnsureCell("isLocked", false);
            CELL_targetDoorName = EnsureCell("targetDoorName", "");
			CELL_programName = EnsureCell("masterProgramName", "OnDoorUsed");
			CELL_elevatorAlternatives = EnsureCell("elevatorAlternatives", new string[] {});
			CELL_elevatorFloor = EnsureCell("elevatorFloor", 0);
			CELL_isElevatorEntrance = EnsureCell<bool>("isElevatorEntrance", false);
			CELL_code = EnsureCell<int>("code", 0);
			CELL_autoLockTimer = EnsureCell<float>("autoLockTimer", 0f);
			CELL_isMoveable = EnsureCell<bool>("isMoveable", false);
			CELL_hasDoorKnob = EnsureCell<bool>("hasDoorKnob", true);
			CELL_useForRoomPathfinding = EnsureCell<bool>("useForRoomPathfinding", true);
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public void ResetAttempts() {
			_attempts = 0;
			//D.Log("Attempts for door " + name + " was reset to 0.");
		}

		const int FAILURE_THRESHOLD = 25;

		public void IncreaseAttempts() {
			_attempts++;
			if(_attempts > FAILURE_THRESHOLD) {
				D.Log("Failures to get through door " + name + " reached " + FAILURE_THRESHOLD + ", will reset code");
				_program.sourceCodeContent = _sourceCodeDispenser.GetSourceCode(_program.sourceCodeName).content;
				_program.Compile();
			} else {
				//D.Log("Attempts for door " + name + " increased to " + _attempts);
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override void FixBeforeSaving ()
		{
			if(prefab.ToLower().Contains("elevator")) {
				hasDoorKnob = false;
			}
		}

		public override void Update (float dt)
		{
			if(autoLockTimer > 0f) {
				autoLockTimer -= dt;
				if(autoLockTimer < 0f) {
					isLocked = true;
					//D.Log(name + " was autolocked");
				}
			}

			UpdateBubbleTimer();
		}

		public override bool autoUnregisterFromUpdate {
			get {
				return false;
				// nope, needs constant update
			}
		}

        private void WriteTargetToTile()
        {
            D.isNull(room, "room is null");
            D.isNull(_roomRunner, "room runner is null!");
			
			PointTileNode tileAtInteractionPoint = room.GetTile(interactionPoints[0]);
            
			if(tileAtInteractionPoint == null) {
				return;
			}
			
			tileAtInteractionPoint.AddOccupant(this);
			
            if (targetPosition == WorldCoordinate.NONE) {
                tileAtInteractionPoint.teleportTarget = null;
			}
            else {
                tileAtInteractionPoint.teleportTarget = _roomRunner.GetRoom(targetPosition.roomName).GetTile(targetPosition.localPosition);
			}
        }

//		public override bool isBeingUsed {
//			get {
//				return AtLeastOneInteractionPointIsOccupied () || targetDoor.AtLeastOneInteractionPointIsOccupied ();
//			}
//		}
		
		public void Open()
		{
			StartAction("Opening", null, 2.0f, 2.0f);
		}

		[SprakAPI("Lock the door, returns success/fail", "The security code")]
		public bool API_Lock(float code) {
			return Lock (code);
		}

		[SprakAPI("Unlock the door, returns success/fail", "The security code")]
		public bool API_Unlock(float code) {
			return Unlock (code);
		}
		
		public bool Lock(float pCode) {
			if((int)pCode == (int)code) {
				isLocked = true;
				SyncLockOnTargetDoor();
				D.Log(name + " was locked with code " + pCode);
				Say("Locked!", "");
				PlaySound("DoorLock");
				audioLoop = false;
				return true;
			} else {
				D.Log(name + " FAILED to be locked with code " + pCode);
				Say("Key is not working, invalid code: " + pCode, "");
				return false;
			}
		}
		
		public bool Unlock(float pCode) {
			if((int)pCode == (int)code || code == 0) {
				isLocked = false;
				SyncLockOnTargetDoor();
				//D.Log(name + " was unlocked with code " + pCode);
				Say("Unlocked!", "");
				PlaySound("DoorUnlock");
				audioLoop = false;
				return true;
			} else {
				D.Log(name + " FAILED to be unlocked with code " + pCode);
				Say("Key is not working, invalid code: " + pCode, "");
				return false;
			}
		}
		
		private void SyncLockOnTargetDoor() {
			if(targetDoor != null) {
				targetDoor.isLocked = isLocked;
			}
		}
		
		public void WalkThrough(Character pUser)
		{
			if(pUser == null) {
				D.Log("Can't call WalkThrough with pUser == null !");
				return;
			}

			// SINCE PROGRAMS DON'T GET RECOMPILED WHEN USED IT'S DANGEROUS TO HAVE ERRORS IN THEM (THEY DOOR STOPS WORKING)
			if(masterProgram.ContainsErrors()) {
				masterProgram.ClearErrors();
				masterProgram.Compile();
			}

			//D.Log("Setting user of door '" + name + "' to '" + pUser.name + "'");
			_user = pUser;

			IncreaseAttempts();
			masterProgram.Start();
			
			//API_TeleportUser(API_GetTargetRoom(), API_GetTargetX(), API_GetTargetY());
		}

		[SprakAPI("Walk out through another door")]
		public void API_Goto(string doorName)
		{
			if(_user == null) {
				D.Log ("User for door " + name + " is null");
				//throw new Error ("No user for door " + name);
				return;
			}

			var otherDoor = _tingRunner.GetTingUnsafe (doorName) as Door;

			if(otherDoor != null) {
				Room otherRoom = otherDoor.room;
				var interactionPoint = otherDoor.interactionPoints [0];
				int otherX = interactionPoint.x;
				int otherY = interactionPoint.y;

				PushAwayBlockers(otherRoom, otherX, otherY, IntPoint.DirectionToIntPoint(otherDoor.direction));

				WorldCoordinate newPosition = new WorldCoordinate(otherRoom.name, otherX, otherY);

				//D.Log("ExitTroughDoor was called on " + name + " and will now teleport " + _user.name + " to " + newPosition);

				_user.targetPositionInRoom = otherDoor.position; // makes the Shell not freak out when it is created in the new scene but target is still in the old one
				_user.position = newPosition;
				_user.direction = otherDoor.direction;
				_user.StopAction();
				_user.StartAction("WalkingThroughDoorPhase2", null, 1.35f, 1.35f);
				ResetAttempts(); // success!

				_dialogueRunner.EventHappened(_user.name + "_open_" + name);
				if(isElevatorEntrance) {
					otherDoor.elevatorFloor = this.elevatorFloor;
				}
			}
			else {
				throw new Error ("Can't find door with name " + doorName);
			}
		}
		
//		[SprakAPI("Teleport user to another position. Returns an error message as a string.", "x", "y")]
//		public string API_TeleportUser(string targetRoom, int x, int y)
//		{
//			if(_user == null) {
//				D.Log ("User for door " + name + " is null");
//				return "Fail";
//				//throw new Exception("User is null for door " + name);
//			}
//			
//			if(targetDoor != null) {
//				var targetRoomRef = _roomRunner.GetRoom(targetRoom);
//				PushAwayBlockers(targetRoomRef, x, y, IntPoint.DirectionToIntPoint(targetDoor.direction));
//				WorldCoordinate newPosition = new WorldCoordinate(targetRoom, x, y); // OLD VERSION: door.targetPosition;
//				logger.Log(name + " opened the door " + name + " and will now teleport to " + newPosition);
//				_user.targetPositionInRoom = targetDoor.position; // makes the Shell not freak out when it is created in the new scene but target is still in the old one
//				_user.position = newPosition;
//				_user.direction = targetDoor.direction;
//				_user.StopAction();
//				_user.StartAction("WalkingThroughDoorPhase2", null, 1.35f, 1.35f);
//				_dialogueRunner.EventHappened(_user.name + "_open_" + name);
//				if(isElevatorEntrance) {
//					targetDoor.elevatorFloor = this.elevatorFloor;
//				}
//				return "Success";
//			}
//			else {
//				return "Door '" + name + "' doesn't have a target";
//			}
//		}
		
		void PushAwayBlockers(Room targetRoom, int x, int y, IntPoint pushDir) {
			var tile = targetRoom.GetTile(x, y);
			if(tile == null) return;
			var blockers = tile.GetOccupants();
			foreach(var blocker in blockers) {
				if(blocker is Character) {
					var newPosition = new WorldCoordinate(blocker.position.roomName, blocker.position.localPosition + pushDir * 2);
					if(targetRoom.GetTile(newPosition.localPosition) != null) {
						//D.Log(blocker + " was pushed by a door from " + blocker.position + " to " + newPosition);
						blocker.position = newPosition;
					} else {
						D.Log("Can't push " + blocker.name + " to " + newPosition + " because there is no tile there");
					}
				}
			}
		}
		
//		[SprakAPI("Get the name of the target door's room")]
//		public string API_GetTargetRoom()
//		{
//			if(targetDoor != null) {
//				return targetDoor.room.name;
//			}
//			else {
//				logger.Log("Target door for " + name + " is null");
//				return "No target";
//			}
//		}
//		
//		[SprakAPI("Get the x position of the target door")]
//		public int API_GetTargetX()
//		{
//			if(targetDoor != null) {
//				return targetDoor.interactionPoints[0].x;
//			}
//			else {
//				logger.Log("Target door for " + name + " is null");
//				return 0;
//			}
//		}
//		
//		[SprakAPI("Get the y position of the target door")]
//		public int API_GetTargetY()
//		{
//			if(targetDoor != null) {
//				return targetDoor.interactionPoints[0].y;
//			}
//			else {
//				logger.Log("Target door for " + name + " is null");
//				return 0;
//			}				
//		}
		
		[SprakAPI("Get the name of the person using the door")]
		public string API_GetUser()
		{
			if(_user != null) {
				return _user.name;
			} else {
				return "";
			}
		}

		[SprakAPI("Stop the person to walk through the door")]
		public void API_StopUser()
		{
			if(_user != null) {
				_user.StopAction();
				_user.ClearState();
			}
		}

		[SprakAPI("Say something")]
		public void API_Say(string message)
		{
			Say (message, "");
		}

		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
		}

//		[SprakAPI("Set the key code for this door")]
//		public void API_SetCode (int newCode)
//		{
//			code = newCode;
//		}
//
//		[SprakAPI("Get the key code for this door")]
//		public int API_GetCode ()
//		{
//			return code;
//		}

		[EditableInEditor]
		public bool isLocked {
			get {
				return CELL_isLocked.data;
			}
			set {
				CELL_isLocked.data = value;
			}
		}

		[EditableInEditor]
		public bool isMoveable {
			get {
				return CELL_isMoveable.data;
			}
			set {
				CELL_isMoveable.data = value;
			}
		}
		
		[EditableInEditor]
		public bool isElevatorEntrance {
			get {
				return CELL_isElevatorEntrance.data;
			}
			set {
				CELL_isElevatorEntrance.data = value;
			}
		}
        
		[ShowInEditor]
		public WorldCoordinate targetPosition
        {
            get {
				Door d = targetDoor;
				if(d != null) {
					return new WorldCoordinate(d.room.name, d.interactionPoints[0]);
				}
				else {
                    return WorldCoordinate.NONE;
				}
			}
        }
		
		public Door targetDoor
		{
			get {
				return _tingRunner.GetTingUnsafe(targetDoorName) as Door;
			}
		}

		[ShowInEditor]
		public string targetDoorReferenceStatus {
			get {
				if(targetDoor == null) {
					return "null";
				}
				else {
					return "OK, " + targetDoor.name;
				}
			}
		}

		[EditableInEditor]
		public string targetDoorName
		{
			get
            {
                return CELL_targetDoorName.data;
            }
            set 
			{ 
                CELL_targetDoorName.data = value;
                WriteTargetToTile();
				MimanPathfinder2.ClearRoomNetwork();
            }
		}
		
		[ShowInEditor]
		public string elevatorAlternativesString {
			get {
				return string.Join(", ", CELL_elevatorAlternatives.data);
			}
		}

		public string[] elevatorAlternatives
		{
			get
            {
                return CELL_elevatorAlternatives.data;
            }
            set 
			{ 
                CELL_elevatorAlternatives.data = value;
				//SetLinksToElevatorAlternatives();
            }
		}

		[EditableInEditor]
		public int elevatorFloor
		{
			get
            {
                return CELL_elevatorFloor.data;
            }
            set 
			{ 
				if (CELL_elevatorFloor.data != value) {
					StartAction ("Moving", null, 4.0f, 4.0f);
				}

                CELL_elevatorFloor.data = value;
				if(elevatorFloor >= 0 && elevatorFloor < elevatorAlternatives.Length) {
					CELL_targetDoorName.data = elevatorAlternatives[elevatorFloor];
                	WriteTargetToTile();
					SetSourceCodeFromDoorTarget ();
					//D.Log("Elevator " + name + " went to floor " + elevatorFloor + " and door " + CELL_targetDoorName.data);
				}
				else {
					D.Log("Can't go to floor " + elevatorFloor);
				}
            }
		}

		[EditableInEditor]
		public int code
		{
			get
            {
                return CELL_code.data;
            }
            set 
			{ 
            	CELL_code.data = value;
            }
		}

        public override string tooltipName {
			get {
				return "door";
			}
		}
		
		public override string verbDescription {
			get {
				return "open";
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { localPoint + IntPoint.DirectionToIntPoint(direction) * 3 };
			}
		}

		public IntPoint waitingPoint {
			get {
				return localPoint + IntPoint.DirectionToIntPoint(direction) * 4;
			}
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
		
		public override Program masterProgram {
			get {
				if(_program == null) {

					if (masterProgramName == "OnDoorUsed" || masterProgramName == "MinistryDoor") {
						// Generate a special program instead!
						string specialProgramName = "SpecialProgram_" + name;
						masterProgramName = specialProgramName;
						bool normalDoor = masterProgramName == "OnDoorUsed";
						var sourceCode = _sourceCodeDispenser.CreateSourceCodeFromString (specialProgramName, normalDoor ? src : ministrySrc);

						_programRunner.CreateProgram (specialProgramName, sourceCode.content, specialProgramName);
						_program = EnsureProgram("MasterProgram", masterProgramName);

					} else {
						_program = EnsureProgram("MasterProgram", masterProgramName);
					}

					var functionDefs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Door)));
					functionDefs.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = functionDefs;
				}
				return _program;
			}
		}

		private string src {
			get {
				return string.Format("\nGoto(\"{0}\")\n", targetDoorName);
			}
		}

		private string ministrySrc {
			get {
				return string.Format("\nGoto(\"{0}\")\n", targetDoorName);
			}
		}

		public void SetSourceCodeFromDoorTarget() {
			masterProgram.StopAndReset ();
			masterProgram.sourceCodeContent = src;
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}

		public override int securityLevel {
			get {
				return 1;
			}
		}

		public Ting GetLinkTarget ()
		{
			if (useForRoomPathfinding) {
				return targetDoor;
			} else {
				return null;
			}
		}

		[EditableInEditor]
		public bool useForRoomPathfinding {
			set {
				CELL_useForRoomPathfinding.data = value;
			}
			get {
				return CELL_useForRoomPathfinding.data;
			}
		}
		
		[ShowInEditor]
		public float autoLockTimer {
			set {
				CELL_autoLockTimer.data = value;
			}
			get {
				return CELL_autoLockTimer.data;
			}
		}

		[EditableInEditor]
		public bool hasDoorKnob {
			set {
				CELL_hasDoorKnob.data = value;
			}
			get {
				return CELL_hasDoorKnob.data;
			}
		}

		public bool isBusy {
			get {
//				if(targetDoor != null && targetDoor.actionName == "Opening") {
//					//D.Log("Target door of " + name + " is busy, so " + name + " is busy too.");
//					return true;
//				}
				return actionName == "Opening"; // || autoLockTimer > 0f;
			}
		}
	}
}