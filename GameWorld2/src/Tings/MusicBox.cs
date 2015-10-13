using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class MusicBox : MimanTing, TingWithButton
	{
        public static new string TABLE_NAME = "Ting_MusicBoxes";
        ValueEntry<string> CELL_onButtonPressedProgramName;
		ValueEntry<string> CELL_onUpdateProgramName;
		ValueEntry<float> CELL_cutoffFrequency;
		ValueEntry<float> CELL_resonance;
		ValueEntry<bool> CELL_small;
		ValueEntry<bool> CELL_mixer;
		ValueEntry<bool> CELL_loop;
		
		Program _onButtonPressedProgram;
		Program _onUpdateProgram;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_onButtonPressedProgramName = EnsureCell("onButtonPressedProgramName", "MusicBoxer");
			CELL_onUpdateProgramName = EnsureCell("onUpdateProgramName", "BlankSlate");
			CELL_cutoffFrequency = EnsureCell("cutoff", 5000f);
			CELL_small = EnsureCell("small", false);
			CELL_mixer = EnsureCell("mixer", false);
			CELL_loop = EnsureCell("loop", true);
			CELL_resonance = EnsureCell("resonance", 1f);
		}

		public override IntPoint[] interactionPoints {
			get {
				if(mixer) {
					return new IntPoint[] { 
						localPoint + IntPoint.DirectionToIntPoint(direction) * 1,
					};
				}
				if (!small) {
					return new IntPoint[] { 
						localPoint + IntPoint.DirectionToIntPoint(direction) * 2,
					};
				} else {
					return base.interactionPoints;
				}
			}
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			if(!small) {
				FixGroupIfOutsideIslandOfTiles();
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return _onButtonPressedProgram != null;
		}

		public override void FixBeforeSaving ()
		{
			if(name.ToLower().Contains("mixer")) {
				mixer = true;
			}
		}
		
		[SprakAPI("Print", "text")]
		public void API_Print(string text)
		{
			Say(text, "");
		}

//		[SprakAPI("Enable or disable repeating", "")]
//		public void API_SetRepeat(bool enable)
//		{
//			loop = enable;
//		}
		
		[SprakAPI("Set the cutoff frequency for the sound that is playing")]
		public void API_SetCutoff(float pCutoff)
		{
			cutoffFrequency = pCutoff;
		}
		
		[SprakAPI("Set the filter resonance for the sound that is playing")]
		public void API_SetResonance(float pResonance)
		{
			resonance = pResonance;
		}
		
		[SprakAPI("Set the pitch for the sound that is playing")]
		public void API_SetPitch(float pPitch)
		{
			pitch = pPitch;
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
		
		[SprakAPI("The cosinus function", "x")]
		public float API_Cos(float x)
		{
			return (float)Math.Cos(x);
		}

		[SprakAPI("Set the sound to play in a loop")]
		public void API_PlayLoop(string pSoundName)
		{
			soundName = pSoundName;
			isPlaying = true;
			audioTime = 0.0f;
			audioLoop = false;
		}

		[SprakAPI("Set the sound to play")]
		public void API_PlaySound(string pSoundName)
		{
			soundName = pSoundName;
			isPlaying = true;
			audioTime = 0.0f;
			audioLoop = false;
		}

		[SprakAPI("Start playing the current sound")]
		public void API_Play()
		{
			isPlaying = true;
			audioTime = 0.0f;
		}
		
		[SprakAPI("Stop the current sound")]
		public void API_Stop()
		{
			isPlaying = false;
		}

		[SprakAPI("Pause or unpause the sound")]
		public void API_TogglePause()
		{
			isPlaying = !isPlaying;
		}

		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			onButtonProgram.sleepTimer = seconds;
		}
		
		[SprakAPI("Is a sound playing?")]
		public bool API_IsPlaying()
		{
			return isPlaying;
		}

		static System.Random s_random = new Random(DateTime.Today.Millisecond * DateTime.Today.Second * DateTime.Today.Minute * DateTime.Today.Hour);

		[SprakAPI("Get a random value between 0 and 1", "")]
		public float API_Random()
		{
			float randomNr = (float)s_random.NextDouble();
			//logger.Log("Called API_random and returned " + randomNr);
			return randomNr;
		}
		
		public void PushButton(Ting pUser)
		{
			PlaySound("Button");
			onButtonProgram.Start();
		}
		
		public override void Update(float dt)
		{
			if(isPlaying) {
				audioTime += dt;
				//D.Log("Music box " + name + " is playing, is at time " + audioTime);
				if(loop && audioTime > audioTotalLength) {
					//D.Log("Music box " + name + " will loop its sound.");
					audioTime = 0f;
					PlaySound(soundName);
				}
			}
		}

		public override bool autoUnregisterFromUpdate {
			get {
				return false;
				// nope, needs constant update
			}
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Locker;
		}
		
		public override bool canBePickedUp {
			get {
				return !mixer && small;
			}
		}
		
		public override string verbDescription {
			get {
//				if (isPlaying) {
//					return "turn off";
//				} else {
//					return "turn on";
//				}

				if (mixer) {
					return "use";
				}
				else if(small) {
					return "press button";
				}
				else {
					return "press button on";
				}
			}
		}
		
		public override string tooltipName {
			get {
				if (mixer) {
					return "mixer";
				} else if (small) {
					return "music box";
				} else {
					if(prefab.ToLower().Contains("theremin")) {
						return "theremin";
					}
					else if(prefab.ToLower().Contains("gramophone")) {
						return "record player";
					}
					else {
						return "jukebox";
					}
				}
			}
		}
		
		[EditableInEditor]
		public string onButtonPressedProgramName {
			get {
                return CELL_onButtonPressedProgramName.data;
			}
			set {
                CELL_onButtonPressedProgramName.data = value;
			}
		}
		
		[EditableInEditor]
		public string onUpdateProgramName {
			get {
                return CELL_onUpdateProgramName.data;
			}
			set {
                CELL_onUpdateProgramName.data = value;
			}
		}
		
		public Program onButtonProgram {
			get {
				if(_onButtonPressedProgram == null) {
					_onButtonPressedProgram = EnsureProgram("OnButtonProgram", onButtonPressedProgramName);
					_onButtonPressedProgram.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(MusicBox)));
				}
				return _onButtonPressedProgram;
			}
		}
		
		public Program onUpdateProgram {
			get {
				if(_onUpdateProgram == null) {
					_onUpdateProgram = EnsureProgram("OnUpdateProgram", onUpdateProgramName);
					_onUpdateProgram.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(MusicBox)));
				}
				return _onUpdateProgram;
			}
		}
		
		public override void PrepareForBeingHacked()
		{
			// Accessing the getter to make sure that a program is generated
			if(onButtonProgram == null) { logger.Log("There was a problem generating the button program"); }
			if(onUpdateProgram == null) { logger.Log("There was a problem generating the update program"); }
		}
		
		public float cutoffFrequency
		{
			get {
				return CELL_cutoffFrequency.data;
			}
			set {
				CELL_cutoffFrequency.data = value;
			}
		}
		
		public float resonance
		{
			get {
				return CELL_resonance.data;
			}
			set {
				CELL_resonance.data = value;
			}
		}
		
		[EditableInEditor]
		public bool small
		{
			get {
				return CELL_small.data;
			}
			set {
				CELL_small.data = value;
			}
		}
		
		[EditableInEditor]
		public bool mixer
		{
			get {
				return CELL_mixer.data;
			}
			set {
				CELL_mixer.data = value;
			}
		}

		[EditableInEditor]
		public bool loop
		{
			get {
				return CELL_loop.data;
			}
			set {
				CELL_loop.data = value;
			}
		}

		public bool isJukebox {
			get {
				return !small && !mixer;
			}
		}

		public override Program masterProgram {
			get {
				return onButtonProgram;
			}
		}
	}
}

