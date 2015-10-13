using System;
using System.Collections.Generic;
using GameWorld2;
using RelayLib;
using GameTypes;
using TingTing;
using GrimmLib;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class FuseBox : MimanTing
	{
		public static new string TABLE_NAME = "Tings_FuseBoxes";
		ValueEntry<string> CELL_programName;
		
		Program _program;
		Character _user;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Electricity");
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
				return "fuse box";
			}
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.Up * 2,
					localPoint + IntPoint.Left * 2,
					localPoint + IntPoint.Right * 2,
					localPoint + IntPoint.Down * 2,
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
					var defs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(FuseBox)));
					defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new ConnectionAPI (this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = defs;
					_program.maxExecutionTime = 5f;
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { 
				logger.Log("There was a problem generating the master program"); 
			}
			else {
				masterProgram.nameOfOwner = name;
			}
		}
		
		public void BeInspected(Character pCharacter) {
			_user = pCharacter;
			masterProgram.Start();
		}
		
		[SprakAPI("Use with caution")]
		public void API_Slurp ()
		{
			if (_user != null) {
				_user.SlurpIntoInternet(this);
			} else {
				//D.Log ("No one to slurp into " + name);
			}
		}

		[SprakAPI("Get the name of the fuse box")]
		public string API_GetName ()
		{
			return name;
		}

		[SprakAPI("Say something")]
		public void API_Say (string text)
		{
			Say (text, "");
		}

#if DEBUG
		[SprakAPI("Log something")]
		public void API_Log (string text)
		{
			D.Log(text);
		}
#endif
	}
}

