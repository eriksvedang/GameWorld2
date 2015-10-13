using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;

namespace GameWorld2
{
	public interface TingWithButton {
		void PushButton(Ting pUser);
	}
	
	public class MysticalCube : MimanTing, TingWithButton
	{
        public static new string TABLE_NAME = "Ting_MysicalCubes";
        ValueEntry<int> CELL_mysteryLevel;
        ValueEntry<string> CELL_onInteractionProgramName;
		ValueEntry<Float3> CELL_color;

		Program _program;

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		protected override void SetupCells()
		{
			base.SetupCells();
            CELL_mysteryLevel = EnsureCell("mysteryLevel", 0);
            CELL_onInteractionProgramName = EnsureCell("onInteractionProgramName", "TheCube");
			CELL_color = EnsureCell("color", new Float3(0, 0, 0));
		}

		[SprakAPI("Increase mystery level by any amount", "The amount")]
		public void API_IncreaseMysteryLevel(float amount)
		{
			logger.Log("Called API_IncreaseMysteryLevel with argument " + amount);
			mysteryLevel += (int)amount;
		}

		[SprakAPI("Set the color of the cube", "Amount of red (0 - 1)", "Amount of green (0 - 1)", "Amount of blue (0 - 1)")]
		public void API_SetColor(float r, float g, float b)
		{
			logger.Log("Called API_SetColor with arguments " + r + ", " + g + ", " + b);
			color = new Float3(r, g, b);
		}
		
		static System.Random s_random = new Random(DateTime.Today.Millisecond * DateTime.Today.Second * DateTime.Today.Minute * DateTime.Today.Hour );

		[SprakAPI("Get a random value between 0 and 1", "")]
		public float API_Random()
		{
			float randomNr = (float)s_random.NextDouble();
			//logger.Log("Called API_random and returned " + randomNr);
			return randomNr;
		}

		[SprakAPI("Play a sound", "name")]
		public void API_PlaySound(string name)
		{
			PlaySound(name);
			audioLoop = false;
		}

		[SprakAPI("The sinus function", "x")]
		public float API_Sin(float x)
		{
			return (float)Math.Sin(x);
		}

		[SprakAPI("The cosinus function", "x")]
		public float API_Cos(float x)
		{
			return (float)Math.Cos(x);
		}

		[SprakAPI("Get the total time as a float")]
		public float API_Time()
		{
			return _worldSettings.totalWorldTime;
		}

		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
		}

		public void PushButton(Ting pUser)
		{
			logger.Log("PushMysteriousButton()");
			masterProgram.Start();
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "push button";
			}
		}
		
		public override string tooltipName {
			get {
				return "mystical Cube";
			}
		}
		
		[ShowInEditor]
		public int mysteryLevel {
			get {
                return CELL_mysteryLevel.data;
			}
			set {
                CELL_mysteryLevel.data = value;
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
		public string onInteractionSourceCodeName {
			get {
                return CELL_onInteractionProgramName.data;
			}
			set {
                CELL_onInteractionProgramName.data = value;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(masterProgram == null) { logger.Log("There was a problem generating an onInteractionProgram"); }
		}
	
		public override bool CanInteractWith (Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Locker;
		}

		public override Program masterProgram {
			get {
				_program = EnsureProgram("MasterProgram", onInteractionSourceCodeName);
				var functionDefs = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(MysticalCube)));
				_program.FunctionDefinitions = functionDefs;
				return _program;
			}
		}
	}
}

