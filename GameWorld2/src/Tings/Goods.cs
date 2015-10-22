using System;
using GameTypes;
using System.Collections.Generic;
using TingTing;
using RelayLib;

namespace GameWorld2
{
	public class Goods : MimanTing
	{
		public static new string TABLE_NAME = "Tings_Goods";
		
		ValueEntry<char[]> CELL_minerals;
		
		protected override void SetupCells()
		{
			base.SetupCells();
			CELL_minerals = EnsureCell("minerals", "zzzzzzzzzzzzzzzz".ToCharArray());
		}
		
		string GetRandomMineralString() {
			string letters = "abcdefghijklmnopqrstuvwxyz";
			var sb = new System.Text.StringBuilder();
			for(int i = 0; i < 16; i += 1) {
				sb.Append(letters[Randomizer.GetIntValue(0, letters.Length)]);
			}
			return sb.ToString();
		}
		
		public void RandomizeMinerals() {
			minerals = GetRandomMineralString().ToCharArray();
		}
		
		public override void FixBeforeSaving ()
		{
			RandomizeMinerals();
		}

		public override bool DoesMasterProgramExist ()
		{
			return false;
		}

		public override IntPoint[] interactionPoints {
			get {
				return new IntPoint[] {
					localPoint + IntPoint.Up * 1,
					localPoint + IntPoint.Right * 1,
					localPoint + IntPoint.Left * 1,
					localPoint + IntPoint.Down * 1
				};
			}
		}
		
		public override bool canBePickedUp {
			get {
				return true;
			}
		}
		
		public override string verbDescription {
			get {
				return "inspect";
			}
		}
		
		public override string tooltipName {
			get {
				return "goods";
			}
		}
		
		public override bool CanInteractWith(Ting pTingToInteractWith)
		{
			return pTingToInteractWith is SendPipe || pTingToInteractWith is Stove;
		}
		
		[ShowInEditor()]
		public string mineralsDisplayString {
			get {
				var sb = new System.Text.StringBuilder();
				foreach(var c in CELL_minerals.data) {
					sb.Append(c);
				}
				return sb.ToString();
			}
		}
		
		public char[] minerals {
			get {
				return CELL_minerals.data;
			}
			set {
				CELL_minerals.data = value;
			}
		}
		
		// How pure is the mineral content? From 0 to 1
		public float GetPureness() {
			float total = 0f;
			foreach (var c in minerals) {
				//int ci = ((int)c) - ((int)'a');
				//total += 1.0f - ((float)(ci) / 26.0f);
				if(c == 'a') {
					total += 1.0f;
				}
				else if(c == 'b') {
					total += 0.75f;
				}
				else if(c == 'c') {
					total += 0.5f;
				}
				else if(c == 'd') {
					total += 0.4f;
				}
				else if(c == 'e') {
					total += 0.3f;
				}
				else if(c == 'f') {
					total += 0.2f;
				}
				else if(c == 'g') {
					total += 0.1f;
				}
				else {
					// not pure
				}
			}
			return total / minerals.Length;
		}

		public override Program masterProgram {
			get {
				return null;
			}
		}
	}
}

