using System;
using TingTing;
using RelayLib;
using GameTypes;
using ProgrammingLanguageNr1;
using System.Collections.Generic;

namespace GameWorld2
{
	public class Tram : Vehicle
	{
		ValueEntry<string> CELL_programName;
		ValueEntry<int> CELL_doorForwardOffset;
		ValueEntry<int> CELL_doorSideOffset;
		ValueEntry<int> CELL_doorRotationOffset;

		Program _program;

		protected override void SetupCells ()
		{
			base.SetupCells ();

			CELL_programName = EnsureCell("masterProgramName", "DefaultTram");
			CELL_doorForwardOffset = EnsureCell("doorForwardOffset", 4);
			CELL_doorSideOffset = EnsureCell("doorSideOffset", 2);
			CELL_doorRotationOffset = EnsureCell("doorRotationOffset", 90);
		}

		[ShowInEditor]
		public override IntPoint movingDoorPositionOffset {
			get {
				var offset = localPoint +
				             IntPoint.DirectionToIntPoint (direction) * doorForwardOffset
				             +
				             IntPoint.DirectionToIntPoint (IntPoint.Turn (direction, 90)) * doorSideOffset;

				return offset;
			}
		}
		
		public override int movingDoorRotationOffset {
			get {
				return doorRotationOffset;
			}
		}

		public override bool canBePickedUp {
			get {
				return false;
			}
		}

		public override string verbDescription {
			get {
				return "enter";
			}
		}

		public override string tooltipName {
			get {
				return "tram";
			}
		}

		[SprakAPI("Set the speed of the tram")]
		public void API_SetSpeed(float speed) {
			logger.Log("Speed of " + name + " was set to " + speed);
			this.speed = speed;
		}

		[SprakAPI("Get the next node in the tram track system")]
		public string API_GetNextNavNode() {
			return nextNavNodeName;
		}

		[SprakAPI("Is the tram at a station?")]
		public bool API_IsAtStation() {
			if(currentNavNode == null) {
				D.Log ("Warning, current nav node for " + name + " is null!");
				return false;
			}
			return currentNavNode.isStation; // && distance < speed; // gives a couple of frames where the tram is at station
		}

		[SprakAPI("Turn left")]
		public void API_TurnLeft() {
			turning = VehicleTurningDirection.LEFT;
		}

		[SprakAPI("Turn right")]
		public void API_TurnRight() {
			turning = VehicleTurningDirection.RIGHT;
		}

		[SprakAPI("Go straight forward")]
		public void API_DoNotTurn() {
			turning = VehicleTurningDirection.FORWARD;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Tram)));
				}
				return _program;
			}
		}

		[EditableInEditor]
		public int doorForwardOffset {
			get {
				return CELL_doorForwardOffset.data;
			}
			set {
				CELL_doorForwardOffset.data = value;
			}
		}

		[EditableInEditor]
		public int doorSideOffset {
			get {
				return CELL_doorSideOffset.data;
			}
			set {
				CELL_doorSideOffset.data = value;
			}
		}

		[EditableInEditor]
		public int doorRotationOffset {
			get {
				return CELL_doorRotationOffset.data;
			}
			set {
				CELL_doorRotationOffset.data = value;
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
	}
}
