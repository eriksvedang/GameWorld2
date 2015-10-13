using System;
using RelayLib;
using System.Collections.Generic;
using TingTing;

namespace GameWorld2
{
	public class Memory : MimanTing
	{
		ValueEntry<Dictionary<string, object>> CELL_data;

		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_data = EnsureCell("data", new Dictionary<string, object> {});
		}

		public override bool canBePickedUp {
			get {
				return true;
			}
		}

		public override string tooltipName {
			get {
				return name; // "hard drive";
			}
		}
		
		//[EditableInEditor]
		public Dictionary<string, object> data {
			get {
				return CELL_data.data;
			}
			set {
				CELL_data.data = value;
			}
		}

		public object this[string pKey] {
			get {
				return CELL_data.data[pKey];
			}
			set {
				CELL_data.data [pKey] = value;
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

