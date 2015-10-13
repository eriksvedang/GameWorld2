using System;
using TingTing;
using RelayLib;
using GameTypes;
using System.Collections.Generic;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class SendPipe : MimanTing
	{
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_stuffName; // the ting being sent through the pipe

		Program _program;

		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "DefaultPipe");
			CELL_stuffName = EnsureCell("stuffName", "");
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
				return "send pipe";
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
					var defs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(SendPipe)));
					defs.AddRange (FunctionDefinitionCreator.CreateDefinitions (new ConnectionAPI (this, _tingRunner, masterProgram), typeof(ConnectionAPI)));
					_program.FunctionDefinitions = defs;
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
		}

		public void PutStuffIntoIt(Ting pStuff) 
		{
			pStuff.isBeingHeld = false;
			pStuff.position = position;
			stuff = pStuff;
			masterProgram.Start();
		}

		public string stuffName {
			get {
				return CELL_stuffName.data;
			}
			set {
				CELL_stuffName.data = value;
			}
		}

		public Ting stuff {
			get {
				return _tingRunner.GetTingUnsafe(CELL_stuffName.data);
			}
			set {
				if(value == null) {
					CELL_stuffName.data = "";
				} else {
					CELL_stuffName.data = value.name;
				}
			}
		}

		[SprakAPI("Send something to another (connected) thing")]
		public void API_Send(float connection)
		{
			if(stuff == null) throw new Error("Nothing to send");
			if(connection < 0) throw new Error("Connection id too low: " + connection);
			if(connection >= connectedTings.Length) throw new Error("Connection id too high: " + connection);

			var target = connectedTings[(int)connection];

			if(target == null) {
				throw new Error("Invalid target");
			}
			else if(target is SendPipe) {
				var targetPipe = target as SendPipe;
				targetPipe.stuff = stuff;
			}

			var pos = target.position;
			stuff.position = pos;
			stuff = null;
		}

		[SprakAPI("Get a random number between 0.0 and 1.0")]
		public float API_Random ()
		{
			return Randomizer.GetValue (0f, 1f);
		}
	}
}

