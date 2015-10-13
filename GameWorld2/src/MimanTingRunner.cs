using System;
using TingTing;
using RelayLib;
using GrimmLib;
using GameTypes;

namespace GameWorld2
{
	public class MimanTingRunner : TingRunner
	{
		private DialogueRunner _dialogueRunner;
		private ProgramRunner _programRunner;
		private SourceCodeDispenser _sourceCodeDispenser;
		private TimetableRunner _timetableRunner;
		private WorldSettings _worldSettings;
				
        public MimanTingRunner(
            RelayTwo pRelay,
            DialogueRunner pDialogueRunner,
            ProgramRunner pProgramRunner,
            SourceCodeDispenser pSourceCodeDispenser,
            RoomRunner pRoomRunner,
			TimetableRunner pTimetableRunner,
			WorldSettings pWorldSettings
            )
            : base(pRelay, pRoomRunner)
		{
			_dialogueRunner = pDialogueRunner;
			_dialogueRunner.AddOnSomeoneSaidSomethingListener(this.OnSomeoneSaidSomething);
			_programRunner = pProgramRunner;
			_sourceCodeDispenser = pSourceCodeDispenser;
			_timetableRunner = pTimetableRunner;
			_worldSettings = pWorldSettings;

			foreach(Ting ting in _tings.Values)
            {
				if(ting is MimanTing) 
                {
                    (ting as MimanTing).SetMimanRunners(_programRunner, _sourceCodeDispenser, _dialogueRunner, _worldSettings);
				}
				if(ting is Character)
				{
					(ting as Character).SetTimetableRunner(_timetableRunner);
				}
			}

			foreach (Ting ting in _tings.Values)
			{
				if (ting is MimanTing) {
					var mimanTing = ting as MimanTing;
					mimanTing.Init();
					if(mimanTing.autoUnregisterFromUpdate && mimanTing.dialogueLine != "") {
						Unregister(mimanTing);
					}
				}
			}
		}

		private void SetupRunners(Ting pTing)
		{
			if(pTing is Character) {
				(pTing as Character).SetTimetableRunner(_timetableRunner);
			}
			if(pTing is MimanTing) {
                (pTing as MimanTing).SetMimanRunners(_programRunner, _sourceCodeDispenser, _dialogueRunner, _worldSettings);
                (pTing as MimanTing).Init();
			}
		}

		public override T CreateTing<T>(string pName, WorldCoordinate pWorldCoordinate, Direction pDirection)
		{
            T ting = base.CreateTing<T>(pName, pWorldCoordinate, pDirection);
			SetupRunners(ting);
			return ting;
		}
		
		public override T CreateTing<T>(string pName, WorldCoordinate pWorldCoordinate, Direction pDirection, string pPrefabName)
		{
            T ting = base.CreateTing<T>(pName, pWorldCoordinate, pDirection, pPrefabName);
			SetupRunners(ting);
			return ting;
		}

		public override T CreateTingAfterUpdate<T>(string pName, WorldCoordinate pWorldCoordinate, Direction pDirection, string pPrefabName)
		{
			T newTing = base.CreateTingAfterUpdate<T>(pName, pWorldCoordinate, pDirection, pPrefabName);
			SetupRunners(newTing);
			return newTing;
		}

		public void OnSomeoneSaidSomething(Speech pSpeech)
		{
#if DEBUG
			if(!HasTing(pSpeech.speaker)) {
				throw new Exception("Can't find speaker with name '" + pSpeech.speaker + "'");
			}
#endif
			MimanTing speaker = GetTing(pSpeech.speaker) as MimanTing;
			speaker.Say(pSpeech.line, pSpeech.conversation);
		}
	}
}

