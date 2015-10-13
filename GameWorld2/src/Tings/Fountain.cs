using System;
using GameTypes;
using RelayLib;
using TingTing;
using ProgrammingLanguageNr1;
using System.Collections.Generic;

namespace GameWorld2
{
	public class Fountain : MimanTing
	{
		ValueEntry<bool> CELL_on;
		ValueEntry<string> CELL_programName;
		
		Program _program;
		
		protected override void SetupCells ()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "BlankSlate");
			CELL_on = EnsureCell("on", true);
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.Up * 3,
					localPoint + IntPoint.Right * 3,
					localPoint + IntPoint.Left * 3,
					localPoint + IntPoint.Down * 3
				};
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
				return "admire";
			}
		}
		
		public override string tooltipName {
			get {
				return "fountain";
			}
		}
		
		[EditableInEditor]
		public bool on {
			get {
                return CELL_on.data;
			}
			set {
                CELL_on.data = value;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Lamp)));
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

