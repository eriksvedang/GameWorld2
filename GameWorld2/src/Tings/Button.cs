using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class Button : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Buttons";
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_user;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Button");
			CELL_user = EnsureCell("user", "");
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public Character user {
			set {
				if(value != null) {
					CELL_user.data = value.name;
				}
				else {
					CELL_user.data = null;
				}
			}
			get {
				return _tingRunner.GetTingUnsafe(CELL_user.data) as Character;
			}
		}
		
		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string tooltipName {
			get {
				return "button";
			}
		}
		
		public override string verbDescription {
			get {
				return "push";
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint - IntPoint.DirectionToIntPoint (direction) * 1
				};
			}
		}

		public void Push(Character pUser)
		{
			user = pUser;
			masterProgram.Start();
		}
		
		[SprakAPI("Print", "text")]
		public void API_Print(string text)
		{
			Say (text, "");
		}

		[SprakAPI("Play a sound", "name")]
		public void API_PlaySound(string name)
		{
			PlaySound(name);
			audioLoop = false;
		}

		[SprakAPI("Get the name of the person using the button")]
		public string API_GetUser()
		{
			if(user != null) {
				return user.name;
			} else {
				return "";
			}
		}
		
		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
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
					var functionDefs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Button)));
					functionDefs.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = functionDefs;
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}
	}
}