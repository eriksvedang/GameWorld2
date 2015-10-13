using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;
using System.Text;

namespace GameWorld2
{
	public class Stove : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Stove";
		ValueEntry<string> CELL_programName;
		ValueEntry<bool> CELL_on;
		
		Program _program;
		MimanTing _objectOnStove;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Stove");
			CELL_on = EnsureCell ("on", false);
		}

		public override void FixBeforeSaving ()
		{
			if (masterProgramName == "BlankSlate") {
				masterProgramName = "Stove";
			}
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { localPoint + IntPoint.DirectionToIntPoint(direction) * 2 };
			}
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
		
		public override string tooltipName {
			get {
				return "stove";
			}
		}
		
		public override string verbDescription {
			get {
				return "turn " + (on ? "off" : "on");
			}
		}

		public void Fry(Character pUser, MimanTing pTing) {
			if (on) {
				_objectOnStove = pTing;
				masterProgram.Start ();
			} else {
				_objectOnStove = null;
				_worldSettings.Notify (pUser.name, "Stove is not on");
			}
		}

		[SprakAPI("Remove all source code in the object on the stove")]
		public void API_ClearCode()
		{
			if (_objectOnStove != null && _objectOnStove.masterProgram != null) {
				_objectOnStove.masterProgram.sourceCodeContent = "";
				_objectOnStove.masterProgram.Compile ();
			} else {
				throw new Error ("No object on stove");
			}
		}

		[SprakAPI("Get the source code in the object on the stove")]
		public string API_GetCode()
		{
			if (_objectOnStove != null && _objectOnStove.masterProgram != null) {
				return _objectOnStove.masterProgram.sourceCodeContent;
			} else {
				throw new Error ("No object on stove");
			}
		}

		[SprakAPI("Add code to the end of the objects program", "The extra code to add")]
		public void API_AppendCode(string code)
		{
			if (_objectOnStove != null && _objectOnStove.masterProgram != null) {
				_objectOnStove.masterProgram.sourceCodeContent += "\n" + code + "\n";
				_objectOnStove.masterProgram.Compile ();
			} else {
				throw new Error ("No object on stove");
			}
		}

		[SprakAPI("Get a random value between 0 and 1")]
		public float API_Random()
		{
			return Randomizer.GetValue (0f, 1f);
		}

//		char[] letters = "abcdefghijklmnopqrstuvxyz1234567890-.*+/ ?".ToCharArray ();
//		string content = pTing.programs [0].sourceCodeContent;
//		StringBuilder sb = new StringBuilder ();
//		foreach (var c in content) {
//			if (Randomizer.OneIn (10)) {
//				sb.Append (Randomizer.RandNth (letters));
//			} else {
//				sb.Append (c);
//			}
//		}
//		pTing.programs [0].sourceCodeContent = sb.ToString ();

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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Stove)));
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
			return false;
		}
	}
}
