using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameWorld2;
using RelayLib;
using TingTing;

namespace GameWorld2_Tests
{
    public class MainClass
    {

        public static void Main( string[] args )
        {
			/*
            FlowerPotTest test = new FlowerPotTest();
            test.SetUp();
            test.RunWorld();
            Console.ReadLine();*/
			
			Console.WriteLine("Start");
			
			InitialSaveFileCreator i = new InitialSaveFileCreator();
			World world = new World(i.CreateRelay("../InitData2"));
			
			foreach(Room r in world.roomRunner.rooms)
			{
				Console.WriteLine("Got room " + r.name + " with " + r.points.Length.ToString() + " tile nodes");
			}
			
			Console.WriteLine("Done");
			
        }
    }
}
