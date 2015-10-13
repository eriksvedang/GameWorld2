using System;
using System.Collections.Generic;
using System.Text;

namespace GameWorld2
{
    public interface IOccupantHolder<T>
    {
        T[] occupants { get; set; }
    }
}
