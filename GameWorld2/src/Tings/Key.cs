using System;
using System.Collections.Generic;
using GameTypes;
using RelayLib;
using GameWorld2;
using TingTing;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Key : MimanTing
	{
		public static new string TABLE_NAME = "Tings_Keys";
		ValueEntry<string> CELL_programName;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "BlankSlate");
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "use";
			}
		}
		
		public override string tooltipName {
			get {
				return masterProgramName;
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override string UseTingOnTingDescription (Ting pOtherTing)
		{
			if(pOtherTing is Door) {
				return ((pOtherTing as Door).isLocked ? "unlock" : "lock") + " door";
			} else {
				return base.UseTingOnTingDescription (pOtherTing);
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Key)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}
		
		public override bool CanInteractWith (Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Door || pTingToInteractWith is Locker || pTingToInteractWith is TrashCan;
		}
		
		public override void InteractWith (Ting pTingToInteractWith)
		{
			Door door = pTingToInteractWith as Door;
			
			if(door == null) {
				D.Log("The ting key is interacting with is not a door.");
				return;
			}
			
			actionOtherObject = door;
			masterProgram.executionsPerFrame = 50;
			masterProgram.maxExecutionTime = -1; // must be able to run indefinitely?
			masterProgram.Start();
		}
		
		[SprakAPI("Unlock, returns true on success")]
		public bool API_Unlock(float code)
		{
			Door door = actionOtherObject as Door;
			if(door == null) {
				D.Log("actionOtherObject is not a Door");
				return false;
			}
			
			return door.Unlock(code);
		}
		
		[SprakAPI("Lock, returns true on success")]
		public bool API_Lock(float code)
		{
			Door door = actionOtherObject as Door;
			if(door == null) {
				D.Log("actionOtherObject is not a Door");
				return false;
			}
			
			return door.Lock(code);
		}
		
		[SprakAPI("Lock or unlock depending on if the door is locked")]
		public bool API_Toggle(float code)
		{
			Door door = actionOtherObject as Door;
			if(door == null) {
				D.Log("actionOtherObject is not a Door");
				return false;
			}
			
			if(door.isLocked) {
				return door.Unlock(code);
			} else {
				return door.Lock(code);
			}

		}
	}
}

