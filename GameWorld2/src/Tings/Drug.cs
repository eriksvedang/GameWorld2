using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class Drug : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Drugs";
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_drugType;
		ValueEntry<int> CELL_charges;
		
		Program _program;
		Character _user;

		public delegate void OnDrugUse();
		public OnDrugUse onDrugUse;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "Citnap");
			CELL_drugType = EnsureCell("drugType", "drug");
			CELL_charges = EnsureCell("charges", 1);
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override void FixBeforeSaving ()
		{
			if(name.ToLower().Contains("bun")) {
				masterProgramName = "Bun";
				drugType = "Bun";
			}
			else if(name.ToLower().Contains("baguette")) {
				masterProgramName = "Baguette";
				drugType = "baguette";
			}
			else if(name.ToLower().Contains("cig")) {
				masterProgramName = "Cigarette";
				drugType = "cigarette";
				charges = 4;
			}
			else if(name.ToLower().Contains("snus")) {
				masterProgramName = "Snus";
				drugType = "snus";
				charges = Randomizer.GetIntValue(6, 10);
			}
			else if(name.ToLower().Contains("slip")) {
				masterProgramName = "Citnap";
				drugType = "drug";
			}
		}

		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string tooltipName {
			get {
				return drugType;
			}
		}
		
		public override string verbDescription {
			get {
				return "eat";
			}
		}

		[EditableInEditor]
		public string drugType
		{
			get
			{
				return CELL_drugType.data;
			}
			set
			{
				CELL_drugType.data = value;
			}
		}

		[EditableInEditor]
		public int charges
		{
			get
			{
				return CELL_charges.data;
			}
			set
			{
				CELL_charges.data = value;
			}
		}

		static string[] trippyAnims = new string[] {
			"Shrug", "Walk", "TalkingInTelephone"
		};

		[SprakAPI("Trippy")]
		public void API_Trippy()
		{
			if (_user == null) {
				return;
			}
			_user.StopAction();
			_user.StartAction(trippyAnims[Randomizer.GetIntValue(0, trippyAnims.Length)], null, 9999f, 3f);
		}

		[SprakAPI("Turn left")]
		public void API_TurnLeft()
		{
			if (_user == null) {
				return;
			}
			_user.TurnLeft();
		}

		[SprakAPI("Turn right")]
		public void API_TurnRight()
		{
			if (_user == null) {
				return;
			}
			_user.TurnRight();
		}

		[SprakAPI("Make time appear to go faster")]
		public void API_FastForward()
		{
			if (_user == null) {
				return;
			}
			_dialogueRunner.EventHappened (_user.name + "_TookFastForwardDrug");
		}

		[SprakAPI("Move forward one step")]
		public void API_Move()
		{
			var newPos = new WorldCoordinate(room.name, localPoint + IntPoint.DirectionToIntPoint(direction));
			position = newPos;
		}

		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
		}

		[SprakAPI("Get a quick energy boost")]
		public void API_QuickBoost ()
		{
			_user.sleepiness -= 10f;
		}

		public void Take(Character pUser)
		{
			_user = pUser;

			charges -= 1;
			masterProgram.maxExecutionTime = 7f;
			masterProgram.Start();

			if(onDrugUse != null) {
				onDrugUse();
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

			_user = null;
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}
	}
}