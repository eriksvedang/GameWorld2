using System;

namespace GameWorld2
{
	public class FlowerPot : MimanTing
	{
		public FlowerPot ()
		{
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

