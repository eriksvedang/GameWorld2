using System;
using System.Collections.Generic;
using RelayLib;
using System.IO;
using TingTing;
using System.Text;

namespace GameWorld2
{
	public class TimetableRunner
	{
		TableTwo _timetableTable;
		Dictionary<string, Timetable> _timetables = new Dictionary<string, Timetable>();

		public TimetableRunner(RelayTwo pRelay) //, TingRunner pTingRunner)
		{
			_timetableTable = pRelay.GetTable(Timetable.TABLE_NAME);

			foreach(var timetable in InstantiatorTwo.Process<Timetable>(_timetableTable)) {
				_timetables[timetable.name] = timetable;
			}
		}

		public void LoadTimetableFromFile(string pFilePath)
		{
			string name = FileHelper.GetNameFromFilepath(pFilePath);

			using (StreamReader sr = File.OpenText (pFilePath)) {
				StringBuilder sb = new StringBuilder ();

				while (!sr.EndOfStream) {
					string line = sr.ReadLine ().Trim ();

					if (line.Length > 0 && line.Substring (0, 1) == "[") {
						// A new section
						if (sb.Length > 0) {
							CreateTimetable (name, sb.ToString ());
							sb = new StringBuilder ();
						}
						name = line.Substring (1, line.Length - 2);
					} else {
						// Normal line
						sb.Append (line + "\n");
					}
				}

				CreateTimetable (name, sb.ToString ());

				sr.Close ();
			}
		}

		public Timetable CreateTimetable(string pName, string pContent)
		{
			//Console.WriteLine("CreateTimetable '" + pName + "' with content " + pContent);
			Timetable newTimetable = new Timetable();
			newTimetable.CreateNewRelayEntry(_timetableTable, typeof(Timetable).Name);
			newTimetable.name = pName;
			newTimetable.fileContent = pContent; // this will generate the timetables
			_timetables[pName] = newTimetable;
			return newTimetable;
		}

		public Timetable GetTimetable(string pName)
		{
			Timetable t = null;
			_timetables.TryGetValue(pName, out t);
#if DEBUG
			if(t == null) {
				throw new Exception("Can't find timetable with name " + pName + " in TimetableRunner");
			}
#endif
			return t;
		}

		public override string ToString()
		{
			return string.Format("TimetableRunner ({0} timetables)", _timetables.Values.Count);
		}
	}
}

