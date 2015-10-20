using System;
using RelayLib;
using GameTypes;
using System.Linq;

namespace GameWorld2
{
	public class WorldSettings
	{
		public const string TABLE_NAME = "WorldSettings";
		
		public delegate void OnNotification(string pName, string pMessage);
		public OnNotification onNotification;

		public delegate void OnHint(string pMessage);
		public OnHint onHint;

		public delegate void OnCameraTarget (string pLookFromHere, string pTargetName);
		public OnCameraTarget onCameraTarget;

		public delegate void CopyToClipboard (string text);
		public CopyToClipboard onCopyToClipboard;
		
		public void Notify(string pName, string pMessage) {
			if(onNotification != null && !muteNotifications) {
				onNotification(pName, pMessage);
			}
		}

		public void Hint(string pMessage) {
			if(onHint != null) {
				onHint(pMessage);
			}
		}

		TableTwo _table;
		ValueEntry<string> CELL_activeRoom;
		ValueEntry<string> CELL_avatarName;
		ValueEntry<float> CELL_totalWorldTime;
		ValueEntry<float> CELL_gameTimeSeconds;
		ValueEntry<float> CELL_gameTimeSpeed;
		ValueEntry<float> CELL_cameraAutoRotateSpeed;
		ValueEntry<float> CELL_rain;
		ValueEntry<int> CELL_tickNr;
		ValueEntry<float> CELL_rainTargetValue;
		ValueEntry<int> CELL_dynamicallyCreatedTingsCount;
		ValueEntry<string> CELL_translationLanguage;
		ValueEntry<bool> CELL_muteNotifications;
		ValueEntry<string> CELL_focusedDialogue;
		ValueEntry<bool> CELL_beaten;
		ValueEntry<bool> CELL_heartIsBroken;
		ValueEntry<string[]> CELL_storyEventLog;
		
		public WorldSettings (RelayTwo pRelay)
		{
			_table = pRelay.GetTable(TABLE_NAME);
			SetupCells();
		}
		
		private void SetupCells()
		{
			const float NR_OF_SETTINGS = 16;
			
			while(_table.GetRows().Length < NR_OF_SETTINGS) {
				_table.CreateRow();
			}
			
			CELL_activeRoom = _table.GetValueEntryEnsureDefault<string>(0, "activeRoom", "Oblivion");
            CELL_avatarName = _table.GetValueEntryEnsureDefault<string>(1, "avatarName", "Sebastian");
			CELL_totalWorldTime = _table.GetValueEntryEnsureDefault<float>(2, "totalWorldTime", 0f);
			CELL_gameTimeSeconds = _table.GetValueEntryEnsureDefault<float>(3, "gameTimeSeconds", 72000f);
			CELL_gameTimeSpeed = _table.GetValueEntryEnsureDefault<float>(4, "gameTimeSpeed", 100f);
			CELL_cameraAutoRotateSpeed = _table.GetValueEntryEnsureDefault<float>(5, "cameraAutoRotateSpeed", 0f);
			CELL_rain = _table.GetValueEntryEnsureDefault<float> (6, "rain", 0f);
			CELL_tickNr = _table.GetValueEntryEnsureDefault<int>(7, "tickNr", 0);
			CELL_rainTargetValue = _table.GetValueEntryEnsureDefault<float> (8, "rainTargetValue", 0f);
			CELL_dynamicallyCreatedTingsCount = _table.GetValueEntryEnsureDefault<int>(9, "dynamicallyCreatedTingsCount", 0);
			CELL_translationLanguage = _table.GetValueEntryEnsureDefault<string> (10, "translationLanguage", "swe");
			CELL_muteNotifications = _table.GetValueEntryEnsureDefault<bool> (11, "muteNotifications", false);
			CELL_focusedDialogue = _table.GetValueEntryEnsureDefault<string> (12, "focusedDialogue", "");
			CELL_beaten = _table.GetValueEntryEnsureDefault<bool> (13, "beaten", false);
			CELL_heartIsBroken = _table.GetValueEntryEnsureDefault<bool> (14, "heartIsBroken", false);
			CELL_storyEventLog = _table.GetValueEntryEnsureDefault<string[]>(15, "storyEventLog", new string[] {});

			// REMEMBER TO ADD ROWS!
		}
		
		public void UpdateRain(float dt)
		{
			float rainChangeSpeed = 50.0f;
			float diff = rainTargetValue - rain;
			if(Math.Abs(diff) < 20.0f) {
				rain = rainTargetValue;
//				D.Log("rain = rainTargetValue = " + rainTargetValue);
			}
			else if(diff < 0f) {
				rain -= rainChangeSpeed * dt;
			}
			else if(diff > 0f) {
				rain += rainChangeSpeed * dt;
			}
		}
		
		public string activeRoom
		{
			get {
				return CELL_activeRoom.data;
			}
			set {
				//D.Log ("Setting active room to " + value);
				CELL_activeRoom.data = value;
			}
		}
		
		public string avatarName
		{
			get {
				return CELL_avatarName.data;
			}
			set {
				CELL_avatarName.data = value;
				D.Log ("avatarName was set to " + value);
			}
		}
		
		/// <summary>
		/// Time in real life seconds since the story started
		/// </summary>
		public float totalWorldTime {
			get {
				return CELL_totalWorldTime.data;
			}
			set {
				CELL_totalWorldTime.data = value;
			}
		}
		
		/// <summary>
		/// Time in game seconds since 00:00, day 0 (the day Sebastian arrives)
		/// </summary>
		public float gameTimeSeconds {
			get {
				return CELL_gameTimeSeconds.data;
			}
			set {
				CELL_gameTimeSeconds.data = value;
			}
		}
		
		/// <summary>
		/// The current time in the game world
		/// </summary>
		public GameTime gameTimeClock {
			get {
				return new GameTime(gameTimeSeconds);
			}
			private set {
				gameTimeSeconds = value.totalSeconds;
			}
		}
		
		public float gameTimeSpeed {
			get {
				return CELL_gameTimeSpeed.data;
			}
			set {
				CELL_gameTimeSpeed.data = value;
			}
		}

		public float cameraAutoRotateSpeed {
			get {
				return CELL_cameraAutoRotateSpeed.data;
			}
			set {
				CELL_cameraAutoRotateSpeed.data = value;
			}
		}

		public float rain {
			get {
				return CELL_rain.data;
			}
			set {
				CELL_rain.data = value;
			}
		}
		
		public int tickNr {
			get {
				return CELL_tickNr.data;
			}
			set {
				CELL_tickNr.data = value;
			}
		}
		
		public float rainTargetValue {
			get {
				return CELL_rainTargetValue.data;
			}
			set {
				CELL_rainTargetValue.data = value;
			}
		}

		public int dynamicallyCreatedTingsCount {
			get {
				return CELL_dynamicallyCreatedTingsCount.data;
			}
			set {
				CELL_dynamicallyCreatedTingsCount.data = value;
			}
		}

		public string translationLanguage {
			get {
				return CELL_translationLanguage.data;
			}
			set {
				CELL_translationLanguage.data = value;
			}
		}

		public bool muteNotifications {
			get {
				return CELL_muteNotifications.data;
			}
			set {
				CELL_muteNotifications.data = value;
			}
		}

		public string focusedDialogue {
			get {
				return CELL_focusedDialogue.data;
			}
			set {
				CELL_focusedDialogue.data = value;
			}
		}

		public bool beaten {
			get {
				return CELL_beaten.data;
			}
			set {
				CELL_beaten.data = value;
			}
		}

		public bool heartIsBroken {
			get {
				return CELL_heartIsBroken.data;
			}
			set {
				CELL_heartIsBroken.data = value;
			}
		}

		public string[] storyEventLog {
			get {
				return CELL_storyEventLog.data;
			}
			set {
				CELL_storyEventLog.data = value;
			}
		}

		public void LogStoryEvent(string e) {
			var newLog = storyEventLog.ToList();
			newLog.Add(e);
			storyEventLog = newLog.ToArray();
		}
	}
}

