using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace prjTileRPG_1
{
    class Quest
    {
        Mob target;
        NPC questGiver = new NPC();
        List<Objective> goals = new List<Objective>();
        
    }

    class Objective
    {
        enum Type
        {
            Boss,
            Item,
            
        }
    }
}
