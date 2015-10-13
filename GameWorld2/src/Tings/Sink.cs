using System;
using System.Collections.Generic;
using GameTypes;
using RelayLib;
using GameWorld2;
using TingTing;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Sink : MimanTing
	{
		ValueEntry<string> CELL_programName;
		ValueEntry<bool> CELL_on;
		ValueEntry<string> CELL_drinkName;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "SimpleSink");
			CELL_on = EnsureCell("on", false);
			CELL_drinkName = EnsureCell("drinkName", "");
		}

		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();

			if (masterProgramName == "" || masterProgramName == "BlankSlate") {
				masterProgramName = "SimpleSink";
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

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}
		
		public override string verbDescription {
			get {
				return (on ? "turn off water in" : "turn on water in");
			}
		}
		
		public override string tooltipName {
			get {
				return "sink";
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.DirectionToIntPoint(direction) * 2,
				};
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
		public bool on {
			get {
				return CELL_on.data;
			}
			set {
				CELL_on.data = value;
			}
		}
		
		public override Program masterProgram {
			get {
				if(_program == null) {
					_program = EnsureProgram("MasterProgram", masterProgramName);
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Sink)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}
		
		public void Toggle ()
		{
			on = !on;
		}

		public Drink currentDrink {
			get {
				if(CELL_drinkName.data == "") {
					return null;
				} else {
					return _tingRunner.GetTing<Drink>(CELL_drinkName.data);
				}
			}
			set {
				if(value == null) {
					CELL_drinkName.data = "";
				} else {
					CELL_drinkName.data = value.name;
				}
			}
		}

		public void UseDrinkOnSink(Drink pDrink) {
			currentDrink = pDrink;
			masterProgram.Start ();
		}

		[SprakAPI("Set the liquid level of the drink", "The value")]
		public void API_SetLiquidAmount(float amount)
		{
			if (currentDrink != null) {
				currentDrink.amount = amount;
			}
		}

		[SprakAPI("Returns the name of the sink")]
		public string API_GetName()
		{
			return name;
		}

		[SprakAPI("Remove all code in the drink")]
		public void API_ClearCode()
		{
			if (currentDrink != null) {
				currentDrink.masterProgram.sourceCodeContent = "";
				currentDrink.masterProgram.Compile ();
			}
		}

		[SprakAPI("Add code to the end of the drink program", "The extra code to add")]
		public void API_AppendCode(string code)
		{
			if (currentDrink != null) {
				currentDrink.masterProgram.sourceCodeContent += "\n" + code + "\n";
				currentDrink.masterProgram.Compile ();
			}
		}
	}
}

