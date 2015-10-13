using System;
using TingTing;
using RelayLib;
using GameTypes;

namespace GameWorld2
{
	public class NavNode : MimanTing
	{
		public static new string TABLE_NAME = "Tings_NavNode";
		
		ValueEntry<bool> CELL_isSwitch;
		ValueEntry<string> CELL_mainTrack;
		ValueEntry<string> CELL_leftTrack;
		ValueEntry<string> CELL_rightTrack;
		ValueEntry<bool> CELL_isStation;
		ValueEntry<string> CELL_stationName;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_isSwitch = EnsureCell("isSwitch", false);
			CELL_mainTrack = EnsureCell("mainTrack", "");
			CELL_leftTrack = EnsureCell("leftTrack", "");
			CELL_rightTrack = EnsureCell("rightTrack", "");
			CELL_isStation = EnsureCell("isStation", false);
			CELL_stationName = EnsureCell("stationName", "");
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		[EditableInEditor()]
		public bool isSwitch {
			get {
				return CELL_isSwitch.data;
			}
			set {
				CELL_isSwitch.data = value;
			}
		}

		[EditableInEditor()]
		public string mainTrackName {
			get {
				return CELL_mainTrack.data;
			}
			set {
				CELL_mainTrack.data = value;
			}
		}

		[EditableInEditor()]
		public string leftTrackName {
			get {
				return CELL_leftTrack.data;
			}
			set {
				CELL_leftTrack.data = value;
			}
		}

		[EditableInEditor()]
		public string rightTrackName {
			get {
				return CELL_rightTrack.data;
			}
			set {
				CELL_rightTrack.data = value;
			}
		}

		public NavNode mainTrack {
			get {
				return _tingRunner.GetTingUnsafe(CELL_mainTrack.data) as NavNode;
			}
			set {
				if(value == null) {
					CELL_mainTrack.data = "";
				} else {
					CELL_mainTrack.data = value.name;
				}
			}
		}

		public NavNode leftTrack {
			get {
				return _tingRunner.GetTingUnsafe(CELL_leftTrack.data) as NavNode;
			}
			set {
				if(value == null) {
					CELL_leftTrack.data = "";
				} else {
					CELL_leftTrack.data = value.name;
				}
			}
		}

		public NavNode rightTrack {
			get {
				return _tingRunner.GetTingUnsafe(CELL_rightTrack.data) as NavNode;
			}
			set {
				if(value == null) {
					CELL_rightTrack.data = "";
				} else {
					CELL_rightTrack.data = value.name;
				}
			}
		}

		[EditableInEditor()]
		public bool isStation {
			get {
				return CELL_isStation.data;
			}
			set {
				CELL_isStation.data = value;
			}
		}

		[EditableInEditor()]
		public string stationName {
			get {
				return CELL_stationName.data;
			}
			set {
				CELL_stationName.data = value;
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

