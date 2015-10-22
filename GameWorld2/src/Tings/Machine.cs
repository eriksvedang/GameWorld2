using System;
using GameTypes;
using System.Collections.Generic;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Machine : MimanTing
	{
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_goodsName;
		
		Program _program;

		public Action onRunBlock;
		public Action onGoodsConverted;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "MachineA");
			CELL_goodsName = EnsureCell("goodsName", "");
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.DirectionToIntPoint(direction) * 2
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
		
		public IntPoint goodsPoint {
			get {
				return IntPoint.DirectionToIntPoint(direction) * 0;				
			}
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
				return "machine";
			}
		}
		
		[ShowInEditor()]
		public string goodsName {
			get {
				return CELL_goodsName.data;
			}
			set {
				CELL_goodsName.data = value;
			}
		}
		
		public Ting currentGoods {
			get {
				if(goodsName == "") {
					//D.Log ("Getting current goods, it was null");
					return null;
				} else {
					//D.Log ("Getting current goods, it was " + goodsName);
					return _tingRunner.GetTing(goodsName);
				}
			}
			set {
				if(value == null) {
					//D.Log ("Setting current goods to null");
					goodsName = "";
				} else {
					//D.Log ("Setting current goods to " + value);
					goodsName = value.name;
				}
			}
		}
		
		public void ProcessGoods(Goods pGoods) {
			currentGoods = pGoods;
			currentGoods.isBeingHeld = false;
			currentGoods.position = goodsPointInWorld;
			masterProgram.executionsPerFrame = 5;
			masterProgram.Start();
		}

		public override void Update (float dt)
		{
			if (currentGoods == null) {
				MimanTing goodsAtPosition = null;
				var occupants = room.GetTile (localPoint).GetOccupants ();
				foreach (var o in occupants) {
					if (o is Machine) {
						// ignore
					} else {
						goodsAtPosition = o as MimanTing;
						break;
					}
				}

				if (goodsAtPosition != null && !goodsAtPosition.isBeingHeld && !goodsAtPosition.isDeleted) {
					currentGoods = goodsAtPosition;
					masterProgram.executionsPerFrame = 10;
					masterProgram.Start ();
				}
			} else if (currentGoods.isBeingHeld) {
				D.Log ("is being held");
				RemovedGoods ();
			} else if (currentGoods.position != goodsPointInWorld) {
				D.Log ("different pos: " + currentGoods.position + " goods point: " + goodsPointInWorld);
				RemovedGoods ();
			} else {
				// Check for several goods at machine position
				/*var occupants = room.GetTile (localPoint).GetOccupants ();
				if (occupants.Length > 2) { // 1 for the machine, one for the goods that should be there
					foreach (var o in occupants) {
						if (o is Machine) {
							// ignore
						} else {
							if (o == currentGoods) {
								o.position = new WorldCoordinate (this.room.name, this.interactionPoints [0]);
								currentGoods = null;
								D.Log ("Pushed out current goods!");
								break;
							}
						}
					}
				}*/
			}
		}

		public override bool autoUnregisterFromUpdate {
			get {
				return false;
				// nope, needs constant update
			}
		}

		public WorldCoordinate goodsPointInWorld {
			get {
				return new WorldCoordinate(room.name, localPoint + goodsPoint);
			}
		}

		void RemovedGoods() {
			D.Log(currentGoods.name + " was removed from " + name);
			_program.StopAndReset();
			currentGoods = null;
		}
		
		char Improve(char c) {
			char better = (char)(c - Randomizer.GetIntValue(1, 6));
			if((int)better <= (int)'a') return 'a';
			return better;
		}

		void RunBlock ()
		{
			if (onRunBlock != null) {
				onRunBlock ();
			}
		}
		
		[SprakAPI("Refine the mineral at a specific position, can accidentally mess up other parts of the mineral chain")]
		public void API_Refine(float pos)
		{
			if(currentGoods == null) {
				D.Log ("No goods to process in " + name);
				//throw new Error("No goods to process");
				return;
			}

			var definitelyGoods = currentGoods as Goods;
			if (definitelyGoods == null) {
				//throw new Error ("Can only refine goods");
				return;
			}

			RunBlock ();
			
			char charAtPos = definitelyGoods.minerals[(int)pos];
			char improvedChar = Improve(charAtPos);
			
			//D.Log("improved " + charAtPos + " to " + improvedChar);

			definitelyGoods.minerals[(int)pos] = improvedChar;
			
			if(Randomizer.OneIn(7)) {
				definitelyGoods.minerals[(int)pos] = Randomizer.RandNth(badChars);
				//Randomizer.GetIntValue(0, definitelyGoods.minerals.Length)
			}

			masterProgram.sleepTimer = 2.0f;
		}

		static char[] badChars = new char[] {'u', 'v', 'w', 'x','y','z'};

		[SprakAPI("Get an overview of the minerals inside the goods")]
		public string API_Analyze()
		{
			if (currentGoods == null) {
				return "";
			}

			var definitelyGoods = currentGoods as Goods;
			if (definitelyGoods == null) {
				//throw new Error ("Can only analyze goods");
				return "Can only analyze goods";
			}

			return definitelyGoods.mineralsDisplayString;
		}

		[SprakAPI("Get an estimate of the purity of the goods")]
		public float API_Purity()
		{
			if (currentGoods == null) {
				return 0f;
			}

			var definitelyGoods = currentGoods as Goods;
			if (definitelyGoods == null) {
				//throw new Error ("Can only get the purity from goods");
				return 0f;
			}

			return definitelyGoods.GetPureness ();
		}
				
		[SprakAPI("Convert the goods into an object")]
		public string API_Convert()
		{
			if(currentGoods == null) {
				//throw new Error("No goods to process");
				return "No goods to process";
			}

			var definitelyGoods = currentGoods as Goods;
			if (definitelyGoods == null) {
				//throw new Error ("Can only convert goods");
				return "Can only convert goods";
			}

			RunBlock ();

			float purity = definitelyGoods.GetPureness ();

			if (onGoodsConverted != null) {
				onGoodsConverted ();
			}

			_tingRunner.RemoveTingAfterUpdate(currentGoods.name);
			currentGoods.isDeleted = true;
			currentGoods = null;

			masterProgram.sleepTimer = 2.0f;

			if (purity > 0.9f) {
				var modifier = _tingRunner.CreateTing<Hackdev> ("Modifier" + _worldSettings.tickNr, this.position, this.direction, "SmallHackdev");
				currentGoods = modifier;
				return modifier.name;
			} else if (purity > 0.7f) {
				var cube = _tingRunner.CreateTing<MysticalCube> ("MysticalCube" + _worldSettings.tickNr, this.position, this.direction, "MysticalCube");
				currentGoods = cube;
				return cube.name;
			} else if (purity > 0.5f) {
				var key = _tingRunner.CreateTing<Key> ("Key" + _worldSettings.tickNr, this.position, this.direction, "Old_Key");
				currentGoods = key;
				return key.name;
			} else if (purity > 0.25f) {
				var floppy = _tingRunner.CreateTing<Floppy> ("Floppy" + _worldSettings.tickNr, this.position, this.direction, "Diskette_Diskette" + Randomizer.GetIntValue(1, 10));
				currentGoods = floppy;
				return floppy.name;
			} else {
				var screwdriver = _tingRunner.CreateTing<Screwdriver> ("Screwdriver" + _worldSettings.tickNr, this.position, this.direction, "Screwdriver_Screwdriver");
				currentGoods = screwdriver;
				return screwdriver.name;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Machine)));
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

