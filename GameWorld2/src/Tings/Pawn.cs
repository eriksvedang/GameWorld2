using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class Pawn : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Pawns";
		ValueEntry<string> CELL_programName;
		ValueEntry<WorldCoordinate> CELL_startPosition;
		ValueEntry<Direction> CELL_startRotation;
		ValueEntry<bool> CELL_dead;
		ValueEntry<int> CELL_moveNr;
		ValueEntry<int> CELL_team;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Pawn");
			CELL_startPosition = EnsureCell("startPosition", WorldCoordinate.NONE);
			CELL_startRotation = EnsureCell("startRotation", Direction.DOWN);
			CELL_dead = EnsureCell("dead", false);
			CELL_moveNr = EnsureCell("moveNr", 0);
			CELL_team = EnsureCell("team", 0);
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}
		
		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();
			startPosition = position;
			startRotation = direction;
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string tooltipName {
			get {
				return name;
			}
		}
		
		public override string verbDescription {
			get {
				return "";
			}
		}
		
		[SprakAPI("Resurrect if dead and move back to start position")]
		public void API_Reset()
		{
			D.Log("Reset was called on pawn " + name);
			position = startPosition;
			direction = startRotation;
			dead = false;
			moveNr = 0;
		}

		[SprakAPI("Turn left")]
		public void API_TurnLeft()
		{
			if(dead) {
				return;
			}
			TurnDegrees(90);
			moveNr++;
		}

		[SprakAPI("Turn right")]
		public void API_TurnRight()
		{
			if(dead) {
				return;
			}
			TurnDegrees(-90);
			moveNr++;
		}
		
		[SprakAPI("How many moves has the pawn made?")]
		public int API_GetMoveNr()
		{
			return moveNr;
		}
		
		[SprakAPI("Modulus")]
		public int API_Modulus(float a, float b)
		{
			return (int)a % (int)b;
		}
		
		[SprakAPI("Log")]
		public void API_Log(string pText)
		{
			D.Log(pText);
		}
		
		[SprakAPI("Print text")]
		public void API_Print(string text)
		{
			Say (text, "");
		}
		
		[SprakAPI("Can move one step forward?")]
		public bool API_CanMove()
		{
			var newPos = new WorldCoordinate(room.name, localPoint + IntPoint.DirectionToIntPoint(direction));
			var newTile = _roomRunner.GetRoom(room.name).GetTile(newPos.localPosition);
			if(newTile == null) {
				return false;
			}
			else if(newTile.GetOccupants().Length > 0) {
				foreach(var occupant in newTile.GetOccupants()) {
					if(occupant is Pawn) {
						var p = (occupant as Pawn);
						if(p.team == this.team) {
							return false;
						}
					}
				}
			}
			return true;
		}

		[SprakAPI("Get a random number between 0.0 and 1.0")]
		public float API_Random ()
		{
			return Randomizer.GetValue (0f, 1f);
		}
		
		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
		}
		
		[SprakAPI("Move forward one step")]
		public void API_Move()
		{
			if(dead) {
				return;
			}
			
			var newPos = new WorldCoordinate(room.name, localPoint + IntPoint.DirectionToIntPoint(direction));
			var newTile = _roomRunner.GetRoom(room.name).GetTile(newPos.localPosition);
			if(newTile == null) {
				//D.Log(name + " can't move forward since there is no tile to move to");
			}
			else if(newTile.GetOccupants().Length > 0) {
				//D.Log(name + " can't move forward since there are occupants there: ");
				foreach(var occupant in newTile.GetOccupants()) {
					//D.Log(occupant.name);
					if(occupant is Pawn) {
						var otherPawn = (occupant as Pawn);
						if(otherPawn.team != this.team && !otherPawn.dead) {
							otherPawn.GetHit();
							PlaySound ("FishAttack");
						}
					}
				}
			}
			else {
				position = newPos;
			}
			
			moveNr++;
		}
		
		private void GetHit() {
			dead = true;
		}
		
		[EditableInEditor]
		public int moveNr {
			get {
                return CELL_moveNr.data;
			}
			set {
                CELL_moveNr.data = value;
			}
		}
		
		[EditableInEditor]
		public bool dead {
			get {
                return CELL_dead.data;
			}
			set {
                CELL_dead.data = value;
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
		
		[EditableInEditor]
		public int team {
			get {
                return CELL_team.data;
			}
			set {
                CELL_team.data = value;
			}
		}
		
		public override Program masterProgram {
			get {
				if(_program == null) {
					_program = EnsureProgram("MasterProgram", masterProgramName);
					var functionDefs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Pawn)));
					functionDefs.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = functionDefs;
				}
				_program.executionsPerFrame = 10;
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}
		
		[ShowInEditor]
		public WorldCoordinate startPosition {
			get {
				return CELL_startPosition.data;
			}
			set {
				CELL_startPosition.data = value;
			}
		}
		
		[ShowInEditor]
		public Direction startRotation {
			get {
				return CELL_startRotation.data;
			}
			set {
				CELL_startRotation.data = value;
			}
		}

		public override void OnPutDown ()
		{
			D.Log (name + " will start program when put down!");
			masterProgram.Start ();
		}
	}
}