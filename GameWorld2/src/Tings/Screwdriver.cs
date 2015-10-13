using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;

namespace GameWorld2
{
	public class Screwdriver : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Screwdriver";
		ValueEntry<string> CELL_programName;
		
		Program _program;
		//Character _user;

		Computer _computerTarget;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Screwdriver");
		}

		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();

			if (masterProgramName == "BlankSlate") {
				masterProgramName = "Screwdriver";
			}
		}

		public override bool canBePickedUp {
			get {
				return true;
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}
		
		public override string tooltipName {
			get {
				return "screwdriver";
			}
		}
		
		public override string verbDescription {
			get {
				return "use";
			}
		}

		public void UseOnComputer(Computer pComputer) {
			_computerTarget = pComputer;
			masterProgram.Start ();
		}

		[SprakAPI("Set the speed of the computer you use the screwdriver on", "Mhz (max 500)")]
		public void API_SetMhz(float mhz)
		{
			if (_computerTarget != null && _computerTarget.masterProgram != null) {
				if (mhz < 0)
					mhz = 0;
				if (mhz > 500)
					mhz = 500;
				_computerTarget.mhz = (int)mhz;
			} else {
				throw new Error ("No computer found.");
			}
		}

		[SprakAPI("Set the maximum execution time for the computer", "-2 = infinite, -1 = default, 0+ = time in seconds")]
		public void API_SetMaxTime(float maxTime)
		{
			if (_computerTarget != null && _computerTarget.masterProgram != null) {
				_computerTarget.maxExecutionTime = (int)maxTime;
			} else {
				throw new Error ("No computer found.");
			}
		}

		[SprakAPI("Enable an API", "The name of the API")]
		public void API_EnableAPI(string name)
		{
			if (_computerTarget != null && _computerTarget.masterProgram != null) {
				switch (name.ToLower()) {
				case "internet":
					_computerTarget.hasInternetAPI = true;
					break;
				case "arcade":
					_computerTarget.hasArcadeMachineAPI = true;
					break;
				case "floppy":
					_computerTarget.hasFloppyAPI = true;
					break;
				case "memory":
					_computerTarget.hasMemoryAPI = true;
					break;
				case "door":
					_computerTarget.hasDoorAPI = true;
					break;
				default:
					throw new Error ("No API with name '" + name + "' found.");
				}
				_computerTarget.RemovePrograms (); // force regeneration of api
			} else {
				throw new Error ("No computer found.");
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Screwdriver)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove || pTingToInteractWith is Computer;
		}
	}
}

