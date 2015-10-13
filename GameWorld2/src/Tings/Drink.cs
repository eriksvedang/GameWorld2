using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class Drink : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Drinks";
		ValueEntry<float> CELL_ammount;
		ValueEntry<string> CELL_liquidType;
		ValueEntry<string> CELL_programName;
		
		Program _program;
		Character _drinker; // TODO: Byt ut mot en riktig cell!
		
		protected override void SetupCells()
		{
			base.SetupCells ();
			CELL_ammount = EnsureCell ("ammount", 100f);
			CELL_liquidType = EnsureCell ("liquidType", "beer");
			CELL_programName = EnsureCell ("masterProgramName", "FolkBeer");
		}
		
		[EditableInEditor]
		public float amount
        {
			get
            {
                return CELL_ammount.data;
			}
			set
            {
				CELL_ammount.data = value;
				if(amount < 0f) {
					amount = 0;
				}
				else if(amount > 99999f || float.IsInfinity(amount)) {
					amount = 99999f;
				}
			}
		}
		
		[EditableInEditor]
		public string liquidType
        {
			get
            {
                return CELL_liquidType.data;
			}
			set
            {
				CELL_liquidType.data = value;
			}
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string tooltipName {
			get {
				return liquidType + (amount <= 0f ? " (empty)" : " (" + amount + "%)"); // + " (" + amount + "% full)";
			}
		}
		
		public override string verbDescription {
			get {
				return "drink";
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}
		
		public override void FixBeforeSaving ()
		{
			if(name.ToLower().Contains("coffee") || name.ToLower().Contains("cup")) {
				masterProgramName = "Coffee";
				liquidType = "coffee";
			}
			else if(name.ToLower().Contains("soda")) {
				masterProgramName = "WellspringSoda";
				liquidType = "Wellspring soda";
			}
			else if(name.ToLower().Contains("cola") || name.ToLower().Contains("coke")) {
				masterProgramName = "WellspringSoda";
				liquidType = "cola";
			}
			else if(name.ToLower().Contains("beer")) {
				masterProgramName = "FolkBeer";
				liquidType = "beer";
			}
			else if(name.ToLower().Contains("booze")) {
				masterProgramName = "AlcoholicDrink";
				liquidType = "booze";
			}
			else if(name.ToLower().Contains("margherita") ||
			        name.ToLower().Contains("longisland") ||
			        name.ToLower().Contains("bloodymary") ||
			        name.ToLower().Contains("drymartini")) 
			{
				masterProgramName = "AlcoholicDrink";
				liquidType = "drink";
			}
			else if(name.ToLower().Contains("water") || name.ToLower().Contains("glass")) {
				masterProgramName = "Water";
				liquidType = "water";
			}
		}

		private float CalculateEffect(float d)
		{
			if(d > 0 && d > amount) return amount;
			else if (d < 0 && d < -amount) return -amount;
			else return d;
		}
		
		
		[SprakAPI("Change the speed of the drinker", "amount")]
		public void API_Speed(float d)
		{
			if(_drinker == null) return;

			float effect = CalculateEffect(d);
			_drinker.walkSpeed += effect / 25.0f;
			if(_drinker.walkSpeed < 2f) {
				_drinker.walkSpeed = 2f;
			}
			else if(_drinker.walkSpeed > 8f) {
				_drinker.walkSpeed = 8f;
			}
			amount -= Math.Abs(effect);
		}
		
		[SprakAPI("Change the charisma of the drinker", "amount")]
		public void API_Charisma(float d)
		{
			if(_drinker == null) return;

			float effect = CalculateEffect(d);
			_drinker.charisma += effect;
			amount -= Math.Abs(effect);
		}
		
		[SprakAPI("Change the smelliness of the drinker", "amount")]
		public void API_Smelliness(float d)
		{
			if(_drinker == null) return;

			float effect = CalculateEffect(d);
			_drinker.smelliness += effect;
			amount -= Math.Abs(effect);
		}
		
		[SprakAPI("Change the sleepiness of the drinker", "amount")]
		public void API_Sleepiness(float d)
		{
			if(_drinker == null) return;

			float effect = CalculateEffect(d);
			_drinker.sleepiness += effect; // * 0.1f;
			amount -= Math.Abs(effect);
		}
		
		[SprakAPI("Change the drunkenness of the drinker", "amount")]
		public void API_Drunkenness(float d)
		{
			if(_drinker == null) return;

			float effect = CalculateEffect(d);
			_drinker.drunkenness += effect;
			amount -= Math.Abs(effect);
		}

		[SprakAPI("Just lower the amount of liquid", "x")]
		public void API_Drink(float d)
		{
			if(_drinker == null) return;

			amount -= Math.Max(0, d);
		}

		[SprakAPI("Undocumented effect", "amount")]
		public void API_Corruption(float d)
		{
			if(_drinker == null) return;

			float effect = CalculateEffect(d);
			_drinker.corruption += effect;
			amount -= Math.Abs(effect);
		}

		[SprakAPI("Test if the drinker has a certain name")]
		public bool API_IsUser(string name)
		{
			if(_drinker != null) {
				return _drinker.name.ToLower() == name.ToLower();
			} else {
				return name == "";
			}
		}
		
		[SprakAPI("Get the name of the drinker")]
		public string API_GetUser()
		{
			if(_drinker != null) {
				return _drinker.name;
			} else {
				return "";
			}
		}
		
		[SprakAPI("Get the room of the drink")]
		public string API_GetRoom()
		{
			return room.name;
		}

		[SprakAPI("Say something")]
		public void API_Say(string something)
		{
			Say (something, "");
		}
		
		public void DrinkFrom(Character pDrinker)
		{
			_drinker = pDrinker;
			masterProgram.maxExecutionTime = 10f;
			masterProgram.Start();
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
					//D.Log("Creating program '" + masterProgramName + "' for Drink '" + name + "'.");
					_program = EnsureProgram("MasterProgram", masterProgramName);
					
					var functionDefs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Drink)));
					functionDefs.AddRange(FunctionDefinitionCreator.CreateDefinitions(new ConnectionAPI(this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = functionDefs;
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

			_drinker = null;
		}
		
		public override bool CanInteractWith (Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Sink || pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}
		
		public override void InteractWith (Ting pTingToInteractWith)
		{
			if(pTingToInteractWith is Sink) {
				amount = 100f;
			}
		}
	}
}

