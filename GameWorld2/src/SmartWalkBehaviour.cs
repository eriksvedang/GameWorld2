//#define PROFILE
//#define DEEP
//#define LOG

using System;
using Pathfinding;
using TingTing;
using GameTypes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GameWorld2
{
	public class SmartWalkBehaviour
	{
		public static Logger s_logger = new Logger();

		private static PathSolver _tilePathSolver = new PathSolver();
		private MimanPathfinder2 _mimanPathFinder;

		private Character _character;
		private RoomRunner _roomRunner;
		private TingRunner _tingRunner;
		private WorldSettings _worldSettings;

		private Path _tilePath;
		private PointTileNode _startTileNode;
		private PointTileNode _goalTileNode;

		private MimanPath _mimanPath = new MimanPath();

		public SmartWalkBehaviour (Character pCharacter, RoomRunner pRoomRunner, TingRunner pTingRunner, WorldSettings pWorldSettings)
		{
#if LOG && DEEP
			s_logger.Log("Created SmartWalkBehaviour for character " + pCharacter.name);
#endif

			_character = pCharacter;
			_roomRunner = pRoomRunner;
			_tingRunner = pTingRunner;
			_worldSettings = pWorldSettings;

			_mimanPathFinder = new MimanPathfinder2(_tingRunner, _roomRunner);
			//_mimanPathFinder = new MimanPathfinder_DEPRECATED(_tingRunner);

			CalculateFinalTargetPosition();
			bool startWalkingAgain = RefreshPaths();
			if(startWalkingAgain) {
				_character.StartAction("Walking", null, Character.LONG_TIME, Character.LONG_TIME);
			}
			else {

			}
		}

		private void CalculateFinalTargetPosition()
		{			
			if(_character.walkMode == Character.WalkMode.WALK_TO_TING_AND_HACK ||
			   _character.walkMode == Character.WalkMode.WALK_TO_TING_AND_INTERACT ||
			   _character.walkMode == Character.WalkMode.WALK_TO_TING_AND_USE_HAND_ITEM) 
			{
				_character.finalTargetPosition = _character.finalTargetTing.position;
			}
			else {
				// it's already correct
				//D.Log("WalkTo - Final target position for " + _character.name + ": " + _character.finalTargetPosition);
			}
		}

		public void Update (float dt)
		{
#if LOG && DEEP
			s_logger.Log("### Calling update in SmartWalkBehaviour, " + _character + " has action " + _character.actionName);
#endif

			if (_tilePath.status == PathStatus.DESTINATION_UNREACHABLE) {
#if LOG
				s_logger.Log("_tilePath.status == PathStatus.DESTINATION_UNREACHABLE, returning from update");
#endif
				return;
			}
			
			if (_mimanPath.status == MimanPathStatus.NO_PATH_FOUND) {
#if LOG
				s_logger.Log("_mimanPath.status == MimanPathStatus.NO_PATH_FOUND, returning from update");
#endif
				return;
			}
			
			if (_character.actionName == "Walking") {
				Move (dt);
			} else if (_character.actionName == "") {
				StartWalkingAgain ();
			}
		}

		public void StartWalkingAgain()
		{
#if LOG 
			s_logger.Log("# Time to start walking again");
#endif
			bool shouldStartActionWalking = RefreshPaths();
			if(shouldStartActionWalking) {
#if LOG
				s_logger.Log("Started action Walking!");
#endif
				_character.StartAction("Walking", _character.actionOtherObject, Character.LONG_TIME, Character.LONG_TIME);
				_character.walkTimer = 0f;
			} else {
#if LOG
				s_logger.Log("No success so will not start action walking");
#endif
				//_character.CancelWalking(); // THIS LINE WAS A BAD IDEA!!! Tried to stop people from start walking to unreachable places but not good solution for that
			}
		}

		bool FastForwardWalkIteratorToCharacterPosition()
		{
			PointTileNode p;
			while (true) {
				if (_character.walkIterator > _tilePath.nodes.Length - 1) {
#if LOG
					s_logger.Log("walkIterator for " + _character.name + " is exceeds length!");
#endif
					return false;
				}
				p = _tilePath.nodes[_character.walkIterator];
				if (p.position == _character.position) {
#if LOG
					s_logger.Log("Found character " + _character.name + " at position " + p.position + ", continuing from there");
#endif
					return true;
				}
				_character.walkIterator++;
			}
		}

		static Type[] notTrueObstacles = new Type[] { typeof(Door), typeof(Portal), typeof(Point) };
		static Type[] notTrueObstaclesIncludingCharacters = new Type[] { typeof(Door), typeof(Portal), typeof(Point), typeof(Character) };

		private void Move(float dt)
		{
#if LOG && DEEP
			s_logger.Log("# Move");
#endif
			
			if (_tilePath.status == PathStatus.DESTINATION_UNREACHABLE) {
#if LOG
				s_logger.Log("DESTINATION_UNREACHABLE, returning");
#endif
				return;
			}

			_character.walkTimer += _character.calculateFinalWalkSpeed() * dt;

			if(_character.walkTimer >= 1f) {
				_character.walkTimer = 0f;
				_character.walkIterator++;
				
				if(_tilePath.nodes == null) {
#if LOG
					s_logger.Log("_tilePath.nodes == null");
#endif
					_character.CancelWalking();
					return;
				}
				
				if(_character.walkIterator > _tilePath.nodes.Length - 1) {
#if LOG
					s_logger.Log("Walk iterator > _path.nodes.Length");
#endif
					_character.CancelWalking();
					return;
				}

				PointTileNode newNode = _tilePath.nodes[_character.walkIterator];

#if LOG
				if(newNode.HasOccupantsButIgnoreSomeTypes(notTrueObstacles)) {
					_character.logger.Log(_character.name + " is trying to move to a occupied tile (" + newNode.GetOccupantsAsString() + ") at " + newNode.position + "");
				}
#endif
				
				// Look ahead to final position when getting close to final position
				// TODO: THIS CODE IS PROBLEMATIC

				if(_character.walkIterator > _tilePath.nodes.Length - 7) {
					var lastNode = _tilePath.nodes[_tilePath.nodes.Length - 1];
					var lastTile = _character.room.GetTile(lastNode.position.localPosition);
					if(lastTile.HasOccupants<Character>(_character)) {
						//D.Log("The object(s) " + lastTile.GetOccupantsAsString() + " is/are at the position where " + _character + " wanted to end up!");
						_character.CancelWalking ();
						_character.timetableTimer = 1.0f;
						return;
					}
				}






				/*
				int lookAheadIndex = Math.Min(_character.walkIterator + 4, _tilePath.nodes.Length - 1);
				var lookAheadNode = _tilePath.nodes[lookAheadIndex];
				var lookAheadTile = _character.room.GetTile(lookAheadNode.position.localPosition);
				if(lookAheadTile.HasOccupants<Character>(_character)) {
					D.Log("Some character is at the position where " + _character + " wants to end up!");
					_character.CancelWalking();
					return;
				}
				*/
				
				_character.direction = (newNode.localPoint - _character.localPoint).ToDirection();
				_character.position = newNode.position;

#if LOG && DEEP
				s_logger.Log("Reached new position: " + _character.position);
#endif

				AnalyzeNewTile();
			}
		}

	 	public void AnalyzeNewTile()
		{
			if (_character.position == _character.finalTargetPosition) {
				ReachedFinalPosition ();
			} 
			else if (_character.position == _character.targetPositionInRoom)
			{
				if (_mimanPath.tings.Length == 0) {
#if LOG
					s_logger.Log("No tings left on _mimanPath for character " + _character.name);
#endif
				} else {
#if LOG
					s_logger.Log(_character.name + " is at IExit interaction point");
#endif
					_character.walkIterator = 0;
					_character.targetPositionInRoom = WorldCoordinate.NONE;
					Ting nextTing = _mimanPath.tings[0];
#if LOG
					s_logger.Log("Next ting is " + nextTing.name);
#endif
					if(nextTing is Door && (nextTing as Door).isLocked) {
#if LOG
						s_logger.Log(_character.name + " must try to open the locked door with a key");
#endif
						UseKeyToGetThroughDoor(nextTing as Door);
					}
					else if(_character.handItem is Key) {
#if LOG
						s_logger.Log ("Putting away key in hand");
#endif
						_character.PutHandItemIntoInventory(); // OBS! this messes with the BeatGame script since it applies to Sebastian too
					}
					else {
#if LOG
						s_logger.Log(_character.name + " will try to go through the IExit by interacting with it");
#endif

						var door = nextTing as Door;

						if(_character.hasHackdev && _character.timetableName != "" && door != null) {
							string originalSourceCode = door.sourceCodeDispenser.GetSourceCode(door.masterProgram.sourceCodeName).content;

							if(door.masterProgram.sourceCodeContent != originalSourceCode) {
								D.Log("Code for the door " + door.name + " that " + _character + " is using has been hacked, she/he will try to fix.");
								door.masterProgram.sourceCodeContent = originalSourceCode;
								_character.handItem = _character.hackdev;
								_character.Hack(door);
								_character.timetableTimer = 15f;
								return;
							}
						}

						_character.InteractWith(nextTing);
					}					
				}
			} 
			else {
#if LOG && DEEP
				s_logger.Log ("Analyzed new tile but it was boring: " + _character.position.localPosition + " in " + _character.position.roomName);
#endif
			}
		}

		public void UseKeyToGetThroughDoor (Door pDoor)
		{
			Key key = null;
			List<Key> keys = new List<Key>();
			foreach(var item in _character.inventoryItems) {
				if(item is Key) {
					keys.Add(item as Key);
				}
			}
			if (_character.handItem is Key) {
				keys.Add (_character.handItem as Key);
			}
			if (keys.Count > 0) {
				int r = Randomizer.GetIntValue(0, keys.Count);
				key = keys[r];
			}

			if(key == null) {
//#if LOG
				D.Log("No key found for character " + _character.name + " at door " + pDoor);
//#endif
				_character.CancelWalking();
				return;
			}
			
			if(_character.handItem != key) {
				_character.MoveHandItemToInventory();
				_character.handItem = key;
			}
			
			_character.UseHandItemToInteractWith(pDoor);
			pDoor.autoLockTimer = 4f;

		}
		
		// Returns true if the character should start walking again
		private bool RefreshPaths()
		{
			//Console.WriteLine("RefreshPaths()");

			if (_character.finalTargetPosition == WorldCoordinate.NONE) {
#if LOG
				s_logger.Log(_character + " is trying to refresh path with finalTargetPosition set to NONE");
#endif
				_worldSettings.Notify (_character.name, "Can't walk there");
				return false;
			}
			/*else if (_character.position == _character.finalTargetPosition) {
				ReachedFinalPosition();
				return false;
			} */
			else if (_character.room.name == _character.finalTargetPosition.roomName) {
#if LOG
				s_logger.Log(_character + " is in final room");
#endif
				return TilePathfindToTargetPositionInRoom();
			} else {
#if LOG
				s_logger.Log(_character + " is not in final room yet");
#endif

				bool foundPathThroughRooms = MimanPathfindToTargetRoom();
				if (foundPathThroughRooms) {
					return TilePathfindToTargetPositionInRoom();					
				} else {
#if LOG
					s_logger.Log(_character + " didn't find path through rooms from " + _character.position + " to " + _character.finalTargetPosition + " so will not do tile pathfinding in current room");
#endif
					return false;
				}
			}
		}
		
		// Returns true if the character should start walking again
		private bool TilePathfindToTargetPositionInRoom()
		{
#if PROFILE
			Stopwatch sw = new Stopwatch();
			sw.Start();
#endif

			_tilePath = Path.EMPTY;

			_startTileNode = _character.tile;

			if(_startTileNode == null) {
				TryToFindAlternativeStartingTile();

				if (_startTileNode != null) {
					//D.Log (_character + " has no start tile, will set his pos to alternative start tile " + _startTileNode);
					_character.StartAction ("", null, 0f, 0f);
					_character.position = _startTileNode.position;
				} else {
					//D.Log (_character + " has no start tile but didn't find alternative start tile");
				}
			}

			Room finalRoom = _roomRunner.GetRoom(_character.finalTargetPosition.roomName);

			if (_character.room == finalRoom) {
				if (_character.finalTargetTing != null) {
					// Try to go to the closest interaction point instead of straight to the actual ting position 
					// (which will lead to an error)

#if LOG
					s_logger.Log(_character + " with timetable " + _character.timetable + " will tilepathfind to closest interaction point of " + _character.finalTargetTing + " at position " + _character.finalTargetTing.position);
#endif

					bool ignoreCharacters = (_character.finalTargetTing is Door || _character.finalTargetTing is Portal);

					IntPoint closestInteractionPoint;
					if (_character.finalTargetTing.interactionPointsTryTheseFirst != null &&
						GetClosestInteractionPoint(_roomRunner, finalRoom, _character.tile, _character.finalTargetTing.interactionPointsTryTheseFirst, out closestInteractionPoint, _character, ignoreCharacters)) {
						_character.finalTargetPosition = new WorldCoordinate(_character.finalTargetPosition.roomName, closestInteractionPoint);	
#if LOG
						s_logger.Log("Found a closest target position for " + _character.name + ": " + _character.finalTargetPosition);
#endif
					} else if (GetClosestInteractionPoint(_roomRunner, finalRoom, _character.tile, _character.finalTargetTing.interactionPoints, out closestInteractionPoint, _character, ignoreCharacters)) {
						_character.finalTargetPosition = new WorldCoordinate(_character.finalTargetPosition.roomName, closestInteractionPoint);	
#if LOG
						s_logger.Log("Found a closest target position B for " + _character.name + ": " + _character.finalTargetPosition);
#endif
					} else {
						//Console.WriteLine(_character.name + " can't find closest target position for ting " + _character.finalTargetTing + " at pos " + _character.finalTargetTing.position);
						_character.CancelWalking ();
						_character.timetableTimer = Randomizer.GetValue(5.0f, 10.0f); // how long to wait? (used to be 0)
						return false;
					}
				}
				// The target in the room should be the same as the final target position
				_character.targetPositionInRoom = _character.finalTargetPosition;
			} else {
				// In this case the target position in the room should be already set by MimanPathfinding
			}

			_goalTileNode = _character.room.GetTile(_character.targetPositionInRoom.localPosition);

			if(_goalTileNode == null) {
				TryToFindAlternativeGoalTile();
			}

			if (_goalTileNode == null) {
#if LOG
				s_logger.Log ("Tilepathfinding for " + _character.name + " failed, can't find a goal tile node");
#endif
				return false;
			}

			if(_startTileNode.group > -1 && _goalTileNode.group > -1 &&
			   _startTileNode.group != _goalTileNode.group) 
			{
				_character.CancelWalking ();
				_character.timetableTimer = Randomizer.GetValue(5.0f, 10.0f); // how long to wait? (used to be 0)
#if LOG
				s_logger.Log("Tilepathfinding for " + _character.name + " failed, startTileNode at " + _startTileNode.position + " has group " + _startTileNode.group + " while _goalTileNode at " + _goalTileNode.position + " has group " + _goalTileNode.group);
#endif
				return false;
			}

			/*
			if(_character.HasRecentlyMadeAFailedPathFindingSearch(_startTileNode, _goalTileNode)) {
				_character.logger.Log(_character.name + " will not pathfind; has recently made a failed path finding search between " + _character.tile + " and " + _goalTileNode);
				_character.CancelWalking();
				return false;
			}
			*/

#if LOG
			s_logger.Log(_character.name + " will pathfind from " + _character.tile + " to " + _goalTileNode);
#endif

			_character.room.Reset();
			_tilePath = _tilePathSolver.FindPath(_startTileNode, _goalTileNode, _roomRunner, false);

			foreach(var tilePathNode in _tilePath.nodes) {
				Fence fence = tilePathNode.GetOccupantOfType<Fence>();
				if(fence != null) {
					//s_logger.Log("There is a Fence on the path of " + _character + ", will interact with that instead");
					_character.CancelWalking ();
					_character.WalkToTingAndInteract(fence);
					return false;
				}
			}

#if LOG
			s_logger.Log("Tile path solve from " + _startTileNode + " to " + _goalTileNode + ": " + _tilePath.status); // + ", PATH NODES " + _tilePath);
#endif

#if PROFILE
			sw.Stop();
			if(sw.Elapsed.TotalSeconds > 0.001f) {
				D.Log("TilePathFinding for " + _character + " from " + _startTileNode + " to " + _goalTileNode + " took " + sw.Elapsed.TotalSeconds + " s.");
			}
#endif

			if(_tilePath.status == PathStatus.DESTINATION_UNREACHABLE) {
#if LOG
				s_logger.Log("The destination was unreachable so we'll register it and cancel walking");
#endif
				//_character.RegisterFailedPathFindingSearch(_startTileNode, _goalTileNode);
#if LOG
				s_logger.Log(_character.name + ": The destination " + _goalTileNode.position + " was unreachable from " + _startTileNode.position);
#endif
				//_character.CancelWalking();
				return false;
			}
			else if(_tilePath.status == PathStatus.ALREADY_THERE)
			{
#if LOG
				s_logger.Log("Already there, analyze tile");
#endif

				AnalyzeNewTile();
				return false;
			}
			else if(_tilePath.status == PathStatus.FOUND_GOAL) {
#if LOG
				s_logger.Log("Found goal at " + _goalTileNode);
				s_logger.Log("Path for " + _character + ": " + string.Join(", ", _tilePath.nodes.Select(t => t.ToString()).ToArray()));
#endif

				return true;
			}
			else {
#if LOG
				s_logger.Log("This case should really not happen");
#endif
			}
			
			return false;
		}

		void TryToFindAlternativeStartingTile ()
		{
			_character.logger.Log(_character.name + " is not on a tile, will look for an alternative starting tile...");


			// TODO Need a better method for finding a good point that is both close to the current position and in the general direction of the goal tile (which is not set yet)

			PointTileNode node = _character.room.FindClosestTile(_character.localPoint);
			//PointTileNode node = Randomizer.RandNth (_character.room.tiles); // ALTERNATIVE METHOD, NOT VERY GOOD


			if(node != null) {
#if LOG
				_character.logger.Log("Found an alternative starting tile at " + node.worldPoint);
#endif
				_startTileNode = node;
			}
			else {
#if LOG
				_character.logger.Log("Found no alternative starting tile, it will be null (and no path will be found)");
#endif
			}
		}

		void TryToFindAlternativeGoalTile ()
		{
#if LOG
			_character.logger.Log(_character.name + " has an invalid goal tile, will look for an alternative one close to target pos " + _character.targetPositionInRoom.localPosition);
#endif
			PointTileNode node = _character.room.FindClosestFreeTile(_character.targetPositionInRoom.localPosition, _character.tileGroup);

			if(node != null) {
#if LOG
				_character.logger.Log("Found an alternative goal tile at " + node.worldPoint);
#endif
				_goalTileNode = node;
			}
			else {
#if LOG
				_character.logger.Log("Found no alternative goal tile, it will be null (and no path will be found)");
#endif
			}
		}

		private bool MimanPathfindToTargetRoom()
		{
#if PROFILE
			Stopwatch sw = new Stopwatch();
			sw.Start();
#endif

			Ting start = _character;
			Ting goal = null;

			if (_character.finalTargetTing != null) {
				goal = _character.finalTargetTing;
			} else {
				// HACK!!!
				// Since there is no specific ting to pathfind to we just grab one in the final room where we want to go
				// The final target position will not be affected since we have no final target ting

				string finalRoomName = _character.finalTargetPosition.roomName;
				
				if(finalRoomName == WorldCoordinate.UNDEFINED_ROOM) {
#if LOG
					s_logger.Log("No finalTargetTing and no final target position");
#endif
					return false;
				}
				
				Ting targetTing = null;
				var tingsInFinalRoom = _tingRunner.GetTingsInRoom(finalRoomName);
				if (tingsInFinalRoom.Length == 0) {
					D.Log(_character + ": No tings in final room " + finalRoomName + ", can't do room pathfinding to there!");
					return false;
				} else {
					targetTing = tingsInFinalRoom[0];
				}
				goal = targetTing;
				//throw new Exception("No ting to pathfind to for " + _character.name);
			}

			if(start == null) {
#if LOG
				s_logger.Log("start is null");
#endif
				return false;
			}
			if(goal == null) {
#if LOG
				s_logger.Log("goal is null");
#endif
				return false;
			}

			_mimanPath = _mimanPathFinder.Search(start, goal);
#if LOG
			s_logger.Log(_mimanPath.ToString());
#endif
			//D.Log(_character + " did pathfinding between rooms: " + _mimanPath.ToString());

#if PROFILE
			sw.Stop();
			if(sw.Elapsed.TotalSeconds > 0.015f) {
				D.Log("MimanRoomPathFinding for " + _character + " from " + start + "(" + start.position + ")" + " to " + goal + "(" + goal.position + ")" + " took " + sw.Elapsed.TotalSeconds + " s. Status = " + _mimanPath.ToString());
			}
#endif

			if (_mimanPath.status == MimanPathStatus.FOUND_GOAL) {
				if(_mimanPath.tings == null) {
					throw new Exception("tings == null in _mimanPath!");
				}
				else if(_mimanPath.tings.Length == 0) {
					throw new Exception("No tings in _mimanPath!");
				} else {
#if LOG
					s_logger.Log("Setting " + _character.name + "'s targetPositionInRoom to " + _mimanPath.tings[0].name + "'s interaction point");
#endif
					var firstTingToInteractWith = _mimanPath.tings[0];
					D.isNull(firstTingToInteractWith, "firstTingToInteractWith is null");
					_character.targetPositionInRoom = new WorldCoordinate(firstTingToInteractWith.room.name,
					                                                      firstTingToInteractWith.interactionPoints[0]);
					return true;
				}
			} else if (_mimanPath.status == MimanPathStatus.IN_THE_SAME_ROOM_ALREADY) {
				return true;
			} else if (_mimanPath.status == MimanPathStatus.NO_PATH_FOUND) {
#if LOG
				s_logger.Log("Can't find path through rooms, cancels walking.");
#endif
				_character.CancelWalking();
				return false;
			} else {
				throw new Exception("Failed to find matching case");
			}
		}

		private void ReachedFinalPosition()
		{
#if LOG
			s_logger.Log("Reached final position at " + _character.finalTargetPosition);
#endif

			if(_character.walkMode == Character.WalkMode.WALK_TO_POINT) {
#if LOG
				s_logger.Log(_character.name + " ending WALK_TO_POINT");
#endif
				_character.StopAction();
			}
			else if(_character.walkMode == Character.WalkMode.WALK_TO_TING_AND_INTERACT) {
#if LOG
				s_logger.Log(_character.name + " ending WALK_TO_TING_AND_INTERACT");
#endif

				if(_character.finalTargetTing == null) {
#if LOG
					s_logger.Log("Final target ting is null, it might have been destroyed while the character was walking to interact with it.");
#endif
				}
				else if(!CharacterIsAtInteractionPointOfFinalTargetTing()) {
#if LOG
					s_logger.Log(_character + "'s finalTargetTing " + _character.finalTargetTing + " is not within reach! Ting pos: " + _character.finalTargetTing.position + ", character pos: " + _character.position); //, it might have been moved while the character was walking to interact with it. Trying to walk to it again.");
#endif					
					if(_character.timetable != null) {
						D.Log(_character + " is at interaction point of his/her finalTargetTing, will reset the timetable timer to zero, has timetable: " + _character.timetable.name);
						_character.CancelWalking();
						_character.timetableTimer = 0f; // better to see if the behaviour can generate another target to walk to
					} else {
#if LOG
						s_logger.Log(_character + " will immedieately try to walk to the target again");
#endif
						_character.WalkToTingAndInteract(_character.finalTargetTing);
					}
					return;
				}
				else if(_character.finalTargetTing.canBePickedUp) {
#if LOG
					s_logger.Log(_character + " will pick up final target ting");
#endif
					_character.PickUp(_character.finalTargetTing);
				}
				else if(_character.CanInteractWith(_character.finalTargetTing)) {
#if LOG
					s_logger.Log(_character + " will interact with final target ting");
#endif
					_character.InteractWith(_character.finalTargetTing);
				}
				else {
					s_logger.Log(_character.name + " can't interact with " + _character.finalTargetTing.name);
				}
			}
			else if(_character.walkMode == Character.WalkMode.WALK_TO_TING_AND_HACK) {
#if LOG
				s_logger.Log(_character.name + " ending WALK_TO_TING_AND_HACK");
#endif
				if(_character.finalTargetTing == null) {
#if LOG
					s_logger.Log("Final target ting is null, it might have been destroyed while the character was walking to hack with it.");
#endif
				}
				else if(!CharacterIsAtInteractionPointOfFinalTargetTing()) {
#if LOG
					s_logger.Log(_character + "'s finalTargetTing " + _character.finalTargetTing + " is not within reach! Ting pos: " + _character.finalTargetTing.position + ", character pos: " + _character.position); //, it might have been moved while the character was walking to interact with it. Trying to walk to it again.");
#endif					
					_character.WalkToTingAndHack(_character.finalTargetTing as MimanTing);
				}
				else {
					MimanTing hackableTing = _character.finalTargetTing as MimanTing;
					D.isNull(hackableTing);
					_character.Hack(hackableTing);	
				}
			}
			else if(_character.walkMode == Character.WalkMode.WALK_TO_TING_AND_USE_HAND_ITEM) {
#if LOG
				s_logger.Log(_character.name + " ending WALK_TO_TING_AND_USE_HAND_ITEM");
#endif
				MimanTing finalTing = _character.finalTargetTing as MimanTing;
				D.isNull(finalTing);
				_character.UseHandItemToInteractWith(finalTing);			
			}

			_character.ClearWalkingData();
		}

		private bool CharacterIsAtInteractionPointOfFinalTargetTing ()
		{
			if (_character.finalTargetTing.room != _character.room) {
				return false;
			}

			foreach (IntPoint interactionPoint in _character.finalTargetTing.interactionPoints) {
				if(interactionPoint == _character.localPoint) {
					return true;
				}
			}

			return false;
		}

		// Returns true on success
		private static bool GetClosestInteractionPoint(RoomRunner pRoomRunner, Room pRoom, PointTileNode pStartTile, IntPoint[] pPossiblePoints, out IntPoint closestPoint, Character pCharacter, bool pIgnoreCharacters)
		{
			D.isNull(pRoom, "pRoom is null");
			D.isNull(pPossiblePoints, "possiblePoints is null");

			if (pRoom != pCharacter.room) {
				throw new Exception("Error for " + pCharacter.name + "! Can only pathfind to closest interaction point in the same room: " + pCharacter.room.name + ", tried to do it in: " + pRoom.name);
			}

			closestPoint = IntPoint.Zero;
			float shortestDistance = float.MaxValue;
			bool foundSomething = false;

#if LOG
			s_logger.Log("Trying to find closest interaction point for " + pCharacter + ", nr of possible points: " + pPossiblePoints.Length);
#endif

			foreach(IntPoint p in pPossiblePoints)
			{
				PointTileNode tileNode = pRoom.GetTile(p);
				if(tileNode == null) {
#if LOG
					s_logger.Log("Node at " + p + " was null, ignoring it");
#endif
					continue;
				}

				var ignoreList = notTrueObstacles;

				if (pIgnoreCharacters) {
					ignoreList = notTrueObstaclesIncludingCharacters;
				}

				if(tileNode.HasOccupantsButIgnoreSomeTypes(ignoreList)) {
#if LOG
					s_logger.Log("Will ignore node at " + p + " since it has occupants: " + tileNode.GetOccupantsAsString());
#endif
					continue;
				}

#if LOG
				s_logger.Log("Checking tile node " + tileNode);
#endif

				pRoom.Reset();
				var path = _tilePathSolver.FindPath(pStartTile, tileNode, pRoomRunner, false);

#if LOG
				s_logger.Log("RESULT Path from " + pStartTile + " to " + tileNode + ": " + path.status);
#endif
				//D.Log("RESULT Path from " + pStartTile + " to " + tileNode + ": " + path.status);

				D.isNull(path, "path is null");
				if((path.status == PathStatus.FOUND_GOAL || path.status == PathStatus.ALREADY_THERE) && path.pathLength < shortestDistance) {
					closestPoint = p;
					shortestDistance = path.pathLength;
					foundSomething = true;
				}

#if LOG
				s_logger.Log("path.status = " + path.status);
#endif
			}

			if(!foundSomething) {
#if LOG
				s_logger.Log(pCharacter + " at position " + pCharacter.position + " can't find an interaction point for final target " + pCharacter.finalTargetTing);
#endif
				return false;
			}

			return true;
		}
	}
}

