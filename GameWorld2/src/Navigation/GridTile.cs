using System;
using System.Collections.Generic;
using System.Text;
using TingTing;

namespace GameWorld2
{
    class GridTile : TileNode, IOccupantHolder<Ting>
    {
        public GridTile(Room pRoom, int pX, int pY, TileType pType) : base(pRoom, pX, pY, pType) { }
        public Ting[] occupants { get; set; }
    }
}
