using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

//This is the class file for the tile-based Maps used in the game.

namespace prjTileRPG_1
{

    class Map
    {
        Random r = new Random();
        public string name;        
        public enum mapType { Town, Ruins, Cavern, Camp, dwarfHold, Temple, desertTemple, Tower };
        public Vector2 moveVector; //this represents the vector that map origin moves from
        public Vector2 origin; //represents the origin of the map
        private Game mainGame; //represents the main game

        public int originX;
        public int originY; //represents screen coordinates where the screen will be drawn

        public List<List<Tile>> lowerTiles = new List<List<Tile>>();
        public List<List<Tile>> midTiles = new List<List<Tile>>();
        public List<List<Tile>> upperTiles = new List<List<Tile>>();
        public List<List<Tile>> mlowerTiles = new List<List<Tile>>();
        public List<List<Tile>> mMidTiles = new List<List<Tile>>();
        public List<List<Tile>> mUpperTiles = new List<List<Tile>>();
        public int height; //height of the map in terms of pixels 
        public int width; //width of the map in terms of pixels

        public List<NPC> npcs = new List<NPC>(); //the list of npcs on the map
        public List<Mob> mobTypes = new List<Mob>(); //the list of possible mobs on the map to copy from
        public List<Mob> mobs = new List<Mob>(); //the list of mobs on the map

        public List<Door> doors = new List<Door>(); //doors in the map, doors[0] is the "main" entrance
        public List<Container> chests = new List<Container>(); //list of treasure chests in the map

        /********************************************************
         *                                                      *
         *   BELOW: BEST EXAMPLE OF ARRAYS                      *
         *                                                      *
         ********************************************************/

        public void generate(Game mainGame, mapType type, int width, int height)
        {
            this.mainGame = mainGame;
            this.height = height;
            this.width = width;
            for (int y = 0; y < height; y++) //a is the current y-coordinate
            {
                lowerTiles.Add(new List<Tile>());
                midTiles.Add(new List<Tile>());
                upperTiles.Add(new List<Tile>());
                for (int x = 0; x < width; x++) //b is the current x-coordinate
                {
                    lowerTiles[y].Add(new Tile());
                    lowerTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    lowerTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                    lowerTiles[y][x].tileType = 0;

                    midTiles[y].Add(new Tile());
                    midTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    midTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                    midTiles[y][x].tileType = 0;

                    upperTiles[y].Add(new Tile());
                    upperTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    upperTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                    upperTiles[y][x].tileType = 0;
                }
            }

            if (type == mapType.Town)
            {
                //town generation algorithm, incomplete
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < height; x++)
                        lowerTiles[y][x].tileType = 4;

                doors.Add(new Door());
                doors[0].destination = Door.Direction.Down;
                doors[0].sprite = new Sprite();
                doors[0].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                doors[0].sprite.Size = new Rectangle(0, 0, 32, 32);
                doors[0].sprite.tileCode = 0;
                doors[0].positionRelativeToMapOrigin = new Vector2(32 * doors[0].X, 32 * doors[0].Y);

                //road generation
                /* Roads are limited to the inner half of the map
                 *  #### # = no roads valid
                 *  #  # blank = roads valid
                 *  #  #
                 *  ####
                 */
                int nRoads = r.Next(1, 4);
                for (int road = 0; road < nRoads; road++)
                {
                    int roadDirection = r.Next(1, 300);
                    int roadLeft = 0, roadRight = 0, roadTop = 0, roadBottom = 0;
                    if (roadDirection > 200) //road is horizontal
                    {
                        roadLeft = 0;
                        roadRight = width - 1;
                        roadTop = r.Next((int)(height / 4), height - (int)(height / 4));
                        roadBottom = roadTop + 2;

                        for (int y = roadTop; y <= roadBottom; y++)
                            for (int x = roadLeft; x <= roadRight; x++)
                                lowerTiles[y][x].tileType = 5;

                        doors[0].X = r.Next(r.Next((int)(width / 4), width - (int)(width / 4)));
                        doors[0].Y = r.Next(r.Next((int)(height / 4), height - (int)(height / 4)));
                    }
                    else if (roadDirection > 100) //road is vertical
                    {
                        roadLeft = r.Next((int)(width / 4), width - (int)(width / 4));
                        roadRight = roadLeft + 2;
                        roadTop = 0;
                        roadBottom = height - 1;

                        for (int y = roadTop; y <= roadBottom; y++)
                            for (int x = roadLeft; x <= roadRight; x++)
                                lowerTiles[y][x].tileType = 5;
                        doors[0].X = r.Next(r.Next((int)(width / 4), width - (int)(width / 4)));
                        doors[0].Y = r.Next(r.Next((int)(height / 4), height - (int)(height / 4)));
                    }
                    else if (roadDirection > 0) //road is circular
                    {
                        Vector2 roadCenter = new Vector2(r.Next((int)(width / 4), width - (int)(width / 4)), r.Next((int)(height / 4), height - (int)(height / 4)));
                        int radius = r.Next(5, 8);
                        int innerRadius = radius - 2;

                        for (int y = (int)(roadCenter.Y - radius); y <= roadCenter.Y + radius; y++)
                            for (int x = (int)(roadCenter.X - radius); x <= roadCenter.X + radius; x++)
                            {
                                if ((int)(Math.Sqrt(Math.Pow(x - roadCenter.X, 2.0) + Math.Pow(y - roadCenter.Y, 2.0))) <= radius &&
                                    (int)(Math.Sqrt(Math.Pow(x - roadCenter.X, 2.0) + Math.Pow(y - roadCenter.Y, 2.0))) >= innerRadius)
                                    lowerTiles[y][x].tileType = 5;
                                else if ((int)(Math.Sqrt(Math.Pow(x - roadCenter.X, 2.0) + Math.Pow(y - roadCenter.Y, 2.0))) < innerRadius)
                                    lowerTiles[y][x].tileType = 4;
                            }
                                
                        

                        doors[0].X = r.Next((int)roadCenter.X - innerRadius, (int)roadCenter.X + innerRadius);
                        doors[0].Y = r.Next((int)roadCenter.Y - innerRadius, (int)roadCenter.Y + innerRadius);
                    }
                }

                doors.Add(new Door());
                do
                {
                    doors[1].Y = r.Next(3, (int)(height / 4));
                    doors[1].X = r.Next(3, width - 3);
                }
                while (midTiles[doors[1].Y][doors[1].X].tileType != 0);
                doors[1].destination = Door.Direction.Up;
                doors[1].sprite = new Sprite();
                doors[1].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                doors[1].sprite.Size = new Rectangle(0, 0, 32, 32);
                doors[1].sprite.tileCode = 1;
                doors[1].positionRelativeToMapOrigin = new Vector2(32 * doors[1].X, 32 * doors[1].Y);
                for (int y = doors[1].Y - 1; y <= doors[1].Y; y++)
                    for (int x = doors[1].X - 1; x <= doors[1].X + 1; x++)
                        midTiles[y][x].tileType = 3;
                midTiles[doors[1].Y][doors[1].X].tileType = 0;

                do
                {
                    doors[0].X = r.Next(doors[1].X - 3, doors[1].X + 3);
                    doors[0].Y = r.Next(doors[1].Y - 3, doors[1].Y + 3);
                }
                while (midTiles[doors[0].Y][doors[0].X].tileType != 0);

                for (int y = 0; y < height; y++)
                {
                    midTiles[y][0].tileType = 4;
                    midTiles[y][width - 1].tileType = 4;
                }
                for (int x = 0; x < width; x++)
                {
                    midTiles[0][x].tileType = 4;
                    midTiles[height - 1][x].tileType = 4;
                }
            }
            else if (type == mapType.Ruins)
            {
                int maxRooms = r.Next(30, 50);
                int roomX = r.Next(2, (int)width / 4), roomY = r.Next(2, (int)height / 4);
                int roomWidth = r.Next(7, 15), roomHeight = r.Next(7, 15);
                for (int room = 0; room < maxRooms; room++)
                {
                    for (int y = roomY - 1; y <= roomY + roomHeight; y++)
                    {
                        for (int x = roomX - 1; x <= roomX + roomWidth; x++)
                        {
                            if ((x >= roomX) && (x <= roomX + roomWidth) && (y >= roomY) && (y <= roomY + roomHeight))
                            {
                                lowerTiles[y][x].tileType = 7; //floor
                            }
                            else
                            {
                                midTiles[y][x].tileType = 22; //walls
                            }
                        }
                    }

                    int corDirection = 2 * r.Next(1, 2);
                    int corTop = 0, corBottom = 0, corLeft = 0, corRight = 0;
                    for (int corridor = 0; corridor < r.Next(1, 4); corridor++)
                    {
                        if (corDirection == 1)//goes up
                        {
                            corBottom = roomY;
                            corTop = roomY + r.Next(5, 10);
                            if (corTop < 1)
                                corTop = 1;
                            corLeft = r.Next(roomX + 2, roomX + roomWidth - 5);
                            corRight = corLeft + 2;
                        }
                        else if (corDirection == 2) //goes down
                        {
                            corTop = roomY + roomHeight;
                            if (corTop > height - 1)
                                corTop = height - 1;
                            corBottom = corTop + r.Next(5, 10);
                            corLeft = r.Next(roomX + 2, roomX + roomWidth - 5);
                            corRight = corLeft + 2;
                        }
                        else if (corDirection == 3) //goes left
                        {
                            corRight = roomX;
                            corLeft = roomX - r.Next(5, 10);
                            if (corLeft < 1)
                                corLeft = 1;
                            corTop = r.Next(roomY + 1, roomY + roomHeight - 5);
                            corBottom = corTop + 2;
                        }
                        else if (corDirection == 4) //goes right
                        {
                            corRight = roomX + roomHeight;
                            corLeft = roomX - r.Next(5, 10);
                            if (corLeft > width - 1)
                                corLeft = width - 1;
                            corTop = r.Next(roomY + 1, roomY + roomHeight - 5);
                            corBottom = corTop + 2;
                        }

                        //generate corridor here
                        for (int y = corTop - 1; y <= corBottom + 1; y++)
                        {
                            for (int x = corLeft - 1; x <= corRight + 1; x++)
                            {
                                if (corDirection == 1 || corDirection == 2)
                                {
                                    if ((x == corLeft - 1) || (x == corRight + 1))
                                        if (lowerTiles[y][x].tileType != 7)
                                            midTiles[y][x].tileType = 22;
                                }
                                else
                                {
                                    if ((y == corTop - 1) || (y == corBottom + 1))
                                        if (lowerTiles[y][x].tileType != 7)
                                            midTiles[y][x].tileType = 22;
                                }

                                if (midTiles[y][x].tileType == 0)
                                    lowerTiles[y][x].tileType = 7;
                            }
                        }

                        //generates next corridor/room
                        roomWidth = r.Next(7, 15);
                        roomHeight = r.Next(7, 15);
                        if (corDirection == 1)//goes up
                        {
                            roomX = corLeft - r.Next(1, roomWidth / 2);
                            roomY = corTop;

                            corDirection = r.Next(3, 4);

                        }
                        else if (corDirection == 2) //goes down
                        {
                            corTop = roomY + roomHeight;
                            if (corTop > height - 1)
                                corTop = height - 1;
                            corBottom = corTop + r.Next(5, 10);
                            corLeft = r.Next(roomX + 2, roomX + roomWidth - 5);
                            corRight = corLeft + 2;
                        }
                        else if (corDirection == 3) //goes left
                        {
                            corRight = roomX;
                            corLeft = roomX - r.Next(5, 10);
                            if (corLeft < 1)
                                corLeft = 1;
                            corTop = r.Next(roomY + 1, roomY + roomHeight - 5);
                            corBottom = corTop + 2;
                        }
                        else if (corDirection == 4) //goes right
                        {
                            corRight = roomX + roomHeight;
                            corLeft = roomX - r.Next(5, 10);
                            if (corLeft > width - 1)
                                corLeft = width - 1;
                            corTop = r.Next(roomY + 1, roomY + roomHeight - 5);
                            corBottom = corTop + 2;
                        }
                    }
                }

                doors.Add(new Door());
                do
                {
                    doors[0].Y = r.Next(1, (int)height / 4);
                    doors[0].X = r.Next(1, (int)width / 4);
                }
                while (midTiles[doors[0].Y][doors[0].X].tileType != 0);
            }
            else if (type == mapType.Cavern)
            {
                //this algorithm generates cavern-like dungeons using iterative cellular automation.

                for (int y = 0; y < height; y++) //a is the current y-coordinate
                {
                    for (int x = 0; x < width; x++) //b is the current x-coordinate
                    {
                        lowerTiles[y][x].tileType = 2;
                        if (x > 1 && x < width - 2 && y > 1 && y < width - 2)
                        {
                            if (r.Next(1, 100) > 65)
                                midTiles[y][x].tileType = 1;
                            else
                                midTiles[y][x].tileType = 0;
                        }
                        else
                            midTiles[y][x].tileType = 0;
                    }
                }

                //modifying cellularly generated map
                for (int n = 0; n < 6; n++)
                {
                    for (int y = 2; y < height - 2; y++) //a is the current y-coordinate
                    {
                        for (int x = 2; x < width - 2; x++) //b is the current x-coordinate
                        {
                            int neighbors = 0;
                            if (midTiles[y - 1][x - 1].tileType == 1)
                                neighbors++;
                            if (midTiles[y - 1][x].tileType == 1)
                                neighbors++;
                            if (midTiles[y - 1][x + 1].tileType == 1)
                                neighbors++;
                            if (midTiles[y][x - 1].tileType == 1)
                                neighbors++;
                            if (midTiles[y][x + 1].tileType == 1)
                                neighbors++;
                            if (midTiles[y + 1][x - 1].tileType == 1)
                                neighbors++;
                            if (midTiles[y + 1][x].tileType == 1)
                                neighbors++;
                            if (midTiles[y + 1][x + 1].tileType == 1)
                                neighbors++;

                            if (midTiles[y][x].tileType == 1)
                            {
                                if ((neighbors >= 4) && (n <= 3))
                                    midTiles[y][x].tileType = 1;
                                else if ((neighbors <= 2) && (n > 3))
                                    midTiles[y][x].tileType = 0;

                                if (n > 4)
                                    if (neighbors < 3)
                                        midTiles[y][x].tileType = 0;
                            }
                            else if (midTiles[y][x].tileType == 0)
                            {
                                if ((neighbors >= 5) && (n < 4))
                                    midTiles[y][x].tileType = 1;
                            }
                        }
                    }
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if ((x > 0) && (x < width - 1) && (y > 0) && (y < height - 1))
                        {
                            if (midTiles[y][x].tileType == 1)
                            {
                                if ((midTiles[y + 1][x].tileType == 0) || (midTiles[y - 1][x].tileType == 0) || (midTiles[y][x + 1].tileType == 0) || (midTiles[y][x - 1].tileType == 0))
                                    midTiles[y][x].tileType = 3;
                            }
                            else if (midTiles[y][x].tileType == 0) //ore generation
                            {
                                if (r.Next(1, 100) > 97)
                                {
                                    int oreType = r.Next(1, 1000);
                                    if (oreType > 980)
                                        lowerTiles[y][x].tileType = 20;
                                    else if (oreType > 950)
                                        lowerTiles[y][x].tileType = 17;
                                    else if (oreType > 850)
                                        lowerTiles[y][x].tileType = 18;
                                    else if (oreType > 500)
                                        lowerTiles[y][x].tileType = 19;
                                }
                            }
                        }
                        else
                        {
                            midTiles[y][x].tileType = 3;
                        }
                    }
                }

                doors.Add(new Door());
                doors[0].Y = 1;
                doors[0].X = r.Next(((int)(width / 2) - (int)width / 6), ((int)(width / 2) + (int)width / 6));
                doors[0].destination = Door.Direction.Down;
                doors[0].sprite = new Sprite();
                doors[0].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                doors[0].sprite.Size = new Rectangle(0, 0, 32, 32);
                doors[0].sprite.tileCode = 27;
                doors[0].positionRelativeToMapOrigin = new Vector2(32 * doors[0].X, 32 * doors[0].Y);

                doors.Add(new Door());
                doors[1].Y = height - 2;
                doors[1].X = r.Next(((int)(width / 2) - (int)width / 6), ((int)(width / 2) + (int)width / 6));
                doors[1].destination = Door.Direction.Up;
                while (midTiles[doors[1].Y][doors[1].X].tileType != 0) ;
                doors[1].sprite = new Sprite();
                doors[1].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                doors[1].sprite.Size = new Rectangle(0, 0, 32, 32);
                doors[1].sprite.tileCode = 26;
                doors[1].positionRelativeToMapOrigin = new Vector2(32 * doors[1].X, 32 * doors[1].Y);
            }

            if (type != mapType.Town)
            {
                for (int a = 0; a < r.Next(10, 30); a++)
                {
                    this.chests.Add(new Container());
                        
                }
            }
        }

        public void miniMapGen(Game mainGame)
        {
            for (int y = 0; y < this.height; y++)
            {
                mlowerTiles.Add(new List<Tile>());
                mMidTiles.Add(new List<Tile>());
                mUpperTiles.Add(new List<Tile>());

                for (int x = 0; x < this.width; x++)
                {
                    mlowerTiles[y].Add(new Tile());
                    mlowerTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    mMidTiles[y].Add(new Tile());
                    mMidTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    mUpperTiles[y].Add(new Tile());
                    mUpperTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");

                    mlowerTiles[y][x].tileType = lowerTiles[y][x].tileType;
                    mlowerTiles[y][x].sprite.Scale = 0.25f;
                    mlowerTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);

                    mMidTiles[y][x].tileType = midTiles[y][x].tileType;
                    mMidTiles[y][x].sprite.Scale = 0.25f;
                    mMidTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);

                    mUpperTiles[y][x].tileType = upperTiles[y][x].tileType;
                    mUpperTiles[y][x].sprite.Scale = 0.25f;
                    mUpperTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                }
            }

            for (int a = 0; a < this.doors.Count; a++)
            {
                mMidTiles[doors[a].Y][doors[a].X].tileType = 21;
                mMidTiles[doors[a].Y][doors[a].X].sprite.color = Color.Yellow;
            }
        }

        //populate the map with mobs
        public void populateWithMobs(Mob[] mobs, int scaleLevel)
        {
            int freeTiles = 0; //to count the free tiles on the map
            for (int y = 1; y < this.height - 1; y++)
                for (int x = 1; x < this.width - 1; x++)
                    if (midTiles[y][x].tileType == 0)
                        freeTiles++;

            int numberOfMobs = (int)freeTiles / 10; //to count the number of mobs that should be spawned
            for (int a = 0; a < r.Next(numberOfMobs - 5, numberOfMobs + 5); a++)
            {
                Mob sourceMob = (Mob)mobs[r.Next(0, mobs.Length - 1)];
                this.mobs.Add((Mob)sourceMob.getMemberwiseClone());

                this.mobs[a].sprite = new Sprite();
                this.mobs[a].sprite.LoadContent(mainGame.Content, "monster_sprites\\" + this.mobs[a].assetName);

                this.mobs[a].vitality = (int)(this.mobs[a].vitalityRatio * scaleLevel);
                this.mobs[a].maxHP = (int)(this.mobs[a].vitality * 5) + 5;
                this.mobs[a].HP = this.mobs[a].maxHP;

                bool invalidMob = false;
                do
                {
                    this.mobs[a].x = r.Next(1, this.width - 1);
                    this.mobs[a].y = r.Next(1, this.height - 1);

                    /*if (midTiles[this.mobs[a].y][this.mobs[a].x].tileType != 0)
                        invalidMob = true;

                    this.mobs[a].sprite.Position = new Vector2(r.Next(33, 32 * (this.width - 1)), r.Next(33, 32 * (this.height - 1)));
                    for (int y = (int)getMapPosition(this.mobs[a].sprite).Y - 1; y <= (int)getMapPosition(this.mobs[a].sprite).Y + 1; y++)
                        for (int x = (int)getMapPosition(this.mobs[a].sprite).X - 1; x <= (int)getMapPosition(this.mobs[a].sprite).X + 1; x++)
                            if (x > 0 && x < this.width - 1 && y > 0 && y < this.height - 1)
                                if (this.mobs[a].sprite.collidesWith(midTiles[y][x].sprite))
                                    invalidMob = true;*/


                    //need to fix mob positioning
                    this.mobs[a].positionRelativeToMapOrigin = new Vector2(32 * (this.mobs[a].x), 32 * (this.mobs[a].y));
                    this.mobs[a].sprite.Position = Vector2.Add(this.origin, new Vector2(32 * (this.mobs[a].x), 32 * (this.mobs[a].y)));
                }
                while (midTiles[this.mobs[a].y][this.mobs[a].x].tileType != 0 || this.mobs[a].sprite.isWithin(this.doors[0].sprite, 64));
            }
        }

        //simple pathfinding algorithm, I should probably make one based on Dijkstra or A* later (not sure if enough time)
        public void simpleFindPath(Sprite spriteFrom, Sprite spriteTo)
        {
            List<Vector3> queue = new List<Vector3>(); //main list of cells to cycle through
            List<Vector3> adjacentCells = new List<Vector3>(); //list of adjacent cells
            
            Vector3 startCell = new Vector3(getMapPosition(spriteTo).X, getMapPosition(spriteTo).Y, 0);
            Vector3 endCell = new Vector3(getMapPosition(spriteFrom).X, getMapPosition(spriteFrom).Y, 0);
            queue.Add(endCell); //adds the end cell to the queue
            bool cycle = true; //decides whether or not to cycle through again

            do //cycles through the map n times
            {
                for (int a = 0; a < queue.Count; a++)
                {
                    //makes a list of adjacent cells
                    adjacentCells.Clear();
                    adjacentCells.Add(new Vector3(queue[a].X, queue[a].Y - 1, queue[a].Z + 1));
                    adjacentCells.Add(new Vector3(queue[a].X, queue[a].Y + 1, queue[a].Z + 1));
                    adjacentCells.Add(new Vector3(queue[a].X - 1, queue[a].Y, queue[a].Z + 1));
                    adjacentCells.Add(new Vector3(queue[a].X + 1, queue[a].Y, queue[a].Z + 1));
                    for (int b = 0; b < adjacentCells.Count; b++)
                    {
                        //checking loop
                        if (midTiles[(int)adjacentCells[a].Y][(int)adjacentCells[a].X].tileType != 0)
                        {
                            //remove it from the list
                            adjacentCells.Remove(adjacentCells[a]);
                        }

                        for (int c = 0; c < queue.Count; c++) //cycles through main list
                        {
                            //removes identical cells
                            if (queue[c].X == adjacentCells[b].X &&
                                queue[c].Y == adjacentCells[b].Y &&
                                queue[c].Z <= adjacentCells[b].Z)
                            {
                                adjacentCells.Remove(adjacentCells[a]);
                            }
                        }
                    }

                    for (int b = 0; b < adjacentCells.Count; b++) //adds the remaining cells into the queue
                        queue.Add(adjacentCells[b]);
                }

                if (queue.Count == 1 && (new Vector2(queue[0].X, queue[0].Y) == new Vector2(startCell.X, startCell.Y)))
                {
                    //check if there's only one cell left, and if that cell is the start cell
                    cycle = false;
                }

                List<Vector2> playerPoints = new List<Vector2>(); //represents the list of points that the player nees to go to resemble continuous motion
                //divide the path from above by player speed
            }
            while (cycle);
        }

        public void saveMap(string path, string fileName)
        {
            Directory.CreateDirectory(path + "\\maps");

            StreamWriter mapSave = new StreamWriter(path + "\\maps\\" + fileName + ".map");
            
            

            
        }

        public void loadMap(string path, string fileName)
        {
            //loads the map from a file (this function is incomplete)
            StreamReader mapLoad = new StreamReader(path + "\\" + fileName + ".map");

            List<string[]> readData = new List<string[]>();
            while (mapLoad.Peek() != -1)
                readData.Add(mapLoad.ReadLine().Split(' '));

            int width = Convert.ToInt16(readData[0][0]);
            int height = Convert.ToInt16(readData[0][1]);

            for (int y = 0; y < height; y++) //a is the current y-coordinate
            {
                lowerTiles.Add(new List<Tile>());
                midTiles.Add(new List<Tile>());
                upperTiles.Add(new List<Tile>());
                for (int x = 0; x < width; x++) //b is the current x-coordinate
                {
                    lowerTiles[y].Add(new Tile());
                    lowerTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    lowerTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                    lowerTiles[y][x].tileType = 0;

                    midTiles[y].Add(new Tile());
                    midTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    midTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                    midTiles[y][x].tileType = 0;

                    upperTiles[y].Add(new Tile());
                    upperTiles[y][x].sprite.LoadContent(mainGame.Content, "tilemap\\tileset");
                    upperTiles[y][x].sprite.Size = new Rectangle(0, 0, 32, 32);
                    upperTiles[y][x].tileType = 0;
                }
            }

            int currentCol = 0, currentRow = 0; //represents the coordinate read from the file
            for (int a = 0; a < 3; a++)
            {
                for (int y = 0; y < height; y++)
                {
                    currentCol = 0;
                    for (int x = 0; x < width; x++)
                    {
                        if (a == 0) //reads to lower tiles
                        {
                            lowerTiles[y][x].tileType = Convert.ToInt16(readData[currentCol][currentRow]);
                        }
                        else if (a == 1) //reads to mid tiles
                        {
                            midTiles[y][x].tileType = Convert.ToInt16(readData[currentCol][currentRow]);
                        }
                        else if (a == 2) //reads to upper tiles
                        {
                            upperTiles[y][x].tileType = Convert.ToInt16(readData[currentCol][currentRow]);
                        }
                        currentRow = 0;
                    }
                }
                currentCol++;
            }

            currentCol++;
            doors.Add(new Door());


        }

        //set coordinates of objecst
        public void setCoords()
        {
            for (int a = 0; a < mobs.Count; a++)
                mobs[a].positionRelativeToMapOrigin = Vector2.Subtract(mobs[a].sprite.Position, origin);
            for (int a = 0; a < doors.Count; a++)
                doors[a].positionRelativeToMapOrigin = Vector2.Subtract(doors[a].sprite.Position, origin);
        }

        //update objects on the map position
        public void updateCoords()
        {
            for (int a = 0; a < mobs.Count; a++)
            {
                mobs[a].sprite.Position = Vector2.Add(this.origin, mobs[a].positionRelativeToMapOrigin);
                if (mobs[a].sprite.Position.X > -64 &&
                    mobs[a].sprite.Position.X < mainGame.GraphicsDevice.Viewport.Width &&
                    mobs[a].sprite.Position.Y > -64 &&
                    mobs[a].sprite.Position.Y < mainGame.GraphicsDevice.Viewport.Height)
                {
                    //mobs[a].moveRandomly(this);
                }
            }

            for (int a = 0; a < this.doors.Count; a++)
                doors[a].sprite.Position = new Vector2(this.origin.X + doors[a].positionRelativeToMapOrigin.X, this.origin.Y + doors[a].positionRelativeToMapOrigin.Y); 
        }

        //find the location of a sprite on the tilemap
        public Vector2 getMapPosition(Sprite sprite)
        {
            return new Vector2((int)((sprite.Position.X + (sprite.Size.Width / 2) - this.origin.X) / 32), (int)((sprite.Position.Y + (sprite.Size.Height / 2) - this.origin.Y) / 32));
        }

        //draws the map on the screen
        public void Draw(SpriteBatch spriteBatch, bool miniMap, int level, int originX, int originY)
        {
            List<List<Tile>> tileMap = new List<List<Tile>>();
            int spriteSize;
            if (miniMap)
            {
                spriteSize = 8;
                if (level == -1)
                    tileMap = mlowerTiles;
                if (level == 0)
                    tileMap = mMidTiles;
                if (level == 1)
                    tileMap = mUpperTiles;
            }
            else
            {
                spriteSize = 32;
                if (level == -1)
                    tileMap = lowerTiles;
                if (level == 0)
                    tileMap = midTiles;
                if (level == 1)
                    tileMap = upperTiles;
            }
            int currentX = originX;
            int currentY = originY;
            for (int y = 0; y < tileMap.Count; y++)
            {
                for (int x = 0; x < tileMap[y].Count; x++)
                {
                    tileMap[y][x].sprite.Position = new Vector2(currentX, currentY);
                    if ((tileMap[y][x].sprite.Position.X > -50) && (tileMap[y][x].sprite.Position.X < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width + 50) && (tileMap[y][x].sprite.Position.Y > -50) && (tileMap[y][x].sprite.Position.Y < GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height + 50))
                    {
                        if (miniMap == true)
                        {
                            if (tileMap[y][x].discovered == true)
                                tileMap[y][x].sprite.Draw(spriteBatch, tileMap[y][x].tileType, new Color(255, 255, 255, 175));
                        }
                        else
                        {
                            if (tileMap == lowerTiles)
                                if (midTiles[y][x].tileType == 0 && upperTiles[y][x].tileType == 0)
                                    tileMap[y][x].sprite.Draw(spriteBatch, tileMap[y][x].tileType);
                            if (tileMap == midTiles)
                                if (upperTiles[y][x].tileType == 0)
                                    tileMap[y][x].sprite.Draw(spriteBatch, tileMap[y][x].tileType);
                        }
                    }
                    currentX += spriteSize;
                }
                currentY += spriteSize;
                currentX = originX;
            }

            if (!miniMap)
                for (int a = 0; a < this.doors.Count; a++)
                {
                    //doors[a].sprite.Position = Vector2.Add(this.origin, doors[a].positionRelativeToMapOrigin);
                    doors[a].Draw(spriteBatch, this);
                }
        }

        public void drawMobs(SpriteBatch spriteBatch)
        {
            for (int a = 0; a < this.mobs.Count; a++)
                if (mobs[a].sprite.Position.X > -32 &&
                    mobs[a].sprite.Position.X < 672 &&
                    mobs[a].sprite.Position.Y > -32 &&
                    mobs[a].sprite.Position.Y < 512)
                {
                    mobs[a].sprite.Draw(spriteBatch);
                }
        }
    }

    class Town : Map
    {

    }

    class DungeonLevel : Map
    {

    }
}
