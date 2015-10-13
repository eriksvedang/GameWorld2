//#define LOG

using System;
using System.Collections.Generic;
using GameTypes;
using TingTing;
using GrimmLib;
using System.Linq;
using Pathfinding;

namespace GameWorld2
{
	public interface TimetableBehaviour 
	{
		// Returns the time until the timetable should be executed again
		float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings);
		void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner);
		bool IsAtFinalPartOfTask(Character pCharacter);
		void Reset();
	}

	public class Behaviour_BeAtPosition : TimetableBehaviour
	{
		WorldCoordinate _targetPosition;

		public Behaviour_BeAtPosition(WorldCoordinate pTargetPosition) 
		{
			_targetPosition = pTargetPosition;
		}

		private bool ThereYet(Character pCharacter)
		{
			return pCharacter.position == _targetPosition;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings) 
		{
			if(!ThereYet(pCharacter) && pCharacter.finalTargetPosition != _targetPosition) {
				#if LOG
				pCharacter.logger.Log("BeAtPosition behaviour tells " + pCharacter + " to walk to " + _targetPosition);
				#endif
				pCharacter.WalkTo(_targetPosition);
			}

			return 0f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return ThereYet(pCharacter);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {

		}

		public override string ToString()
		{
			return string.Format("[BeAtPosition] {0}", _targetPosition);
		}

		public void Reset() {}
	}
	
	public class Behaviour_RunStory : TimetableBehaviour
	{
		string _storyName;

		public Behaviour_RunStory(string pStoryName)
		{
			_storyName = pStoryName;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings) 
		{
			if(!pDialogueRunner.ConversationIsRunning(_storyName)) {
				pDialogueRunner.StartConversation(_storyName);
			}
			
			return 10f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return true;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public override string ToString()
		{
			return string.Format("[RunStory] {0}", _storyName);
		}

		public void Reset() {}
	}

	public class Behaviour_BeAtTing : TimetableBehaviour
	{
		string _tingName;
		Ting _targetTing;
		//bool _hasSnappedDir = false;

		public Behaviour_BeAtTing(string pTingName)
		{
			_tingName = pTingName;
		}

		private bool ThereYet(Character pCharacter)
		{
			if(_targetTing == null) {
				//D.Log(pCharacter + " (when executing ThereYet in BeAtTing) hasn't set targetTing " + _tingName);
				return false;
			}
			return pCharacter.position == _targetTing.position;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings) 
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute be at ting behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute be at ting behaviour");
				#endif
				return 1f;
			}

			if(_targetTing == null) {
				_targetTing = pTingRunner.GetTing(_tingName);
			}

			if (ThereYet(pCharacter)) {
				//if(!_hasSnappedDir) {
					pCharacter.direction = _targetTing.direction;
					//_hasSnappedDir = true;
				//}
			}
			else {
				if (pCharacter.finalTargetPosition != _targetTing.position) {
					#if LOG
					pCharacter.logger.Log("BeAtTing behaviour tells " + pCharacter + " to walk to " + _targetTing.position);
					#endif
					//pCharacter.WalkTo(_targetTing.position); // old way
					pCharacter.WalkToTingAndInteract(_targetTing);
				}
			}

			return 1f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return ThereYet(pCharacter);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public override string ToString()
		{
			return string.Format("[BeAtTing] {0}", _tingName);
		}

		public void Reset() {}
	}

	public class Behaviour_PlayTrumpet : TimetableBehaviour
	{
		string _tingName;
		Ting _targetTing;
		//bool _hasSnappedDir = false;

		public Behaviour_PlayTrumpet(string pTingName)
		{
			_tingName = pTingName;
		}

		private bool ThereYet(Character pCharacter)
		{
			if(_targetTing == null) {
				return false;
			}
			return pCharacter.position == _targetTing.position;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings) 
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute PlayTrumpet behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute PlayTrumpet behaviour");
				#endif
				return 1f;
			}

			if(_targetTing == null) {
				_targetTing = pTingRunner.GetTing(_tingName);
			}

			if (ThereYet(pCharacter)) {
				pCharacter.direction = _targetTing.direction;
				StartTrumpeting (pCharacter);
				return 30f;
			}
			else {
				if (pCharacter.finalTargetPosition != _targetTing.position) {
					#if LOG
					pCharacter.logger.Log("PlayTrumpet behaviour tells " + pCharacter + " to walk to " + _targetTing.position);
					#endif
					pCharacter.WalkToTingAndInteract (_targetTing);
				}
			}

			return 1f;
		}

		private void StartTrumpeting(Character pCharacter) {
			pCharacter.StartAction ("Trumpeting", null, Character.LONG_TIME, Character.LONG_TIME);
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return ThereYet(pCharacter);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {

		}

		public override string ToString()
		{
			return string.Format("[BeAtTing] {0}", _tingName);
		}

		public void Reset() {}
	}

	public class Behaviour_BeInRoom : TimetableBehaviour
	{
		string _roomName;

		public Behaviour_BeInRoom(string pRoomName)
		{
			_roomName = pRoomName;
		}

		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			bool inRoom = (pCharacter.room.name == _roomName);

			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute be in room behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute be in room behaviour");
				#endif
				return 1f;
			}

			if (!inRoom || (pCharacter.finalTargetPosition != WorldCoordinate.NONE && pCharacter.finalTargetPosition.roomName != _roomName)) {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " will pathfind to target room " + _roomName);
				#endif
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
			}
			else {

				if(pCharacter.actionName == "Angry" || pCharacter.seat == null || Randomizer.OneIn(7))
				{
					Seat randomSeat = InteractionHelper.GetRandomTingWhere<Seat>(pRoomRunner, pCharacter.room.name, (s => s.tile.group == pCharacter.tileGroup && !s.isBeingUsed));
					if(randomSeat != null) {
						#if LOG
						pCharacter.logger.Log(pCharacter.name + " will be told to sit on seat " + randomSeat.name + " by BeInRoom behaviour");
						#endif
						pCharacter.WalkToTingAndInteract(randomSeat);
					}
					else {
						//IntPoint randomPoint = Randomizer.RandNth (pCharacter.room.points);
						var randomPoint = InteractionHelper.GetAnyRandomTing<Point>(pTingRunner, _roomName, pCharacter.tileGroup);
						if(randomPoint != null) {
							pCharacter.WalkToTingAndInteract(randomPoint);
							#if LOG
							pCharacter.logger.Log(pCharacter + " at BeInRoom behavior (" + _roomName + "), will try walking to random point: " + randomPoint);
							#endif
						} else {
							#if LOG
							pCharacter.logger.Log(pCharacter + " at BeInRoom behavior (" + _roomName + "), found no random point.");
							#endif
						}
					}
				}
			}
			
			#if LOG
			pCharacter.logger.Log(pCharacter.name + " reached end of Execute() on BeInRoom behavior");
			#endif
			return Randomizer.GetValue(10f, 30f);
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public override string ToString()
		{
			return string.Format("[BeInRoom] {0}", _roomName);
		}

		public void Reset() {

		}
	}

	public class Behaviour_WorkWithModifier : TimetableBehaviour
	{
		string _roomName;
		
		public Behaviour_WorkWithModifier(string pRoomName)
		{
			_roomName = pRoomName;
		}
		
		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			bool inRoom = (pCharacter.room.name == _roomName);
			
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute WorkWithModifier behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute WorkWithModifier behaviour");
				#endif
				return 1f;
			}
			
			if (!inRoom || (pCharacter.finalTargetPosition != WorldCoordinate.NONE && pCharacter.finalTargetPosition.roomName != _roomName)) {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " will pathfind to target room " + _roomName);
				#endif
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
			}
			else {
				if(pCharacter.hackdev != null) {
					float r = Randomizer.GetIntValue(0, 30);
					if(r < 10) {
						var randomLamp = InteractionHelper.GetRandomTing<Lamp>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
						if(randomLamp != null && !randomLamp.HasNoFreeInteractionPoints()) {
							pCharacter.WalkToTingAndHack(randomLamp);
							pCharacter.MoveHandItemToInventory();
							pCharacter.handItem = pCharacter.hackdev;
							return Randomizer.GetValue(10f, 20f);
						}
					}
					else if(r < 20) {
						var randomComputer = InteractionHelper.GetRandomTing<Computer>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
						if(randomComputer != null && !randomComputer.HasNoFreeInteractionPoints()) {
							pCharacter.WalkToTingAndHack(randomComputer);
							pCharacter.MoveHandItemToInventory();
							pCharacter.handItem = pCharacter.hackdev;
							return Randomizer.GetValue(10f, 60f);
						}
					}
					else {
						// fall through
					}
				}

				if(pCharacter.actionName == "Angry" || pCharacter.seat == null || Randomizer.OneIn(7))
				{
					Seat randomSeat = InteractionHelper.GetRandomTing<Seat>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
					if(randomSeat != null && !randomSeat.isBeingUsed) {
						pCharacter.WalkToTingAndInteract(randomSeat);
					}
					else {
						var randomPoint = InteractionHelper.GetAnyRandomTing<Point>(pTingRunner, _roomName, pCharacter.tileGroup);
						pCharacter.WalkToTingAndInteract(randomPoint);
					}
				}
			}
			
			#if LOG
			pCharacter.logger.Log(pCharacter.name + " reached end of Execute() on WorkWithModifier behavior");
			#endif
			return Randomizer.GetValue(10f, 30f);
		}
		
		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}
		
		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			if(pCharacter.handItem is Hackdev) {
//				pCharacter.PutHandItemIntoInventory();
//				pCharacter.timetableTimer = 5.0f;
				pCharacter.MoveHandItemToInventory();
			}
		}
		
		public override string ToString()
		{
			return string.Format("[WorkWithModifier] {0}", _roomName);
		}
		
		public void Reset() {
			
		}
	}


	/*
	public class Behaviour_GetThroughFence : TimetableBehaviour {

		string _fenceName;
		Fence _fence;
		
		public Behaviour_GetThroughFence(string pFenceName)
		{
			_fenceName = pFenceName;
		}
		
		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner)
		{
			if(_fence == null) {
				_fence = pTingRunner.GetTing<Fence>(_fenceName);
			}

			bool inRoom = (pCharacter.room == _fence.room);
			
			if (pCharacter.busy || pCharacter.talking)
			{
				return 1f;
			}
			
			if (!inRoom || (pCharacter.finalTargetPosition != WorldCoordinate.NONE && pCharacter.finalTargetPosition.roomName != _roomName)) {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " will pathfind to target room " + _roomName);
				#endif
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
			}
			else {
				
				if(pCharacter.actionName == "Angry" || pCharacter.seat == null || Randomizer.OneIn(7))
				{
					Seat randomSeat = InteractionHelper.GetRandomTing<Seat>(pTingRunner, pCharacter.room.name);
					if(randomSeat != null && !randomSeat.isBeingUsed) {
						#if LOG
						pCharacter.logger.Log(pCharacter.name + " will be told to sit on seat " + randomSeat.name + " by BeInRoom behaviour");
						#endif
						pCharacter.WalkToTingAndInteract(randomSeat);
					}
				}
			}
			
			#if LOG
			pCharacter.logger.Log(pCharacter.name + " reached end of Execute() on GetThroughFence behavior");
			#endif
			return Randomizer.GetValue(10f, 30f);
		}
		
		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}
		
		public override string ToString()
		{
			return string.Format("[GetThroughFence] {0}", _fenceName);
		}

	}
	*/
	
	public class Behaviour_Fika : TimetableBehaviour
	{
		string _roomName;
		Character _barista;
		Seat _favSeat;

		public Behaviour_Fika(string pRoomName)
		{
			_roomName = pRoomName;
		}

		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy || pCharacter.talking || pCharacter.sleeping)
			{
				return 1f;
			}

			bool inRoom = (pCharacter.room.name == _roomName);

			if (!inRoom) {
				_barista = null;
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
				return 10f;
			}

			// In room, time to find a drink
			var drink = pCharacter.handItem as Drink;
			bool hasDrink = (drink != null);
			//bool shouldGetDrink = !hasDrink || drink.amount <= 0.0f;
			//bool idle = (pCharacter.actionName == "");

			if(!hasDrink) {
				if(_barista == null) {
					_barista = Behaviour_Party.GetBartender (pCharacter, pTingRunner);
				}

				if(_barista != null) {

					if (_barista.room != pCharacter.room || !_barista.IsAtTimetableTaskOfType(typeof(Behaviour_Sell)) ) {
						_barista = null;
						return 1f;
					}

					if (pCharacter.sitting) {
						pCharacter.GetUpFromSeat ();
						return 1.8f;
					}

					if (_barista.IsIdle() &&
						(_barista.conversationTarget == null || !MimanGrimmApiDefinitions.AreTingsWithinDistance (pCharacter, _barista, 7))
					) {
						#if LOG
						pCharacter.logger.Log(pCharacter.name + " has a target barista, walking towards it");
						#endif
						pCharacter.WalkToTingAndInteract (_barista);
					} else {
						#if LOG
						pCharacter.logger.Log(pCharacter.name + " has a target barista, but it is busy so will wait to talk to him");
						#endif
						pCharacter.CancelWalking ();
					}
					return 0.5f;
				}
				else {
					#if LOG
					pCharacter.logger.Log(pCharacter.name + " can't find a barista");
					#endif
					return Randomizer.GetValue(3f, 6f);
				}
			}

			// Has a drink
			_barista = null;

			if(!pCharacter.sitting) {
				if(_favSeat == null || _favSeat.isBeingUsed) {
					_favSeat = InteractionHelper.GetRandomTing<Seat>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
				}

				if(_favSeat != null) {
					pCharacter.WalkToTingAndInteract(_favSeat);
					#if LOG
					pCharacter.logger.Log(pCharacter.name + " will be told to sit on seat " + _favSeat.name + " by Fika behaviour");
					#endif
					return Randomizer.GetValue(3f, 6f);
				}
				else {
					#if LOG
					pCharacter.logger.Log(pCharacter.name + " can't find a seat to sit at while drinking its coffee in Fika behavior");
					#endif
					return Randomizer.GetValue(3f, 6f);
				}
			}

			// Is sitting down with drink
			if(drink.amount > 0) {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " is idling in Behaviour_Fika, will take a sip");
				#endif
				pCharacter.InteractWith(pCharacter.handItem);
				return Randomizer.GetValue(10f, 20f);
			}
			else {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " has drunk up in Behaviour_Fika, will put down drink");
				#endif
				pCharacter.DropHandItemFar();
				return Randomizer.GetValue(30f, 45f);
			}

			//return 1f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			//D.Log("OnFinish in Fika behaviour for " + pCharacter.name);

			var coffee = pCharacter.handItem as Drink;
			if(coffee != null) { // && coffee.liquidType == "coffee") {
				#if LOG
				pCharacter.logger.Log(pCharacter + " is ending fika behavior and will drop her/his cup of coffee");
				#endif
//				if(pCharacter.sitting) {
//					pCharacter.DropHandItemFar();
//				} else {
//					pCharacter.PutHandItemIntoInventory();
//				}
//				pCharacter.timetableTimer = 4f;
				pCharacter.MoveHandItemToInventory();
			}
			else {
				#if LOG
				pCharacter.logger.Log(pCharacter + " is ending fika behaviorbut has no cup to drop");
				#endif
			}
		}

		public override string ToString()
		{
			return string.Format("[Fika] {0}", _roomName);
		}

		public void Reset() {
			//D.Log("Fika behaviour was reset");
			_favSeat = null;
			_barista = null;
		}
	}

	public class Behaviour_Smoke : TimetableBehaviour
	{
		string _tingName;
		Ting _targetTing;
		
		public Behaviour_Smoke(string pTingName)
		{
			_tingName = pTingName;
		}
		
		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if(_targetTing == null) {
				_targetTing = pTingRunner.GetTing(_tingName);
			}

			bool inRoom = (pCharacter.room.name == _targetTing.room.name);
			bool isNearTargetTing = MimanGrimmApiDefinitions.AreTingsWithinDistance(pCharacter, _targetTing, 7);
			
			if (pCharacter.busy || pCharacter.talking)
			{
				return 1f;
			}

			bool hasCig = pCharacter.handItem is Cigarette;
			
			if (!inRoom) {
				pCharacter.WalkToTingAndInteract(_targetTing);
				return 5f;
			}
			else if(!pCharacter.sitting && _targetTing is Seat) {
				pCharacter.WalkToTingAndInteract(_targetTing);
				return 5f;
			}
			else if(!isNearTargetTing) {
				pCharacter.WalkTo(_targetTing.position);
				return 3f;
			}
			else if(hasCig) {
				(pCharacter.handItem as Cigarette).charges = 3; // recharge
				pCharacter.InteractWith(pCharacter.handItem);
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " is smoking");
				#endif
				return Randomizer.GetValue(7f, 15f);
			}
			else if(pCharacter.HasInventoryItemOfType("Cigarette")) {
				Cigarette cig = null;
				foreach(var item in pCharacter.inventoryItems) {
					if(item is Cigarette) {
						cig = item as Cigarette;
						break;
					}
				}
				if(cig != null) {
					pCharacter.TakeOutInventoryItem(cig);
				} else {
					D.Log("Cig was null!");
				}
				return 4f;
			}
			else {
				string prefabName = "Tagg_Cigarrette";
				var pos = new WorldCoordinate(pCharacter.inventoryRoomName, IntPoint.Zero);
				string safeName = prefabName + "_toSmoke_" + pWorldSettings.dynamicallyCreatedTingsCount++;

				var existingCigg = pTingRunner.GetTingUnsafe(safeName) as Cigarette;

				if(existingCigg != null) {
					if(existingCigg.room.name == "Sebastian_inventory" || existingCigg.isBeingHeld) {
						D.Log("There's already a " + safeName + " but a character is holding it (or avatar has it)");
						for(int i = 0; i < 9999; i++) {
							string newName = safeName + "_safe_" + i;
							var item = pTingRunner.GetTingUnsafe(newName) as MimanTing;
							if(item == null) {
								// free name!
								safeName = newName;
								break;
							}
						}
					}
					else {
						D.Log("There's already a " + safeName + ", will use that one instead!");
						existingCigg.charges = 3;
						existingCigg.position = pos;
					}
				}

				/*var newCig = */pTingRunner.CreateTingAfterUpdate<Cigarette>(safeName, pos, Direction.DOWN, prefabName);
				return 3f;
			}
		}
		
		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			if(_targetTing == null) return false;
			return (pCharacter.room.name == _targetTing.room.name);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}
		
		public override string ToString()
		{
			return string.Format("[Smoke] {0}", _tingName);
		}

		public void Reset() {}
	}

	public class Behaviour_Party : TimetableBehaviour
	{
		string _roomName;
		Character _bartender;
		Point _dancePoint;

		public Behaviour_Party(string pRoomName)
		{
			_roomName = pRoomName;
		}
		
		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			bool inRoom = (pCharacter.room.name == _roomName);
			
			if (pCharacter.busy || pCharacter.talking)
			{
				return 1f;
			}
			
			//var drink = pCharacter.handItem as Drink;
			
			bool hasDrink = pCharacter.handItem is Drink;
			//bool drinkIsEmpty = hasDrink && drink.amount <= 0.0f;
			//bool idle = (pCharacter.actionName == "");

			if (!inRoom) {
				_bartender = null;
				_dancePoint = null;
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
				return 15f;
			}
			else if(!hasDrink) {

				#if LOG
				pCharacter.logger.Log(pCharacter + " doesn't have a drink, find bartender or create drink for him/her");
				#endif

				if (_bartender == null) {
					_bartender = GetBartender (pCharacter, pTingRunner);
				}

				if (_bartender == null) {
					// Cheat and create a drink anyway!
					pCharacter.handItem = Behaviour_Sell.CreateTingToSell(pCharacter, pTingRunner, "Beer", pWorldSettings);
					return 2f;
				}

				if (_bartender.IsIdle() &&
					(_bartender.conversationTarget == null || !MimanGrimmApiDefinitions.AreTingsWithinDistance (pCharacter, _bartender, 7))) 
				{
					pCharacter.WalkToTingAndInteract (_bartender);
					return 0.5f;
				} else {
					pCharacter.CancelWalking ();
					return 0.5f;
				}
			}
			else {
				_bartender = null;

				if (_dancePoint != null) {

#if LOG
					pCharacter.logger.Log(pCharacter + " has a _dancePoint, will se if he/she can walk there");
#endif

					var occupantsOnDancePoint = _dancePoint.tile.GetOccupants();
					foreach(var o in occupantsOnDancePoint) {
						if(o is Character && o != pCharacter) {
#if LOG
							D.Log("The dance point for " + pCharacter + " is taken by " + o + ", will go somewhere else.");
#endif
							return GoSit (pCharacter, pTingRunner, pRoomRunner);
						}
					}

					if(pCharacter.tile != null) {
						var occupantsOnSameTile = pCharacter.tile.GetOccupants();
						foreach(var o in occupantsOnSameTile) {
							if(o is Character && o != pCharacter) {
#if LOG
								D.Log("The tile point for " + pCharacter + " is taken by " + o + ", will go somewhere else.");
#endif
								return GoSit (pCharacter, pTingRunner, pRoomRunner);
							}
						}
					}

					if (MimanGrimmApiDefinitions.AreTingsWithinDistance (pCharacter, _dancePoint, 8)) {
						#if LOG
						pCharacter.logger.Log(pCharacter.name + " is close to its dance point");
						#endif

						if (Randomizer.OneIn (2) || pCharacter.handItem == null) {
							pCharacter.StartAction ("Dancing", null, Character.LONG_TIME, Character.LONG_TIME);
							return Randomizer.GetIntValue (5, 30);
						} else {
							if (pCharacter.handItem is Drink && (pCharacter.handItem as Drink).amount > 30f) {
								pCharacter.CancelWalking();
								pCharacter.InteractWith (pCharacter.handItem);
								return 3f;
							} else {
								pCharacter.PutHandItemIntoInventory ();
								_dancePoint = null;
								return 3f;
							}
						}
					}

					#if LOG
					D.Log(pCharacter + " don't seem to have a good dance point, let's sit instead");
					#endif
					return GoSit (pCharacter, pTingRunner, pRoomRunner);

				} else if (pCharacter.sitting) {

					#if LOG
					pCharacter.logger.Log(pCharacter + " is sitting and partying, maybe go dance?");
					#endif

					if(Randomizer.OneIn (3)) {
						return GoDancing (pCharacter, pRoomRunner);
					}

				}
				else {
					#if LOG
					pCharacter.logger.Log(pCharacter.name + " will try to find a seat or dance point...");
					#endif

					return GoSit (pCharacter, pTingRunner, pRoomRunner);
				}

			}

			#if LOG
			pCharacter.logger.Log(pCharacter.name + " fell through all cases in Party behaviour (usually not good)");
			#endif
			return 1f;
		}

		float GoSit (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner)
		{
			var randomSeat = InteractionHelper.GetRandomTing<Seat> (pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
			if ((randomSeat != null && !randomSeat.isBeingUsed) && Randomizer.OneIn (2)) {
				pCharacter.WalkToTingAndInteract (randomSeat);
				return Randomizer.GetIntValue (5, 10);
			}
			else {
				return GoDancing (pCharacter, pRoomRunner);
			}
		}

		float GoDancing (Character pCharacter, RoomRunner pRoomRunner)
		{
			_dancePoint = InteractionHelper.GetRandomTingWhere<Point> (pRoomRunner, pCharacter.room.name, t =>  {
				return t.name.ToLower ().Contains ("dance");
			});

			if (_dancePoint != null) {
				pCharacter.WalkTo (_dancePoint.position);
#if LOG
				pCharacter.logger.Log(pCharacter.name + " walking to dance point at " + _dancePoint);
#endif
				return 3f;
			}
			else {
#if LOG
				pCharacter.logger.Log(pCharacter.name + " didn't find a dance point.");
#endif
				return 3f;
			}
		}

		public static Character GetBartender (Character pCharacter, MimanTingRunner pTingRunner)
		{
			foreach (var c in pTingRunner.GetTingsOfTypeInRoom<Character> (pCharacter.room.name)) {
				if (c.IsAtTimetableTaskOfType (typeof(Behaviour_Sell))) {
					return c;
				}
			}
			return null;
		}
		
		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			if(pCharacter.handItem is Drink) {
				pCharacter.MoveHandItemToInventory();
			}
		}

		public override string ToString()
		{
			return string.Format("[Party] {0}", _roomName);
		}

		public void Reset() {}
	}

	public class Behaviour_Sell : TimetableBehaviour
	{		
		string _sellPointName, _roomName = "";
		Ting _sellPoint;
		List<string> _tingNamesToSell = new List<string>();
		bool _collectEmptyDrinks;

		static PathSolver _solver = new PathSolver();
		
		public Behaviour_Sell(string[] pArgs)
		{
			_sellPointName = pArgs[1];
			_collectEmptyDrinks = (pArgs[2] == "CollectEmptyDrinks");
			
			for(int i = 3; i < pArgs.Length; i++) {
				_tingNamesToSell.Add(pArgs[i]);
			}
		}
		
		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute sell behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute sell behaviour");
				#endif
				return 1f;
			}

			if(_sellPoint == null) {
				_sellPoint = pTingRunner.GetTing(_sellPointName);
				_roomName = _sellPoint.room.name;
			}
			
			bool inRoom = (pCharacter.room.name == _roomName);
			bool holdingDrink = (pCharacter.handItem is Drink);
			bool holdingSellableItem = holdingDrink;
			//bool drinking = (pCharacter.actionName == "Drinking");
			//bool walking = (pCharacter.actionName == "Walking");
			//bool talking = (pCharacter.actionName == "Talking");
			bool idle = (pCharacter.actionName == "");
			bool atSellPoint = pCharacter.position == _sellPoint.position;
			
			if(atSellPoint) {
				pCharacter.direction = _sellPoint.direction;
			}
			
			if(inRoom) {
				if(pCharacter.timetableMemory == "") {
					if(holdingDrink) {
						pCharacter.logger.Log (pCharacter.name + " will put away drink");
						pCharacter.PutHandItemIntoInventory();
					}
					else if(idle && _collectEmptyDrinks/* && atSellPoint*/) {
						var leftOverDrink = GetLeftOverDrink(pCharacter, pTingRunner);
						if (leftOverDrink != null) {
							var path = _solver.FindPath (pCharacter.tile, leftOverDrink.tile, pRoomRunner, true);
							if (path.status == PathStatus.FOUND_GOAL) {
								pCharacter.WalkToTingAndInteract (leftOverDrink);
								pCharacter.logger.Log (pCharacter.name + " will walk to left over drink " + leftOverDrink.name);
							} else {
								pCharacter.logger.Log (pCharacter.name + " can't pathfind to " + leftOverDrink.name + " will delete it...");
								pTingRunner.RemoveTingAfterUpdate (leftOverDrink.name);
							}
						} else {
							pCharacter.WalkTo (_sellPoint.position);
							pCharacter.logger.Log (pCharacter.name + " will walk back to sell point");
						}
					}
					else if(pCharacter.finalTargetTing == null && idle) {
						pCharacter.logger.Log (pCharacter.name + " will walk to sell point");
						pCharacter.WalkTo (_sellPoint.position);
					}
				}
				else {
					// Has a customer
					pCharacter.logger.Log (pCharacter.name + " has a customer");
					if(pCharacter.conversationTarget != null) {
						if (holdingSellableItem) {
							if (holdingDrink) {
								(pCharacter.handItem as Drink).amount = 100f;
							}
							pCharacter.GiveHandItemToPerson ();
							pCharacter.Say ("Varsågod!", "OrderingDrinks");
							//pCharacter.messageTimer -= 2.5f; // shorter lines
							pCharacter.timetableMemory = "";
							pCharacter.logger.Log (pCharacter.name + " gave thing to sell");
						} else {

							string prefabName = _tingNamesToSell [0];

							/*if (prefabName == "CoffeeCup_CoffeeCup") {
								var coffeeCup = InteractionHelper.GetClosestDrink (pCharacter, pCharacter.room, "coffee");
								pCharacter.InteractWith (coffeeCup);
								return 2.0f;
							}*/

							pCharacter.logger.Log (pCharacter.name + " will create " + prefabName + " to sell");
							pCharacter.handItem = CreateTingToSell(pCharacter, pTingRunner, prefabName, pWorldSettings);
						}
					}
					else {
						var customer = pTingRunner.GetTing(pCharacter.timetableMemory) as Character;
						D.isNull(customer);
						pCharacter.WalkToTingAndInteract(customer);
						pCharacter.logger.Log (pCharacter.name + " will interact with customer");
					}
				}
				
				return 2f;
			}
			else {
				InteractionHelper.GoToRoom(pCharacter, pRoomRunner, _roomName, pTingRunner);
				pCharacter.timetableMemory = "";
				return 3f;
			}
		}
		
		public Drink GetLeftOverDrink(Character pCharacter, TingRunner pTingRunner) {
			foreach(Drink d in pTingRunner.GetTingsOfTypeInRoom<Drink>(pCharacter.room.name))
			{
				if(!d.isBeingHeld && d.amount < 100f) {
					return d;
				}
			}
			return null;
		}

		public static int CountNrOfTingsWithPrefab (TingRunner pTingRunner, string pPrefabName)
		{
			var tings = pTingRunner.GetTings ();
			int count = 0;
			foreach (var t in tings) {
				if (t.prefab == pPrefabName) {
					count++;
				}
			}
			return count;
		}
		
		public static MimanTing CreateTingToSell(Character pCharacter, TingRunner pTingRunner, string pPrefabName, WorldSettings pWorldSettings) {

			var safeName = pPrefabName + "_sale_" + pWorldSettings.dynamicallyCreatedTingsCount++;

			if(pWorldSettings.dynamicallyCreatedTingsCount > 20) {
				pWorldSettings.dynamicallyCreatedTingsCount = 0;
			}

			#if LOG
			pCharacter.logger.Log(pCharacter + " is creating (or finding) item with name " + safeName + " and prefab " + pPrefabName);
			#endif
	
			MimanTing newItem = pTingRunner.GetTingUnsafe(safeName) as MimanTing;

			if(newItem != null) {
				if(newItem.room.name == "Sebastian_inventory" || newItem.isBeingHeld) {
					D.Log("There's already a " + safeName + " but a character is holding it (or avatar has it)");
					for(int i = 0; i < 9999; i++) {
						string newName = safeName + "_safe_" + i;
						var item = pTingRunner.GetTingUnsafe(newName) as MimanTing;
						if(item == null) {
							// free name!
							safeName = newName;
							break;
						}
					}
				}
				else {
					D.Log("There's already a " + safeName + ", will use that one instead!");
					(newItem as Drink).amount = 100f;
					newItem.position = new WorldCoordinate(pCharacter.inventoryRoomName, IntPoint.Zero);
					return newItem;
				}
			}
			
			if(pPrefabName == "Beer") {
				newItem = pTingRunner.CreateTingAfterUpdate<Drink>(safeName, new WorldCoordinate(pCharacter.inventoryRoomName, IntPoint.Zero), Direction.DOWN, pPrefabName);
				(newItem as Drink).masterProgramName = "FolkBeer";
				(newItem as Drink).liquidType = "beer";
			}
			else if(pPrefabName == "WellspringSoda") {
				newItem = pTingRunner.CreateTingAfterUpdate<Drink>(safeName, new WorldCoordinate(pCharacter.inventoryRoomName, IntPoint.Zero), Direction.DOWN, pPrefabName);
				(newItem as Drink).masterProgramName = "WellspringSoda";
				(newItem as Drink).liquidType = "soda";
			}
			else if(pPrefabName == "CoffeeCup_CoffeeCup") {
				newItem = pTingRunner.CreateTingAfterUpdate<Drink>(safeName, new WorldCoordinate(pCharacter.inventoryRoomName, IntPoint.Zero), Direction.DOWN, pPrefabName);
				(newItem as Drink).masterProgramName = "CafeCoffee";
				(newItem as Drink).liquidType = "coffee";
			}
			else if(pPrefabName == "Margherita_Margherita" ||
			        pPrefabName == "DryMartini_DryMartini" ||
			        pPrefabName == "BloodyMary_BloodyMary" ||
			        pPrefabName == "LongIslandIceTea_LongIslandIceTea") 
			{
				newItem = pTingRunner.CreateTingAfterUpdate<Drink>(safeName, new WorldCoordinate(pCharacter.inventoryRoomName, IntPoint.Zero), Direction.DOWN, pPrefabName);
				(newItem as Drink).masterProgramName = "AlcoholicDrink";
				(newItem as Drink).liquidType = "drink";
			}
			else {
				throw new Exception("Don't know how to create item with prefab " + pPrefabName);
			}
			
			return newItem;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void Reset() {}
	}
	
	public class Behaviour_RefineGoods : TimetableBehaviour
	{
		string _roomName;

		public Behaviour_RefineGoods(string pRoomName)
		{
			_roomName = pRoomName;
		}

		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			bool inRoom = (pCharacter.room.name == _roomName);

			if (pCharacter.busy || pCharacter.talking)
			{
				return 1f;
			}
			
			bool hasGoods = pCharacter.handItem is Goods;
			
			if (!inRoom) {
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
			}
			else if(hasGoods) {
				var goods = pCharacter.handItem as Goods;
				if(goods.GetPureness() > 0.8f) {
					//DropItOff();
					pCharacter.DropHandItem();
					return 5f;
				}
				else {
					return TryToRefineIt(pCharacter, pTingRunner);
				}
			}
			else {
				var targetGoods = InteractionHelper.GetRandomTing<Goods>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
				if(targetGoods != null) {
					#if LOG
					pCharacter.logger.Log(pCharacter.name + " will be told to get goods " + targetGoods.name + " by refine behaviour");
					#endif
					pCharacter.WalkToTingAndInteract(targetGoods);
					return 7f;
				}
			}

			return 1f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		/*
		void DropItOff() {
			var point = InteractionHelper.GetRandomTing<Point>(pTingRunner, pCharacter.room.name);
			if(point != null) {
				pCharacter.WalkToTingAndInteract(point);
				return 15f;
			}
		}
		*/
		
		float TryToRefineIt(Character pCharacter, MimanTingRunner pTingRunner) 
		{
			var machine = InteractionHelper.GetRandomTing<Machine>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
			if(machine != null && !machine.isBeingUsed) {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " will be told to refine goods in machine " + machine.name + " by BeInRoom behaviour");
				#endif
				pCharacter.WalkToTingAndInteract(machine);
			}
			
			return 15f;
		}

		public override string ToString()
		{
			return string.Format("[RefiningGoods] {0}", _roomName);
		}

		public void Reset() {}
	}
	
	public class Behaviour_Guard : TimetableBehaviour
	{
		string _roomName;

		public Behaviour_Guard(string pRoomName)
		{
			_roomName = pRoomName;
		}

		public float Execute (Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			bool inRoom = (pCharacter.room.name == _roomName);
			
			//D.Log("Executing guard behaviour");

			if (pCharacter.busy || pCharacter.talking || pCharacter.actionName == "Walking") {
				return 1f;
			}

			if (!inRoom) {
				InteractionHelper.GoToRoom (pCharacter, pRoomRunner, _roomName, pTingRunner);
			}
			else {

				Point randomPoint = InteractionHelper.GetRandomTing<Point>(pTingRunner, pCharacter.room.name, pCharacter.tileGroup);
				//D.Log("Found random point: " + randomPoint.name);
				if(randomPoint != null && !randomPoint.isBeingUsed) {
					pCharacter.WalkToTingAndInteract(randomPoint);
					return 1f;
				}
				
			}

			return 1f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public override string ToString()
		{
			return string.Format("[BeInRoom] {0}", _roomName);
		}

		public void Reset() {}
	}

	public class Behaviour_Sleep : TimetableBehaviour
	{
		string _bedName;
		Ting _bed;

		public Behaviour_Sleep(string pBedName)
		{
			_bedName = pBedName;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute sleep behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute sleep behaviour");
				#endif
				return 1f;
			}

			if(_bed == null) {
				_bed = pTingRunner.GetTing(_bedName);
			}

			bool atGoalBed = pCharacter.bed == _bed;
			bool goalIsCorrectlySet = (_bed.position == pCharacter.finalTargetPosition) || _bed.HasInteractionPointHere(pCharacter.finalTargetPosition);
			bool sleeping = (pCharacter.actionName == "Sleeping");

			/*
			if(atGoal && !sleeping) {
				#if LOG
				pCharacter.logger.Log("Sleep behaviour tells " + pCharacter + " to start sleeping");
				#endif
				pCharacter.Sleep(24);
			}*/

			if(!sleeping && _bed is Point && MimanGrimmApiDefinitions.AreTingsWithinDistance(pCharacter, _bed, 2)) {
				// Sleep at point
				#if LOG
				pCharacter.logger.Log("Found a Point for " + pCharacter.name + " to sleep at");
				#endif
				pCharacter.Sleep(10);
				return 30f;
			}
			else if(!atGoalBed && !goalIsCorrectlySet) {
				#if LOG
				pCharacter.logger.Log("Sleep behaviour tells " + pCharacter + " to walk to bed " + _bed.position);
				#endif
				pCharacter.WalkToTingAndInteract(_bed);
				return 5f;
			}
			else if(sleeping) {
				#if LOG
				pCharacter.logger.Log(pCharacter.name + " is sleeping in Sleep_Behaviour");
				#endif
				return 30f;
			}
			else {
				#if LOG
				pCharacter.logger.Log("Nothing to do in Sleep behaviour, atGoal " + atGoalBed + " goalIsSet: " + goalIsCorrectlySet + " sleeping: " + 
				sleeping + " current action: " + pCharacter.actionName);
				#endif
				return 5f;
			}

			//return 10f;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return _bed != null && (pCharacter.bed == _bed);
		}

		public void Reset() {}
	}
	
	public class Behaviour_Interact : TimetableBehaviour
	{
		string _tingName;
		Ting _ting;
		bool _hackTargetTing;
		bool _hasInteracted = false;

		public Behaviour_Interact(string pTingName, bool pHackTargetTing)
		{
			_tingName = pTingName;
			_hackTargetTing = pHackTargetTing;
		}

		private bool AtGoal(Character pCharacter)
		{
			if (_ting == null) {
				D.Log ("Ting " + _tingName + " can't be found for character " + pCharacter.name + " in Interact behaviour");
				return false;
			}
			return _ting.HasInteractionPointHere(pCharacter.position) || _ting.position == pCharacter.position;
		}

		private void ListenForInteraction(string pEvent) {
			if (pEvent.Contains (_tingName)) {
				_hasInteracted = true;
			}
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute interact behaviour");
				#endif
				return 1f;
			}
			else if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute interact behaviour");
				#endif
				return 1f;
			}

			if(_ting == null) {
				_ting = pTingRunner.GetTing(_tingName);
			}

			bool atGoal = AtGoal(pCharacter);
			bool goalIsCorrectlySet = (_ting.position == pCharacter.finalTargetPosition) || (_ting.HasInteractionPointHere(pCharacter.finalTargetPosition));
			bool isIdle = pCharacter.actionName == "";
			
			if(atGoal && isIdle && _hasInteracted) {
				#if LOG
				pCharacter.logger.Log("Interact behaviour for " + pCharacter + " is idle");
				#endif
			}
			else if(!atGoal && !goalIsCorrectlySet) {
				#if LOG
				pCharacter.logger.Log("Interact behaviour tells " + pCharacter + " to interact with " + _ting.name);
				#endif
				if(_hackTargetTing) {
					pCharacter.WalkToTingAndHack(_ting as MimanTing);
				} else {
					pCharacter.WalkToTingAndInteract(_ting);
				}
			}
			else {
				#if LOG
				pCharacter.logger.Log("Nothing to do, atGoal " + atGoal + " goalIsSet: " + goalIsCorrectlySet);
				#endif
			}

			return 1f;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner)
		{
			_hasInteracted = false;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return AtGoal(pCharacter); 
		}

		public void Reset() {}
	}

	public class Behaviour_Dj : TimetableBehaviour
	{
		string _mixerName;
		MusicBox _mixer;
		string[] _songNames;

		public Behaviour_Dj(string pMixerName, string[] pSongNames)
		{
			_mixerName = pMixerName;
			_songNames = pSongNames;
		}

		private bool AtGoal(Character pCharacter)
		{
			return _mixer.HasInteractionPointHere(pCharacter.position) || _mixer.position == pCharacter.position;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute DJ behaviour");
				#endif
				return 1f;
			}

			if(_mixer == null) {
				_mixer = pTingRunner.GetTing<MusicBox>(_mixerName);
				if (!_mixer.mixer) {
					D.Log("The music box " + _mixer.name + " isn't a dj Mixer");
				}
			}

			bool atGoal = AtGoal(pCharacter);
			bool goalIsCorrectlySet = (_mixer.position == pCharacter.finalTargetPosition) || (_mixer.HasInteractionPointHere(pCharacter.finalTargetPosition));
			bool isIdle = pCharacter.actionName == "";

			if(atGoal && isIdle) {
				#if LOG
				pCharacter.logger.Log("Interact behaviour for " + pCharacter + " is idle, should start DJing");
				#endif
				_mixer.soundName = _songNames[Randomizer.GetIntValue(0, _songNames.Length)];
				_mixer.audioTime = 0f;
				_mixer.isPlaying = false;
				pCharacter.InteractWith(_mixer);
			}
			else if(!atGoal && !goalIsCorrectlySet) {
				#if LOG
				pCharacter.logger.Log("Interact behaviour tells " + pCharacter + " to interact with " + _mixer.name);
				#endif
				pCharacter.WalkTo(new WorldCoordinate(_mixer.room.name, _mixer.interactionPoints[0]));
			}
			else {
				#if LOG
				pCharacter.logger.Log("Nothing to do, atGoal " + atGoal + " goalIsSet: " + goalIsCorrectlySet);
				#endif
			}

			return 1f;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return AtGoal(pCharacter); 
		}

		public void Reset() {}
	}

	public class Behaviour_GuideTo : TimetableBehaviour
	{
		string _tingName;
		string _waitForCharacterName;
		Ting _ting;
		Character _waitForCharacter;
	
		string _currentRoom;
		string _prevRoom;

		public Behaviour_GuideTo(string pTingName, string pWaitFor)
		{
			_tingName = pTingName;
			_waitForCharacterName = pWaitFor;
		}

		private bool AtGoal(Character pCharacter)
		{
			return _ting.HasInteractionPointHere(pCharacter.position) || _ting.position == pCharacter.position;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy (" + pCharacter.actionName + ") so will not execute guide behaviour");
				#endif
				return 1f;
			}

			if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute guide behaviour");
				#endif
				return 1f;
			}

			if(_ting == null) {
				_ting = pTingRunner.GetTing(_tingName);
			}

			if (_waitForCharacter == null) {
				_waitForCharacter = pTingRunner.GetTing<Character>(_waitForCharacterName);
			}

			if (pCharacter.room.name != _currentRoom) {
				_prevRoom = _currentRoom;
				_currentRoom = pCharacter.room.name;
				pCharacter.logger.Log (pCharacter + " set prev room to " + _prevRoom);
			}

			if (_waitForCharacter.room != pCharacter.room) {
				#if LOG
				pCharacter.logger.Log(pCharacter + " is in another room than its target " + _waitForCharacterName);
				#endif

				if (_waitForCharacter.room.name == _prevRoom) {
					pCharacter.logger.Log (_waitForCharacter.name + " is in previous room, will wait a bit");
					pCharacter.CancelWalking ();
					return 3f;
				} else {
					pCharacter.logger.Log (_waitForCharacter.name + " has gone off the tracks, will interact with him/her");
					pCharacter.WalkToTingAndInteract (_waitForCharacter);
					return 3f;
				}
			}

			bool tooFarAway = !MimanGrimmApiDefinitions.AreTingsWithinDistance(pCharacter, _waitForCharacter, 18);

			bool atGoal = _ting.HasInteractionPointHere(pCharacter.position) || _ting.position == pCharacter.position;
			bool goalIsCorrectlySet = (_ting.position == pCharacter.finalTargetPosition) || (_ting.HasInteractionPointHere(pCharacter.finalTargetPosition));
			bool isIdle = pCharacter.actionName == "";
			bool isWalking = pCharacter.actionName == "Walking";

			if (tooFarAway && isWalking) {
				#if LOG
				pCharacter.logger.Log(pCharacter + " is too far away from " + _waitForCharacterName + ", will wait");
				#endif
				pCharacter.CancelWalking();
				return 1f;
			}
			else if(atGoal && isIdle) {
				#if LOG
				pCharacter.logger.Log("GuideTo behaviour for " + pCharacter + " is idle at goal");
				#endif
				return 1f;
			}
			else if (goalIsCorrectlySet && isWalking) {
				#if LOG
				pCharacter.logger.Log("GuideTo behaviour for " + pCharacter + " is walking towards goal");
				#endif
				return 5f;
			}
			else if(!atGoal && !tooFarAway) {
				#if LOG
				pCharacter.logger.Log("GuideTo behaviour tells " + pCharacter + " to interact with " + _ting.name);
				#endif
				pCharacter.WalkToTingAndInteract(_ting);
				return 1f;
			}
			else if(!atGoal && tooFarAway) {
				#if LOG
				pCharacter.logger.Log("GuideTo behaviour tells " + pCharacter + " to wait for " + _waitForCharacterName);
				#endif
				pCharacter.CancelWalking();
				return 1f;
			}
			else {
				//#if LOG
				D.Log("GuideTo behaviour has no matching clause for " + pCharacter + ", at goal: " + atGoal.ToString() + 
				", isIdle: " + isIdle.ToString() + ", goalIsCorrectlySet: " + goalIsCorrectlySet.ToString() + ", tooFarAway: " + tooFarAway.ToString());
				//#endif
				return 1f;
			}
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return AtGoal(pCharacter); 
		}

		public void Reset() {

		}
	}

	public class Behaviour_Sit : TimetableBehaviour
	{
		string[] _seatNames;
		Seat _seat;

		public Behaviour_Sit(string[] pSeatNames)
		{
			_seatNames = pSeatNames;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute sit behaviour");
				#endif
				return 1f;
			}

			if (pCharacter.talking)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is talking so will not execute sit behaviour");
				#endif
				return 1f;
			}

			if(_seat == null) {
				foreach (var seatName in _seatNames) {
					var seat = pTingRunner.GetTing (seatName) as Seat;
					if(seat != null && !seat.isBeingUsed) {
						_seat = seat;
						break;
					}
				}
			}

			if (_seat == null) {
				return 1f;
			}

			bool atGoal = (pCharacter.seat == _seat);
			bool goalIsCorrectlySet = (_seat.position == pCharacter.finalTargetPosition) || (_seat.HasInteractionPointHere(pCharacter.finalTargetPosition));

			if(!atGoal && !goalIsCorrectlySet) {
				if (_seat.isBeingUsed) {
					_seat = null;
					return 1f;
				}
				#if LOG
				pCharacter.logger.Log("Sit behaviour tells " + pCharacter + " to walk to seat " + _seat.position);
				#endif
				pCharacter.WalkToTingAndInteract(_seat);
			}
			else {
				#if LOG
				pCharacter.logger.Log("Nothing to do in Sit behaviour, at goal " + atGoal + " goal is set: " + goalIsCorrectlySet + " sitting: " + pCharacter.sitting);
				#endif
			}

			if(atGoal) {
				#if LOG//
				pCharacter.logger.Log(pCharacter.name + " should be at correct position now " + pCharacter.position);
				#endif
			}

			#if LOG
			pCharacter.logger.Log(pCharacter.name + " is doing action " + pCharacter.actionName + " at position " + pCharacter.position);
			#endif

			return 1f;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return _seat != null && (pCharacter.seat == _seat);
		}

		public void Reset() {}
	}

	public class InteractionHelper
	{
		public static TingType GetRandomTing<TingType>(MimanTingRunner pTingRunner, string pRoomName, int pTileGroup) where TingType : Ting
		{
			TingType[] tings = pTingRunner.GetTingsOfTypeInRoom<TingType>(pRoomName);

			int count = tings.Length;
			int index = Randomizer.GetIntValue(0, tings.Length);

			while(count > 0)
			{
				TingType ting = tings[index];

				if(!ting.isBeingHeld && (pTileGroup == -1 || ting.tile.group == pTileGroup)) {
					return ting;
				}

				index++;
				if(index == tings.Length) {
					index = 0;
				}

				count--;
			}

			return null;
		}

		public static TingType GetAnyRandomTing<TingType>(MimanTingRunner pTingRunner, string pRoomName, int pTileGroup) where TingType : Ting
		{
			TingType[] tings = pTingRunner.GetTingsOfTypeInRoom<TingType>(pRoomName);
			
			int count = tings.Length;
			int index = Randomizer.GetIntValue(0, tings.Length);
			
			while(count > 0)
			{
				TingType ting = tings[index];
				
				if(!ting.isBeingHeld && ting.tile.group == pTileGroup) {
					return ting;
				}
				
				index++;
				if(index == tings.Length) {
					index = 0;
				}
				
				count--;
			}
			
			return null;
		}

		public static TingType GetRandomTingWhere<TingType>(RoomRunner pRoomRunner, string pRoomName, Predicate<TingType> pPredicate) where TingType : Ting
		{
			var room = pRoomRunner.GetRoom (pRoomName);
			var filtered = room.GetTingsOfType<TingType> ().FindAll (pPredicate);

			if (filtered.Count > 0) {
				return Randomizer.RandNth (filtered);
			} else {
				return null;
			}
		}

		public static Drink GetClosestDrink (Ting pCharacter, Room pRoom, string pLiquidType)
		{
			var filtered = pRoom.GetTingsOfType<Drink> ().Where(d => d.liquidType == pLiquidType && d.amount >= 100.0f).ToList();

			filtered.Sort ((a, b) => {
				float dist1 = pCharacter.localPoint.DistanceTo(a.localPoint);
				float dist2 = pCharacter.localPoint.DistanceTo(b.localPoint);
				return (int)(dist1 - dist2);
			});

			if (filtered.Count == 0) {
				return null;
			}

			return filtered [0];
		}

		public static void GoToRoom(Character pCharacter, RoomRunner pRoomRunner, string pRoomName, MimanTingRunner pTingRunner)
		{
			if(pCharacter.finalTargetPosition.roomName == pRoomName) {
				return;
			}
			//IntPoint[] tilePoints = pRoomRunner.GetRoom(pRoomName).tilePoints;
			//WorldCoordinate target = new WorldCoordinate(pRoomName, tilePoints[(int)Randomizer.GetValue(0, tilePoints.Length)]);
			Ting randomTing = GetRandomTing<Point>(pTingRunner, pRoomName, -1);
			
			if(randomTing == null) {
				#if LOG
				#endif
				pCharacter.logger.Log("No target Point found in " + pRoomName);
				return;
			}
			
			pCharacter.WalkToTingAndInteract(randomTing);
			#if LOG
			pCharacter.logger.Log("Set new target for " + pCharacter.name + " to " + randomTing + " at " + randomTing.position);
			#endif
		}

		public void Reset() {}
	}

	public class Behaviour_Drink : TimetableBehaviour
	{
		string _roomName;

		public Behaviour_Drink(string pRoomName)
		{
			_roomName = pRoomName;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			if (pCharacter.busy)
			{
				#if LOG
				pCharacter.logger.Log(pCharacter + " is busy so will not execute drink behaviour");
				#endif
				return 1f;
			}

			bool inRoom = (pCharacter.room.name == _roomName);
			bool holdingDrink = (pCharacter.handItem is Drink);
			//bool drinking = (pCharacter.actionName == "Drinking");
			//bool walking = (pCharacter.actionName == "Walking");
			bool talking = (pCharacter.actionName == "Talking");
			bool idle = (pCharacter.actionName == "");

			if(inRoom) {
				if(holdingDrink) {
					if(idle) {

						Drink drink = pCharacter.handItem as Drink;
						if(drink.amount > 0f) {
							#if LOG
							pCharacter.logger.Log("Taking a sip");
							#endif
							pCharacter.InteractWith(pCharacter.handItem);
							return Randomizer.GetValue(2f, 7f);
						} else {
							#if LOG
							pCharacter.logger.Log("Dropping empty bottle");
							#endif
							pCharacter.DropHandItem();
						}

						/*
						Character otherPerson = InteractionHelper.GetRandomTing<Character>(pTingRunner, _roomName);
						if(otherPerson != pCharacter) {
							pCharacter.WalkToTingAndInteract(otherPerson);
						}
						*/
					}
					else if(talking) {
						#if LOG
						pCharacter.logger.Log("Stopping talk action");
						#endif
						pCharacter.StopAction();
					}
				}
				else {
					if(pCharacter.finalTargetTing is Drink) {
						#if LOG//
						pCharacter.logger.Log(pCharacter.name + " is set on drink " + pCharacter.finalTargetTing.name);
						#endif
					}
					else if(idle) {
						Drink drink = InteractionHelper.GetRandomTing<Drink>(pTingRunner, _roomName, pCharacter.tileGroup);
						if(drink != null) {
							#if LOG
							pCharacter.logger.Log("Found a drink to walk to and pick up");
							#endif
							pCharacter.WalkToTingAndInteract(drink);
						}
						else {
							#if LOG
							pCharacter.logger.Log("Can't find a drink for " + pCharacter.name + " in " + _roomName);
							#endif
						}
					}
				}
				return 1f;
			}
			else {
				#if LOG
				pCharacter.logger.Log("Going to room " + _roomName);
				#endif
				InteractionHelper.GoToRoom(pCharacter, pRoomRunner, _roomName, pTingRunner);
				return 1f;
			}
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void Reset() {}
	}

	public class Behaviour_ServeDrinks : TimetableBehaviour
	{
		string _roomName;

		public Behaviour_ServeDrinks(string pRoomName)
		{
			_roomName = pRoomName;
		}

		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			bool inRoom = (pCharacter.room.name == _roomName);
			bool holdingDrink = (pCharacter.handItem is Drink);
			//bool drinking = (pCharacter.actionName == "Drinking");
			//bool walking = (pCharacter.actionName == "Walking");
			//bool talking = (pCharacter.actionName == "Talking");
			bool idle = (pCharacter.actionName == "");

			if(inRoom) {
				if(holdingDrink) {
					if(idle) {
						#if LOG
						pCharacter.logger.Log("Serving beer!");
						#endif
						pCharacter.DropHandItem();
					}
					else {
						#if LOG
						pCharacter.logger.Log("Walking...");
						#endif
					}
				}
				else {

					bool outOfBeerInRoom = true;

					foreach(Drink d in pTingRunner.GetTingsOfTypeInRoom<Drink>(pCharacter.room.name))
					{
						if(d.amount > 0f) {
							outOfBeerInRoom = false;
							break;
						}
					}

					if(outOfBeerInRoom) {
						//pCharacter.dialogueLine = "Gotta get some beer...";

						/*
						int count = pTingRunner.GetTingsOfType<Drink>().Length;
						Drink newDrink = pTingRunner.CreateTingAfterUpdate<Drink>("Drink_" + count, pCharacter.position, Direction.RIGHT);
						newDrink.prefab = "Beer";
						newDrink.masterProgramName = "FolkBeer";
						pCharacter.handItem = newDrink;
						Character c = InteractionHelper.GetRandomTing<Character>(pTingRunner, pCharacter.room.name);
						pCharacter.WalkTo(c.position);
						#if LOG
						pCharacter.logger.Log("Going to leave a beer at " + pCharacter.finalTargetPosition);
						#endif
						*/
					}
				}
				return 1f;
			}
			else {
				InteractionHelper.GoToRoom(pCharacter, pRoomRunner, _roomName, pTingRunner);
				return 1f;
			}
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return (pCharacter.room.name == _roomName);
		}

		public void Reset() {}
	}

	public class Behaviour_Photograph : TimetableBehaviour
	{
		public float Execute(Character pCharacter, MimanTingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner, WorldSettings pWorldSettings)
		{
			return 0f;
		}

		public bool IsAtFinalPartOfTask(Character pCharacter)
		{
			return true;
		}

		public void OnFinish(Character pCharacter, TingRunner pTingRunner, RoomRunner pRoomRunner, DialogueRunner pDialogueRunner) {
			
		}

		public void Reset() {}
	}
}

