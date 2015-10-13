using System;
using System.Collections.Generic;
using TingTing;
using GameTypes;
using System.Text;

namespace GameWorld2
{
	public enum MimanPathStatus {
		NOT_SET,
		NO_PATH_FOUND,
		FOUND_GOAL,
		IN_THE_SAME_ROOM_ALREADY
	}
	
	public class MimanPath
	{
		public MimanPathStatus status = MimanPathStatus.NOT_SET;
		public Ting[] tings = new Ting[] {}; // a list of tings (mostly doors and portals) leading to the goal (if a Character would interact with them one at a time)
		public int iterations;

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("MimanPath [" + status.ToString() + "] (" + iterations + " iterations) with tings: ");
			
			int i = 0;
			foreach(var ting in tings) {
				sb.Append(ting.name);
				i++;
				if(i < tings.Length) {
					sb.Append(", ");
				}
			}
			
			return sb.ToString();
		}
	}
}

