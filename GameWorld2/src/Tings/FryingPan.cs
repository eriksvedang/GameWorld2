using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class FryingPan : MimanTing
	{
        public static new string TABLE_NAME = "Ting_FryingPan";
		ValueEntry<string> CELL_programName;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "BlankSlate");
		}

		public override IntPoint[] interactionPoints {
			get {
				return base.interactionPoints;
			}
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string tooltipName {
			get {
				return "frying pan";
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override string verbDescription {
			get {
				return "use";
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Drug)));
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
			return pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}
	}
}
