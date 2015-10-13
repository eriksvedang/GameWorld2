using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class Lamp : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Lamps";
        ValueEntry<string> CELL_programName;
		ValueEntry<Float3> CELL_color;
		ValueEntry<bool> CELL_on;
		
		Program _program;

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "StreetLight");
			CELL_color = EnsureCell("color", new Float3(0, 0, 0));
			CELL_on = EnsureCell("on", true);
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}

		[SprakAPI("Set the color of the lamp", "red", "green", "blue")]
		public void API_SetColor(float r, float g, float b)
		{
			logger.Log("Called API_SetColor with arguments " + r + ", " + g + ", " + b);
			color = new Float3(r, g, b);
			return;
		}
		
		[SprakAPI("Returns true if it is night")]
		public bool API_IsNight()
		{
			bool itIsNight = (gameClock.hours > 18 || gameClock.hours < 6);
			return itIsNight;
		}
		
		static System.Random s_random = new Random(DateTime.Today.Millisecond * DateTime.Today.Second * DateTime.Today.Minute * DateTime.Today.Hour );
		
		[SprakAPI("Returns a random value between 0.0 and 1.0")]
		public float API_Random()
		{
			return (float)s_random.NextDouble();
		}
		
		[SprakAPI("Returns the time since day 0, whatever that means (in seconds)")]
		public float API_Time()
		{
			return gameClock.totalSeconds;
		}
		
		[SprakAPI("The sinus function", "x")]
		public float API_Sin(float x)
		{
			return (float)Math.Sin(x);
		}
		
		[SprakAPI("Say something")]
		public void API_Say (string text)
		{
			Say (text, "");
		}

		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
		}

		[SprakAPI("Turn on the lamp", "")]
		public void API_TurnOn ()
		{
			on = true;
		}

		[SprakAPI("Turn off the lamp", "")]
		public void API_TurnOff ()
		{
			on = false;
		}

		[SprakAPI("Play a sound", "name")]
		public void API_PlaySound(string name)
		{
			PlaySound(name);
			audioLoop = false;
		}
		
		public void Kick()
		{
			//D.Log(name + " was kicked!");
			masterProgram.Start();
			_dialogueRunner.EventHappened ("LampWasKicked");
		}
		
		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string verbDescription {
			get {
				return "kick";
			}
		}
		
		public override string tooltipName {
			get {
				return "lamp";
			}
		}
		
		[ShowInEditor]
		public Float3 color {
			get {
                return CELL_color.data;
			}
			set {
                CELL_color.data = value;
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Lamp)));
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
		}
	}
}

