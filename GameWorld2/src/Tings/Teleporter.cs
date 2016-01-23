using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class Teleporter : MimanTing, TingWithButton
	{
		public static new string TABLE_NAME = "Ting_Teleporters";
        ValueEntry<string> CELL_programName;
		
		Program _program;
		Ting _user;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "TeleporterSoftware");
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}
		
		[SprakAPI("Print", "text")]
		public void API_Print(string text)
		{
			Say (text, "");
		}
		
		[SprakAPI("Get the coordinates of your current position")]
		public string API_Position()
		{
			if(_user != null) {
				return "x: " + _user.localPoint.x + ", y: " + _user.localPoint.y;
			}
			else {
				throw new Exception("User is null");
			}
		}
		
		[SprakAPI("Teleport to another position in the same room. Returns an error message as a string.", "x", "y")]
		public string API_Teleport(float x, float y)
		{
			if(IsAllowedToTeleport(_user as Character)) {
				WorldCoordinate coord = new WorldCoordinate(_user.room.name, (int)x, (int)y);
				PointTileNode tile = _user.room.GetTile(coord.localPosition);
				if(tile != null) {
					_user.position = coord;
					return "Success";
				}
				else {
					return "Can't move there";
				}
			}
			else {
				D.Log("Not allowed to teleport");
				return "Not allowed";
			}
		}

		[SprakAPI("Teleport to another position anywhere in the world, returns status.", "room", "x", "y")]
		public string API_SetWorldPosition(string room, float x, float y)
		{
			if(room.Contains("inventory") || room.Contains("locker")) {
				return "Can't teleport there";
			}

			if(IsAllowedToTeleport(_user as Character)) {
				if(_roomRunner.HasRoom(room)) {
					WorldCoordinate coord = new WorldCoordinate(room, (int)x, (int)y);
					_user.position = coord;
					return "Success";
				}
				else {
					return "Can't find room '" + room + "'";
				}
			}
			else {
				D.Log("Not allowed to set world position");
				return "Not allowed";
			}
		}

		bool IsAllowedToTeleport(Character pUser) {
			return pUser != null && !pUser.talking && pUser.conversationTarget == null;
		}

		public void PushButton(Ting pUser)
		{
			_user = pUser;
			D.Log(name + " was activated!");
			masterProgram.Start();
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "press button";
			}
		}
		
		public override string tooltipName {
			get {
				return "teleporting device";
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
					_program = EnsureProgram("MasterProgram", masterProgramName);
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Teleporter)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }

			_user = null;
		}
	}
}

