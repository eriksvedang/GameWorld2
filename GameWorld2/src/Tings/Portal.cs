using System;
using System.Collections.Generic;
using ProgrammingLanguageNr1;
using RelayLib;
using TingTing;
using GameTypes;
using GrimmLib;

namespace GameWorld2
{
	public class Portal : MimanTing, IExit
	{
        public static new string TABLE_NAME = "Ting_Portals";
		ValueEntry<string> CELL_targetPortalName;
		
		protected override void SetupCells()
		{
			base.SetupCells();
            CELL_targetPortalName = EnsureCell("targetPortalName", "");
		}
		
        public override void Init()
        {
            base.Init();
            //WriteTargetToTile(); // it's essential that we can resolve the starting room and point so that we can write to the tile grid.
        }

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}
		
        private void WriteTargetToTile()
        {
            D.isNull(room, "room is null");
            D.isNull(_roomRunner, "room runner is null!");
			
			PointTileNode tileAtInteractionPoint = room.GetTile(interactionPoints[0]);
			
			if(tileAtInteractionPoint == null) {
				return;
			}
			
			tileAtInteractionPoint.AddOccupant(this);
			
            if (targetPosition == WorldCoordinate.NONE) {
                tileAtInteractionPoint.teleportTarget = null;
			}
            else {
                tileAtInteractionPoint.teleportTarget = _roomRunner.GetRoom(targetPosition.roomName).GetTile(targetPosition.localPosition);
			}
        }

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public void WalkThrough (Character pCharacter)
		{
			if (targetPortal == null) {
				_worldSettings.Notify(pCharacter.name, "This arrow is leading nowhere!");
				return;
			}

			WorldCoordinate newPosition = new WorldCoordinate(targetPortal.room.name, targetPortal.interactionPoints[0]);
			logger.Log(name + " used the portal " + name + " and will now teleport to " + newPosition);
			pCharacter.position = newPosition;
			pCharacter.direction = targetPortal.direction;
			pCharacter.StopAction();
			pCharacter.StartAction("WalkingThroughPortalPhase2", null, 2.2f, 2.2f);
			//pCharacter.StartAction("WalkingThroughDoorPhase2", null, 2.0f, 2.0f);
			//_dialogueRunner.EventHappened(_user.name + "_open_" + name);
		}
        
		[ShowInEditor]
		public WorldCoordinate targetPosition
        {
            get {
				Portal p = targetPortal;
				if(p != null) {
					return new WorldCoordinate(p.room.name, p.interactionPoints[0]);
				}
				else {
                    return WorldCoordinate.NONE;
				}
			}
        }

		public Portal targetPortal
		{
			get {
				return _tingRunner.GetTingUnsafe(targetPortalName) as Portal;
			}
		}

		[ShowInEditor]
		public string targetPortalReferenceStatus {
			get {
				if(targetPortal == null) {
					return "null";
				}
				else {
					return "OK, " + targetPortal.name;
				}
			}
		}
		
		[EditableInEditor]
		public string targetPortalName
		{
			get
            {
                return CELL_targetPortalName.data;
            }
            set 
			{ 
                CELL_targetPortalName.data = value;
                WriteTargetToTile();
            }
		}

        public override string tooltipName {
			get {
				return "another area";
			}
		}
		
		public override string verbDescription {
			get {
				return "walk over to";
			}
		}
		
		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { localPoint + IntPoint.DirectionToIntPoint(direction) * 2 };
			}
		}
		
		public Ting GetLinkTarget ()
		{
			return targetPortal;
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

