#define PROFILE_NETWORK
#define PROFILE_SEARCH
//#define LOG

using System;
using System.Collections.Generic;
using TingTing;
using GameTypes;
using System.Text;
using System.Diagnostics;
using Pathfinding;
using System.Linq;
using System.Runtime.InteropServices;
using GameWorld2;

namespace GameWorld2 {

	public class RoomGroupNode {
		public RoomGroup roomGroup;
		public float gscore;
		public float fscore;
		public RoomGroupNode prev;
		public bool hasBeenTouched = false;
		public int depth;
		
		public override string ToString ()
		{
			return roomGroup.ToString();
		}
		
		public override int GetHashCode ()
		{
			return roomGroup.GetHashCode();
		}
	}

	public class MimanPathfinder2
	{
		TingRunner _tingRunner;
		RoomRunner _roomRunner;
		static RoomNetwork _roomNetwork;

		public static RoomNetwork roomNetwork {
			get { return _roomNetwork; }
		}

		public static void ClearRoomNetwork ()
		{
			_roomNetwork = null;
		}

		public MimanPathfinder2 (TingRunner pTingRunner, RoomRunner pRoomRunner)
		{
			_tingRunner = pTingRunner;
			_roomRunner = pRoomRunner;
		}

		public RoomNetwork RecreateRoomNetwork ()
		{
			#if PROFILE_NETWORK
			Stopwatch sw = new Stopwatch();
			sw.Start();
			#endif
			
			_roomNetwork = new RoomNetwork();

			foreach(Room room in _roomRunner.rooms) {

				foreach(Ting exit in GetExits(room)) {
					var linkingTings = GetLinkedExitsInOtherRooms(exit);
					int group = exit.tile.group;
					var roomGroup = new RoomGroup(room, group);

					Dictionary<RoomGroup, Ting> maybeRooms = null;

					if(!_roomNetwork.linkedRoomGroups.TryGetValue(roomGroup, out maybeRooms)) {
						maybeRooms = new Dictionary<RoomGroup, Ting>();
						_roomNetwork.linkedRoomGroups.Add(roomGroup, maybeRooms);
					}

					D.isNull(maybeRooms, "maybeRooms is null");

					//D.Log("The exit " + exit.name + " in " + exit.room.name + " is linked to the following tings: ");
					foreach(var linkingTing in linkingTings) {
						D.isNull(linkingTing, "linking ting is null");
						maybeRooms[RoomGroup.FromTing(linkingTing)] = exit;
						//D.Log(linkedTing.name + " in " + linkedTing.room);
					}
				}

			}

			#if PROFILE_NETWORK
			sw.Stop();
			if(sw.Elapsed.TotalSeconds > 0.0f) {
				D.Log("Recreating Room Network took " + sw.Elapsed.TotalSeconds + " s.");
			}
			#endif

			return _roomNetwork;
		}

		public void EnsureRoomNetwork ()
		{
			if(_roomNetwork == null) {
				RecreateRoomNetwork();
			}
		}

		public MimanPath Search(Ting pStart, Ting pGoal) {

			EnsureRoomNetwork();

			#if PROFILE_SEARCH
			Stopwatch sw = new Stopwatch();
			sw.Start();
			#endif
			
			var path = AStar(pStart, pGoal);
			
			#if PROFILE_SEARCH
			sw.Stop();
			if(sw.Elapsed.TotalSeconds > 0.01f) {
				D.Log("SLOW MimanPathfinder2 search from " + pStart.name + " at " + pStart.position + " to " + pGoal.name + " at " + pGoal.position + " with result: " + path.status + " took " + sw.Elapsed.TotalSeconds + " s. Iterations: " + path.iterations);
			}
			#endif
			
			return path;
		}

		private MimanPath AStar(Ting pStart, Ting pGoal) {

			if (pStart.room == pGoal.room) {
				return new MimanPath() {
					status = MimanPathStatus.IN_THE_SAME_ROOM_ALREADY
				};
			}

			var visited = new HashSet<RoomGroup>();

			var startGroup = RoomGroup.FromTing(pStart);
			var goalGroup = RoomGroup.FromTing(pGoal);

			RoomGroupNode startNode = new RoomGroupNode() {
				roomGroup = startGroup,
				gscore = 0f,
				fscore = CostEstimate(startGroup, goalGroup),
				hasBeenTouched = true,
				depth = 0,
			};

			var unvisited = new List<RoomGroupNode>() { startNode };
			
			int iterations = 0;

			while(unvisited.Count > 0) {
				iterations++;

				if(iterations > 1000) {
					D.Log("Hit maximum iterations when doing MimanPathfinder2 search from " + pStart.position + " to " + pGoal.position);
					return new MimanPath() {
						status = MimanPathStatus.NO_PATH_FOUND,
						iterations = iterations,
						tings = new Ting[] {},
					};
				}

				RoomGroupNode current = GetCheapest(unvisited);
				
				if(current.prev != null) {
					current.depth = current.prev.depth + 1;
				}

				#if LOG
				Console.WriteLine("Current node: " + current + ", with gscore " + current.gscore + " and depth: " + current.depth);
				#endif

				if(current.roomGroup.Equals(goalGroup)) {
					return new MimanPath() {
						status = MimanPathStatus.FOUND_GOAL,
						tings = GetListOfTingsLeadingThroughRoomGroups(current),
						iterations = iterations,
					};
				}
				
				unvisited.Remove(current);
				visited.Add(current.roomGroup);

				Dictionary<RoomGroup,Ting> linkedRooms = null;

				if(!_roomNetwork.linkedRoomGroups.TryGetValue(current.roomGroup, out linkedRooms)) {
					linkedRooms = new Dictionary<RoomGroup,Ting>();
				}
				
				#if LOG
				Console.WriteLine("Nr of linked rooms: " + linkedRooms.Count);
				#endif

				List<RoomGroupNode> neighbours = new List<RoomGroupNode>();
				foreach(var roomGroup in linkedRooms.Keys) {
					neighbours.Add(new RoomGroupNode() {
						roomGroup = roomGroup,
					});
				}

				foreach(RoomGroupNode neighbour in neighbours) {
					#if LOG
					Console.WriteLine("Testing out neighbour " + neighbour);
					#endif
					
					float tentativeGScore = current.gscore + ActualCost(current.roomGroup, neighbour.roomGroup);
					bool hasBeenVisited = visited.Contains(neighbour.roomGroup);
					
					if(hasBeenVisited && tentativeGScore > neighbour.gscore) {
						#if LOG
						//Console.WriteLine("It has been visited and its score is too high, lets skip it");
						#endif
						continue;
					}
					
					#if LOG
					//Console.WriteLine("Tentative gscore: " + tentativeGScore + ", neighbour score: " + neighbour.gscore);
					#endif
					
					if(!neighbour.hasBeenTouched || tentativeGScore < neighbour.gscore) {
						neighbour.hasBeenTouched = true;
						neighbour.prev = current;
						neighbour.gscore = tentativeGScore;
						neighbour.fscore = neighbour.gscore + CostEstimate(neighbour.roomGroup, goalGroup);
						if(!unvisited.Contains(neighbour)) {
							#if LOG
							//Console.WriteLine("Adding neighbour " + neighbour.ting.name + " to unvisited list.");
							#endif
							unvisited.Add(neighbour);
						}
					}
				}
				
			}

			return new MimanPath() {
				status = MimanPathStatus.NO_PATH_FOUND,
				iterations = iterations,
				tings = new Ting[] {},
			};
		}

		Ting[] GetListOfTingsLeadingThroughRoomGroups (RoomGroupNode current)
		{
#if LOG
			D.Log("Will GetListOfTingsLeadingThroughRoomGroups!");
			D.Log("Starting at goal of " + current);
#endif

			var roomGroupsPath = new List<RoomGroup>();
			while(current.prev != null) {
				roomGroupsPath.Add(current.roomGroup);
				current = current.prev;
			}
			roomGroupsPath.Add(current.roomGroup);
			roomGroupsPath.Reverse(); // REVERSE!

			string[] roomGroupsAsStrings = roomGroupsPath.Select(r => r.ToString()).ToArray();
#if LOG
			D.Log("Room groups to go through: " + string.Join(", ", roomGroupsAsStrings));
#endif

			List<Ting> tingsToInteractWith = new List<Ting>();

			for(int i = 0; i < roomGroupsPath.Count - 1; i++) {
				var a = roomGroupsPath[i];
				var b = roomGroupsPath[i + 1];
#if LOG
				D.Log("Will find ting that connects " + a + " with " + b);
#endif

				var connections = _roomNetwork.linkedRoomGroups[a];

				Ting ting = connections[b];

				tingsToInteractWith.Add(ting);
			}

			return tingsToInteractWith.ToArray();
		}

		private RoomGroupNode GetCheapest(List<RoomGroupNode> pNodes) 
		{
			if(pNodes.Count == 0) {
				throw new Exception("Can't find cheapest node in pNodes since it is empty");	
			}
			
			var lowest = pNodes[0];
			
			foreach(var node in pNodes) {
				if(node.gscore < lowest.gscore) {
					lowest = node;
				}
			}
			
			return lowest;
		}

		private float CostEstimate(RoomGroup pStart, RoomGroup pGoal) {
			if(pStart.Equals(pGoal)) {
				return 1f;
			}
			int dx = pStart.room.worldPosition.x - pGoal.room.worldPosition.x;
			int dy = pStart.room.worldPosition.y - pGoal.room.worldPosition.y;
			float estimate = Math.Abs(dx) + Math.Abs(dy);
#if LOG	
			D.Log("Estimate: " + estimate);
#endif
			return estimate;
		}

		Dictionary<RoomGroup, Dictionary<RoomGroup, float>> _costCache = new Dictionary<RoomGroup, Dictionary<RoomGroup, float>>();

		// TODO: this function seems WEIRD!
		private float ActualCost(RoomGroup pStart, RoomGroup pGoal) {
			if(pStart.Equals(pGoal)) {
				return 50f;
			}

			return 100.0f;
			
			/*Dictionary<RoomGroup, float> innerCache;

			if (_costCache.TryGetValue (pStart, out innerCache)) {
				float cachedCost;
				if (innerCache.TryGetValue (pGoal, out cachedCost)) {
					return cachedCost;
				}
			}

			float cost = pStart.room == pGoal.room ? 0.1f : 1.0f;
			
			if (innerCache == null) {
				innerCache = new Dictionary<RoomGroup, float> ();
				_costCache.Add (pStart, innerCache);
			}
			
			innerCache [pGoal] = cost;
			
			return cost;*/
		}
				
		private HashSet<Ting> GetLinkedExitsInSameRoom(Ting pTing) {
			D.isNull(pTing, "pTing is null in GetLinkedExitNodes");
			
			var exitNodes = new HashSet<Ting>();

			foreach(var exit in GetExits(pTing.room)) {
				if(InSameRoomAndLinked(pTing, exit)) {
					exitNodes.Add(exit);
				}
			}

			return exitNodes;
		}

		private HashSet<Ting> GetLinkedExitsInOtherRooms(Ting pTing) {

			var exitNodes = new HashSet<Ting>();

			if(pTing is IExit) {
				var target = (pTing as IExit).GetLinkTarget();

				if (target != null) {
					exitNodes.Add(target);
				}
				
				// Extra exits if it's an elevator door
				var elevator = pTing as Door;
				if(elevator != null && elevator.elevatorAlternatives.Length > 0) {
					foreach(var alternative in elevator.elevatorAlternatives) {
						var elevatorExit = _tingRunner.GetTing(alternative);
						exitNodes.Add(elevatorExit);
					}
					#if LOG
					string[] exitsAsStrings = exitNodes.Select(n => n.ToString()).ToArray();
					//D.Log("Elevator " + elevator + " has exits: " + string.Join(", ", exitsAsStrings));
					#endif
				}
			}
			
			return exitNodes;
		}

		private bool InSameRoomAndLinked(Ting a, Ting b) {
			if(a.tile == null || b.tile == null) {
				#if LOG
				Console.WriteLine("Link between " + a + " and " + b + " not allowed");
				#endif
				return false;
			}
			
			int groupA = a.tile.group;
			int groupB = b.tile.group;
			
			if(groupA == -1) {
				D.Log("Found tile " + a.tile + " with group -1");
			}
			else if(groupB == -1) {
				D.Log("Found tile " + b.tile + " with group -1");
			}
			
			return groupA == groupB;
		}

		private Ting[] GetExits(Room pRoom) {
			string roomName = pRoom.name;
			Ting[] exits;
			if (_exitForRoomsCache.TryGetValue (roomName, out exits)) {
				return exits;
			} else {
				return BuildExitsCacheForRoom (roomName);
			}
		}

		Dictionary<string, Ting[]> _exitForRoomsCache = new Dictionary<string, Ting[]>();
		
		Ting[] BuildExitsCacheForRoom (string pRoomName) {
			//D.Log("Building exits cache for " + pRoomName + ": ");
			var exits = new HashSet<Ting>();
			var tingsInRoom = _tingRunner.GetTingsInRoom(pRoomName);
			foreach(var ting in tingsInRoom) {
				if (ting is IExit) {
					exits.Add(ting);
					//D.Log(" - " + ting.name);
				}
			}
			var exitsArray = exits.ToArray ();
			_exitForRoomsCache [pRoomName] = exitsArray;
			return exitsArray;
		}
	}

	public struct RoomGroup {

		public RoomGroup(Room pRoom, int pGroup) {
			room = pRoom;
			group = pGroup;
		}

		public Room room;
		public int group;

		static bool FindCloseAlternativeTile (Ting pTing, out PointTileNode tile)
		{
			var room = pTing.room;
			int x = pTing.localPoint.x;
			int y = pTing.localPoint.y;

			for(int dist = 1; dist < 20; dist++) {
				var north = room.GetTile(new IntPoint(x, y + 1));
				var east = room.GetTile(new IntPoint(x + 1, y));
				var south = room.GetTile(new IntPoint(x, y - 1));
				var west = room.GetTile(new IntPoint(x - 1, y));
				var tiles = new List<PointTileNode>() { north, east, south, west };
				Shuffle(tiles);
				foreach(var t in tiles) {
					if(t != null && t.group > -1) {
						tile = t;
						return true;
					}
				}
			}

			tile = new PointTileNode(IntPoint.Zero, pTing.room);

			return false;
		}

		private static Random rng = new Random();  
		
		public static void Shuffle<T>(IList<T> list)  
		{  
			int n = list.Count;  
			while (n > 1) {  
				n--;  
				int k = rng.Next(n + 1);  
				T value = list[k];  
				list[k] = list[n];  
				list[n] = value;  
			}  
		}

		public static RoomGroup FromTing(Ting pTing) {
			D.isNull(pTing, "pTing is null in RoomGroup.FromTing()");

			if(pTing.tile == null) {
				(pTing as MimanTing).MaybeFixGroupIfOutsideIslandOfTiles(); // only do this on stationary things that are never moved during the whole game
			}

			if(pTing.tile == null) {
				D.Log(pTing.name + " is on null tile, will find a close alternative tile");
				PointTileNode alternativeTile;
				if(FindCloseAlternativeTile(pTing, out alternativeTile)) {
					D.Log("Will use tile " + alternativeTile + " instead");
					return new RoomGroup(pTing.room, alternativeTile.group);
				}
			}

			if(pTing.tile == null) {
				D.Log(pTing.name + " is still on null tile, will fail.");
				// TODO: what is the correct default tile group?!
				return new RoomGroup(pTing.room, -1);
			}
			else {
				return new RoomGroup(pTing.room, pTing.tile.group);
			}
		}

		public override bool Equals (object obj)
		{
			RoomGroup other = (RoomGroup)obj;
			return room == other.room && group == other.group;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override string ToString ()
		{
			return string.Format ("({0} #{1})", room.name, group);
		}
	}

	public class RoomNetwork {

		public Dictionary<RoomGroup, Dictionary<RoomGroup, Ting>> linkedRoomGroups = new Dictionary<RoomGroup, Dictionary<RoomGroup, Ting>>();

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();

			foreach(var roomGroup in linkedRoomGroups.Keys) {
				sb.Append(roomGroup + " => ");
				Dictionary<RoomGroup, Ting> tingsForRoomGroups = linkedRoomGroups[roomGroup];
				foreach(var linkedRoomGroup in tingsForRoomGroups.Keys) {
					sb.Append(linkedRoomGroup + " (via " + tingsForRoomGroups[linkedRoomGroup] + "), ");
				}
				sb.Append("\n");
			}

			return sb.ToString();
		}

	}

	public class RoomNode {

	}
}