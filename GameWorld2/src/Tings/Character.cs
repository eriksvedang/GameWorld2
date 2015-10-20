//#define LOG

using System;
using System.Collections.Generic;
using TingTing;
using GameTypes;
using RelayLib;
using Pathfinding;

namespace GameWorld2
{
	public class Character : MimanTing
	{
        public static new string TABLE_NAME = "Ting_Characters";
		public const float LONG_TIME = 99999f;
		public const int INVENTORY_SIZE = 20;

		public enum WalkMode
        { 
			NO_TARGET,
			WALK_TO_POINT,
			WALK_TO_TING_AND_INTERACT,
			WALK_TO_TING_AND_HACK,
			WALK_TO_TING_AND_USE_HAND_ITEM
		}

		public enum SleepinessState
		{
			FRESH,
			CAN_NOT_RUN,
			SLOW
		}
		
		public delegate void CharacterEvent();
		public CharacterEvent onNewPath;
		public CharacterEvent onRemovedPath;
		
		public delegate void OnNewHandItem(string pNameOfNewHandItem, bool pGivingItemToSomeoneElse);
		public OnNewHandItem onNewHandItem;

		public TimetableSpan lastTimetableSpan = TimetableSpan.NULL;

        ValueEntry<string> CELL_handItemObjectName;
		ValueEntry<WorldCoordinate> CELL_finalTargetPosition;
		ValueEntry<string> CELL_finalTargetTing;
		ValueEntry<WorldCoordinate> CELL_targetPositionInRoom;
		ValueEntry<WalkMode> CELL_walkMode;
		ValueEntry<float> CELL_walkSpeed;
		ValueEntry<float> CELL_walkTimer;
		ValueEntry<int> CELL_walkIterator;
		ValueEntry<float> CELL_charisma;
		ValueEntry<float> CELL_smelliness;
		ValueEntry<float> CELL_sleepiness;
		ValueEntry<float> CELL_drunkenness;
		ValueEntry<float> CELL_supremacy;
		ValueEntry<float> CELL_happiness;
		ValueEntry<float> CELL_corruption;
		ValueEntry<int> CELL_friendLevel;
		ValueEntry<GameTime> CELL_alarmTime;
		ValueEntry<string[]> CELL_knowledge;
		ValueEntry<string> CELL_timetableName;
		ValueEntry<string> CELL_timetableMemory;
		ValueEntry<float> CELL_timetableTimer; // time until the current timetable span is executed again
		ValueEntry<bool> CELL_talking;
		ValueEntry<string> CELL_conversationTargetName;
		ValueEntry<bool> CELL_sitting;
		ValueEntry<bool> CELL_laying;
		ValueEntry<bool> CELL_running;
		ValueEntry<string> CELL_seatName;
		ValueEntry<string> CELL_bedName;
		ValueEntry<bool> CELL_waitForGift;
		ValueEntry<bool> CELL_neverGetsTired;
		ValueEntry<float> CELL_creditCardUsageAmount;
		
        SmartWalkBehaviour _walkBehaviour;
		Timetable _timetable;
		TimetableRunner _timetableRunner;
		//List<UnreachablePathCacheItem> _failedPathfindingSearches = new List<UnreachablePathCacheItem>();

		public bool rememberToHackComputerAfterSittingDown;
		public Door rememberToUseDoorAfterWaitingPolitely;

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		public override void Init()
		{
			base.Init ();
			
			// Ensure inventory room
			if(!_roomRunner.HasRoom(inventoryRoomName)) {
				SimpleRoomBuilder srb = new SimpleRoomBuilder(_roomRunner);
				srb.CreateRoomWithSize(inventoryRoomName, 1, 1);
			}

			if(timetableName != "") {
				RefreshTimetable();
			}
		}
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_handItemObjectName = EnsureCell("handItemObjectName", "");
            CELL_finalTargetPosition = EnsureCell("finalTargetPosition", WorldCoordinate.NONE);
			CELL_finalTargetTing = EnsureCell("finalTargetTing", "");
			CELL_targetPositionInRoom = EnsureCell("targetPositionInRoom", WorldCoordinate.NONE);
			CELL_walkMode = EnsureCell("walkMode", WalkMode.NO_TARGET);
			CELL_walkSpeed = EnsureCell("walkSpeed", 4.0f);
			CELL_walkTimer = EnsureCell("walkTimer", 0f);
			CELL_walkIterator = EnsureCell("walkIterator", -1);
			CELL_charisma = EnsureCell("charisma", 0f);
			CELL_smelliness = EnsureCell("smelliness", 0f);
			CELL_sleepiness = EnsureCell("sleepiness", 0f);
			CELL_drunkenness = EnsureCell("drunkenness", 0f);
			CELL_happiness = EnsureCell("happiness", 0f);
			CELL_supremacy = EnsureCell("supremacy", 0f);
			CELL_alarmTime = EnsureCell("alarmTime", new GameTime(0, 0));
			CELL_friendLevel = EnsureCell("friendLevel", 0);
			CELL_knowledge = EnsureCell("knowledge", new string[] {});
			CELL_timetableName = EnsureCell("timetableName", "");
			CELL_timetableMemory = EnsureCell("timetableMemory", "");
			CELL_timetableTimer = EnsureCell("timetableTimer", 0f);
			CELL_talking = EnsureCell("talking", false);
			CELL_conversationTargetName = EnsureCell("conversationTargetName", "");
			CELL_sitting = EnsureCell("sitting", false);
			CELL_laying = EnsureCell("laying", false);
			CELL_running = EnsureCell ("running", false);
			CELL_seatName = EnsureCell("seatName", "");
			CELL_bedName = EnsureCell("bedName", "");
			CELL_corruption = EnsureCell("corruption", 0f);
			CELL_waitForGift = EnsureCell("waitForGift", false);
			CELL_neverGetsTired = EnsureCell ("neverGetsTired", false);
			CELL_creditCardUsageAmount = EnsureCell("creditCardUsageAmount", 0f);

			AddDataListener<WorldCoordinate>("position", OnPositionChanged);
		}
		
		~Character() 
		{
			//D.Log ("Cleaning up character " + name);
			RemoveDataListener<WorldCoordinate>("position", OnPositionChanged);
		}
		
		void OnPositionChanged(WorldCoordinate pPreviousPosition, WorldCoordinate pNewPosition)
		{
			UpdateHandItemPosition();
		}

		/// <summary>
		/// Makes the character stop walking, not sit or lay down, resets the timetableTimer.
		/// This is useful for example when teleporting the Character so that it's walkbehaviour doesn't execute and teleports him/her back.
		/// </summary>
		public void ClearState()
		{
			CancelWalking();
			sitting = false;
			laying = false;
			timetableTimer = 0.5f;
			seat = null;
			bed = null;
		}
		
		public override string tooltipName {
			get {
				Character avatar = _tingRunner.GetTingUnsafe(_worldSettings.avatarName) as Character;
				if(avatar != null && avatar.HasKnowledge(name)) {
					return name;
				}
				else {
					return "person";
				}
			}
		}
		
		public override string verbDescription {
			get {
				if (sleeping) {
					//return "steal from";
					return "can't talk to sleeping";
				} else {
					return "talk to";
				}
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.DirectionToIntPoint (direction) * 3,
					localPoint + IntPoint.DirectionToIntPoint (direction) * 4,
					//localPoint + IntPoint.DirectionToIntPoint (direction) * 5,
					localPoint + IntPoint.DirectionToIntPoint (IntPoint.Turn(direction, 90)) * 3,
					localPoint + IntPoint.DirectionToIntPoint (IntPoint.Turn(direction, -90)) * 3,
				};
			}
		}

		public override IntPoint[] interactionPointsTryTheseFirst {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.DirectionToIntPoint (direction) * 3,
				};
			}
		}
		
		public override bool CanInteractWith (Ting pTingToInteractWith)
		{
			if (pTingToInteractWith == this) {
				return false;
			}

			if(
				pTingToInteractWith is Drink ||
				pTingToInteractWith is Character ||
				pTingToInteractWith is Bed ||
				pTingToInteractWith is Seat ||
				pTingToInteractWith is MysticalCube ||
				pTingToInteractWith is Door ||
				pTingToInteractWith is Lamp ||
				pTingToInteractWith is Teleporter ||
				pTingToInteractWith is Extractor ||
				pTingToInteractWith is Radio ||
				pTingToInteractWith is MusicBox ||
				pTingToInteractWith is Portal ||
				pTingToInteractWith is Drug ||
				pTingToInteractWith is Computer ||
				pTingToInteractWith is TrashCan ||
				pTingToInteractWith is FuseBox ||
				pTingToInteractWith is Hackdev ||
				pTingToInteractWith is CreditCard ||
				pTingToInteractWith is Button ||
				pTingToInteractWith is Point ||
				pTingToInteractWith is Goods ||
				pTingToInteractWith is Robot ||
				pTingToInteractWith is Key ||
				pTingToInteractWith is Machine ||
				pTingToInteractWith is Sink ||
				pTingToInteractWith is Tv ||
				pTingToInteractWith is Fountain ||
				pTingToInteractWith is Floppy ||
				pTingToInteractWith is Locker ||
				pTingToInteractWith is Jewellery ||
				pTingToInteractWith is Stove ||
				pTingToInteractWith is FryingPan ||
				pTingToInteractWith is SendPipe ||
				pTingToInteractWith is Fence ||
				pTingToInteractWith is Tram ||
				pTingToInteractWith is Screwdriver ||
				pTingToInteractWith is Map ||
				pTingToInteractWith is VendingMachine ||
				pTingToInteractWith is Telephone ||
				pTingToInteractWith is Taser ||
				pTingToInteractWith is Pawn ||
				pTingToInteractWith is Memory
                )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}

		public void GetAngry ()
		{
			StopAction ();
			StartAction("Angry", null, 1.7f, 1.7f);
		}

		public void GetAngryAtComputer()
		{
			StopAction ();
			StartAction("AngryAtComputer", null, 2f, 2f);
		}

		public void Yawn()
		{
			StopAction ();
			StartAction("Yawn", null, 2.0f, 2.0f);
		}

		public override void InteractWith(Ting pTingToInteractWith)
		{
			if (busy) {
				if(isAvatar) {
					_worldSettings.Notify(name, name + " is busy!");
					return;
				}
				else {
					D.Log(name + " is trying to interact with " + pTingToInteractWith + " but she/he is busy.");
					return;
				}
			}

			//CancelWalking (); // TODO: WHY THE HELL DID I ADD THIS HERE?!

			if(pTingToInteractWith != handItem && !sitting && !laying) {
				FaceTing(pTingToInteractWith);
			}

			if (pTingToInteractWith.isBeingUsed && !(pTingToInteractWith is Point)) {
				#if LOG
				logger.Log (name + " is trying to interact with " + pTingToInteractWith.name + " but it is being used, stopping current action");
				#endif
				GetAngry ();
				return;
			}

			if(pTingToInteractWith is Drink)
			{
				if((pTingToInteractWith as Drink).amount > 0) {
					StartAction("Drinking", pTingToInteractWith, 1.5f, 3.3f);
				} else {
					_worldSettings.Notify(name, "It is empty!");
				}
			}
			else if(pTingToInteractWith is Snus)
			{
				if ((pTingToInteractWith as Snus).charges > 0) {
					StartAction ("TakingSnus", pTingToInteractWith, 0.5f, 1.0f);
				} else {
					_worldSettings.Notify(name, "No snus left!");
				}
			}
			else if(pTingToInteractWith is Cigarette)
			{
				if ((pTingToInteractWith as Cigarette).charges > 0) {
					StartAction("SmokingCigarette", pTingToInteractWith, 0.7f, 2.5f);
				} else {
					_worldSettings.Notify(name, "It's all used up!");
				}
			}
			else if(pTingToInteractWith is Drug)
			{
				// All foods are drugs, just with other animations

				if(pTingToInteractWith.name.ToLower().Contains("baguette")) {
					StartAction("Eat", pTingToInteractWith, 1.5f, 2.5f);
				}
				else {
					StartAction("TakingDrug", pTingToInteractWith, 0.5f, 1.0f);
				}
			}
			else if(pTingToInteractWith is Point)
			{
				StopAction();
			}
			else if(pTingToInteractWith is MusicBox && (pTingToInteractWith as MusicBox).mixer)
			{
				if (isAvatar) {
					StartAction ("Shrug", null, 1.0f, 1.0f);
				} else {
					StartAction ("Mixing", pTingToInteractWith, 0.5f, LONG_TIME);
				}
			}
			else if(pTingToInteractWith is Button)
			{
				StartAction("PushingButton", pTingToInteractWith, 0.5f, 1.0f);
			}
			else if(pTingToInteractWith is Tv)
			{
				StartAction("UsingTv", pTingToInteractWith, 0.5f, 1.0f);
			}
			else if(pTingToInteractWith is Stove)
			{
				StartAction("UseStove", pTingToInteractWith, 1.0f, 1.5f);
			}
			else if(pTingToInteractWith is VendingMachine)
			{
				var vendingMachine = pTingToInteractWith as VendingMachine;

				if(vendingMachine.dispensedCoke == null) {
					StartAction("ActivatingVendingMachine", pTingToInteractWith, 0.5f, 1.0f);
				} else {
					PickUp(vendingMachine.dispensedCoke);
				}
			}
			else if (pTingToInteractWith is Character)
            {
				var otherCharacter = pTingToInteractWith as Character;

				if (isAvatar && otherCharacter.sleeping) {
					//D.Log ("Stealing from " + otherCharacter);
					//StartAction ("Stealing", otherCharacter, 1.0f, 2.0f);
					//_dialogueRunner.EventHappened (name + "_steal_" + otherCharacter);
					_worldSettings.Notify (name, "The person is sleeping");
				} else {
					// Talking:

					CancelWalking ();
					conversationTarget = otherCharacter;
					conversationTarget.conversationTarget = this; // talk back!

					if (!sitting && !laying) {
						FaceTing (conversationTarget);
					}

					if (!conversationTarget.sitting && !conversationTarget.laying && conversationTarget.actionName == "") {
						conversationTarget.FaceTing (this);
					}

					if (!isAvatar && otherCharacter.IsAtTimetableTaskOfType (typeof(Behaviour_Sell))) {
						otherCharacter.timetableMemory = this.name;
						Say (GenerateRandomBuySentence (), "OrderingDrinks");
						//messageTimer -= 2.5f; // shorter lines when ordering!
					} else {
						_dialogueRunner.EventHappened (name + "_talk_" + pTingToInteractWith.name);
					}

					//if(!otherCharacter.isSleeping && !otherCharacter.actionOtherObject is Door) {
					//StartAction("Talking", pTingToInteractWith, LONG_TIME, LONG_TIME);
					//}
					//else {
					//_worldSettings.Notify("Can't talk to sleeping person");
					//}
				}
            }
			else if(pTingToInteractWith is Door)
			{
				#if LOG
				logger.Log("It's a door!");	
				#endif
			
				Door door = pTingToInteractWith as Door;
				if(door.isLocked) {
					#if LOG
					//logger.Log(name + " can't open the door '" + pTingToInteractWith.name + "' since it's locked");
					#endif

					StartAction("LockedDoor", pTingToInteractWith, 1.0f, 2.7f); // used to be 3.2f
					return;
				}
				else if(isAvatar && door.autoLockTimer > 0f) {
					D.Log(name + " can't open the door '" + pTingToInteractWith.name + "' since it's auto-locking");
					StartAction("LockedDoor", pTingToInteractWith, 1.0f, 2.7f);
					return;
				}
				else if (door.targetDoor == null) {
					_worldSettings.Notify(name, "The door is broken");
					StopAction ();
					return;
				}
				else {
					if(door.isElevatorEntrance) {
						var charactersInRoom = _tingRunner.GetTingsOfTypeInRoom<Character>(door.targetDoor.room.name);
						if(charactersInRoom.Length > 0 && door.elevatorFloor != door.targetDoor.elevatorFloor) {
							_worldSettings.Notify(name, "The elevator is in use");
							StopAction();
							return;
						}
					}

					StartAction ("WalkingThroughDoor", pTingToInteractWith, isAvatar ? 1.0f : 3.0f, 3.0f);
					door.Open ();
					_dialogueRunner.EventHappened (name + "_open_" + pTingToInteractWith.name);
				}				
			}
			else if(pTingToInteractWith is Portal)
			{
				StartAction("WalkingThroughPortal", pTingToInteractWith, 1.8f, 1.8f); // used to be 2.0
			}
			else if(pTingToInteractWith is Bed)
			{
				if (laying) {
					D.Log ("Can't roll over to other side of the bed");
					return;
				}

				bed = pTingToInteractWith as Bed;

				int i = 0;
				foreach (var p in bed.interactionPoints) {
					if (p == localPoint) {
						bed.exitPoint = i;
						break;
					}
					i++;
				}

				StartAction("LayingDown", pTingToInteractWith, 3.0f, 3.0f);
				_dialogueRunner.EventHappened(name + "_lay_" + pTingToInteractWith.name);
			}
			else if(pTingToInteractWith is Seat)
			{
				seat = pTingToInteractWith as Seat;
				StartAction("GettingSeated", pTingToInteractWith, 5f, 5f);
				_dialogueRunner.EventHappened(name + "_sit_" + pTingToInteractWith.name);
			}
            else if (pTingToInteractWith is TingWithButton)
            {
				var musicBox = pTingToInteractWith as MusicBox;
				if (musicBox != null && musicBox.isJukebox) {
					StartAction("StartingJukebox", pTingToInteractWith, 0.8f, 1.0f);
				} else {
					StartAction("PushingButtonOnHandItem", pTingToInteractWith, 0.8f, 1.0f);
				}                
            }
		 	else if (pTingToInteractWith is Lamp)
            {
                StartAction("KickingLamp", pTingToInteractWith, 0.75f, 1.2f);
            }
			else if (pTingToInteractWith is Telephone)
			{
				StartAction("TalkingInTelephone", pTingToInteractWith, 1.5f, LONG_TIME);
			}
			else if (pTingToInteractWith is Computer)
			{
				//D.Log(name + " will interact with " + pTingToInteractWith);
				StartAction("UsingComputer", pTingToInteractWith, 1.2f, LONG_TIME);
				_dialogueRunner.EventHappened(name + "_use_" + pTingToInteractWith.name);
			}
			else if (pTingToInteractWith is FuseBox)
			{
				StartAction("Inspect", pTingToInteractWith, 0.5f, LONG_TIME);
			}
			else if (pTingToInteractWith is Fountain)
			{
				StartAction("Inspect", pTingToInteractWith, 0.5f, LONG_TIME);
			}
			else if (pTingToInteractWith is TrashCan)
			{
				StartAction("Inspect", pTingToInteractWith, 0.5f, LONG_TIME);
			}
			else if (pTingToInteractWith is Locker)
			{
				//StartAction("LookInLocker", pTingToInteractWith, 0.5f, LONG_TIME);
			}
			else if (pTingToInteractWith is CreditCard)
			{
				StartAction("PushingButtonOnHandItem", pTingToInteractWith, 0.8f, 1.0f);
			}
			else if (pTingToInteractWith is Fence)
			{
				StartAction("WalkingThroughFence", pTingToInteractWith, 2.19f, 2.19f);
				(pTingToInteractWith as Fence).StartedWalkingThrough(this);
			}
			else if (pTingToInteractWith is Map)
			{
				StartAction("LookingAtMap", pTingToInteractWith, 0.5f, 1.0f);
			}
			else if(pTingToInteractWith is Sink)
			{
				StartAction ("UseSink", pTingToInteractWith, 1.0f, 2.0f);
			}
			else if(pTingToInteractWith is Machine)
			{
				StartAction("Inspect", pTingToInteractWith, 0.5f, LONG_TIME);
			}
            else
            { 
				StopAction();
#if DEBUG
                D.Log(this.name + " is trying to but can't interact with " + pTingToInteractWith.name);
#endif
            }
		}

		static string[] buySentences = new string[] {"Hej, kan jag få köpa något?", "Det vanliga tack", "Tjena!", "Jag tar en", "En till om jag får be"};
		static int buySentenceCounter = Randomizer.GetIntValue(0, 100);

		string GenerateRandomBuySentence ()
		{
			buySentenceCounter++;
			return buySentences[buySentenceCounter % buySentences.Length];
		}
		
		public void UseHandItemToInteractWith(Ting pTingToInteractWith)
		{
			if(pTingToInteractWith != handItem) {
				FaceTing(pTingToInteractWith);
			}
			
			if (handItem is Key && pTingToInteractWith is Door) {
				StartAction ("UsingDoorWithKey", pTingToInteractWith, 1.5f, 1.8f);
			} else if (pTingToInteractWith is Locker && handItem.CanInteractWith (pTingToInteractWith)) {
				StartAction ("PuttingTingIntoLocker", pTingToInteractWith, 0.5f, 1.2f);
			} else if (pTingToInteractWith != handItem && handItem is Extractor) {
				D.Log ("Using extractor on " + pTingToInteractWith);
				StartAction ("Extracting", pTingToInteractWith, 0.1f, 1.0f);
			} else if (pTingToInteractWith is SendPipe && handItem != null) {
				StartAction ("PuttingTingIntoSendPipe", pTingToInteractWith, 1.0f, 2.0f);
			} else if (pTingToInteractWith is TrashCan && handItem != null) {
				StartAction ("ThrowingTingIntoTrashCan", pTingToInteractWith, 0.9f, 1.6f);
			} else if (pTingToInteractWith is Sink && handItem is Drink) {
				(pTingToInteractWith as Sink).on = true;
				StartAction ("RefillingDrink", pTingToInteractWith, 0.5f, 1.2f);
			} else if (pTingToInteractWith is Stove && handItem != null) {
				(pTingToInteractWith as Stove).Fry (this, handItem);
			} else if (pTingToInteractWith is Character && handItem is Taser) {
				StartAction ("Tasing", pTingToInteractWith, 0.5f, 1.5f);
			} else if (pTingToInteractWith is Computer && handItem is Screwdriver) {
				StartAction ("Screwing", pTingToInteractWith, 0.5f, 1.2f);
			}
			else {
				StopAction();
#if DEBUG
				D.Log(this.name + " at position " + this.position + " is trying to but can't interact using hand item '" + this.handItem + "' with target '" + pTingToInteractWith.name + "'");
#endif
			}
		}

		public static bool ArePointsWithinDistance(IntPoint p1, IntPoint p2, int pDistance) {
			int distance = pDistance;
			int dx = p1.x - p2.x;
			int dy = p1.y - p2.y;
			return ((dx * dx) + (dy * dy)) < (distance * distance);
		}

		protected override void ActionTriggered (Ting pOtherTing)
		{
			if (actionName == "Drinking") {
				Drink drink = (Drink)pOtherTing;
				D.isNull(drink, "drink is null");
				drink.DrinkFrom (this);
			} else if (actionName == "TakingDrug" || actionName == "Eat") {
				Drug drug = (Drug)pOtherTing;
				D.isNull(drug, "drug is null");
				drug.Take (this);
				actionOtherObject = null;
				SetNoHandItem ();
				_dialogueRunner.EventHappened (name + "_took_" + drug.name);
				_tingRunner.RemoveTingAfterUpdate (drug.name);
			} else if (actionName == "TakingSnus") {
				var snus = pOtherTing as Snus;
				D.isNull(snus, "snus is null");
				snus.Take (this);
				_dialogueRunner.EventHappened (name + "_snusade");
			} else if (actionName == "SmokingCigarette") {
				var cigarette = pOtherTing as Cigarette;
				D.isNull(cigarette, "cigarette is null");
				cigarette.Take (this);
			} else if (actionName == "LockedDoor") {
				_worldSettings.Notify (name, "The door is locked");
				D.isNull(pOtherTing, "pOtherTing is null");
				_dialogueRunner.EventHappened (name + "_yank_" + pOtherTing.name);
			} else if (actionName == "UseDoorReallySoon") {
				D.Log(name + " is triggering UseDoorReallySoon: " + pOtherTing.name);
				var theDoor = (pOtherTing as Door);
				if(theDoor.autoLockTimer <= 0f && ArePointsWithinDistance(localPoint, theDoor.waitingPoint, 1)) {
					StopAction();
					InteractWith(pOtherTing);
				}
				else {
					_worldSettings.Notify(name, "Door was just locked");
				}
			} else if (actionName == "WalkingThroughDoor") {
				Door door = pOtherTing as Door;
				D.isNull(door, "door is null");
				door.WalkThrough (this);
				door.targetDoor.Open (); // mid point
			} else if (actionName == "WalkingThroughDoorPhase2") {
				//D.Log (name + " is done with walk through door phase 2!");
				StopAction ();
				if (_walkBehaviour != null) {
					_walkBehaviour.StartWalkingAgain ();
				}
			} else if (actionName == "WalkingThroughPortal") {
				Portal portal = pOtherTing as Portal;
				D.isNull(portal, "portal is null");
				portal.WalkThrough (this);
			} else if (actionName == "WalkingThroughPortalPhase2") {
				//D.Log(name + " is done with walk through portal phase 2!");
				StopAction ();
				if (_walkBehaviour != null) {
					_walkBehaviour.StartWalkingAgain ();
				}
			} else if (actionName == "WalkingThroughFence") {
				//StopAction ();
				StartAction ("DoneWalkingThroughFence", null, 0.01f, 0.02f);
				var fence = (pOtherTing as Fence);
				D.isNull(fence, "fence is null");
				position = fence.goalPosition;
				fence.user = null;
				timetableTimer = 0f; // re-trigger timetable
			} else if (actionName == "UsingDoorWithKey") {
				var door = pOtherTing as Door;
				D.isNull(door, "door is null");

				var key = (handItem as Key);
				if(key == null) {
					D.Log(name + " is UsingDoorWithKey but has no key in hand, will interact normally with it instead.");
					InteractWith(door);
					return;
				}

				key.InteractWith (door);
				#if LOG
				//logger.Log (name + " unlocked the door " + pOtherTing.name + " using the key '" + handItem.name + "'");
				#endif
			} else if (actionName == "PickingUp") {
				if (pOtherTing.isBeingHeld) {
					#if LOG
					logger.Log (pOtherTing + " is being held and can't be picked up");
					#endif
					return;
				}
				D.isNull(pOtherTing, "pOtherTing is null");
				SetHandItem ((MimanTing)pOtherTing);
				_dialogueRunner.EventHappened (name + "_pickup_" + pOtherTing.name);
				handItem.isBeingHeld = true;
			} else if (actionName == "Dropping" || actionName == "DroppingFar") {
				if(handItem == null) {
					D.Log (name + " is trying to drop hand item but it is null");
					return;
				}
				int dropLength = (actionName == "Dropping") ? 1 : 2;
				var dropTarget = new WorldCoordinate (room.name, localPoint + IntPoint.DirectionToIntPoint (direction) * dropLength);
				handItem.position = dropTarget;
				handItem.isBeingHeld = false;
				handItem.direction = direction;
				handItem.OnPutDown ();
				SetNoHandItem ();
			} else if (actionName == "PutHandItemIntoInventory") {
				MoveHandItemToInventory ();
			} else if (actionName == "TakeOutInventoryItem") {
				SetHandItem ((MimanTing)pOtherTing);
				handItem.position = position;
				handItem.isBeingHeld = true;
			} else if (actionName == "LayingDown") {
				D.isNull(bed, "bed is null");
				LayInBed (bed);
			} else if (actionName == "FallingAsleep") {
				int hours = 8;
				if(!isAvatar) {
					hours = 3; // TODO: do this to make people not sleep as long and miss stuff
				}
				Sleep (gameClock + new GameTime (hours, 0));
			} else if (actionName == "FallingAsleepInChair") {
				Sleep (gameClock + new GameTime (3, 0));
			} else if (actionName == "FallAsleepFromStanding") {
				// Alarm time is set when FallAsleepFromStanding() is called
				Sleep (alarmTime);
				laying = true;
			} else if (actionName == "GettingSeated") {
				var seat = pOtherTing as Seat;
				D.isNull(seat, "seat is null");
				Sit (seat);
			} else if (actionName == "GettingUpFromSeat") {
				//D.Log("GettingUpFromSeat action triggered the normal way");
				AfterGettingUpFromSeat ();
				if (seat != null) {
					GetUpSeatSnap ();
				}
			} else if (actionName == "GettingUpFromBed") {
				//D.Log("GettingUpFromBed action triggered the normal way");
				AfterGettingUpFromBed ();
				if (bed != null) {
					GetUpBedSnap ();
				}
			} else if (actionName == "PushingButtonOnHandItem" || actionName == "StartingJukebox") {
				D.Log ("Triggering action " + actionName);
				TingWithButton tingWithButton = pOtherTing as TingWithButton;
				D.isNull(tingWithButton, "tingWithButton is null");
				if (tingWithButton == null) {
					D.LogError (pOtherTing.name + " is not a TingWithButton");
				}
				tingWithButton.PushButton (this);
			} else if (actionName == "PushingButton") {
				var button = (pOtherTing as Button);
				D.isNull(button, "button is null");
				button.Push (this);
				_dialogueRunner.EventHappened (this.name + "_pressed_" + pOtherTing.name);
			} else if (actionName == "UsingTv") {
				var tv = (pOtherTing as Tv);
				D.isNull(tv, "tv is null");
				tv.Flip ();
			} else if (actionName == "TurnLeft") {
				TurnDegrees (90);
			} else if (actionName == "TurnRight") {
				TurnDegrees (-90);
			} else if (actionName == "KickingLamp") {
				Lamp lamp = pOtherTing as Lamp;
				D.isNull(lamp, "lamp is null");
				lamp.Kick ();
			} else if (actionName == "Extracting") {
				Extractor extractor = handItem as Extractor;
				if (extractor == null) {
					D.LogError (name + " is trying to use a hand item that is not an Extractor to extract things");
				} else {
					extractor.Attach (pOtherTing);
				}
			} else if (actionName == "UsingComputer") {
				Computer computer = pOtherTing as Computer;
				D.isNull(computer, "computer is null");
				Floppy maybeFloppy = handItem as Floppy;
				computer.GetUsedBy (this, maybeFloppy);
				_dialogueRunner.EventHappened (name + "_start_Computer");
			} else if (actionName == "SlurpingIntoComputer") {
				StartAction ("InsideComputer", pOtherTing, LONG_TIME, LONG_TIME);
				_dialogueRunner.EventHappened (name + "_slurped_" + pOtherTing.name);
			} else if (actionName == "Inspect") {
				if (actionOtherObject is FuseBox) {
					(actionOtherObject as FuseBox).BeInspected (this);
					_dialogueRunner.EventHappened (name + "_inspect_Fusebox");
				} else if (actionOtherObject is Fountain) {
					_dialogueRunner.EventHappened (name + "_inspect_Fountain");
				} else if (actionOtherObject is TrashCan) {
					_dialogueRunner.EventHappened (name + "_inspect_TrashCan");
				}
			} else if (actionName == "ThrowingTingIntoTrashCan") {
				var trash = handItem;
				TrashCan trashCan = pOtherTing as TrashCan;
				D.isNull(trashCan, "trashCan is null");
				trashCan.Throw (trash);
			} else if (actionName == "PuttingTingIntoSendPipe") {
				var pipe = pOtherTing as SendPipe;
				D.isNull(pipe, "pipe is null");
				var stuff = handItem;
				handItem.isBeingHeld = false;
				SetNoHandItem ();
				pipe.PutStuffIntoIt (stuff);
			} else if (actionName == "PuttingTingIntoLocker") {
				Locker locker = pOtherTing as Locker;
				D.isNull(locker, "locker is null");
				handItem.isBeingHeld = false;
				bool success = locker.PutTingIntoRandomFreeSpot (handItem);
				if (success) {
					SetNoHandItem ();
				}
			} else if (actionName == "GivingHandItem") {
				Character receiver = pOtherTing as Character;
				D.isNull (receiver, "The receiver is null");
				_dialogueRunner.EventHappened (name + "_give_"/* + handItem.prefab + "_to_" */ + receiver.name);
				if (onNewHandItem != null) {
					onNewHandItem ("", true);
				}
				if (receiver.handItem != null) {
					receiver.MoveHandItemToInventoryForcefully ();
				}
				receiver.SetHandItem (handItem);
				handItem = null;
			} else if (actionName == "BeingBothered") {
				timetableTimer = 0.5f;
			} else if (actionName == "Tasing") {
				var otherCharacter = (pOtherTing as Character);
				D.isNull(otherCharacter, "other character is null");
				otherCharacter.GetTased ();
				_dialogueRunner.EventHappened(name + "_tase_" + otherCharacter);
			} else if (actionName == "GettingTased") {
				ClearWalkingData(); // FIX?!
				FallAsleepFromStanding (2);
			} else if (actionName == "Angry") {
				timetableTimer = 0.5f;
			} else if (actionName == "UseSink") {
				var sink = (actionOtherObject as Sink);
				D.isNull(sink, "sink is null");
				sink.Toggle ();
				if (!sink.on) {
					_dialogueRunner.EventHappened (name + "_turnOff_" + sink.name);
				}
			} else if (actionName == "RefillingDrink") {
				var drink = (handItem as Drink);
				D.isNull(drink, "drink is null");
				var sink = actionOtherObject as Sink;
				if (sink == null) {
					D.Log ("Sink was null, can't refill");
				} else {
					sink.UseDrinkOnSink (drink);
				}
			} else if (actionName == "Screwing") {
				var screwdriver = (handItem as Screwdriver);
				D.isNull(screwdriver, "screwdriver is null");
				var computer = actionOtherObject as Computer;
				if (computer == null) {
					D.Log ("Computer was null, can't screw it");
				} else {
					screwdriver.UseOnComputer (computer);
				}
			} else if (actionName == "Mixing") {
				var mixer = pOtherTing as MusicBox;
				D.isNull(mixer, "mixer is null");
				mixer.isPlaying = true;
			} else if (actionName == "UseStove") {
				var stove = pOtherTing as Stove;
				D.isNull(stove, "stove is null");
				stove.on = !stove.on;
			} else if (actionName == "TalkingInTelephone") {
				var phone = (pOtherTing as Telephone);
				D.isNull(phone, "phone is null");
				phone.Use ();
				_dialogueRunner.EventHappened (name + "_phone_" + pOtherTing.name);
			} else if (actionName == "ActivatingVendingMachine") {
				var vendingMachine = pOtherTing as VendingMachine;
				D.isNull(vendingMachine, "vendingMachine is null");
				vendingMachine.PushCokeDispenserButton (this);
			} else if (actionName == "Stealing") {
				var otherCharacter = actionOtherObject as Character;
				D.isNull(otherCharacter, "other character is null");
				var items = otherCharacter.inventoryItems;
				if (items.Length == 0) {
					_worldSettings.Notify (name, "Nothing to steal");
				} else {
					Ting stolenItem = Randomizer.RandNth (items);
					MoveHandItemToInventory ();
					SetHandItem (stolenItem as MimanTing);
					_worldSettings.Notify (name, "You stole: " + stolenItem.tooltipName);
				}
			}
			else {
				//throw new Exception("Can't find trigger response to action '" + actionName + "'");
			}
		}

		public void GetTased ()
		{
			StartAction ("GettingTased", null, 1.5f, 1.5f);
		}

		public void GetTasedGently ()
		{
			StartAction ("GettingTasedGently", null, 1.5f, 1.5f); // don't fall asleep afterwards
		}

		public void AfterGettingUpFromSeat ()
		{
			sitting = false;
			StopAction();
		}

		public void AfterGettingUpFromBed ()
		{
			laying = false;
			direction = IntPoint.Turn (direction, 180);
			StopAction();
		}
		
		/*
		/// <summary>
		/// Takes out a hand item using animation
		/// </summary>
		public void TakeOutItem(MimanTing pNewHandItem) {
			StartAction("TakeOutItem", pNewHandItem, 1.5f, 3.0f);
		}
		
		/// <summary>
		/// Puts back the current hand item into the inventory
		/// </summary>
		public void PutBackItem() {
			StartAction("PutBackItem", null, 1.5f, 3.0f);
		}
		 */

		/// <summary>
		/// Moves the hand item to inventory immediately, without triggering an action
		/// Return true if successful
		/// </summary>
		public bool MoveHandItemToInventory ()
		{
			if(isAvatar && inventoryIsFull) {
				_worldSettings.Notify(name, "Inventory is full");
				return false;
			}

			if (handItem == null) {
				//D.Log (handItem, name + " is trying to move hand item to inventory but it is null");
				return false;
			} 
//			else if(handItem is Suitcase) {
//				_worldSettings.Notify (name, "Can't put suitcase into bag");
//				return false;
//			}
			else {
				MoveHandItemToInventoryForcefully ();
				return true;
			}
		}

		void MoveHandItemToInventoryForcefully ()
		{
			handItem.isBeingHeld = false;
			handItem.position = new WorldCoordinate (inventoryRoomName, IntPoint.Zero);
			SetNoHandItem ();
		}

		public bool inventoryIsFull {
			get {
				return inventoryItems.Length >= INVENTORY_SIZE;
			}
		}

		private void SetHandItem(MimanTing pNewHandItem)
		{
			if (pNewHandItem == null) {
				D.Log ("pNewHandItem is null!");
				return;
			}

			if (handItem != null) {
				MoveHandItemToInventoryForcefully ();
				if(onNewHandItem != null) {
					onNewHandItem("", false);
				}
			}

			handItem = pNewHandItem;
			handItem.isBeingHeld = true;
			handItem.PrepareForBeingHacked(); // this will make drinks generate programs if they don't have them already (happens when they are made by vending machines). 

			if(onNewHandItem != null) {
				onNewHandItem(handItem.name, false);
			}
		}
		
		private void SetNoHandItem()
		{
			handItem.isBeingHeld = false;
			handItem = null;
			if(onNewHandItem != null) {
				onNewHandItem("", false);
			}
		}
		
		public string inventoryRoomName {
			get {
				return name + "_inventory";
			}
		}
		
		public Ting[] inventoryItems {
			get {
				return _tingRunner.GetTingsInRoom(inventoryRoomName);
			}
		}

		private void FaceTing(Ting pTing)
		{
			direction = (pTing.position.localPosition - localPoint).ToDirection();
		}

		public void LayInBed(Bed pBed) {
			if(pBed == null) {
				D.LogError(name + " is trying to sit on pOtherTing that is null");
				return;
			}

			if(bed == null) {
				bed = pBed;
			}

			position = pBed.position;
			direction = pBed.exitPoint == 1 ? pBed.direction : IntPoint.Turn(pBed.direction, 180);
			laying = true;

			if (actionName != "Sleeping") {
				StopAction ();

				bool sleepyEnough = sleepiness > 20.0f;
				bool tooSunny = _worldSettings.gameTimeClock.isDaytime;

				if(isAvatar && !sleepyEnough) {
					_worldSettings.Notify(name, "Not feeling tired enough to sleep");
				}
				else if(isAvatar && tooSunny && sleepiness < 80.0f) {
					_worldSettings.Notify(name, "It's too bright to sleep");
				}
				else {
					StartAction("FallingAsleep", null, 3f, LONG_TIME);
				}
			}
		}

		public void Sit(Seat pSeat)
		{
			if(pSeat == null) {
				D.LogError(name + " is trying to sit on pOtherTing that is null");
				return;
			}

			if(seat == null) {
				seat = pSeat;
			}
			
			//D.Log("Sit called for " + name + " on seat " + pSeat);
			
			direction = pSeat.direction;
			position = pSeat.position;
			StopAction();
			sitting = true;

			var possibleComputerTile = room.GetTile(seat.computerPoint);
			if (possibleComputerTile != null) {
				var computerInFrontOfChair = possibleComputerTile.GetOccupantOfType<Computer>();
				if (computerInFrontOfChair != null) {
					if (rememberToHackComputerAfterSittingDown) {
						Hack (computerInFrontOfChair);
					} else {
						InteractWith (computerInFrontOfChair);
					}
				}
			}
		}
		
		public void GetUpSeatSnap ()
		{
			D.isNull(seat, "seat is null");
			position = new WorldCoordinate(room.name, seat.GetCurrentExitPoint());		
			seat = null;
			//D.Log(name + " got up and snapped to interaction point of seat!");
		}

		public void GetUpBedSnap ()
		{
			D.isNull(bed, "bed is null");
			int exitPoint = bed.exitPoint;
			position = new WorldCoordinate(room.name, bed.GetCurrentExitPoint ());
			bed = null;
			//D.Log(name + " got up and snapped to bed interaction point " + exitPoint + " at position " + position + "");
		}

		public void PickUp(Ting pTingToPickUp)
		{
			if(!pTingToPickUp.canBePickedUp) {
				throw new Exception("Can't pick up '" + pTingToPickUp + "'");
			}
			if(pTingToPickUp.isBeingHeld) {
				#if LOG
				logger.Log(pTingToPickUp + " is being held and can't be picked up");
				#endif
				return;
			}
			bool allowed = true;

			if(handItem != null) {
				allowed = MoveHandItemToInventory();
			}
				
			StopAction();

			if (allowed) {
				FaceTing (pTingToPickUp);
				StartAction ("PickingUp", pTingToPickUp, 0.6f, 1.82f);
			} else {
				_worldSettings.Notify (name, "Can't put current hand item into bag");
			}
		}
		
		public void DropHandItem()
		{
			if (isAvatar && busy) {
				_worldSettings.Notify(name, name + " is busy!");
				return;
			}

			if(handItem == null) {
				D.Log("Don't have a hand item");
				return;
			}

			var dropTarget = new WorldCoordinate (room.name, localPoint + IntPoint.DirectionToIntPoint (direction) * 1);
			var tileDropTarget = room.GetTile(dropTarget.localPosition);
			if(tileDropTarget == null || tileDropTarget.HasOccupants()) {
				_worldSettings.Notify (name, "Can't put " + handItem.name + " there");
				return;
			}

			StartAction("Dropping", handItem, 1.0f, 1.5f);
		}

		public void DropHandItemFar()
		{
			if (isAvatar && busy) {
				_worldSettings.Notify(name, name + " is busy!");
				return;
			}

			if(handItem == null) {
				D.Log("Don't have a hand item");
				return;
			}

			var dropTarget = new WorldCoordinate (room.name, localPoint + IntPoint.DirectionToIntPoint (direction) * 2);
			var tileDropTarget = room.GetTile(dropTarget.localPosition);

			if(tileDropTarget == null || (isAvatar && tileDropTarget.HasOccupants())) {
				_worldSettings.Notify (name, "Can't put " + handItem.name + " there");
				return;
			}

			StartAction("DroppingFar", handItem, 1.0f, 1.5f);
		}
		
		public void PutHandItemIntoInventory()
		{
			if (isAvatar && busy) {
				_worldSettings.Notify(name, name + " is busy so can't put hand item into inventory!");
				return;
			}

			if(isAvatar && inventoryIsFull) {
				_worldSettings.Notify(name, "Inventory is full");
				return;
			}

			if(handItem == null) {
				D.Log("Don't have a hand item");
			}

			StartAction("PutHandItemIntoInventory", handItem, 0.5f, 1.4f);
		}
		
		public void TakeOutInventoryItem(Ting pTingInInventory)
		{
			if(pTingInInventory == this) {
				D.Log("Can't take out yourself from the inventory");
				return;
			}

			/* OLD (ALMOST CORRECT) CODE!
			bool success = true;

			pTingInInventory.position = new WorldCoordinate("Internet", 1000, 1000); // make room for a possible hand item (it's a swap)

			if(handItem != null) {
				success = MoveHandItemToInventory(); // now there is definitely room for the item since we removed one item
			}

			// put the item back into inventory so we don't lose it (it will be taken out now)
			pTingInInventory.position = new WorldCoordinate (inventoryRoomName, IntPoint.Zero); // put it back again since the swap failed

			if (success) {
				StartAction ("TakeOutInventoryItem", pTingInInventory, 0.5f, 1.0f);
			} else {
				_worldSettings.Notify(name, "Can't put away " + handItem.tooltipName);
			}*/

			// NEW SOLUTION: ALWAYS SUCCEED!

			if(handItem != null) {
				MoveHandItemToInventoryForcefully();
			}

			StartAction ("TakeOutInventoryItem", pTingInInventory, 0.5f, 1.4f);
		}

		public void GiveHandItemToPerson ()
		{
			if(handItem == null) {
				D.Log(name + " can't give hand item since it is null");
				return;
			}

			//D.Log("CONVERSATION TARGET: " + conversationTarget.ToString());
			
			var otherCharacter = (conversationTarget as Character);
			
			if (otherCharacter != null) {
				if(isAvatar && !otherCharacter.waitForGift) {
					D.Log(name + " can't give item to " + otherCharacter + ", won't accept gift.");
					_worldSettings.Notify(name, "Person won't accept gift.");
				} else {
					#if LOG
					logger.Log("Giving hand item " + handItem.name + " to " + conversationTarget.name);
					#endif
					StartAction ("GivingHandItem", conversationTarget, 0.7f, 0.7f);
					otherCharacter.ReceiveHandItem();
				}
			} else {
				D.Log("Can't give hand item to conversationTarget: " + conversationTarget + " because it is not set to a Character");
			}
		}

		public bool isAvatar {
			get {
				return _worldSettings.avatarName == name;
			}
		}

		public void ReceiveHandItem()
		{
			// Force current item into inventory, even if it's full (not worth it to handle in a more dynamic way)
			if (handItem != null) {
				MoveHandItemToInventoryForcefully ();
			}
			StartAction ("ReceivingHandItem", actionOtherObject, 1.6f, 2.0f);
		}

		public void StartTalking()
		{
			if (_walkBehaviour != null) {
				CancelWalking();
			}
			//ClearWalkingData ();
			talking = true;
		}

		public void StopTalking()
		{
			//StopAction();
			timetableTimer = 0.5f; // will start doing something asap
			talking = false;
			ClearConversationTarget();
		}

		public void ClearConversationTarget()
		{
			//D.Log(name + " cleared conversation target ");
			if (conversationTarget != null) {
				conversationTarget.conversationTarget = null;
			}
			conversationTarget = null;
		}

		public void FallAsleepFromStanding(int pHours)
		{
			StartAction("FallAsleepFromStanding", null, 3.0f, 3.0f);
			// alarm time is set here so that it can be passed on from when the action triggers
			alarmTime = gameClock + new GameTime (pHours, 0);
		}

		public void Sleep(int pHours)
		{
			ClearConversationTarget();
			CancelWalking();
			Sleep(gameClock + new GameTime(pHours, 0));
		}
		
		public void Sleep(GameTime pAlarmTime)
		{
			if(isAvatar) {
				D.Log("Sleep was called on avatar with alarm time: " + pAlarmTime);

				if(pAlarmTime.hours > 0 && pAlarmTime.hours < 8) {
					pAlarmTime.hours = 8; // prevent avatar from waking up too early in the morning
					pAlarmTime.minutes = Randomizer.GetIntValue(0, 60);
					D.Log("Prevented avatar from waking up too early, set hours to " + pAlarmTime.hours);
				}
				else if(_tingRunner.gameClock.hours < 8 && pAlarmTime.hours > 11 && pAlarmTime.hours < 18) {
					pAlarmTime.hours = Randomizer.GetIntValue(8, 11); // prevent avatar from waking up too late in the day
					pAlarmTime.minutes = Randomizer.GetIntValue(0, 60);
					D.Log("Prevented avatar from waking up too late, set hours to " + pAlarmTime.hours);
				}
			}

			//dialogueLine = "Zzzzz";
			alarmTime = pAlarmTime;
			#if LOG
			logger.Log(name + " fell asleep and will wake up at " + alarmTime);
			#endif
			StartAction("Sleeping", null, LONG_TIME, LONG_TIME);
			_dialogueRunner.EventHappened (name + "_fellAsleep");
		}
				
		public void Hack(MimanTing pHackableTing)
		{
			if(pHackableTing == null) {
				D.Log("Hackable ting of " + name + " was null!");
				return;
			}

			if(hackdev == null) {
				D.Log(name + " has got no hackdev to hack with");
				return;
			}

			//logger.Log("Going to hack " + pHackableTing.name);

			if (pHackableTing != handItem) {
				FaceTing (pHackableTing);
			}
			pHackableTing.PrepareForBeingHacked();

			if (pHackableTing == hackdev) {
				_worldSettings.Notify (name, "Modifier can't modify itself");
			}
			else if(pHackableTing.programs.Length > 0) {
				MockProgram receiver = new MockProgram (retVal => {
					//D.Log("Got response from hackdev Allow function: " + retVal);
					if(retVal.GetType() == typeof(bool) && ((bool)retVal) == true) {
						StartAction("Hacking", pHackableTing, LONG_TIME, LONG_TIME);
						_dialogueRunner.EventHappened(name + "_hack_" + pHackableTing.name);
					} else {
						D.Log("Hacking not allowed with current device for character " + name);
						_worldSettings.Notify(name, "Not allowed with current device");
						StopAction();
					}
				});

				StartAction("AttemptHacking", pHackableTing, 1.0f, 1.0f);

				pHackableTing.PrepareForBeingHacked ();
				hackdev.PrepareForBeingHacked ();

				if (hackdev.masterProgram.HasFunction ("Allow", true)) {
					hackdev.masterProgram.StartAtFunctionWithMockReceiver ("Allow", new object[] { 
						pHackableTing.name,
						(float)pHackableTing.securityLevel
					}, receiver);
				} else {
					_worldSettings.Notify(name, "No Allow-function in " + hackdev.name);
				}
			}
			else {
				_worldSettings.Notify(name, "No programs to hack in " + pHackableTing.name);
				StopAction();
			}
		}
		
		public void SetKnowledge(string pKnowledge)
		{
			foreach(string s in knowledge) {
				if(s == pKnowledge) {
					return;
				}
			}
			string[] newKnowledgeArray = new string[knowledge.Length + 1];
			int i = 0;
			foreach(string s in knowledge) {
				newKnowledgeArray[i++] = s;
			}
			newKnowledgeArray[i] = pKnowledge;
			knowledge = newKnowledgeArray;
		}
		
		public bool HasKnowledge(string pKnowledge)
		{
			foreach(string s in knowledge) {
				if(s == pKnowledge) {
					return true;
				}
			}
			return false;
		}
		
		public void TurnLeft()
		{
			StartAction("TurnLeft", actionOtherObject, 1.0f, 1.0f);
		}
		
		public void TurnRight()
		{
			StartAction("TurnRight", actionOtherObject, 1.0f, 1.0f);
		}
		
		public void WalkTo(WorldCoordinate pPosition)
		{
			PrepareForNewWalkBehaviour(pPosition, null, WalkMode.WALK_TO_POINT);
		}
		
		/// <summary>
		/// If the other ting is pickupable, the character will just pick it up and not do anything with it. 
		/// If the other ting is not pickupable, the character will interact directly with it using the InteractWith() function.
		/// </summary>
		public void WalkToTingAndInteract(Ting pOtherTing)
		{
			rememberToHackComputerAfterSittingDown = false;

			if(isAvatar && pOtherTing is Door) {
				Door door = pOtherTing as Door;
				if(door.isBusy) {
					_worldSettings.Notify(name, "Door in use, will wait in line");
					rememberToUseDoorAfterWaitingPolitely = door;
					WalkTo (new WorldCoordinate(room.name, door.waitingPoint));
					return;
				}
			}

			if (pOtherTing.HasInteractionPointHere (this.position)) {
				if (pOtherTing.canBePickedUp) {
					logger.Log (name + " will pick up " + pOtherTing + " directly, no need to move");
					PickUp (pOtherTing);
				} else {
					logger.Log (name + " will interact directly with " + pOtherTing + ", no need to move");
					InteractWith (pOtherTing);
				}
				return;
			}

			if(pOtherTing is Character && MimanGrimmApiDefinitions.AreTingsWithinDistance(this, pOtherTing, 10)) {
				Character otherCharacter = pOtherTing as Character;
				if(this.sitting || this.laying || otherCharacter.HasNoFreeInteractionPoints()) {
					#if LOG
					logger.Log(name + " is using direct interaction with " + otherCharacter);
					#endif
					InteractWith(pOtherTing);
					return;
				}
			}

			PrepareForNewWalkBehaviour(pOtherTing.position, pOtherTing, WalkMode.WALK_TO_TING_AND_INTERACT);
		}
		
		public void WalkToTingAndUseHandItem(Ting pOtherTing)
		{
			rememberToHackComputerAfterSittingDown = false;

			if (pOtherTing.HasInteractionPointHere (this.position)) {
				UseHandItemToInteractWith (pOtherTing);
				return;
			}

			PrepareForNewWalkBehaviour(pOtherTing.position, pOtherTing, WalkMode.WALK_TO_TING_AND_USE_HAND_ITEM);
		}
		
		public void WalkToTingAndHack(MimanTing pHackableTing)
		{
			rememberToHackComputerAfterSittingDown = true;

			if (pHackableTing.HasInteractionPointHere (this.position)) {
				Hack (pHackableTing);
				return;
			}

			PrepareForNewWalkBehaviour(pHackableTing.position, pHackableTing, WalkMode.WALK_TO_TING_AND_HACK);
		}
		
		private void PrepareForNewWalkBehaviour (WorldCoordinate pFinalTargetPosition, Ting pFinalTargetTing, WalkMode pWalkMode)
		{
			#if LOG
			logger.Log(name + " is preparing new walk behaviour with final target position " + pFinalTargetPosition);
			#endif

			if (pFinalTargetTing is Computer) {
				var tile = pFinalTargetTing.room.GetTile(pFinalTargetTing.interactionPoints[0]);
				if(tile != null) {
					var chairInFrontOfComputer = tile.GetOccupantOfType<Seat>();
					if(chairInFrontOfComputer != null) {
						pFinalTargetTing = chairInFrontOfComputer;
						pWalkMode = WalkMode.WALK_TO_TING_AND_INTERACT; // changes the intent of the player
					}
				} else {
					D.Log("Tile at interaction point for computer " + pFinalTargetTing + " is null, " + name + " can't walk there.");
				}
			}
			else if (pFinalTargetTing is Tram) {
				var tram = pFinalTargetTing as Tram;
				if(tram.movingDoor != null) {
					//pFinalTargetTing = tram.movingDoor;
					WalkToTingAndInteract (tram.movingDoor);
					D.Log("Switched target for " + name + " from " + tram + " to " + tram.movingDoor);
					return;
				}
			}

			if (pFinalTargetTing is Seat && pFinalTargetTing == this.seat) {
				#if LOG
				logger.Log(name + " can't sit on seat " + pFinalTargetTing + " since she/he is already doing that.");
				#endif
				return;
			}

			if (pFinalTargetTing is Bed && pFinalTargetTing == this.bed) {
				#if LOG
				logger.Log(name + " can't lay in bed " + pFinalTargetTing + " since she/he is already doing that.");
				#endif
				return;
			}

			finalTargetPosition = pFinalTargetPosition;
			finalTargetTing = pFinalTargetTing;
			walkMode = pWalkMode;
			walkIterator = 0;

//			if (actionName == "GettingUpFromSeat" || actionName == "GettingUpFromBed") {
//				return;
//			} else if (actionName == "PickingUp" || actionName == "Dropping" || actionName == "DroppingFar") {
//				return;
//			}
			if (busy) {
				return;
			}
			else if (sitting) {
				if (seat != null) {
					GetUpFromSeat ();
					return;
				} else {
					//D.Log("Just start walking (from sitting)");
					sitting = false;
				}
			} else if (laying) {
				if (bed != null) {
					GetUpFromBed ();
					return;
				} else {
					//D.Log ("Just start walking (from laying down)");
					laying = false;
				}
			}
			
			_walkBehaviour = null;
			
			ClearConversationTarget();
			seat = null;
			bed = null;
		}
		
		public void GetUpFromSeat ()
		{
			if (seat == null) {
				D.Log("Seat is null for character " + name);
				return;
			}
			seat.CalculateNewExitPoint();
			StartAction ("GettingUpFromSeat", seat, 1.9f, 1.95f);
		}

		public void GetUpFromBed() 
		{
			if (bed == null) {
				D.Log("Bed is null for character " + name);
				return;
			}
			StartAction ("GettingUpFromBed", bed, 2.5f, 2.5f);
		}

		private bool IsGettingUp ()
		{
			return (actionName == "GettingUpFromSeat") || (actionName == "GettingUpFromBed");
		}
		
		public void BeBored()
		{
			StartAction("BeBored", null, LONG_TIME, LONG_TIME);
		}

		public void CancelWalking()
		{
#if LOG
			System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			logger.Log(name + " canceled walking. Stack trace: " + t.ToString());
#endif
			StopAction();
			ClearWalkingData();
		}

		/// <summary>
		/// This function should be used from the view (Unity) to make a AI-controlled character
		/// stop briefly and look around (checking who's bothering him or her).
		/// During this pause the avatar will have time to interact with the character and
		/// initiate a conversation. If the avatar changes its mind and goes somewhere else the
		/// AI-character should resume any walking behavior and stuff it was currently doing.
		/// </summary>
		public void Bother()
		{
			if(timetableName == "") {
				//D.Log(name + " can't be bothered because it hasn't got a timetable");
				//Say (RandomNoTimeToTalkLine());
			}
			else if(canBeBotheredWhen.Contains(actionName)) {
				//D.Log(name + " was bothered!");
				CancelWalking();
				StartAction("BeingBothered", null, 4.0f, 4.0f);
			}
			else {
				//D.Log(name + " can't be bothered because its action is " + actionName);
			}
		}

		static List<string> canBeBotheredWhen = new List<string>() { 
			"",
			"Walking",
			"GettingUpFromSeat"
		};

		static List<string> noTimeToTalkLines = new List<string>() {
			"Ursäkta, jag har inte tid att prata nu",
			"Kan vi ta det lite senare?",
			"Va? Vi kan ta det imorgon kanske?",
			"Nej, jag vill inte prata just nu",
			"?!",
			"Jag hinner inte nu",
			"Oj, jag har inte tid att snacka nu",
			"Huh?",
		};

		string RandomNoTimeToTalkLine ()
		{
			return noTimeToTalkLines[Randomizer.GetIntValue(0, noTimeToTalkLines.Count)];
		}

		internal void ClearWalkingData()
		{
			walkMode = WalkMode.NO_TARGET;
			walkIterator = -1;
			targetPositionInRoom = WorldCoordinate.NONE;
			finalTargetPosition = WorldCoordinate.NONE;
			finalTargetTing = null;
			if(onRemovedPath != null) onRemovedPath();
			_walkBehaviour = null;
		}
		
		public void AnalyzeNewTile()
		{
			if(_walkBehaviour != null) {
				_walkBehaviour.AnalyzeNewTile();
			}
		}
		
		public override void Update(float dt)
		{
			if(_timetable != null) {
				_timetable.Update(dt, gameClock, this, _tingRunner as MimanTingRunner, _roomRunner, _dialogueRunner, _worldSettings);
			}

			// CHECK FOR WALK MODE USED TO BE AT THIS LOCATION IN THE CODE, I MOVED IT INTO THE ELSE BRANCH OF THE SLEEP CHECK BELOW

			// this is for not holding on to things that are put into trash cans that have no code in them
			if (handItem != null && !handItem.isBeingHeld) {
				handItem = null;
			}

			if(rememberToUseDoorAfterWaitingPolitely != null) {
				if(rememberToUseDoorAfterWaitingPolitely.isBusy) {
					//D.Log(rememberToUseDoorAfterWaitingPolitely + " is busy");
				}
				else if(actionName != "") {
					//D.Log("Waiting politely with action: " + actionName);
				}
				else {
					D.Log("Door is free now!");
					float t = 0.1f;
					StartAction("UseDoorReallySoon", rememberToUseDoorAfterWaitingPolitely, t, t);
					rememberToUseDoorAfterWaitingPolitely = null;
				}
			}

			if(actionName == "Sleeping") 
			{
				sleepiness  -= _worldSettings.gameTimeSpeed * dt * 0.03f; // TODO: this was 0.02 until Sep 11 2015
				if(sleepiness < 0f) {
					sleepiness = 0f;
				}

				corruption -= _worldSettings.gameTimeSpeed * dt * 0.01f;
				if (corruption < 0f) {
					corruption = 0f;
				}

				drunkenness -= _worldSettings.gameTimeSpeed * dt * 0.02f;
				if(drunkenness < 0f) {
					drunkenness = 0f;
				}

				if(room.exterior) {
					smelliness += dt * 0.5f;
					if(smelliness > 100f) {
						smelliness = 100f;
					}
				}

				if(gameClock > alarmTime) {
					//D.Log(name + ", gameClock > alarmTime, " + gameClock + " > " + alarmTime);
					StopAction();
					dialogueLine = "";
					if (bed == null) {
						laying = false;
					}
					#if LOG
					logger.Log(name + " woke up");
					#endif
				}

			}
			else {
				if(walkMode != WalkMode.NO_TARGET && !IsGettingUp())
				{
					EnsureWalkBehaviour();
					_walkBehaviour.Update(dt);
				}

				if(conversationTarget == null && !neverGetsTired && !talking && !laying && !sitting) {

					sleepiness += _worldSettings.gameTimeSpeed * dt * 0.0015f;
					if (sleepiness > 99.0f && IsIdle ()) {
						if (sitting) {
							#if LOG
							logger.Log (name + " falling asleep in a seat from exhaustion");
							#endif
							StartAction("FallingAsleepInChair", seat, 2.0f, 2.0f);
						} else {
							#if LOG
							logger.Log (name + " falling asleep on the street from exhaustion");
							#endif
							FallAsleepFromStanding (8);
						}
					}
				}
				else {
					//	D.Log(name + " is not gettings sleepy... conversationTarget " + conversationTarget + " talking " + talking + " sitting " + sitting + " laying " + laying);
				}

				// Get clean in the rain
				if(room.exterior && _worldSettings.rain > 10f) {
					smelliness -= dt * 4.0f;
					if(smelliness < 0f) {
						smelliness = 0f;
					}
				}
			}

			if(isAvatar && (actionName == "InsideComputer" || room.name == "Internet")) {
				corruption += dt * 0.001f;
			}

			if(corruption > 100) {
				corruption = 100;
			}
			else if(corruption < 0) {
				corruption = 0;
			}
		}

		public override bool autoUnregisterFromUpdate {
			get {
				return false;
			}
		}

		void EnsureWalkBehaviour()
		{
			if(_walkBehaviour == null) {
				CreateNewWalkBehaviour();
			}
		}
		
		public bool HasWalkBehaviour() {
			return _walkBehaviour != null;
		}

		public void CreateNewWalkBehaviour()
		{
			//D.Log ("CreateNewWalkBehaviour()");
			_walkBehaviour = new SmartWalkBehaviour(this, _roomRunner, _tingRunner, _worldSettings);
		}

		/*
		public bool HasRecentlyMadeAFailedPathFindingSearch(TileNode pStartTile, TileNode pEndTile)
		{
			foreach(UnreachablePathCacheItem item in _failedPathfindingSearches) {
				if(item.startTile == pStartTile && item.endTile == pEndTile) {
					return true;
				}
			}
			return false;
		}

		public void RegisterFailedPathFindingSearch(PointTileNode pStartTile, PointTileNode pEndTile)
		{
			D.Log("Registered failed pathfinding for character " + name + " : From " + pStartTile + " to " + pEndTile);
			_failedPathfindingSearches.Add(new UnreachablePathCacheItem() {
				age = 0f,
				startTile = pStartTile,
				endTile = pEndTile,
			});
		}
		*/

		private void UpdateHandItemPosition()
		{
			if(handItem != null) {
				handItem.position = position;
			}
		}

		private void RefreshTimetable()
		{
			if(timetableName == "") {
				_timetable = null;
			}
			else {
				_timetable = _timetableRunner.GetTimetable(timetableName);
			}
		}

		public bool IsIdle()
		{
			if(walkMode != Character.WalkMode.NO_TARGET) return false;
			return actionName == "";
		}

		public bool sleeping {
			get {
				return actionName == "Sleeping";
			}
		}
		
		#region ACCESSORS
		
		[ShowInEditor]
		public string handItemName {
			get {
				return CELL_handItemObjectName.data;
			}
		}
		
		[ShowInEditor]
		public Character conversationTarget {
			get {
				if(CELL_conversationTargetName.data == "") { 
					return null; 
				}
				else {
					return _tingRunner.GetTing(CELL_conversationTargetName.data) as Character;
				}
			}
			set {
				if(value == null) {
					CELL_conversationTargetName.data = "";
				}
				else if(value == this) {
					throw new Exception(name + " can't have itself as conversation target");
				}
				else {
					CELL_conversationTargetName.data = value.name;
				}
			}
		}
		
		[ShowInEditor]
		public Seat seat {
			get {
				if(CELL_seatName.data == "") { 
					return null; 
				}
				else {
					return _tingRunner.GetTing(CELL_seatName.data) as Seat;
				}
			}
			set {
				if(value == null) {
					CELL_seatName.data = "";
				}
				else {
					CELL_seatName.data = value.name;
				}
			}
		}

		[ShowInEditor]
		public Bed bed {
			get {
				if(CELL_bedName.data == "") { 
					return null; 
				}
				else {
					return _tingRunner.GetTing(CELL_bedName.data) as Bed;
				}
			}
			set {
				if(value == null) {
					CELL_bedName.data = "";
				}
				else {
					CELL_bedName.data = value.name;
				}
			}
		}
		
		public MimanTing handItem {
			get {
				if(CELL_handItemObjectName.data == "") { 
					return null; 
				}
				else {
					var item = _tingRunner.GetTingUnsafe(CELL_handItemObjectName.data) as MimanTing;
					if(item == null) {
						D.Log(name + "'s hand item '" + CELL_handItemObjectName.data + "' was not found, setting to null");
						handItem = null;
						return null;
					} else {
						return item;
					}
				}
			}
			set {
				if(value == null) {
					CELL_handItemObjectName.data = "";
				}
				else if(value == this) {
					throw new Exception(name + " can't hold itself as hand item");
				}
				else {
					if(handItem != null) {
						D.Log("Setting hand item of " + name + " to " + value.name + ", will move current item " + handItem.name + " to inventory.");
						handItem.isBeingHeld = false;
						handItem.position = new WorldCoordinate (inventoryRoomName, IntPoint.Zero);
					}

					CELL_handItemObjectName.data = value.name;
					value.position = this.position;
					value.isBeingHeld = true;
				}
			}
		}
		
		[ShowInEditor]
		public WorldCoordinate finalTargetPosition {
			get {
				return CELL_finalTargetPosition.data;
			}
			set {
				CELL_finalTargetPosition.data = value;
			}
		}
		
		[ShowInEditor]
		public Ting finalTargetTing {
			get {
				if(CELL_finalTargetTing.data == "") { 
					return null; 
				}
				else {
					return _tingRunner.GetTingUnsafe(CELL_finalTargetTing.data) as MimanTing;
				}
			}
			set {
				if(value == null) {
					CELL_finalTargetTing.data = "";
				}
				else {
					CELL_finalTargetTing.data = value.name;
				}
			}
		}
		
		[ShowInEditor]
		public WorldCoordinate targetPositionInRoom {
			get {
				return CELL_targetPositionInRoom.data;
			}
			set {
				CELL_targetPositionInRoom.data = value;
			}
		}
		
		[ShowInEditor]
		public WalkMode walkMode {
			get {
				return CELL_walkMode.data;
			}
			set {
				CELL_walkMode.data = value;

				//System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
				//D.Log("walkMode was set to " + value + " stacktrace: " + t.ToString());
			}
		}
		
		[EditableInEditor]
		public float walkSpeed {
			get {
				return CELL_walkSpeed.data;
			}
			set {
				CELL_walkSpeed.data = value;
			}
		}
	
		/// <summary>
		/// The walk timer goes up from 0 to 1, when it hits 1 the character will move to the next tile
		/// </summary>
		[ShowInEditor]
		public float walkTimer {
			get {
				return CELL_walkTimer.data;
			}
			set {
				CELL_walkTimer.data = value;
			}
		}
		
		[EditableInEditor]
		public float charisma {
			get {
				return CELL_charisma.data;
			}
			set {
				CELL_charisma.data = value < 0f ? 0f : value;
			}
		}
		
		[EditableInEditor]
		public float smelliness {
			get {
				return CELL_smelliness.data;
			}
			set {
				CELL_smelliness.data = value < 0f ? 0f : value;
			}
		}
		
		[EditableInEditor]
		public float sleepiness {
			get {
				return CELL_sleepiness.data;
			}
			set {
				CELL_sleepiness.data = value < 0f ? 0f : value;
			}
		}
		
		[EditableInEditor]
		public float drunkenness {
			get {
				return CELL_drunkenness.data;
			}
			set {
				CELL_drunkenness.data = value < 0f ? 0f : value;
			}
		}

		[EditableInEditor]
		public float supremacy {
			get {
				return CELL_supremacy.data;
			}
			set {
				CELL_supremacy.data = value < 0f ? 0f : value;
			}
		}

		[EditableInEditor]
		public float happiness {
			get {
				return CELL_happiness.data;
			}
			set {
				CELL_happiness.data = value < 0f ? 0f : value;
			}
		}
		
		[EditableInEditor]
		public float corruption {
			get {
				return CELL_corruption.data;
			}
			set {
				CELL_corruption.data = value < 0f ? 0f : value;
			}
		}
		
		[ShowInEditor]
		public GameTime alarmTime {
			get {
				return CELL_alarmTime.data;
			}
			set {
				CELL_alarmTime.data = value;
			}
		}
		
		[EditableInEditor]
		public int friendLevel {
			get {
				return CELL_friendLevel.data;
			}
			set {
				CELL_friendLevel.data = value;
			}
		}

		[EditableInEditor]
		public string timetableName {
			get {
				return CELL_timetableName.data;
			}
			set {
				CELL_timetableName.data = value;
				RefreshTimetable();
			}
		}

		[ShowInEditor]
		public string timetableMemory {
			get {
				return CELL_timetableMemory.data;
			}
			set {
				CELL_timetableMemory.data = value;
			}
		}

		[EditableInEditor]
		public float timetableTimer {
			get {
				return CELL_timetableTimer.data;
			}
			set {
				CELL_timetableTimer.data = value;
			}
		}

		public void ResetCurrentTimetableTask ()
		{
			if(_timetable == null) return;

			var currentBehaviour = _timetable.GetCurrentSpan(gameClock).behaviour;
			if(currentBehaviour == null) {
				D.Log("ResetCurrentTimetableTask for " + name + ", current behaviour is null!");
				return;
			}

			D.Log("Reset Current Timetable Task " + currentBehaviour.ToString() + " for " + name);
			timetableTimer = 0f;
			currentBehaviour.Reset();
		}

		public bool IsAtTimetableTask(string pTimetableTaskName)
		{
			if(_timetable == null) return false;

			bool correctTime = (_timetable.GetCurrentSpan(gameClock).name == pTimetableTaskName);
			
			var currentBehaviour = _timetable.GetCurrentSpan(gameClock).behaviour;
			if(currentBehaviour == null) {
				D.Log("Checking IsAtTimetableTask for " + name + ", current behaviour is null!");
				return false;
			}
			bool isAtFinalPartOfTask = currentBehaviour.IsAtFinalPartOfTask(this);

			return correctTime && isAtFinalPartOfTask;
		}

		public bool IsAtTimetableTaskOfType(Type pTimetableTaskType)
		{
			if(_timetable == null) return false;
			var behaviour = _timetable.GetCurrentSpan(gameClock).behaviour;
			if(behaviour == null) return false;
			return behaviour.GetType() == pTimetableTaskType;
		}

		public string[] knowledge {
			get {
				return CELL_knowledge.data;
			}
			set {
				CELL_knowledge.data = value;
			}
		}
		
		public int walkIterator {
			get {
				return CELL_walkIterator.data;
			}
			set {
				CELL_walkIterator.data = value;
			}
		}

		public void SetTimetableRunner(TimetableRunner pTimetableRunner)
		{
			_timetableRunner = pTimetableRunner;
		}

		public Timetable timetable {
			get {
				return _timetable;
			}
		}

		[EditableInEditor]
		public bool talking {
			get {
				return CELL_talking.data;
			}
			set {
				CELL_talking.data = value;
			}
		}

		[EditableInEditor]
		public bool sitting {
			get {
				return CELL_sitting.data;
			}
			set {
				CELL_sitting.data = value;
			}
		}

		[EditableInEditor]
		public bool laying {
			get {
				return CELL_laying.data;
			}
			set {
				CELL_laying.data = value;
			}
		}

		[EditableInEditor]
		public bool running {
			get {
				return CELL_running.data;
			}
			set {
				CELL_running.data = value;
			}
		}
		
		[EditableInEditor]
		public bool waitForGift {
			get {
				return CELL_waitForGift.data;
			}
			set {
				CELL_waitForGift.data = value;
				if(value == true) {
					if(conversationTarget != null && conversationTarget.name == _worldSettings.avatarName) {
						_dialogueRunner.EventHappened("ShowClickHereHelpArrow");
					}
				}
			}
		}

		[EditableInEditor]
		public bool neverGetsTired {
			get {
				return CELL_neverGetsTired.data;
			}
			set {
				CELL_neverGetsTired.data = value;
			}
		}

		[ShowInEditor]
		public float creditCardUsageAmount {
			get {
				return CELL_creditCardUsageAmount.data;
			}
			set {
				CELL_creditCardUsageAmount.data = value;
			}
		}

		[ShowInEditor]
		public bool isWaitingToBeTalkedTo {
			get {
				string avatarName = "Sebastian";
				string eventName = avatarName + "_talk_" + name;
				return _dialogueRunner.IsWaitingOnEvent(eventName);
			}
		}

		[ShowInEditor]
		public SleepinessState sleepinessState {
			get {
				if (sleepiness < 80.0f) {
					return SleepinessState.FRESH;
				} else if (sleepiness < 95.0f) {
					return SleepinessState.CAN_NOT_RUN;
				} else {
					return SleepinessState.SLOW;
				}
			}
		}

		public float calculateFinalWalkSpeed()
		{
			float sleepinessModifier = (sleepinessState == SleepinessState.SLOW) ? 0.75f : 1.0f;
			return walkSpeed * sleepinessModifier;
		}

		// A character is busy when it's doing something that can't be interrupted in a nice way,
		// such as walking through a door or giving someone something.
		// It is NOT busy when Sitting, Sleeping, Idleing, Walking.
		public bool busy {
			get {
				if(actionName == "" || actionName == "FallingAsleep" || actionName == "FallingAsleepInChair" || actionName == "Mixing" || actionName == "Hacking" || actionName == "Inspect" || actionName == "UsingComputer" || actionName == "Walking" || actionName == "Sitting" || actionName == "Sleeping" || actionName == "Dancing" || actionName == "Trumpeting" || actionName == "TalkingInTelephone") {
					return false;
				}

				return true;
			}
		}
		
		public bool hasHackdev {
			get {
				return hackdev != null;
			}
		}

		public Hackdev hackdev {
			get {
				if (handItem is Hackdev) {
					return handItem as Hackdev;
				}

				var hackdevsInInventory = _tingRunner.GetTingsOfTypeInRoom<Hackdev> (inventoryRoomName);

				if (hackdevsInInventory.Length > 0) {
					return hackdevsInInventory [0];
				} else {
					return null;
				}
			}
		}

		public Suitcase[] extraBags {
			get {
				var bags = new List<Suitcase>();

				if (handItem is Suitcase) {
					bags.Add(handItem as Suitcase);
				}
				
				var bagsInInventory = _tingRunner.GetTingsOfTypeInRoom<Suitcase> (inventoryRoomName);
				
				bags.AddRange(bagsInInventory);

				return bags.ToArray();
			}
		}

		public CreditCard creditCard {
			get {
				if (handItem is CreditCard) {
					return handItem as CreditCard;
				}
				
				var creditCardsInInventory = _tingRunner.GetTingsOfTypeInRoom<CreditCard> (inventoryRoomName);
				
				if (creditCardsInInventory.Length > 0) {
					return creditCardsInInventory [0];
				} else {
					return null;
				}
			}
		}

		public bool HasInventoryItemOfType(string pTingTypeName)
		{
			if (handItem != null && handItem.GetType().Name == pTingTypeName) {
				return true;
			}

			foreach (var ting in inventoryItems) {
				if (ting.GetType().Name == pTingTypeName) {
					return true;
				}
			}
			return false;
		}

		public string PrettyPrintableInfo()
		{
			return position.ToString() + " Action: '" + actionName + "' " + actionOtherObjectToStr;
		}

		string actionOtherObjectToStr {
			get {
				if (actionOtherObject == null) {
					return "";
				} else {
					return "(" + actionOtherObject.name + ")";
				}
			}
		}

		public void SlurpIntoInternet(MimanTing pStartingTing) {
			if(actionName != "UsingComputer" && actionName != "Inspect") {
				D.Log(name + " can't slurp because she/he is " + actionName);
				return;
			}
			StartAction ("SlurpingIntoComputer", pStartingTing, 2.0f, 2.0f);
		}

		public int tileGroup {
			get {
				if (this.tile == null) {
					return -1;
				} else {
					return this.tile.group;
				}
			}
		}

		#endregion
	}

}

