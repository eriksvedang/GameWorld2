using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;
using System.Linq;

namespace GameWorld2
{
	public class Extractor : MimanTing, TingWithButton
	{
        public static new string TABLE_NAME = "Ting_Extractors";
        ValueEntry<string> CELL_programName;
		
		Program _program;
		MimanTing _target;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_programName = EnsureCell("masterProgramName", "ExtractorSoftware");
		}

		public override bool DoesMasterProgramExist ()
		{
			return _program != null;
		}

		public override void Update (float dt)
		{
			UpdateBubbleTimer();
		}
		
		[SprakAPI("Get the name of the attached thing")]
		public string API_GetName()
		{
			API_Sleep (Randomizer.GetValue(1.0f, 3.0f));
			return _target.name;
		}

		[SprakAPI("Get the user defined label of the attached thing")]
		public string API_GetLabel()
		{
			API_Sleep (Randomizer.GetValue(1.0f, 2.0f));
			return _target.userDefinedLabel;
		}

		[SprakAPI("Set the user defined label of the attached thing")]
		public string API_SetLabel()
		{
			return _target.userDefinedLabel;
		}

		[SprakAPI("Sleepiness of attached character")]
		public float API_GetSleepiness()
		{
			API_Sleep (Randomizer.GetValue(1.0f, 3.0f));
			if (_target is Character) {
				return (_target as Character).sleepiness;
			} else {
				throw new ProgrammingLanguageNr1.Error ("Attached thing is not a Character");
			}
		}

		[SprakAPI("Speed of attached character")]
		public float API_GetSpeed()
		{
			API_Sleep (Randomizer.GetValue(1.0f, 3.0f));
			if (_target is Character) {
				return (_target as Character).walkSpeed;
			} else {
				throw new ProgrammingLanguageNr1.Error ("Attached thing is not a Character");
			}
		}

		[SprakAPI("Charisma of attached character")]
		public float API_GetCharisma()
		{
			API_Sleep (Randomizer.GetValue(1.0f, 3.0f));
			if (_target is Character) {
				return (_target as Character).charisma;
			} else {
				throw new ProgrammingLanguageNr1.Error ("Attached thing is not a Character");
			}
		}

		[SprakAPI("Get the connections of the attached thing")]
		public object[] API_GetConnections()
		{
			API_Sleep (Randomizer.GetValue(2.0f, 3.0f));
			return _target.connectedTings.Select (t => t.name).ToArray();
		}
		
		[SprakAPI("Say something", "text")]
		public void API_Say(string text)
		{
			Say (text, "");
		}

		//public Action<String> copyToClipboard;

		[SprakAPI("Copy a piece of text to the clipboard", "text")]
		public void API_CopyToClipboard(string text)
		{
			if(_worldSettings.onCopyToClipboard != null) {
				_worldSettings.onCopyToClipboard(text);
			}
			else {
				D.Log("copyToClipboard is null");
			}
		}

		[SprakAPI("Pause the master program", "number of seconds to pause for")]
		public void API_Sleep (float seconds)
		{
			masterProgram.sleepTimer = seconds;
		}
		
		public void Attach(Ting pTarget)
		{
			//D.Log(name + " attached to " + pTarget + " will run program with content " + masterProgram.sourceCodeContent);
			_target = pTarget as MimanTing;
			masterProgram.Start();
		}
		
		public void PushButton(Ting pUser)
		{
			dialogueLine = "";
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return true;
		}

		public override string UseTingOnTingDescription (Ting pOtherTing)
		{
			return "Extract from " + pOtherTing.tooltipName;
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "reset";
			}
		}
		
		public override string tooltipName {
			get {
				return "extractor";
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
					_program.FunctionDefinitions = new List<FunctionDefinition>(FunctionDefinitionCreator.CreateDefinitions(this, typeof(Extractor)));
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

