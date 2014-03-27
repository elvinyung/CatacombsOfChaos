using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Possibly used; this is the class file for 

namespace prjTileRPG_1
{
    class World
    {
        public string name;
        List<Map> locations = new List<Map>();

    }

    class Dungeon
    {
        public string name;
        public List<Map> levels = new List<Map>();
        public Map.mapType dungeonType;

        public void saveDungeon(string path)
        {

        }

        public void loadDungeon(string path)
        {

        }
    }
}
