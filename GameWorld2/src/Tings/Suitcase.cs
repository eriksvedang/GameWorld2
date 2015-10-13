using System;
using GameTypes;

namespace GameWorld2
{
	public class Suitcase : Locker
	{
		public override string tooltipName {
			get {
				return "suitcase";
			}
		}
		
		public override string verbDescription {
			get {
				return "look inside";
			}
		}

		public override bool canBePickedUp {
			get {
				return true;
			}
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.Up * 2,
					localPoint + IntPoint.Left * 2,
					localPoint + IntPoint.Right * 2,
					localPoint + IntPoint.Down * 2,
				};
			}
		}

	}
}

