using System;
using NUnit.Framework;
using TingTing;
using GrimmLib;
using GameWorld2;
using GameTypes;

namespace GameWorld2_Tests
{
	public class WorldTestHelper
	{
		public const string INIT_DATA_PATH = "../InitData1/";
		public const string INITIAL_SAVE_1 = "../Saves/InitialSave1.json";

		public static void GenerateInitData()
		{
            InitialSaveFileCreator i = new InitialSaveFileCreator();
            World world = new World(i.CreateEmptyRelay());

            SimpleRoomBuilder srb = new SimpleRoomBuilder(world.roomRunner);
            srb.CreateRoomWithSize("Eden", 5, 5);
            srb.CreateRoomWithSize("Hallway", 5, 5);
            srb.CreateRoomWithSize("Kitchen", 10, 5);
            srb.CreateRoomWithSize("Bedroom", 5, 5);

            world.tingRunner.CreateTing<Character>("Adam", new WorldCoordinate("Eden", new IntPoint(0, 0)));
            world.tingRunner.CreateTing<Character>("Eva", new WorldCoordinate("Eden", new IntPoint(4, 4)));

            MysticalCube c = world.tingRunner.CreateTing<MysticalCube>("PowerCube", new WorldCoordinate("Eden", new IntPoint(2, 2)));
            c.onInteractionSourceCodeName = "PowerCube";
            
			world.relay.SaveTableSubsetSeparately(Room.TABLE_NAME, INIT_DATA_PATH + "Rooms.json");
            RelayLib.RelayTwo tingSubset = new RelayLib.RelayTwo();

            foreach(string table in world.tingRunner.loadedTingTables)
                tingSubset.MergeWith(world.relay.Subset(table, (o) => { return true; }));

            tingSubset.SaveAll(INIT_DATA_PATH + "Tings.json");

		}

		public static void PreloadWorld(World pWorld)
		{
			if(!pWorld.isReadyToPlay) {
                foreach (string s in pWorld.Preload())
				{
                    //Console.WriteLine(s);
				}
			}
		}		                               

		public static void UpdateWorld(World pWorld, float pTime)
		{
			PreloadWorld(pWorld);

			const float stepSize = 1.0f / 30f;
			float time = 0f;
			while(time < pTime)
			{
				pWorld.Update(stepSize);
				time += stepSize;
			}
		}

		public static void UpdateWorldUntilGameTime(World pWorld, GameTime pGameTime)
		{
			PreloadWorld(pWorld);

			const float stepSize = 1f;

			//Console.WriteLine("Total world time: " + pWorld.settings.gameTimeClock.totalSeconds);
			//Console.WriteLine("pGameTime.totalSeconds: " + pGameTime.totalSeconds);

			int i = 1000;

			while(pWorld.settings.gameTimeClock.totalSeconds < pGameTime.totalSeconds)
			{
				i--;
				//Console.WriteLine("Total world time: " + pWorld.settings.gameTimeClock.totalSeconds);
				pWorld.Update(stepSize);
			}
		}
	}
}

