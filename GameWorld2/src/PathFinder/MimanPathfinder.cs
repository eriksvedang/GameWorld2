//#define LOG
//#define PROFILE

using System;
using System.Collections.Generic;
using TingTing;
using GameTypes;
using System.Text;
using System.Diagnostics;
using Pathfinding;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameWorld2
{
	/*
	
		Pathfinder that has knowledge about how the game actually works.
		Finds the best series of tings to interact with to get from one room to another (anywhere in the world).
	
	*/
	
	public class MimanPathfinder_DEPRECATED
	{
		TingRunner _tingRunner;

		public MimanPathfinder_DEPRECATED (TingRunner pTingRunner)
		{
			_tingRunner = pTingRunner;
		}

//		private IEnumerable<Ting> GetExits(Room pRoom) {
//			foreach (Door d in pRoom.GetTingsOfType<Door>()) {
//				yield return d;
//			}
//			foreach (Portal p in pRoom.GetTingsOfType<Portal>()) {
//				yield return p;
//			}
//		}

		Dictionary<string, Ting[]> _exitForRoomsCache = new Dictionary<string, Ting[]>();

		Ting[] BuildExitsCacheForRoom (string pRoomName) {
			var exits = new List<Ting>();
			var tingsInRoom = _tingRunner.GetTingsInRoom(pRoomName);
			foreach(var ting in tingsInRoom) {
				if (ting is IExit) {
					exits.Add(ting);
				}
			}
			var exitsArray = exits.ToArray ();
			_exitForRoomsCache [pRoomName] = exitsArray;
			return exitsArray;
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
		
		public MimanPath Search(Ting pStart, Ting pGoal) {
			#if PROFILE
			Stopwatch sw = new Stopwatch();
			sw.Start();
			#endif

			var path = AStar(pStart, pGoal);

			#if PROFILE
			sw.Stop();
			if(sw.Elapsed.TotalSeconds > 0.0f) {
				D.Log("Search from " + pStart.name + " at " + pStart.position + " to " + pGoal.name + " at " + pGoal.position + " with result: " + path.status + " took " + sw.Elapsed.TotalSeconds + " s. Iterations: " + path.iterations);
			}
			#endif

			return path;
		}
		
		private MimanPath AStar(Ting pStart, Ting pGoal) {

			//List<string> visitedLog = new List<string>();

			if (pStart.room == pGoal.room) {
				return new MimanPath() {
					status = MimanPathStatus.IN_THE_SAME_ROOM_ALREADY
				};
			}

			var visited = new HashSet<Ting>();
			
			Node startNode = new Node() {
				ting = pStart,
				gscore = 0f,
				fscore = CostEstimate(pStart, pGoal),
				hasBeenTouched = true,
				depth = 0,
			};

			var unvisited = new List<Node>() { startNode };

			int iterations = 0;

			while(unvisited.Count > 0) {
				iterations++;

//				if(iterations > 1000) {
//					D.Log("Pathfinding from " + pStart.name + " to " + pGoal.name +  " took too many iterations, will bail out");
//					return new MimanPath() {
//						status = MimanPathStatus.NO_PATH_FOUND,
//					};
//				}

				Node current = GetCheapest(unvisited);

				if(current.prev != null) {
					current.depth = current.prev.depth + 1;
				}

				//visitedLog.Add(current.ting.name);

#if LOG
				Console.WriteLine("Current: " + current.ting.name + " at world pos " + current.ting.worldPoint + " with gscore " + current.gscore + " and depth: " + current.depth);
#endif

				if(current.ting.room == pGoal.room) {
					return new MimanPath() {
						status = MimanPathStatus.FOUND_GOAL,
						tings = SearchBackwardsForTingsOnTheWay(current),
						iterations = iterations,
						//visitedLog = visitedLog,
					};
				}
				
				unvisited.Remove(current);
				visited.Add(current.ting);

#if LOG
				Console.WriteLine("Added node " + current.ting.name + " to visited set");
#endif
								
//				if(current.depth > 20) {
//					D.Log("Will ignore node with '" + current.ting.name + "' since depth is to deep");
//					continue;	
//				}
				
				var neighbours = GetLinkedExitNodes(current.ting);

#if LOG
				Console.WriteLine("Nr of linked exit nodes: " + neighbours.Count);
#endif

				foreach(Node neighbour in neighbours) {
#if LOG
					Console.WriteLine("Testing out neighbour " + neighbour.ting.name);
#endif

					float tentativeGScore = current.gscore + ActualCost(current.ting, neighbour.ting);
					bool hasBeenVisited = visited.Contains(neighbour.ting);

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
						neighbour.fscore = neighbour.gscore + CostEstimate(neighbour.ting, pGoal);
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
			};
		}

		private Ting[] SearchBackwardsForTingsOnTheWay(Node pFinalNode) {
			var tings = new List<Ting>();

			var n = pFinalNode;

			while (n.prev != null) {
				tings.Add(n.ting);
				n = n.prev;
			}

			// At this point the list is backwards and contains all the pairs of linked portals/doors
			// Let's fix that now:

			tings.Reverse();

			var fixedTings = new List<Ting>();

			Ting prevTing = null;
			foreach(var t in tings) {
				if (prevTing != null && (prevTing as IExit).GetLinkTarget() == t) {
#if LOG
					Console.WriteLine("Skipping " + t + " since it's pointed to by previous ting in the list");
#endif
				} else {
					fixedTings.Add(t);
				}
				prevTing = t;
			}

			return fixedTings.ToArray();
		}

		/*private bool HasBeenVisited(List<Node> pVisitedNodes, Node pNode) {
			foreach (Node n in pVisitedNodes) {
				if (n.ting == pNode.ting) {
					return true;
				}
			}
			return false;
		}*/

		private bool LinkAllowed(Ting a, Ting b) {

			// Nullcheck not needed?
			/*a == null || b == null || */ 

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
		
		private HashSet<Node> GetLinkedExitNodes(Ting pTing) {
			//D.isNull(pTing, "pTing is null");
			
			var exitNodes = new HashSet<Node>();
			foreach(var exit in GetExits(pTing.room)) {
				if(LinkAllowed(pTing, exit)) {
					exitNodes.Add(new Node() {
						ting = exit
					});
				}
			}

			if(pTing is IExit) {
				var target = (pTing as IExit).GetLinkTarget();
				if (target != null) {
					if(IsTargetDoorInABusyElevator(target as Door)){
						//D.Log("Target door " + target + " is in a busy elevator");
					} else {
						exitNodes.Add(new Node() {
							ting = target
						});
					}
				}

				// Extra exits if it's an elevator door
				var elevator = pTing as Door;
				if(elevator != null && elevator.elevatorAlternatives.Length > 0) {
					foreach(var alternative in elevator.elevatorAlternatives) {
						var elevatorExit = _tingRunner.GetTing(alternative);
						exitNodes.Add(new Node() {
							ting = elevatorExit
						});
					}
#if LOG
					string[] exitsAsStrings = exitNodes.Select(n => n.ToString()).ToArray();
					//D.Log("Elevator " + elevator + " has exits: " + string.Join(", ", exitsAsStrings));
#endif
				}
			}

			return exitNodes;
		}

		bool IsTargetDoorInABusyElevator(Door pTargetDoor) {
			return pTargetDoor != null && pTargetDoor.elevatorAlternatives.Length > 0 && pTargetDoor.room.GetTingsOfType<Character>().Count > 0; 
		}
		
		private Node GetCheapest(List<Node> pNodes) 
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
		
		private float CostEstimate(Ting pStart, Ting pGoal) {
			var start = (pStart as IExit);
			if(start != null && start.GetLinkTarget() == pGoal) {
				return 0f;
			}
			int dx = pStart.worldPoint.x - pGoal.worldPoint.x;
			int dy = pStart.worldPoint.y - pGoal.worldPoint.y;
			return Math.Abs(dx) + Math.Abs(dy);
		}

		Dictionary<Ting, Dictionary<Ting, float>> _costCache = new Dictionary<Ting, Dictionary<Ting, float>>();
			
		private float ActualCost(Ting pStart, Ting pGoal) {

			var start = (pStart as IExit);
			if(start != null && start.GetLinkTarget() == pGoal) {
				return 0f;
			}

			Dictionary<Ting, float> innerCache;
			if (_costCache.TryGetValue (pStart, out innerCache)) {
				float cachedCost;
				if (innerCache.TryGetValue (pGoal, out cachedCost)) {
					return cachedCost;
				}
			}

//			int dx = pStart.worldPoint.x - pGoal.worldPoint.x;
//			int dy = pStart.worldPoint.y - pGoal.worldPoint.y;
//			float cost = (float)Math.Sqrt(dx * dx + dy * dy);

			float cost = pStart.room == pGoal.room ? 0.1f : 1.0f;

			if (innerCache == null) {
				innerCache = new Dictionary<Ting, float> ();
				_costCache.Add (pStart, innerCache);
			}

			innerCache [pGoal] = cost;

			return cost;
		}
	}
				
	public class Node {
		public Ting ting;
		public float gscore;
		public float fscore;
		public Node prev;
		public bool hasBeenTouched = false;
		public int depth;

		public override string ToString ()
		{
			return string.Format ("[Node] " + ting.name);
		}

		public override int GetHashCode ()
		{
			//try {
				return ting.worldPoint.GetHashCode();
			/*} catch(Exception e) {
				D.Log ("Error in node for ting " + ting.name + ": " + e);
				throw e;
			}*/
		}
	}
	
	public class ApproximateSqrt
	{
		public static float Sqrt(float z)
		{
			if (z == 0) return 0;
			FloatIntUnion u;
			u.tmp = 0;
			u.f = z;
			u.tmp -= 1 << 23; /* Subtract 2^m. */
			u.tmp >>= 1; /* Divide by 2. */
			u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
			return u.f;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct FloatIntUnion
		{
			[FieldOffset(0)]
			public float f;

			[FieldOffset(0)]
			public int tmp;
		}
	}
}
