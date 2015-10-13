using System;
using System.Collections.Generic;
using GameTypes;
using RelayLib;
using GameWorld2;
using TingTing;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class TrashCan : MimanTing
	{
		public static new string TABLE_NAME = "Tings_TrashCans";
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_currentTrashName;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "TrashCan");
			CELL_currentTrashName = EnsureCell("currentTrash", "");
		}

		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();

			if (masterProgramName == "" || masterProgramName == "BlankSlate") {
				masterProgramName = "TrashCan";
			}
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string verbDescription {
			get {
				return "inspect";
			}
		}
		
		public override string tooltipName {
			get {
				return "trash can";
			}
		}

		public void Throw(MimanTing pTing) {
			currentTrash = pTing;
			masterProgram.Start ();
		}

		public Ting currentTrash {
			get {
				if(CELL_currentTrashName.data == "") {
					return null;
				} else {
					return _tingRunner.GetTingUnsafe(CELL_currentTrashName.data);
				}
			}
			set {
				if(value == null) {
					CELL_currentTrashName.data = "";
				} else {
					CELL_currentTrashName.data = value.name;
				}
			}
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		[SprakAPI("Move the current trash to a room")]
		public void API_MoveToRoom (string roomName)
		{
			if(currentTrash == null) {
				Say("No current trash", "");
				return;
			}
				
			currentTrash.isBeingHeld = false;

			var targetRoom = _roomRunner.GetRoomUnsafe(roomName);

			if (targetRoom == null) {
				throw new Error ("Can't find a room called " + roomName);
			}

			var tilePoints = targetRoom.points;
			PointTileNode freeTileNode = null;

			foreach(var p in tilePoints) {
				var tile = targetRoom.GetTile(p);
				if(!tile.HasOccupants()) {
					freeTileNode = tile;
					break;
				}
			}

			if(freeTileNode != null) {
				currentTrash.position = freeTileNode.position;
			}
			else {
				Say("Can't throw away thing, " + roomName + " is full!", "");
			}
		}

		[SprakAPI("Delete the current trash (irreversibly)")]
		public void API_Delete ()
		{
			if(currentTrash == null) {
				Say("No current trash", "");
				return;
			}

			_tingRunner.RemoveTing(currentTrash.name);
		}

		[SprakAPI("Check if the current trash is a specific type")]
		public bool API_IsType (string type)
		{
			return API_GetType ().ToLower() == type.ToLower();
		}

		[SprakAPI("Get the type of the current trash")]
		public string API_GetType ()
		{
			if(currentTrash == null) {
				Say("No current trash", "");
				return "";
			}

			string type = currentTrash.GetType().Name.ToLower();

			if (type == "hackdev") {
				type = "modifier";
			}

			return type;
		}

		[SprakAPI("Say something")]
		public void API_Say (string text)
		{
			Say (text, "");
		}

		[SprakAPI("Sleep", "seconds to sleep")]
		public void API_Sleep (float time)
		{
			masterProgram.sleepTimer = time;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(TrashCan)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }

			currentTrash = null;
		}
	}
}

