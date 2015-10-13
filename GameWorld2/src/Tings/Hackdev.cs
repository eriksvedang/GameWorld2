using System;
using System.Collections.Generic;
using GameTypes;
using RelayLib;
using GameWorld2;
using TingTing;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Hackdev : MimanTing
	{
		public static new string TABLE_NAME = "Tings_Hackdevs";
		ValueEntry<string> CELL_programName;
		ValueEntry<int> CELL_level;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "BasicHackdev");
			CELL_level = EnsureCell("level", 0);
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
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
				return name; // "modifier";
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
					var defs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Hackdev)));
					defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new ConnectionAPI (this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = defs;
				}
				return _program;
			}
		}

		public override bool CanInteractWith (Ting pTingToInteractWith)
		{
			return pTingToInteractWith is SendPipe;
		}

		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}
		
		[EditableInEditor]
		public int level {
			get {
				return CELL_level.data;
			}
			set {
				CELL_level.data = value;
			}
		}

		#if DEBUG
		[SprakAPI("Log")]
		public void API_Log(string text)
		{
			D.Log("LOG: " + text);
		}
		#endif

	}
}
