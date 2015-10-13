using System;
using GameWorld2;
using GameTypes;
using RelayLib;
using TingTing;

namespace GameWorld2
{
	public class Seat : MimanTing
	{
		ValueEntry<int> CELL_exitPoint; // which one of the interaction points that should be used when exiting the seat

		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_exitPoint = EnsureCell("exitPoint", 0);
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] { 
					localPoint + IntPoint.DirectionToIntPoint(direction) * 1,
					localPoint + IntPoint.DirectionToIntPoint(direction).RotatedWithDegrees(-90.0f),
					localPoint + IntPoint.DirectionToIntPoint(direction).RotatedWithDegrees(90.0f)
				};
			}
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		// The point where a computer has to be if the person on this seat should be able to use it
		public IntPoint computerPoint {
			get {
				return localPoint + IntPoint.DirectionToIntPoint(direction) * 2;
			}
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
		
		public override string tooltipName {
			get {
				return "";
			}
		}
		
		public override string verbDescription {
			get {
				return "sit down";
			}
		}

		[TingTing.ShowInEditor]
		public override bool isBeingUsed {
			get {
				return AnotherTingSharesTheTile ();
			}
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

