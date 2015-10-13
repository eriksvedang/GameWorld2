using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class Radio : MimanTing, TingWithButton
	{
        public static new string TABLE_NAME = "Ting_Radios";
        
		ValueEntry<string> CELL_programName;
		ValueEntry<int> CELL_channel;
		
		Program _program;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "RadiOS");
			CELL_channel = EnsureCell("channel", 1);
		}
		
		public override void Update(float dt)
		{
			if(isPlaying) {
				audioTime += dt;
				if(audioTime > audioTotalLength) {
					audioTime = 0f;
					PlaySound(soundName);
				}
			}

			UpdateBubbleTimer();
		}

		public override bool autoUnregisterFromUpdate {
			get {
				return false;
				// nope, needs constant update
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		[SprakAPI("Print", "text")]
		public void API_Print(string text)
		{
			Say (text, "");
		}
		
		[SprakAPI("Make a blip sound")]
		public void API_Blip()
		{
			PlaySound("Blip");
			audioLoop = true;
		}
		
		[SprakAPI("Get the nr of the current channel")]
		public int API_GetChannel()
		{
			return channel;
		}
		
		[SprakAPI("Set the channel")]
		public void API_SetChannel(float newChannel)
		{
			channel = (int)newChannel;
			audioLoop = true;
		}

		[SprakAPI("")]
		public void API_TurnOffSound()
		{
			isPlaying = false;
		}

		[SprakAPI("")]
		public void API_TurnOnSound()
		{
			isPlaying = true;
		}
		
		public void PushButton(Ting pUser)
		{
			masterProgram.Start();
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "turn wheel";
			}
		}
		
		public override string tooltipName {
			get {
				return "radio";
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
		public int channel {
			get {
                return CELL_channel.data;
			}
			set {
                CELL_channel.data = value;
			}
		}
		
		public override Program masterProgram {
			get {
				if(_program == null) {
					_program = EnsureProgram("MasterProgram", masterProgramName);
					var defs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Radio)));
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
	}
}

