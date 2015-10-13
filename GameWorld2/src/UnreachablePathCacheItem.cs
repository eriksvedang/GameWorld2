using System.Collections.Generic;
using GameTypes;
using TingTing;
using Pathfinding;

namespace GameWorld2
{
	/// <summary>
	/// Symbolizes a recently made Path-finding search that resulted in an invalid path
	/// </summary>
	public class UnreachablePathCacheItem
	{
		public float age; // seconds since the failed search was done
		public PointTileNode startTile;
		public PointTileNode endTile;
	}
}
