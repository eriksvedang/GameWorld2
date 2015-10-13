using System;
using GameTypes;
using System.Collections.Generic;

namespace GameWorld2
{
	public class Robot : MimanTing
	{
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.Up * 1,
					localPoint + IntPoint.Right * 1,
					localPoint + IntPoint.Left * 1,
					localPoint + IntPoint.Down * 1
				};
			}
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "inspect";
			}
		}
		
		public override string tooltipName {
			get {
				return "robot";
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}
	}
}

