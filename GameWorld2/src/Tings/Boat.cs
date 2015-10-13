using System;
using TingTing;
using RelayLib;
using GameTypes;

namespace GameWorld2
{
	public class Boat : Vehicle
	{
		public override IntPoint movingDoorPositionOffset {
			get {
				return localPoint + IntPoint.DirectionToIntPoint(IntPoint.Turn(direction, 90)) * 10;
			}
		}

		public override int movingDoorRotationOffset {
			get {
				return 90;
			}
		}
	}
}
