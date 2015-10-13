#define TIMING

using System;
using System.Reflection;
using GameTypes;
using RelayLib;
using GrimmLib;
using TingTing;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameWorld2
{    
	public sealed class World : IPreloadable
	{
        public bool isReadyToPlay { get; private set; }
        
		public World(RelayTwo pRelay) {
			Init(pRelay);
		}
		
		public World(string pFilepath) {
			Init(new RelayTwo(pFilepath));
		}
				
		public bool paused {
			get;
			set;
		}

		private void Init(RelayTwo pRelay)
        {
			paused = false;
			isReadyToPlay = false;
			relay = pRelay;
			dialogueRunner = new DialogueRunner(relay, Language.DEFAULT);
			roomRunner = new RoomRunner(relay);
			programRunner = new ProgramRunner(relay);
			sourceCodeDispenser = new SourceCodeDispenser(relay);
			timetableRunner = new TimetableRunner(relay);
			settings = new WorldSettings(relay);
            tingRunner = new MimanTingRunner(relay, dialogueRunner, programRunner, sourceCodeDispenser, roomRunner, timetableRunner, settings);
			grimmApiDefinitions = new MimanGrimmApiDefinitions(this);
			grimmApiDefinitions.RegisterFunctions();
			grimmApiDefinitions.RegisterExpressions();
			translator = new Translator (Translator.Language.SWEDISH);
		}
	
		public void Save(string pFilepath)
        {
			relay.SaveAll(pFilepath);
		}
		
		public void Update(float dt)
        {
            if (!isReadyToPlay)
            {
                throw new Exception("Must preload before update!");
            }

			if(paused) {
				return;
			}

			settings.tickNr++;
			settings.totalWorldTime += dt;
			settings.gameTimeSeconds += dt * settings.gameTimeSpeed;
			settings.UpdateRain(dt);

#if TIMING
			Stopwatch sw = new Stopwatch();
			sw.Start();
#endif

			programRunner.Update(dt);

#if TIMING
			sw.Stop();
			double programRunnerTime = sw.Elapsed.TotalSeconds;
			sw.Reset();
			sw.Start();
#endif

			dialogueRunner.Update(dt);

#if TIMING
			double dialogueRunnerTime = sw.Elapsed.TotalSeconds;
			sw.Reset();
			sw.Start();
#endif

            tingRunner.Update(dt, settings.gameTimeClock, settings.totalWorldTime);

#if TIMING
			double tingRunnerTime = sw.Elapsed.TotalSeconds;

			double total = programRunnerTime + dialogueRunnerTime + tingRunnerTime;
			if(total > 0.16f) {
				D.Log("TIMING DATA SLOW FRAME (" + total + " s.) | programs: " + programRunnerTime + ", dialogue: " + dialogueRunnerTime + ", tings: " + tingRunnerTime);
			}
#endif
		}
		
		#region ACCESSORS
		
		public MimanTingRunner tingRunner { get; private set; }
		public DialogueRunner dialogueRunner { get; private set; }
		public RoomRunner roomRunner { get; private set; }
		public ProgramRunner programRunner { get; private set; }
		public SourceCodeDispenser sourceCodeDispenser { get; private set; }
		public RelayTwo relay { get; private set; }
		public WorldSettings settings { get; private set; }
		public TimetableRunner timetableRunner { get; set; }
		public MimanGrimmApiDefinitions grimmApiDefinitions { get; set; }
		public Translator translator { get; private set; }
		
		#endregion

        #region IPreloadable Members

        public IEnumerable<string> Preload()
        {
            yield return "Preparing rooms";

			Stopwatch roomPreloadTimer = new Stopwatch();
			roomPreloadTimer.Start();

            foreach (string s in roomRunner.Preload()) {
                yield return s;
			}

			roomPreloadTimer.Stop();
			//D.Log("Preparing rooms took " + roomPreloadTimer.ElapsedMilliseconds / 1000.0f + " s.");
			
			yield return "Preparing programs";
            foreach (MimanTing t in tingRunner.GetTings()) {
                t.PrepareForBeingHacked();
				t.MaybeFixGroupIfOutsideIslandOfTiles();
				t.StartMasterProgramIfItIsOn();
			}

			RefreshTranslationLanguage ();
			MimanPathfinder2.ClearRoomNetwork();

            isReadyToPlay = true;
        }

        #endregion

		public void RefreshTranslationLanguage ()
		{
			Dictionary<string, Translator.Language> langsForStrings = new Dictionary<string, Translator.Language> () {
				{"swe", Translator.Language.SWEDISH},
				{"eng", Translator.Language.ENGLISH},
				{"lat", Translator.Language.LATIN},
			};

			translator.SetLanguage (langsForStrings [settings.translationLanguage]);
		}
    }
}

