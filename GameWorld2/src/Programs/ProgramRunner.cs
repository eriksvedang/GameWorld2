using System;
using System.Collections.Generic;
using RelayLib;
using GameTypes;
using System.Reflection;
using System.Linq;

namespace GameWorld2
{
	public class ProgramRunner
	{
		TableTwo _programTable;
		Dictionary<int, Program> _programsDictionary = new Dictionary<int, Program>();
		List<Program> _programsList;
		List<Program> _newPrograms = new List<Program>();

		public ProgramRunner(RelayTwo pRelay)
		{
			D.isNull(pRelay);
			_programTable = pRelay.GetTable(Program.TABLE_NAME);
            _programsList = InstantiatorTwo.Process<Program>(_programTable);
			foreach (var program in _programsList) {
				program.Init(this);
				_programsDictionary.Add(program.objectId, program);
			}
		}
				
		public Program CreateProgram(string pName, string pSourceCodeContent, string pNameOfSourceCode)
		{
			Program newProgram = new Program();
			newProgram.CreateNewRelayEntry(_programTable, typeof(Program).Name);
			newProgram.name = pName;
			newProgram.sourceCodeContent = pSourceCodeContent;
			newProgram.sourceCodeName = pNameOfSourceCode;
			newProgram.Init(this);
         
			_newPrograms.Add(newProgram);

			return newProgram;
		}
		
		public Program CreateProgram(SourceCode pSourceCode)
		{
			return CreateProgram(pSourceCode.name, pSourceCode.content, "unknown");
		}
		
		public Program GetProgram(int pObjectId)
		{
			Program p = null;
			_programsDictionary.TryGetValue(pObjectId, out p);

			if(p != null) {
				return p;
			} else {
				throw new Exception("Can't find program with object id " + pObjectId + " in ProgramRunner");
			}
		}

		public Program GetProgramUnsafe(int pObjectId)
		{
			Program p = null;
			if(_programsDictionary.TryGetValue(pObjectId, out p)) {
				return p;
			}
			else {
				foreach(var np in _newPrograms) {
					if(np.objectId == pObjectId) {
						return np;
					}
				}
				return null;
			}
		}

		public void Update(float dt)
		{
			foreach(var newProgram in _newPrograms) {
				// Must add to both lists!
				_programsDictionary.Add (newProgram.objectId, newProgram);
				_programsList.Add(newProgram);
			}
			_newPrograms.Clear();

			foreach(Program program in _programsList)
			{
				if(program.isOn) {
					program.Update(dt);
				}
			}
		}
		
		public override string ToString()
		{
			return string.Format("ProgramRunner ({0} programs)", _programsDictionary.Count);
		}

		public Program[] GetAllPrograms() {
			return _programsList.ToArray ();
		}
	}
}

