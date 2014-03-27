using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace prjTileRPG_1
{
    class Tile
    {
        public Sprite sprite = new Sprite();
        public bool walkThru; //probably wont need this any more
        public bool discovered; 

        public int x, y; //represents coordinates on the screen

        public int tileType;
        public int lightLevel; // determines the opacity of the tile
        /* A NOTE ON THE tileType INTEGER:
         * The tileType integer is based on the Minecraft data values. 
         */
    }
}
