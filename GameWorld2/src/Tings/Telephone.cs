using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class Telephone : MimanTing
	{
		public static new string TABLE_NAME = "Ting_Telephones";
		ValueEntry<string> CELL_programName;
		ValueEntry<bool> CELL_ringing;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "BlankSlate");
			CELL_ringing = EnsureCell("ringing", false);
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
				return "answer";
			}
		}
		
		public override string tooltipName {
			get {
				return "telephone";
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
		public bool ringing {
			get {
				return CELL_ringing.data;
			}
			set {
				CELL_ringing.data = value;
			}
		}
		
		public override Program masterProgram {
			get {
				if(_program == null) {
					_program = EnsureProgram("MasterProgram", masterProgramName);
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Telephone)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}

		public void Use() {
			ringing = false;
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}
	}
}
