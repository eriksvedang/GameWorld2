using System;

namespace GameWorld2
{
	public class Point : MimanTing
	{
		public Point ()
		{
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		[TingTing.ShowInEditor]
		public override bool isBeingUsed {
			get {
				return AnotherTingSharesTheTile ();
			}
		}
		
		public override GameTypes.IntPoint[] interactionPoints {
			get {
				return new GameTypes.IntPoint[] { localPoint };
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

