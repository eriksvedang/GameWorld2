using System;
using TingTing;
using GameWorld2;
using GrimmLib;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class Floppy : MimanTing
	{
		ValueEntry<string> CELL_programName;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "BlankSlate");
		}

		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();

			if (masterProgramName == "BlankSlate") {
				if (room.name.Contains ("Ministry")) {
					masterProgramName = "MinistryData" + Randomizer.GetIntValue (0, 10);
				} else {
					masterProgramName = "DigitalTrash" + Randomizer.GetIntValue (0, 30);
				}
			}
		}

		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			/*var computer = pTingToInteractWith as Computer;

			if (computer != null && computer.hasFloppyAPI) {
				return true;
			}*/

			return pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override string tooltipName {
			get {
//				string src = programs [0].sourceCodeContent;
//				return src.Substring(0, 20) + (src.Length > 20 ? "..." : "");


				return masterProgramName; // + " (floppy)";

//				if (_tingRunner.GetTing<Character> (_worldSettings.avatarName).hasHackdev) {
//					return masterProgramName + " (floppy)";
//				} else {
//					return "floppy";
//				}
			}
		}
		
		public override string verbDescription {
			get {
				return "inspect";
			}
		}
		
		public override bool canBePickedUp {
			get {
				return true;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Floppy)));
					_program.compilationTurnedOn = false;
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

