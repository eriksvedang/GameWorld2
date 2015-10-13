using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;

namespace GameWorld2
{
	public class Cigarette : Drug
	{
		public override string tooltipName {
			get {
				return "cigarette" + (charges <= 0 ? " (used)" : "");
			}
		}
		
		public override string verbDescription {
			get {
				return "smoke";
			}
		}
	}
}

