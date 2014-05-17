using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WumpusDungeon
{
    public static class Helpers
    {
        public static Vector2 TilesToAbsolute(this Vector2 vector)
        {
            return new Vector2(vector.X * 64, vector.Y * 64);
        }
        public static Vector2 AbsoluteToTiles(this Vector2 vector)
        {
            return new Vector2((int)vector.X / 64, (int)vector.Y / 64);
        }
    }
}
