using System;
using TingTing;
using RelayLib;
using ProgrammingLanguageNr1;
using System.Collections.Generic;

namespace GameWorld2
{
	public class Snus : Drug
	{
		public override string tooltipName {
			get {
				return "snus" + (charges <= 0 ? " (empty)" : "");
			}
		}
		
		public override string verbDescription {
			get {
				return "take one";
			}
		}
	}
}

