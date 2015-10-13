#define PROFILE

using System;
using System.IO;
using GameTypes;
using RelayLib;
using GrimmLib;
using TingTing;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameWorld2
{
	public class InitialSaveFileCreator
	{	
		public Logger logger = new Logger();
		
		RelayTwo _relay;
		DialogueScriptLoader _dialogueScriptLoader;
		SourceCodeDispenser _sourceCodeDispenser;
		TimetableRunner _timetableRunner;

		public RelayTwo CreateEmptyRelay()
		{
			RelayTwo relay = new RelayTwo();
            
			relay.CreateTable(Ting.TABLE_NAME);
			relay.CreateTable(Room.TABLE_NAME);
			relay.CreateTable(DialogueNode.TABLE_NAME);
			relay.CreateTable(SourceCode.TABLE_NAME);
			relay.CreateTable(Program.TABLE_NAME);
			relay.CreateTable(WorldSettings.TABLE_NAME);
			relay.CreateTable(Timetable.TABLE_NAME);

			return relay;
		}
		
		public RelayTwo CreateRelay(string pInputDirectory)
		{
			return CreateRelay(pInputDirectory, false);
		}
		
		public RelayTwo CreateRelay(string pInputDirectory, bool pOnlyLoadTingsAndRooms)
		{
			_relay = CreateEmptyRelay();
			DialogueRunner dialogueRunner = new DialogueRunner(_relay, Language.DEFAULT);
			_dialogueScriptLoader = new DialogueScriptLoader(dialogueRunner);
			_sourceCodeDispenser = new SourceCodeDispenser(_relay);
			_timetableRunner = new TimetableRunner(_relay);

            foreach (string s in GetFilesRecursively(pInputDirectory)) {
                FoundFile(s, pOnlyLoadTingsAndRooms);
			}
			
			return _relay;
		}
		
        public IEnumerable<float> LoadFromFile(string pFilename)
        {
            _relay = new RelayTwo();
            return _relay.Load(pFilename);
        }
		
		public IEnumerable<float> LoadRelayFromDirectory(string pInputDirectory)
        {	
			return LoadRelayFromDirectory(pInputDirectory, false);
		}
		
        public IEnumerable<float> LoadRelayFromDirectory(string pInputDirectory, bool pOnlyLoadTingsAndRooms)
        {	
#if PROFILE
			Stopwatch sw = new Stopwatch();
			sw.Start();
#endif
			
            _relay = CreateEmptyRelay();
            DialogueRunner dialogueRunner = new DialogueRunner(_relay, Language.DEFAULT);
            _dialogueScriptLoader = new DialogueScriptLoader(dialogueRunner);
            _sourceCodeDispenser = new SourceCodeDispenser(_relay);
			_timetableRunner = new TimetableRunner(_relay);
            
			string[] files = GetFilesRecursively(pInputDirectory);
			
            for (int i = 0; i < files.Length; i++)
            {
                FoundFile(files[i], pOnlyLoadTingsAndRooms);
				yield return (float)i / (float)files.Length;
            }
			
#if PROFILE
			sw.Stop();
			logger.Log("Loading relay from directory " + pInputDirectory + " took " + sw.Elapsed.TotalSeconds + " s.");
#endif
        }
		
        public RelayTwo GetLoadedRelay()
        {
            return _relay;
        }
		
		/// <summary>
		/// Creates a Relay save file suitable for loading into World when a new game of Miman is created.
		/// The input directory specified will be searched through recursively and all files with approperiate
		/// file endings (.dia, .sprak, etc) will be added to the save.
		/// </summary>
		public void CreateSaveFile(string pInputDirectory, string pOutputFilepath, bool pOnlyLoadTingsAndRooms)
		{
			CreateRelay(pInputDirectory, pOnlyLoadTingsAndRooms);
			_relay.SaveAll(pOutputFilepath);
		}
		
		public void CreateSaveFile(string pInputDirectory, string pOutputFilepath)
		{
			CreateSaveFile(pInputDirectory, pOutputFilepath, false);
		}

        private string[] GetFilesRecursively(string pPath)
        {
            List<string> foundFiles = new List<string>();
            string[] dirs = Directory.GetDirectories(pPath);
            string[] files = Directory.GetFiles(pPath);
			
            foreach (string fileName in files)
            {
                foundFiles.Add(fileName);
            }
            
			foreach (string dirName in dirs)
            {
                foundFiles.AddRange(GetFilesRecursively(dirName));
            }
            
			return foundFiles.ToArray();
        }
		
		private void FoundFile(string pFilepath, bool pOnlyLoadTingsAndRooms)
		{
			//logger.Log("Found file with path '" + pFilepath + "'");
			
            if (pFilepath.Contains(".svn")) return;

			#if PROFILE
			Stopwatch sw = new Stopwatch();
			sw.Start();
			#endif
			
			if(pFilepath.EndsWith(".dia") && !pOnlyLoadTingsAndRooms)
			{
				//logger.Log("Loading it as Dialogue");
				_dialogueScriptLoader.LoadDialogueNodesFromFile(pFilepath);
			}
            else if (pFilepath.EndsWith(".tings"))
            {
                _relay.AppendTables(pFilepath);
            }
			else if (pFilepath.EndsWith(".json")/* && !pOnlyLoadTingsAndRooms*/)
            {
                //logger.Log("Loading " + pFilepath + " as Relay Database");
                _relay.MergeWith(new RelayTwo(pFilepath));
            }
            else if (pFilepath.EndsWith(".sprak") && !pOnlyLoadTingsAndRooms)
            {
                //logger.Log("Loading it as Program");
                _sourceCodeDispenser.LoadSourceCode(pFilepath);
            }
			else if (pFilepath.EndsWith(".ttt") && !pOnlyLoadTingsAndRooms)
            {
				_timetableRunner.LoadTimetableFromFile(pFilepath);   
            }

			#if PROFILE
			sw.Stop();
			if(sw.Elapsed.TotalSeconds > 0.1f) {
				//Console.WriteLine("\nOBS! Loading " + pFilepath + " took " + sw.Elapsed.TotalSeconds + " s.");
			}
			#endif
		}
	}
}

