using System;
using TingTing;
using RelayLib;
using GameTypes;

namespace GameWorld2
{
	public enum VehicleTurningDirection {
		LEFT, RIGHT, FORWARD
	};

	public class Vehicle : MimanTing
	{
		public static new string TABLE_NAME = "Tings_Vehicles";
		
		ValueEntry<string> CELL_movingDoorName;
		ValueEntry<string> CELL_currentNavNodeName;
		ValueEntry<string> CELL_nextNavNodeName;
		ValueEntry<VehicleTurningDirection> CELL_turning;
		ValueEntry<float> CELL_speed;
		ValueEntry<float> CELL_distance; // distance travelled from the latest node

		Door _movingDoorCache;
		public bool safetySwitchOn = false;

		public delegate void OnNewNavNode(NavNode pOldNavNode, NavNode pNewNavNode);
		public OnNewNavNode onNewNavNode;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_movingDoorName = EnsureCell("movingDoorName", "");
			CELL_currentNavNodeName = EnsureCell("currentNavNodeName", "");
			CELL_nextNavNodeName = EnsureCell("nextNavNodeName", "");
			CELL_turning = EnsureCell("turning", VehicleTurningDirection.FORWARD);
			CELL_speed = EnsureCell("speed", 0f);
			CELL_distance = EnsureCell("distance", 0f);

			AddDataListener<WorldCoordinate>("position", OnPositionChanged);
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		~Vehicle() {
			RemoveDataListener<WorldCoordinate>("position", OnPositionChanged);
		}

		public override void Update (float dt)
		{
			//D.Log("updating vehicle");

			UpdateMovingDoorPosition();

			if(currentNavNode == null || nextNavNode == null) {
				return;
			}

			if (safetySwitchOn) {
				if (movingDoor.actionName == "") {
					// only allow it to go when the moving door is not operating
					safetySwitchOn = false;
				}
				return;
			}

			distance += speed * dt;

			if(distanceFraction >= 0.99f) {
				var oldCurrent = currentNavNode;
				distance = 0f;
				currentNavNode = nextNavNode;
				nextNavNode = _tingRunner.GetTingUnsafe(currentNavNode.mainTrackName) as NavNode;
				if(nextNavNode == null) {
					throw new Exception("nextNavNode is null for vehicle " + name + " at position " + position + " and current nav node " + currentNavNode.name);
				}
				position = currentNavNode.position;
				if(onNewNavNode != null) {
					onNewNavNode(oldCurrent, currentNavNode);
				}
			}
			
			if(currentNavNode.room != nextNavNode.room) {
				position = new WorldCoordinate(room.name, 10000, 10000);
				//D.Log(name + " is going between scenes");
			}
			else {
				var delta = nextNavNode.localPoint - currentNavNode.localPoint;
				var middlePosition = currentNavNode.localPoint + delta.scale(distanceFraction);
				position = new WorldCoordinate(room.name, middlePosition);
				direction = delta.Clamped().ToDirection();
			}
		}

		public override bool autoUnregisterFromUpdate {
			get {
				return false;
				// nope, needs constant update
			}
		}
	
		[EditableInEditor()]
		public string movingDoorName {
			get {
				return CELL_movingDoorName.data;
			}
			set {
				CELL_movingDoorName.data = value;
			}
		}

		[EditableInEditor()]
		public float distance {
			get {
				return CELL_distance.data;
			}
			set {
				CELL_distance.data = value;
			}
		}

		[ShowInEditor()]
		public float distanceFraction {
			get {
				if(distanceBetweenNodes == 0.0f) return 1f;
				return distance / distanceBetweenNodes;
			}
		}

		[ShowInEditor()]
		public float distanceBetweenNodes {
			get {
				if(currentNavNode == null) return 0f;
				if(nextNavNode == null) return 0f;

				if(currentNavNode.room != nextNavNode.room) {
					return 0.0f;
				}

				var p1 = currentNavNode.localPoint;
				var p2 = nextNavNode.localPoint;

				return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
			}
		}

		[EditableInEditor()]
		public VehicleTurningDirection turning {
			get {
				return CELL_turning.data;
			}
			set {
				CELL_turning.data = value;
			}
		}

		public virtual float maxSpeed {
			get {
				return 40.0f;
			}
		}

		float Clamp(float value, float min, float max) {
			if (value < min) return min;
			else if (value > max) return max;
			else return value;
		}

		[EditableInEditor()]
		public float speed {
			get {
				return CELL_speed.data;
			}
			set {
				CELL_speed.data = Clamp(value, 0f, maxSpeed);
			}
		}

		public Door movingDoor {
			get {
				if(_movingDoorCache == null) {
					_movingDoorCache = _tingRunner.GetTingUnsafe(CELL_movingDoorName.data) as Door;
				}
				return _movingDoorCache;
			}
		}

		void OnPositionChanged(WorldCoordinate pPrevPos, WorldCoordinate pNewPos) {
			UpdateMovingDoorPosition();
		}

		public virtual IntPoint movingDoorPositionOffset {
			get {
				return localPoint + IntPoint.DirectionToIntPoint (IntPoint.Turn(direction, 90)) * 4;
			}
		}

		public virtual int movingDoorRotationOffset {
			get {
				return 0;
			}
		}

		void UpdateMovingDoorPosition() {
			if(movingDoor != null) {
				movingDoor.position = new WorldCoordinate(position.roomName, movingDoorPositionOffset);
				movingDoor.direction = IntPoint.Turn(direction, movingDoorRotationOffset);
			}
		}

		[EditableInEditor()]
		public string nextNavNodeName {
			get {
				return CELL_nextNavNodeName.data;
			}
			set {
				CELL_nextNavNodeName.data = value;
			}
		}

		public NavNode nextNavNode {
			get {
				return _tingRunner.GetTingUnsafe(CELL_nextNavNodeName.data) as NavNode;
			}
			set {
				if(value == null) {
					CELL_nextNavNodeName.data = "";
				}
				else {
					CELL_nextNavNodeName.data = value.name;
				}
			}
		}

		[EditableInEditor()]
		public string currentNavNodeName {
			get {
				return CELL_currentNavNodeName.data;
			}
			set {
				CELL_currentNavNodeName.data = value;
			}
		}
		
		public NavNode currentNavNode {
			get {
				return _tingRunner.GetTingUnsafe(CELL_currentNavNodeName.data) as NavNode;
			}
			set {
				if(value == null) {
					CELL_currentNavNodeName.data = "";
				}
				else {
					CELL_currentNavNodeName.data = value.name;
				}
			}
		}

		public override bool canBePickedUp {
			get {
				return false;
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

