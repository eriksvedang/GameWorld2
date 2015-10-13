using System;
using System.Collections.Generic;
using System.Text;
using TingTing;

namespace GameWorld2
{
    public class GridDoor : TileNode, IOccupantHolder<Ting>
    {
        public Ting[] occupants { get; set; }
    }
}
