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

//This is the class for operateable in-game objects in the Map object.

namespace prjTileRPG_1
{
    class Objects
    {
        public string name; //obviously, name of the object
        public int X, Y; //represents coordinates on the tile Map
        public int screenX, screenY; //represents coordinates on the screen
        public Sprite sprite;
        public Vector2 positionRelativeToMapOrigin; //represents the coord position of sprite rel to map
        public bool canUse = false; //decides whether the player can use it or not

        public void Draw(SpriteBatch spriteBatch, Map currentMap)
        {
            //this.sprite.Position = new Vector2(currentMap.originX + (32 * this.X), currentMap.originY + (32 * this.Y));
            this.sprite.Draw(spriteBatch, this.sprite.tileCode);
        }
    }

    class Container : Objects
    {
        //for chests, boxes, etc...
        public List<Item> contains = new List<Item>();
    }

    class Trap : Objects
    {
        public Ability ability;
        public int range;
    }

    class Spawner : Objects
    {
        Mob mobSpawn;
    }

    class Door : Objects
    {
        public enum Direction { Up, Down };
        public Direction destination; //represents the change in dungeon level index
    }

    class Shrine : Objects
    {

    }
}
