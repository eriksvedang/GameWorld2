using System;
using GameTypes;
using TingTing;
using ProgrammingLanguageNr1;

namespace GameWorld2
{
	public class Locker : MimanTing
	{
		public override void Init()
		{
			base.Init ();
			
			// Ensure inventory room
			if(!_roomRunner.HasRoom(inventoryRoomName)) {
				SimpleRoomBuilder srb = new SimpleRoomBuilder(_roomRunner);
				srb.CreateRoomWithSize(inventoryRoomName, 5, 4);
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.Up * 2
				};
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}
		
		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string verbDescription {
			get {
				return "open";
			}
		}
		
		public override string tooltipName {
			get {
				return "locker";
			}
		}
		
		public string inventoryRoomName {
			get {
				return name + "_locker";
			}
		}
		
		public Ting[] GetItems ()
		{
			return _roomRunner.GetRoom(inventoryRoomName).GetTings().ToArray();
		}

		public bool isFull {
			get {
				return GetItems().Length >= Character.INVENTORY_SIZE;
			}
		}

		public bool PutTingIntoRandomFreeSpot(MimanTing pTing)
		{
			Room room = _roomRunner.GetRoom(inventoryRoomName);

			foreach (var tilePoint in room.points) {
				if (!room.GetTile(tilePoint).HasOccupants()) {
					pTing.position = new WorldCoordinate(inventoryRoomName, tilePoint);
					return true;
				}
			}

			D.Log("No free spot in the locker");
			return false;
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

