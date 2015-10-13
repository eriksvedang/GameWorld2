using System;
using System.Collections.Generic;
using GameTypes;
using TingTing;

namespace GameWorld2
{
	public struct TimetableSpan
	{
		public string name;
		public GameTime startTime;
		public GameTime endTime;
		public TimetableBehaviour behaviour;

		public bool IsTimeWithinBounds(GameTime pTime)
		{
			//Console.WriteLine("Checking time table span " + startTime + " to " + endTime + " with pTime " + pTime);
			return pTime.IsWithinMinuteBounds(startTime, endTime);
		}

		public override string ToString()
		{
			return string.Format("[TimetableSpan] From {0} , to {1} with behaviour {2}", startTime, endTime, behaviour);
		}

		public static TimetableSpan NULL {
			get {
				return new TimetableSpan() {
					name = "NULL",
					startTime = new GameTime(),
					endTime = new GameTime(),
					behaviour = null
				};
			}
		}

		public override bool Equals(object obj)
		{
			if(obj is TimetableSpan) {
				return (TimetableSpan)obj == this;
			}
			else {
				return false;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		public static bool operator==(TimetableSpan g1, TimetableSpan g2) {
			return 
				(g1.behaviour == g2.behaviour) &&
				(g1.name == g2.name) &&
				(g1.startTime == g2.startTime) &&
				(g1.name == g2.name);
		}

		public static bool operator!=(TimetableSpan g1, TimetableSpan g2) {
			return !(g1 == g2);
		}
	}
}

