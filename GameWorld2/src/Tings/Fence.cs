using System;
using TingTing;
using RelayLib;
using GameTypes;
using System.Collections.Generic;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Fence : MimanTing
	{
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_userName;
		ValueEntry<int> CELL_goalPointIndex;

		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "DefaultFence");
			CELL_userName = EnsureCell("userName", "");
			CELL_goalPointIndex = EnsureCell("goalPointIndex", 0);
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		public override void Init ()
		{
			base.Init ();

			PointTileNode tileA = room.GetTile(interactionPoints[0]);
			PointTileNode tileB = room.GetTile(interactionPoints[1]);

			tileA.teleportTarget = tile;
			tileB.teleportTarget = tile;

			this.tile.RemoveAllLinks ();
		}

		public override IntPoint[] interactionPointsTryTheseFirst {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.DirectionToIntPoint(direction) * 2, 
					localPoint + IntPoint.DirectionToIntPoint(direction) * -2
				};
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.DirectionToIntPoint(direction) * 2, 
					localPoint + IntPoint.DirectionToIntPoint(direction) * -2
				};
			}
		}
		
		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string verbDescription {
			get {
				return "try to walk through";
			}
		}
		
		public override string tooltipName {
			get {
				return "fence";
			}
		}

		[EditableInEditor]
		public int goalPointIndex {
			get {
				return CELL_goalPointIndex.data;
			}
			set {
				CELL_goalPointIndex.data = value;
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
		public string userName {
			get {
				return CELL_userName.data;
			}
			set {
				CELL_userName.data = value;
			}
		}

		public Character user {
			get {
				if(userName == "") {
					return null;
				} else {
					return _tingRunner.GetTingUnsafe(userName) as Character;
				}
			}
			set {
				if(value == null) {
					userName = "";
				} else {
					userName = value.name;
				}
			}
		}

		public WorldCoordinate goalPosition {
			get {
				return new WorldCoordinate(room.name, interactionPoints[goalPointIndex]);
			}
		}
		
		public override Program masterProgram {
			get {
				if(_program == null) {
					_program = EnsureProgram("MasterProgram", masterProgramName);
					var defs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Fence)));
					defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new ConnectionAPI (this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = defs;
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}

		public void StartedWalkingThrough(Character pCharacter) {
			if (pCharacter == null) {
				D.Log ("pCharacter argument to StartedWalkingThrough() for fence " + name + " is null!");
				return;
			}

			user = pCharacter;

			if(user.localPoint == interactionPoints[0]) {
				goalPointIndex = 1;
			} else {
				goalPointIndex = 0;
			}

			//D.Log (pCharacter.name + " started walking through fence " + name);
			masterProgram.Start();

			if(masterProgram.ContainsErrors()) {
				API_Grill();
			}
		}

		[SprakAPI("Stops the person to walk through the fence")]
		public void API_Grill()
		{
			D.Log("Grill!");
			if(user != null) {
				_dialogueRunner.EventHappened (name + "_grill_" + user.name);
				user.StopAction();
			}
		}

		[SprakAPI("Get the name of the person walking through fence")]
		public string API_GetUser()
		{
			if(user == null) {
				return "";
			}
			return user.name;
		}
		
		[SprakAPI("Is the user carrying a modifier?")]
		public bool API_UserHasModifier()
		{
			if(user == null) {
				return false;
			}
			return user.hasHackdev;
		}

		[SprakAPI("Say something")]
		public void API_Say(string text)
		{
			//D.Log("Fence '" + name + "' is saying: " + text);
			Say(text, "");
		}

		public override int securityLevel {
			get {
				return 0; // TODO: should be 1
			}
		}
	}
}

