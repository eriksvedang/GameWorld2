using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class VendingMachine : MimanTing
	{
		public static new string TABLE_NAME = "Ting_VendingMachine";
		ValueEntry<string> CELL_programName;
		//ValueEntry<string> CELL_cokeBottleName;
		
		Program _program;
		Character _user;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "VendingMachine");
			//CELL_cokeBottleName = EnsureCell("cokeBottleName", "");
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.DirectionToIntPoint (direction) * -2
				};
			}
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string tooltipName {
			get {
				return "vending machine";
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}
		
		public override string verbDescription {
			get {
				if(dispensedCoke == null) {
					return "press button on";
				} else {
					return "pick up coke from";
				}
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
					if (masterProgramName == "BlankSlate") {
						masterProgramName = "VendingMachine";
					}

					_program = EnsureProgram("MasterProgram", masterProgramName);
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(VendingMachine)));
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

		[SprakAPI("Get a random number between 0.0 and 1.0")]
		public float API_Random ()
		{
			return Randomizer.GetValue (0f, 1f);
		}

		[SprakAPI("Get the name of who is using the vending machine, if any")]
		public string API_GetUser ()
		{
			if (_user == null) {
				return "";
			} else {
				return _user.name;
			}
		}

		[SprakAPI("Get the name of the vending machine")]
		public string API_Name ()
		{
			return name;
		}

		[SprakAPI("Get the total time as a float")]
		public float API_Time()
		{
			return _worldSettings.totalWorldTime;
		}

		[SprakAPI("Create a drink", "The type of drink", "How much liquid the drink should have")]
		public void API_CreateDrink(string type, float liquidAmount) {

			string prefabName = "";
			string liquidType = "";
			string programName = "";

			switch (type.ToLower ()) {
			case "coke":
				prefabName = "Coke";
				liquidType = "coke";
				programName = "Coke";
				break;
			case "beer":
				prefabName = "Beer";
				liquidType = "beer";
				programName = "FolkBeer";
				break;
			case "coffee":
				prefabName = "CoffeeCup_CoffeeCup";
				liquidType = "coffee";
				programName = "CafeCoffee";
				break;
			case "margherita":
				prefabName = "Margherita_Margherita";
				liquidType = "drink";
				programName = "AlcoholicDrink";
				break;
			case "drymartini":
				prefabName = "DryMartini_DryMartini";
				liquidType = "drink";
				programName = "AlcoholicDrink";
				break;
			case "bloodymary":
				prefabName = "BloodyMary_BloodyMary";
				liquidType = "drink";
				programName = "AlcoholicDrink";
				break;
			case "longislandicetea":
				prefabName = "LongIslandIceTea_LongIslandIceTea";
				liquidType = "drink";
				programName = "AlcoholicDrink";
				break;
			default:
				throw new Error ("Can't create drink of type " + type);
			}

			int count = Behaviour_Sell.CountNrOfTingsWithPrefab(_tingRunner, prefabName);
			var newDrink = _tingRunner.CreateTingAfterUpdate<Drink>(prefabName + "_Drink_dispensed_" + count, position, GameTypes.Direction.DOWN, prefabName);
			newDrink.masterProgramName = programName;
			newDrink.liquidType = liquidType;
			newDrink.amount = liquidAmount;
			//newDrink.PrepareForBeingHacked (); // MAKES A NEW PROGRAM GET GENERATED WHICH CRASHES THE PROGRAM RUNNER SINCE THE COLLECTION IS MODIFIED WHILE RUNNING
		}

		public void PushCokeDispenserButton(Character pUser) {
			_user = pUser;
			if(dispensedCoke == null) {
				masterProgram.Start ();
			}
			else {
				Say("Can't dispense coke, tray is full", "");
			}
		}

		public Drink dispensedCoke {
			get {
				return tile.GetOccupantOfType<Drink>();
			}
		}
	}
}
