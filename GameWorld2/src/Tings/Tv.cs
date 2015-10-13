using System;
using System.Collections.Generic;
using GameTypes;
using RelayLib;
using GameWorld2;
using TingTing;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Tv : MimanTing
	{
		ValueEntry<string> CELL_programName;
		ValueEntry<string> CELL_channelName;
		ValueEntry<bool> CELL_on;

		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "TvProgram");
			CELL_channelName = EnsureCell ("channelName", "Bergman");
			CELL_on = EnsureCell ("on", true);
		}

		public override void FixBeforeSaving ()
		{
			base.FixBeforeSaving ();

			if (masterProgramName == "BlankSlate") {
				masterProgramName = "TvProgram";
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
				return "turn " + (on ? "off" : "on");
			}
		}
		
		public override string tooltipName {
			get {
				return "tv";
			}
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.DirectionToIntPoint (direction) * 1,
					localPoint + IntPoint.Up * 2,
					localPoint + IntPoint.Left * 2,
					localPoint + IntPoint.Right * 2,
					localPoint + IntPoint.Down * 2,
				};
			}
		}

		public void Flip ()
		{
			on = !on;
			if (on) {
				masterProgram.Start ();
			}
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		[SprakAPI("Print", "text")]
		public void API_Print(string text)
		{
			Say (text, "");
		}

		[SprakAPI("Get the name of the current channel")]
		public string API_GetChannel()
		{
			return channelName;
		}

		[SprakAPI("Set the channel")]
		public void API_SetChannel(string newChannelName)
		{
			channelName = newChannelName;
		}

		[SprakAPI("Get the current hour")]
		public int API_GetHour()
		{
			return _worldSettings.gameTimeClock.hours;
		}

		[EditableInEditor]
		public string channelName {
			get {
				return CELL_channelName.data;
			}
			set {
				CELL_channelName.data = value;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Tv)));
				}
				return _program;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating the master program"); }
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
	}
}

