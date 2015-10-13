//#define LOG

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using GameTypes;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace GameWorld2
{
	public class Translator
	{
		public Logger logger = new Logger();

		public enum Language
		{
			NOT_SET,
			SWEDISH,
			ENGLISH,
			LATIN,
		}
			
		Language _language = Language.SWEDISH;
		Dictionary<Language, Dictionary<string, Dictionary<string, string>>> _dict; // Lang -> Dia (filename) -> Swe -> Translation

		public Translator (Language pLanguage) {
			_language = pLanguage;
		}

		public void LoadTranslationFiles(string pRootDirectory)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			_dict = new Dictionary<Language, Dictionary<string, Dictionary<string, string>>> ();
			_dict.Add (Language.ENGLISH, new Dictionary<string, Dictionary<string, string>> ());
			_dict.Add (Language.LATIN, new Dictionary<string, Dictionary<string, string>> ());

			string[] files = GetFilesRecursively(pRootDirectory);

			for (int i = 0; i < files.Length; i++)
			{
				FoundFile(files[i]);
			}

			stopwatch.Stop();
			//D.Log("Loading translation files took " + stopwatch.ElapsedMilliseconds / 1000.0f + " s.");
		}

		private string[] GetFilesRecursively(string pPath) {
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

		void FoundFile(string pFilepath) {
			if (pFilepath.EndsWith (".mtf")) {
				Language lang = Language.NOT_SET;

				if(pFilepath.Contains(".eng")) {
					lang = Language.ENGLISH;
				}
				else if(pFilepath.Contains(".lat")) {
					lang = Language.LATIN;
				}

				if (lang != Language.NOT_SET) {
					LoadTranslationsFile (pFilepath, lang);
				} else {
					throw new Exception ("Can't handle file path " + pFilepath);
				}
			}
		}

		static Regex regex = new Regex("\"(.+)\" => \"(.+)\"");

		void LoadTranslationsFile(string pFilepath, Language pLanguage) {
			#if LOG
			D.Log ("Loading translations from " + pFilepath + " (" + pLanguage + ")");
			#endif

			var filestream = File.OpenText (pFilepath);
			int lineNr = 0;

			while(!filestream.EndOfStream) {
				string line = filestream.ReadLine ();

				//D.Log (" - " + line);

				Match match = regex.Match (line);

				if (match.Success) {
					string swedishKeySentence = match.Groups [1].Value;
					string translation = match.Groups [2].Value;
					string dia = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(pFilepath)); // remove both the .mtf extension AND the language extension
					var dictForLanguage = _dict [pLanguage];
					Dictionary<string, string> dictForDia = null;		
					if(!dictForLanguage.TryGetValue(dia, out dictForDia)) {
						//D.Log("No dict for dia '" + dia + "' in language: " + pLanguage + ", will create empty dictionary.");
						dictForDia = new Dictionary<string, string>();
						dictForLanguage[dia] = dictForDia;
					}
					dictForDia [swedishKeySentence] = translation;

					#if LOG
					D.Log ("Added translation in dia '" + dia + "': " + swedishKeySentence + " => " + translation);
					//D.Log ("Added translation: " + swedishKeySentence + " => " + translation);
					#endif
				} else {
					//D.Log ("Failed to match line " + lineNr + ": " + line + " in file " + pFilepath);
				}

				lineNr++;
			}

			filestream.Close ();
		}

		internal void SetLanguage(Language pLanguage) {
			if (pLanguage != Language.SWEDISH && _dict == null) {
				throw new Exception ("Must load translation files before changing the language to something other than Swedish");
			}
			_language = pLanguage;
		}

		public string Get(string pSentenceToTranslate, string pDialogue) {

			D.isNull(pDialogue, "pSentenceToTranslate can't be null!");
			D.isNull(pDialogue, "pDialogue can't be null!");

			if (_language == Language.SWEDISH) {
				return pSentenceToTranslate;
			}

			if (_dict == null) {
				throw new Exception ("Can't translate sentence, translations files are not loaded");
			}

			string translation;
			var dictForLanguage = _dict [_language];
			Dictionary<string, string> dictForDia = null;

			if(!dictForLanguage.TryGetValue(pDialogue, out dictForDia)) {
				D.Log("WARNING! Didn't find translation for '" + pSentenceToTranslate + "' and dialogue '" + pDialogue + "' in language '" + _language + "'.");
				return pSentenceToTranslate;
			}

			bool foundTranslation = dictForDia.TryGetValue (pSentenceToTranslate, out translation);

			if (foundTranslation) {
				return translation;
			} else {
				D.Log ("Found no translation for " + pSentenceToTranslate + " in dia '" + pDialogue + "'");
				return pSentenceToTranslate;
			}
		}
	}
}

