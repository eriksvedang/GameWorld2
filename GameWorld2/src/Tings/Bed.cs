using System;
using GameTypes;
using RelayLib;
using TingTing;

namespace GameWorld2
{
	public class Bed : MimanTing
	{
		ValueEntry<int> CELL_exitPoint;

		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_exitPoint = EnsureCell("exitPoint", 0);
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.DirectionToIntPoint (IntPoint.Turn(direction, 0)) * 2,
					localPoint + IntPoint.DirectionToIntPoint (IntPoint.Turn(direction, 180)) * 2,
				};
			}
		}

		public override void MaybeFixGroupIfOutsideIslandOfTiles ()
		{
			FixGroupIfOutsideIslandOfTiles();
		}

		public void CalculateNewExitPoint()
		{
			var points = interactionPoints;
			var cachedRoom = room;
			for (int i = 0; i < points.Length; i++) {
				var point = points[i];
				var tile = cachedRoom.GetTile(point);
				if (tile != null && !tile.HasOccupants()) {
					exitPoint = i;
					break;
				}
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		public IntPoint GetCurrentExitPoint()
		{
			return interactionPoints[CELL_exitPoint.data];
		}

		[EditableInEditor]
		public int exitPoint {
			get {
				return CELL_exitPoint.data;
			}
			set {
				CELL_exitPoint.data = value;
			}
		}
		
		public override bool canBePickedUp {
			get {
				return false;
			}
		}
		
		public override string verbDescription {
			get {
				return "sleep in";
			}
		}
		
		public override string tooltipName {
			get {
				return "bed";
			}
		}

		public override bool isBeingUsed {
			get {
				return AnotherTingSharesTheTile ();
			}
		}
	}
}

