using System;
using System.Collections.Generic;
using GameTypes;
using TingTing;
using System.IO;
using RelayLib;
using System.Text;
using GrimmLib;

namespace GameWorld2
{
	public class Timetable : RelayObjectTwo
	{
		public const string TABLE_NAME = "Timetables";

		private ValueEntry<string> CELL_name;
		private ValueEntry<string> CELL_fileContent;

		private List<TimetableSpan> _timetableSpans = new List<TimetableSpan>();

		public Timetable()
		{
		}

		protected override void SetupCells()
		{
			CELL_name = EnsureCell("name", "unnamed");
			CELL_fileContent = EnsureCell("fileContent", "");
			GenerateTimetableSpansFromContentString();
		}

		/// <summary>
		/// WARNING! Adding spans during runtime is not supported since they will not be saved in the database.
		/// This function is only used for testing.
		/// </summary>
		public void CreateTimetableSpanInternal(GameTime pStartTime, GameTime pEndTime, TimetableBehaviour pBehaviour) 
		{
			TimetableSpan span = new TimetableSpan() {
				startTime = pStartTime,
				endTime = pEndTime,
				behaviour = pBehaviour
			};
			_timetableSpans.Add(span);
		}

		public void Update(float dt, GameTime pCurrentTime, Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			TimetableSpan currentSpan = GetCurrentSpan(pCurrentTime);

			if(currentSpan != TimetableSpan.NULL) {

				if (pCharacter.isAvatar) {
					return;
				}

				if(pCharacter.lastTimetableSpan != currentSpan) {
					if(pCharacter.lastTimetableSpan != TimetableSpan.NULL) {
						pCharacter.logger.Log(pCharacter.name + " ended span " + pCharacter.lastTimetableSpan.name);
						pCharacter.lastTimetableSpan.behaviour.OnFinish(pCharacter, pTingRunner, pRoomRunner, pDialogueRunner);
					} else {
						pCharacter.logger.Log(pCharacter.name + " ended span NULL");
					}
				}
				pCharacter.lastTimetableSpan = currentSpan;

				//pCharacter.logger.Log("Current timetable span to update: " + currentSpan.ToString());
				if(pCharacter.timetableTimer <= 0f) {
					pCharacter.timetableTimer = currentSpan.behaviour.Execute(pCharacter, pTingRunner, pRoomRunner, pDialogueRunner, pWorldSettings);
					//pCharacter.logger.Log(pCharacter.name + " executed " + currentSpan.behaviour + " and set timetableTimer to " + pCharacter.timetableTimer);
				} else {
					pCharacter.timetableTimer -= dt;
					//pCharacter.logger.Log(pCharacter.name + " timetableTimer = " + pCharacter.timetableTimer);
				}
			}
			else {
				D.Log("Found no matching time span in Timetable for character " + pCharacter.name + " at time " + pCurrentTime);
			}
		}

		public TimetableSpan GetCurrentSpan(GameTime pCurrentTime) 
		{
			//return _timetableSpans.Find(span => span.IsTimeWithinBounds(pCurrentTime));

			foreach (var span in _timetableSpans) {
				if(span.IsTimeWithinBounds(pCurrentTime)) {
					return span;
				}
			}

			return TimetableSpan.NULL;
		}

		public string name {
			get {
				return CELL_name.data;
			}
			set {
				CELL_name.data = value;
			}
		}

		public string fileContent {
			get {
				return CELL_fileContent.data;
			}
			set {
				CELL_fileContent.data = value;
				GenerateTimetableSpansFromContentString();
			}
		}

		public TimetableSpan[] timetableSpans {
			get {
				return _timetableSpans.ToArray();
			}
		}

		public string TimetableSpansToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(var span in _timetableSpans) {
				sb.Append(span.ToString() + "\n");
			}
			return sb.ToString();
		}

		private void GenerateTimetableSpansFromContentString()
		{
			//System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			//Console.WriteLine("Stack trace: " + t.ToString());

			int n = 0;
			foreach(string line in fileContent.Split(new char[] {'\n'}))
			{
				try {
					ProcessLine(line);
				}
				catch (Exception e) {
					string errorMessage = "[" + n + "] Couldn't process line '" + line + "', error: " + e.ToString();
					//Console.WriteLine();
					throw new Exception(errorMessage);
				}

				n++;
			}
		}

		private void ProcessLine(string pLine)
		{
			string[] allTokensOnLine = pLine.Split(new char[] {' ', '\t', ':'}, StringSplitOptions.RemoveEmptyEntries);
			foreach(string s in allTokensOnLine) {
				s.Trim();
			}

			if(allTokensOnLine.Length <= 6) {
				//Console.WriteLine("Ignored line: '" + pLine + "'");
				return;
			}

			// Structure of a line in the .ttt files:
			// XX:XX - XX:XX [TASK_NAME] [BEHAVIOUR_NAME] & [OPTIONAL PARAMETERS]

			int startHour = Convert.ToInt32(allTokensOnLine[0]);
			int startMinute = Convert.ToInt32(allTokensOnLine[1]);
			// (token at index 2 is the dash)
			int endHour = Convert.ToInt32(allTokensOnLine[3]);
			int endMinute = Convert.ToInt32(allTokensOnLine[4]);
			string taskName = allTokensOnLine[5];
			// Take all the tokens except the first 6
			string[] tokensExceptTimestamps = new string[allTokensOnLine.Length - 6];
			for(int i = 0; i < tokensExceptTimestamps.Length; i++) {
				tokensExceptTimestamps[i] = allTokensOnLine[i + 6];
			}

			_timetableSpans.Add(CreateTimetableSpan(startHour, startMinute, endHour, endMinute, taskName, tokensExceptTimestamps));
		}

		private TimetableSpan CreateTimetableSpan(int pStartHour, int pStartMinute, int pEndHour, int pEndMinute, string pName, string[] pTokens)
		{
			GameTime start = new GameTime(pStartHour, pStartMinute);
			GameTime end = new GameTime(pEndHour, pEndMinute);

			TimetableSpan span = new TimetableSpan();
			span.name = pName;
			span.startTime = start;
			span.endTime = end;
			span.behaviour = CreateBehaviourFromTokens(pTokens);

			//Console.WriteLine("Created span: " + span);

			return span;
		}

		private TimetableBehaviour CreateBehaviourFromTokens(string[] pTokens)
		{
			string first = pTokens[0];

			switch(first) 
			{
			case "BeAtPosition":
				return new Behaviour_BeAtPosition(GetWorldCoordinateFromTokens(pTokens, 1));

			case "BeAtTing":
				return new Behaviour_BeAtTing(pTokens[1]);
				
			case "RunStory":
				return new Behaviour_RunStory(pTokens[1]);

			case "BeInRoom":
				return new Behaviour_BeInRoom(pTokens[1]);

			case "WorkWithModifier":
				return new Behaviour_WorkWithModifier(pTokens[1]);
				
			case "Fika":
				return new Behaviour_Fika(pTokens[1]);

			case "Party":
				return new Behaviour_Party(pTokens[1]);
				
			case "Guard":
				return new Behaviour_Guard(pTokens[1]);

			case "Sleep":
				return new Behaviour_Sleep(pTokens[1]);
				
			case "Interact":
				return new Behaviour_Interact(pTokens[1], false);

			case "Hack":
				return new Behaviour_Interact(pTokens[1], true);

			case "Drink":
				return new Behaviour_Drink(pTokens[1]);

			case "ServeDrinks":
				return new Behaviour_ServeDrinks(pTokens[1]);

			case "Sit":
				var seatNames = new List<string>();
				for (int i = 1; i < pTokens.Length; i++) {
					seatNames.Add(pTokens[i]);
				}
				return new Behaviour_Sit(seatNames.ToArray());

			case "Photograph":
				return new Behaviour_Photograph();
				
			case "RefineGoods":
				return new Behaviour_RefineGoods(pTokens[1]);

			case "GuideTo":
				return new Behaviour_GuideTo(pTokens[1], pTokens[2]);

			case "Sell":
				return new Behaviour_Sell(pTokens);

			case "Smoke":
				return new Behaviour_Smoke(pTokens[1]);

			case "PlayTrumpet":
				return new Behaviour_PlayTrumpet(pTokens[1]);
			
			case "Dj":
				var songNames = new List<string>();
				for (int i = 2; i < pTokens.Length; i++) {
					songNames.Add(pTokens[i]);
				}
				return new Behaviour_Dj(pTokens[1], songNames.ToArray());
							
			default:
				throw new Exception("Can't understand token " + pTokens[0]);
			}
		}

		private WorldCoordinate GetWorldCoordinateFromTokens(string[] pTokens, int pStartIndex)
		{
			int x = Convert.ToInt32(pTokens[pStartIndex + 1]);
			int y = Convert.ToInt32(pTokens[pStartIndex + 2]);

			return new WorldCoordinate(pTokens[pStartIndex], new IntPoint(x, y));
		}
	}
}

