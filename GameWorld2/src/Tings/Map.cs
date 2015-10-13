using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;
using GameTypes;

namespace GameWorld2
{
	public class Map : MimanTing
	{
		public static new string TABLE_NAME = "Ting_Maps";
		
		protected override void SetupCells()
		{
			base.SetupCells();
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string tooltipName {
			get {
				return "map";
			}
		}
		
		public override string verbDescription {
			get {
				return "look at [m]";
			}
		}

		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is Locker || pTingToInteractWith is TrashCan || pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
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
