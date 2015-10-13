//#define DEBUG_WRITE

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using RelayLib;

namespace GameWorld2
{
	public class SourceCodeDispenser
	{		
		TableTwo _sourceCodeTable;
		List<SourceCode> _sourceCodes = new List<SourceCode>();
		
		public SourceCodeDispenser(RelayTwo pRelay)
		{
			_sourceCodeTable = pRelay.GetTable(SourceCode.TABLE_NAME);
            //Console.WriteLine("Getting source code from " + _sourceCodeTable.name);
            _sourceCodes = InstantiatorTwo.Process<SourceCode>(_sourceCodeTable);
            foreach (SourceCode sc in _sourceCodes)
            {
                //Console.WriteLine("found source code " + sc.name);
            }
		}

		public SourceCode CreateSourceCodeFromString(string pName, string pContent)
		{
			SourceCode newSourceCode = new SourceCode();
			newSourceCode.CreateNewRelayEntry(_sourceCodeTable, typeof(SourceCode).Name);
			newSourceCode.content = pContent;
			newSourceCode.name = pName;

			_sourceCodes.Add(newSourceCode);

			return newSourceCode;
		}
		
		public void LoadSourceCode(string pFilePath)
		{
			string name = FileHelper.GetNameFromFilepath(pFilePath);
			using (StreamReader sr = File.OpenText (pFilePath)) {
				string content = sr.ReadToEnd ();
#if DEBUG_WRITE
			Console.WriteLine("Read source code '" + name + "':\n" + content + "\n -eof-");
#endif
			
				CreateSourceCodeFromString (name, content);
				sr.Close ();
			}
		}
		
		public SourceCode GetSourceCode(string pName)
		{
			SourceCode s = _sourceCodes.Find(o => o.name == pName);
			if(s != null) {
				return s;
			}
			else {
				throw new Exception("Can't find SourceCode with name '" + pName + "' in Source Code Dispenser");
			}			
		}
		
		public override string ToString()
		{
			return string.Format("SourceCodeDispenser ({0} source codes)", _sourceCodes.Count);
		}
	}
}

