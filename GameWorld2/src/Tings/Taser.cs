using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class Taser : MimanTing, TingWithButton
	{
        public static new string TABLE_NAME = "Ting_Tasers";
        ValueEntry<string> CELL_programName;
		
		Program _program;
		//Ting _target;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Taser");
		}
		
		public void PushButton(Ting pUser)
		{
			dialogueLine = "";
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Character || pTingToInteractWith is Locker;
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
				return "taser";
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Extractor)));
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
