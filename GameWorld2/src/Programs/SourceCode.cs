using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using RelayLib;

namespace GameWorld2
{
	public class SourceCode : RelayObjectTwo
	{
		public const string TABLE_NAME = "SourceCodes";
		
		ValueEntry<string> CELL_name;
		ValueEntry<string> CELL_content;
		
		protected override void SetupCells()
		{
			CELL_name = EnsureCell("name", "undefined");
            CELL_content = EnsureCell("content", "");
		}
		
		public string name {
			get {
				return CELL_name.data;
			}
			set {
                CELL_name.data = value;
			}
		}
		
		public string content {
			get {
                return CELL_content.data;
			}
			set {
				CELL_content.data = value;
			}
		}
		
	}
}

