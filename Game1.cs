using System;
using System.Collections.Generic;
using System.Linq;
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

/* MAIN TO-DO:
 * -COMBAT, COMBAT, COMBAT
 * -magic
 * -sprite animation*/

/* BONUS TO-DO:
 * -character classes
 * -randomly generated mobs+adjective
 * -multiplayer (lol)
 */

namespace prjTileRPG_1
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Random r = new Random();

        Player player;
        bool playerMove; //mouse toggles player moving by click (that is, pathfinding move)
        Vector2 moveVector;
        bool attack; //toggle attack
        int attackTarget = 0; //target of attack
        Sprite magicMissile; //sprite for spell

        Dungeon currentDungeon;
        Map currentMap;
        Sprite darkness;
        int frames = 0;
        
        #region Module-related Variables
        string moduleName;
        List<Mob> moduleMobs;
        List<Item> gameItems;
        #endregion

        #region Interface Variables and Sprites
        enum State { mainMenu, pauseMenu, splashScreen, game, newGame, gameOver, dialogue, inventory, spellBook, merchant, controls }; //the various "screens", not all of them are used
        State gameState = State.splashScreen; //the current game screen State
        State prevState = State.splashScreen; //to hold the previous game screen State
        bool map = false, drawQLog = false, drawUI = true, drawInv = false, drawCharSheet = false, drawSaveDialog = false, drawLoadDialog = false; //booleans for drawing various windows and dialogs
        string saveName; //this will hold the name of the current save file
        string[] mainMenuItems = { "New Game", "Load Game", "Controls", "Quit" };
        string[] pauseMenuItems = { "Resume Game", "Save Game", "Controls", "Quit" };
        Sprite logo, background, actionBar, cursor, mouseTarget, hMeter, mMeter; //interface sprites
        Window inventory, saveGameDialog, loadGameDialog, charSheet, qLog, menu;
        Text message, deathText, continueText, scoreText; //text objects
        Menu splashInfo; //for the information in the splash screen
        SpriteFont spriteFont;
        TimeSpan frameTimer = TimeSpan.Zero, hitTimer = TimeSpan.Zero, splashTimer = TimeSpan.Zero, regenTimer = TimeSpan.Zero, woundTimer = TimeSpan.Zero;

        //key bindings
        Window keyBindings;
        Keys[] tempKeys; //array used to store temporary key bindings in the controls
        int tempKeyIndex = 0; //the index of the key binding to be changed
        Keys keyUp, keyDown, keyLeft, keyRight; //direction movement keys
        Keys keyQLog, keyMap, keyPause, keyUse, keyMagic, keyCharSheet, keyUItog, keyInv;
        #endregion

        #region Input Variables
        KeyboardState currentKey = new KeyboardState();
        KeyboardState prevKey = new KeyboardState();
        MouseState currentClick = new MouseState();
        MouseState prevClick = new MouseState();
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            spriteFont = Content.Load<SpriteFont>("mainFont");

            player = new Player();
            player.name = "player"; //will be able to set this later
            player.sprite = new Sprite();
            player.sprite.LoadContent(this.Content, "player_sprites\\fighter");
            player.sprite.Size = new Rectangle(0, 0, 32, 32);
            player.Generate();
            player.sprite.Position = new Vector2(graphics.PreferredBackBufferWidth / 2 - 16, graphics.PreferredBackBufferHeight / 2 - 16);

            magicMissile = new Sprite();

            //animation testing
            player.sprite.tileCode = 16;

            #region Interface loading
            string[] bgFileNames = Directory.GetFiles("Content\\background\\");
            background = new Sprite();
            background.LoadContent(this.Content, bgFileNames[r.Next(0, bgFileNames.Length)].Replace("Content\\","").Replace(".xnb",""));

            logo = new Sprite();
            logo.LoadContent(this.Content, "interface\\logo-large");
            logo.color.A = 0;

            splashInfo = new Menu(this, new string[] { "Elvin Yung", "June 19th, 2012", "ICS3U-01", "Mr. Krnic"}, 20);

           
            menu = new Window(this, 25, 240, 130, 125, new Color(100, 100, 100, 230));
            menu.origin = new Vector2(25, 240);
            menu.menus.Add(new Menu(this, mainMenuItems, 25));

            saveGameDialog = new Window(this, 175, 150, 150, 100, new Color(100, 100, 100, 230));
            saveGameDialog.text.Add(new Text(spriteFont, "Your character was saved to ", new Vector2(155, 155)));
            saveGameDialog.menus.Add(new Menu(this, new string[] { " ", "OK" }, 25));

            loadGameDialog = new Window(this, 175, 100, 260, 240, new Color(100, 100, 100, 230));
            loadGameDialog.menus.Add(new Menu(this, new string[] { "Load these games" }, 20));

            string[] keyBindText = new string[12];
            tempKeys = new Keys[11];
            keyBindings = new Window(this, 100, 150, 260, 260, new Color(100, 100, 100, 230));
            keyBindings.menus.Add(new Menu(this, keyBindText, 20));

            actionBar = new Sprite();
            actionBar.LoadContent(this.Content, "interface\\actionbar");
            actionBar.Position = new Vector2(0, graphics.PreferredBackBufferHeight - actionBar.Size.Height);
            actionBar.color.A = 230;
            
            inventory = new Window(this, 400, 150, 175, 200, new Color(100, 100, 100, 230));
            inventory.text.Add(new Text(spriteFont, "Inventory", new Vector2(405, 155)));
            inventory.text[0].color = Color.White;
            inventory.grids.Add(new Grid(this, inventory.origin.X + 5, inventory.origin.Y + 30, 5, 5, 32, 1, Color.White, new Color(255, 255, 255)));

            hMeter = new Sprite();
            hMeter.LoadContent(this.Content, "interface\\meter");
            hMeter.color = new Color(175, 0, 0, 230);
            hMeter.Position = new Vector2(actionBar.Position.X + 131, actionBar.Position.Y + 3);

            mMeter = new Sprite();
            mMeter.LoadContent(this.Content, "interface\\meter");
            mMeter.color = new Color(0, 80, 213, 230);
            mMeter.Position = new Vector2(actionBar.Position.X + 427, actionBar.Position.Y + 3);

            cursor = new Sprite();
            cursor.LoadContent(this.Content, "interface\\cursor");
            cursor.Scale = 0.5f;
            message = new Text(spriteFont, "", new Vector2(0, 0));
            message.position = new Vector2(graphics.PreferredBackBufferWidth / 2 - spriteFont.MeasureString(message.content).X / 2, 5);

            charSheet = new Window(this, 75, 75, 200, 300, new Color(100, 100, 100, 230));
            //charSheet.text.Add(new Text(spriteFont, player.name, new Vector2(charSheet.origin.X + 5, charSheet.origin.Y + 5)));
            string[] charSheetText = {"Level " + player.level, "XP until next Level: " + player.XP, "Strength: " + player.strength, "Agility: " + player.agility, "Vitality: " + player.vitality, "Intellect: " + player.intellect };
            charSheet.menus.Add(new Menu(this, charSheetText, 20));

            mouseTarget = new Sprite();
            mouseTarget.LoadContent(this.Content, "interface\\mouseTarget");

            //this game uses the WASD keyboard pattern as default.
            //these keys will be changeable
            keyUp = Keys.W;
            keyLeft = Keys.A;
            keyDown = Keys.S;
            keyRight = Keys.D;

            keyQLog = Keys.Q;
            keyUse = Keys.E;
            keyMagic = Keys.F;
            keyCharSheet = Keys.C;
            keyPause = Keys.Escape;
            keyMap = Keys.Tab;
            keyUItog = Keys.F1;
            keyInv = Keys.I;

            tempKeys[0] = keyUp;
            tempKeys[1] = keyLeft;
            tempKeys[2] = keyDown;
            tempKeys[3] = keyRight;
            tempKeys[4] = keyPause;
            tempKeys[5] = keyMap;
            tempKeys[6] = keyQLog;
            tempKeys[7] = keyInv;
            tempKeys[8] = keyCharSheet;
            tempKeys[9] = keyUse;
            tempKeys[10] = keyUItog;

            deathText = new Text(spriteFont, "GAME OVER", new Vector2(this.graphics.PreferredBackBufferWidth / 2, 50));
            continueText = new Text(spriteFont, "Press " + keyPause + " to continue", new Vector2(this.graphics.PreferredBackBufferWidth / 2, 120));
            scoreText = new Text(spriteFont, "Total levels delved: ", new Vector2(this.graphics.PreferredBackBufferWidth / 2, 100));
            #endregion

            #region Module Data Loading
            moduleName = "default_module"; //eventually, players will be able to select

            //mob loading
            int mobNumber = 0;
            moduleMobs = new List<Mob>();
            List<string> readMobData = new List<string>();
            StreamReader mobRead = new StreamReader("Content\\data\\modules\\" + moduleName + "\\mobs.txt");
            while (mobRead.Peek() != -1)
                readMobData.Add(mobRead.ReadLine());
            mobRead.Close();
            for (int a = 0; a < readMobData.Count; a++)
            {
                if (readMobData[a].Contains("*"))
                {
                    moduleMobs.Add(new Mob());
                    moduleMobs[mobNumber].name = readMobData[a].Substring(1, readMobData[a].Length - 1);
                    string[] mobStats = readMobData[a + 1].Split(' ');
                    moduleMobs[mobNumber].assetName = mobStats[0];
                    moduleMobs[mobNumber].sprite.Size = new Rectangle(0, 0, Convert.ToInt16(mobStats[1]), Convert.ToInt16(mobStats[2]));
                    moduleMobs[mobNumber].strengthRatio = Convert.ToDouble(mobStats[3]);
                    moduleMobs[mobNumber].agilityRatio = Convert.ToDouble(mobStats[4]);
                    moduleMobs[mobNumber].vitalityRatio = Convert.ToDouble(mobStats[5]);
                    moduleMobs[mobNumber].intellectRatio = Convert.ToDouble(mobStats[6]);
                    moduleMobs[mobNumber].sprite = new Sprite();
                    moduleMobs[mobNumber].sprite.LoadContent(this.Content, "monster_sprites\\" + moduleMobs[mobNumber].assetName);
                    mobNumber++;
                }
            }

            //player weapon generation
            player.mainEquip.Generate(this, moduleName, 1, Item.Quality.Mundane, Weapon.weaponType.Sword);
            player.mainEquip.sprite.Position = Vector2.Add(player.sprite.Position, new Vector2(8, 8));
            #endregion

            currentDungeon = new Dungeon();
            currentDungeon.dungeonType = Map.mapType.Cavern;
            currentDungeon.levels.Add(new Map());

            currentDungeon.levels[0].generate(this, Map.mapType.Town, 60, 60);
            currentDungeon.levels[0].miniMapGen(this);
            currentMap = currentDungeon.levels[0];
            currentMap.origin = new Vector2((int)player.sprite.Position.X - (32 * currentMap.doors[0].X),(int)(player.sprite.Position.Y - (32 * currentMap.doors[0].Y)));

            darkness = new Sprite(); //used for covering
            darkness.LoadContent(this.Content, "interface\\whitepixel");
            darkness.color = new Color(0, 0, 0, 0); //completely black, but transparent initially
            darkness.Size = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            darkness.Position = new Vector2(0, 0);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //currentMap.setCoords();

            currentKey = Keyboard.GetState();
            currentClick = Mouse.GetState();

            cursor.Position = new Vector2(currentClick.X, currentClick.Y);

            #region Game screen logic
            if (gameState == State.game)
            {
                darkness.color.A = (byte)(40 * currentDungeon.levels.IndexOf(currentMap));
                if (currentDungeon.levels.IndexOf(currentMap) > 6 || darkness.color.A > 130)
                    darkness.color.A = 130;
                frameTimer += gameTime.ElapsedGameTime;

                //get player's position on tilemap

                player.x = (int)currentMap.getMapPosition(player.sprite).X;
                player.y = (int)currentMap.getMapPosition(player.sprite).Y;

                Vector2 oldOrigin = currentMap.origin; //the old origin of the map
                moveVector = new Vector2(0, 0); // the vector of moving

                MathHelper.Clamp(player.x, 1, currentMap.width - 1);
                MathHelper.Clamp(player.y, 1, currentMap.height - 1);

                player.sprite.currentState = Sprite.State.stand;
            }
            else
                darkness.color.A = 0;

            if (gameState == State.mainMenu || gameState == State.pauseMenu)
            {
                if (gameState == State.mainMenu)
                {
                    menu.menus[0].items[0].content = mainMenuItems[0];
                    menu.menus[0].items[1].content = mainMenuItems[1];
                }
                else if (gameState == State.pauseMenu)
                {
                    menu.menus[0].items[0].content = pauseMenuItems[0];
                    menu.menus[0].items[1].content = pauseMenuItems[1];
                }

                menu.menus[0].checkHover(menu.origin, new Point(currentClick.X, currentClick.Y), spriteFont);
            }
            if (gameState == State.controls)
            {
                keyBindings.menus[0].items[0].content = "Up: " + keyUp.ToString();
                keyBindings.menus[0].items[1].content = "Left: " + keyLeft.ToString();
                keyBindings.menus[0].items[2].content = "Down: " + keyDown.ToString();
                keyBindings.menus[0].items[3].content = "Right: " + keyRight.ToString();
                keyBindings.menus[0].items[4].content = "Pause/Resume/Menu: " + keyPause.ToString();
                keyBindings.menus[0].items[5].content = "Map: " + keyMap.ToString();
                keyBindings.menus[0].items[6].content = "Quest Log: " + keyQLog.ToString();
                keyBindings.menus[0].items[7].content = "Inventory: " + keyInv.ToString();
                keyBindings.menus[0].items[8].content = "Character Sheet: " + keyCharSheet.ToString();
                keyBindings.menus[0].items[9].content = "Use Object/Open Door: " + keyUse.ToString();
                keyBindings.menus[0].items[10].content = "Toggle UI: " + keyUItog.ToString();
                keyBindings.menus[0].items[11].content = "Cast magic: Right click";

                keyBindings.menus[0].checkHover(keyBindings.origin, new Point(currentClick.X, currentClick.Y), spriteFont);
            }
            if (drawLoadDialog)
            {
                loadGameDialog.menus[0].checkHover(loadGameDialog.origin, new Point(currentClick.X, currentClick.Y), spriteFont);
            }
            if (drawSaveDialog)
            {
                saveGameDialog.menus[0].checkHover(saveGameDialog.origin, new Point(currentClick.X, currentClick.Y), spriteFont);
            }
            #endregion

            #region automap processing
            //this updates the automap to make everything 4 tiles around the player show on the minimap.
            for (int y = player.y - 4; y <= player.y + 4; y++)
            {
                for (int x = player.x - 4; x <= player.x + 4; x++)
                {
                    if (x >= 0 && x < currentMap.width && y >= 0 && y < currentMap.height)
                    {
                        if (Math.Sqrt(Math.Pow(player.x - x, 2) + Math.Pow(player.y - y, 2)) <= 4)
                        {
                            currentMap.mlowerTiles[y][x].discovered = true;
                            currentMap.mMidTiles[y][x].discovered = true;
                            currentMap.mUpperTiles[y][x].discovered = true;
                        }
                    }
                }
            }
            #endregion


            if ((currentClick.LeftButton == ButtonState.Pressed) && (prevClick != currentClick))
            {
                //calculate distance vector by using the distance formula from currentClick to player.sprite.Position
                //find vertical and horizontal components, makes sure total velocity is 3 * player.speedModifier

                if (gameState == State.mainMenu || gameState == State.pauseMenu)
                {
                    if (drawSaveDialog) //saves a game
                    {
                        if (saveGameDialog.menus[0].items[1].selected)
                            drawSaveDialog = false;
                    }
                    if (drawLoadDialog) //loads a game
                    {
                        for (int a = 0; a < loadGameDialog.menus[0].items.Count; a++)
                        {
                            if (loadGameDialog.menus[0].items[a].selected)
                            {
                                //loads the game
                                player.load(this, "saves\\" + loadGameDialog.menus[0].items[a].content + ".sav");
                                drawLoadDialog = false;
                                gameState = State.game;
                                break;
                            }
                        }
                    }

                    if (menu.menus[0].items[0].selected) //New Game
                    {
                        if (gameState == State.mainMenu)
                        {
                            player.Generate();

                            //makes a new game
                            currentDungeon.levels.Clear();
                            currentDungeon.levels.Add(new Map());
                            currentDungeon.levels[0].generate(this, Map.mapType.Town, 60, 60);
                            currentDungeon.levels[0].miniMapGen(this);
                            currentMap = currentDungeon.levels[0];
                            currentMap.origin = new Vector2((int)player.sprite.Position.X - (32 * currentMap.doors[0].X), (int)(player.sprite.Position.Y - (32 * currentMap.doors[0].Y)));
                        }
                        gameState = State.game;
                    }
                    else if (menu.menus[0].items[1].selected) //save or load game
                    {
                        if (gameState == State.mainMenu) //loads the game
                        {
                            string[] gameSaves = Directory.GetFiles("saves\\");
                            for (int a = 0; a < gameSaves.Length; a++)
                                gameSaves[a] = gameSaves[a].Replace("saves\\", "").Replace(".sav", "");

                            //bubble sorting the list of save games in chronological order
                            for (int n = 0; n < 5; n++)
                            {
                                for (int a = 0; a < gameSaves.Length - 1; a++)
                                {
                                    if (Convert.ToInt32(gameSaves[a].Substring(0, 9)) > Convert.ToInt32(gameSaves[a + 1].Substring(0, 9)))
                                    {
                                        string tempSaveName = gameSaves[a + 1];
                                        gameSaves[a + 1] = gameSaves[a];
                                        gameSaves[a] = tempSaveName;
                                    }
                                }
                            }

                            loadGameDialog.menus[0].setItems(gameSaves);
                            drawLoadDialog = true;
                        }
                        else if (gameState == State.pauseMenu) //saves the game
                        {
                            saveName = player.save();
                            saveGameDialog.text[0].content = "Your game was saved as " + saveName;
                            drawSaveDialog = true;
                        }
                    }
                    else if (menu.menus[0].items[2].selected) //controls
                    {
                        gameState = State.controls;
                        prevState = gameState;
                    }
                    else if (menu.menus[0].items[3].selected) //Quit
                        this.Exit();
                }
                else if (gameState == State.controls)
                {
                    for (int a = 0; a < keyBindings.menus[0].items.Count; a++)
                    {
                        if (keyBindings.menus[0].items[a].selected)
                        {
                            tempKeyIndex = a;
                            break;
                        }
                    }
                }
                else if (gameState == State.game)
                {
                    for (int a = 0; a < currentMap.mobs.Count; a++)
                    {
                        Rectangle mobRect = new Rectangle((int)currentMap.mobs[a].sprite.Position.X, (int)currentMap.mobs[a].sprite.Position.Y, currentMap.mobs[a].sprite.Size.Width, currentMap.mobs[a].sprite.Size.Height);
                        if (mobRect.Contains(new Point(currentClick.X, currentClick.Y)))
                        {
                            attack = true;
                            hitTimer = TimeSpan.Zero;
                            attackTarget = a;
                            break;
                        }
                    }

                    mouseTarget.Position = new Vector2(currentClick.X - 8, currentClick.Y - 8);
                    mouseTarget.color.A = 175;
                }
            }
            if ((currentClick.RightButton == ButtonState.Pressed) && (prevClick != currentClick))
            {
                if (gameState == State.game)
                {

                    #region Magic missile casting
                    if (player.MP >= 3)
                    {
                        player.MP -= 3;
                        //magic spell
                        bool createNewMissile = true;
                        for (int a = 0; a < player.ability.Count; a++)
                        {
                            if (player.ability[a].Visible == false)
                            {
                                createNewMissile = false;
                                player.ability[a].Visible = true;
                                player.ability[a].spellVisual.Position = Vector2.Add(player.sprite.Position, new Vector2(8, 8));
                                player.ability[a].startPosition = player.ability[a].spellVisual.Position;
                                player.ability[a].maxRange = 128 + (5 * player.intellect);
                                player.ability[a].directionUnit = 3 * Vector2.Normalize(Vector2.Subtract(new Vector2(currentClick.X, currentClick.Y), player.sprite.Position));
                                player.ability[a].spellVisual.Scale = 0.66f;
                                break;
                            }
                        }

                        if (createNewMissile)
                        {
                            player.ability.Add(new Ability());
                            player.ability[player.ability.Count - 1].spellVisual = new Sprite();
                            player.ability[player.ability.Count - 1].spellVisual.LoadContent(this.Content, "spells\\mmissile");

                        }
                    }
                    else
                    {
                        message.content = "You are out of mana.";
                    }
                    #endregion
                }
            }

            if ((currentKey.IsKeyDown(keyUp) == true) || (currentKey.IsKeyDown(Keys.Up) == true))
            {
                if (gameState == State.game)
                {
                    if (player.y > 0)
                        if (currentMap.midTiles[player.y - 1][player.x].tileType == 0 || player.sprite.overlapsWith(currentMap.midTiles[player.y - 1][player.x].sprite) != true)
                            moveVector.Y = 3 * player.speedModifier;
                }
            }
            if ((currentKey.IsKeyDown(keyLeft) == true) || (currentKey.IsKeyDown(Keys.Left) == true))
            {
                player.sprite.effect = SpriteEffects.None;
                player.mainEquip.sprite.effect = SpriteEffects.FlipHorizontally;
                player.mainEquip.sprite.Position = Vector2.Add(player.sprite.Position, new Vector2(-8, 0));
                if (player.x > 0)
                {
                    if (currentMap.midTiles[player.y][player.x - 1].tileType == 0 || player.sprite.overlapsWith(currentMap.midTiles[player.y][player.x - 1].sprite) != true)
                    {
                        moveVector.X = 3 * player.speedModifier;
                    }
                }
            }
            if ((currentKey.IsKeyDown(keyDown) == true) || (currentKey.IsKeyDown(Keys.Down) == true))
            {
                if (player.y < currentMap.height)
                {
                    if (currentMap.midTiles[player.y + 1][player.x].tileType == 0 || player.sprite.overlapsWith(currentMap.midTiles[player.y + 1][player.x].sprite) != true)
                    {
                        moveVector.Y = -3 * player.speedModifier;
                    }
                }
            }

            if ((currentKey.IsKeyDown(keyRight) == true) || (currentKey.IsKeyDown(Keys.Right) == true))
            {
                player.sprite.effect = SpriteEffects.FlipHorizontally;
                player.mainEquip.sprite.effect = SpriteEffects.None;
                player.mainEquip.sprite.Position = Vector2.Add(player.sprite.Position, new Vector2(8, 0));
                if (player.x < currentMap.width)
                {
                    if (currentMap.midTiles[player.y][player.x + 1].tileType == 0 || player.sprite.overlapsWith(currentMap.midTiles[player.y][player.x + 1].sprite) != true)
                    {
                        moveVector.X = -3 * player.speedModifier;
                    }
                }
            }

            if (currentKey.IsKeyDown(keyUse) && currentKey != prevKey) //the use key
            {
                //check doors
                for (int a = 0; a < currentMap.doors.Count; a++)
                {
                    if (player.sprite.isWithin(currentMap.doors[a].sprite, 64))
                    {
                        darkness.color.A = 255;
                        if (currentMap.doors[a].destination == Door.Direction.Up)
                        {
                            if (currentDungeon.levels.IndexOf(currentMap) == currentDungeon.levels.Count - 1)
                            {
                                currentDungeon.levels.Add(new Map());
                                currentDungeon.levels[currentDungeon.levels.IndexOf(currentMap) + 1].generate(this, currentDungeon.dungeonType, currentMap.width, currentMap.height);
                                currentDungeon.levels[currentDungeon.levels.IndexOf(currentMap) + 1].miniMapGen(this);
                                currentMap = currentDungeon.levels[currentDungeon.levels.IndexOf(currentMap) + 1];
                                currentMap.populateWithMobs(moduleMobs.ToArray(), currentDungeon.levels.IndexOf(currentMap));
                                currentMap.origin = new Vector2((int)player.sprite.Position.X - (32 * currentMap.doors[0].X), (int)(player.sprite.Position.Y - (32 * currentMap.doors[0].Y)));
                            }
                            else
                            {
                                currentMap = currentDungeon.levels[currentDungeon.levels.IndexOf(currentMap) + 1];
                                currentMap.origin = new Vector2((int)player.sprite.Position.X - (32 * currentMap.doors[0].X), (int)(player.sprite.Position.Y - (32 * currentMap.doors[0].Y)));
                            }
                        }
                        else if (currentMap.doors[a].destination == Door.Direction.Down && currentDungeon.levels.IndexOf(currentMap) != 0)
                        {
                            currentMap = currentDungeon.levels[currentDungeon.levels.IndexOf(currentMap) - 1];
                            currentMap.origin = new Vector2((int)player.sprite.Position.X - (32 * currentMap.doors[1].X), (int)(player.sprite.Position.Y - (32 * currentMap.doors[1].Y)));
                        }
                        break;
                    }
                }
            }

            if (currentKey.IsKeyDown(keyQLog) == true)
            {
                if (gameState == State.game)
                    if (prevKey != currentKey) //toggles qlog
                        drawQLog = !drawQLog;
            }

            if (currentKey.IsKeyDown(keyMap) == true)
            {
                //toggles the automap
                if (gameState == State.game)
                    if (prevKey != currentKey)
                        map = !map;
            }

            if (currentKey.IsKeyDown(keyUItog) && prevKey != currentKey)
            {
                //toggle GUI draw
                if (gameState == State.game)
                    drawUI = !drawUI;
            }
            if (currentKey.IsKeyDown(keyInv) && prevKey != currentKey) //toggle inventory
                if (gameState == State.game)
                    drawInv = !drawInv;
            if (currentKey.IsKeyDown(Keys.C) && prevKey != currentKey) //toggle character sheet
                if (gameState == State.game)
                    drawCharSheet = !drawCharSheet;

            if (currentKey.IsKeyDown(keyPause) && prevKey != currentKey)
            {
                if (gameState == State.pauseMenu)
                    gameState = State.game;
                else if (drawLoadDialog)
                    drawLoadDialog = false;
                else if (drawSaveDialog)
                    drawSaveDialog = false;
                else if (gameState == State.splashScreen)
                    logo.color.A = 254;
                else if (gameState == State.controls)
                    gameState = prevState;
                else if (gameState == State.game)
                    gameState = State.pauseMenu;
                else if (gameState == State.gameOver)
                    gameState = State.mainMenu;
            }

            if (gameState == State.splashScreen)
            {
                splashTimer += gameTime.ElapsedGameTime;
                if (splashTimer.Milliseconds > 250)
                    logo.color.A += 1;

                if (logo.color.A >= 255)
                    gameState = State.mainMenu;

            }
            if (gameState == State.controls)
            {
                //tempKeys[tempKeyIndex] = currentKey.GetPressedKeys()[0];

                keyUp = tempKeys[0];
                keyLeft = tempKeys[1];
                keyDown = tempKeys[2];
                keyRight = tempKeys[3];
                keyPause = tempKeys[4];
                keyMap = tempKeys[5];
                keyQLog = tempKeys[6];
                keyInv = tempKeys[7];
                keyCharSheet = tempKeys[8];
                keyUse = tempKeys[9];
                keyUItog = tempKeys[10];
            }
            if (gameState == State.game)
            {
                if (mouseTarget.color.A != 0) //flickering mouse target
                    mouseTarget.color.A -= 3; //sets transparency of the target
                if (player.sprite.collidesWith(mouseTarget)) //if player collides with it, make it transparent
                    mouseTarget.color.A = 0;

                #region Movement/Collision

                woundTimer += gameTime.ElapsedGameTime;
                for (int a = 0; a < currentMap.mobs.Count; a++)
                {
                    if (currentMap.mobs[a].sprite.isWithin(player.sprite, 128))
                    {
                        currentMap.mobs[a].sprite.Position = Vector2.Add(currentMap.mobs[a].sprite.Position, (2 * (Vector2.Normalize(Vector2.Subtract(player.sprite.Position, currentMap.mobs[a].sprite.Position)))));
                        if (woundTimer.Milliseconds > 250)
                        {
                            if (currentMap.mobs[a].sprite.collidesWith(player.sprite) && currentMap.mobs[a].sprite.color.A > 0)
                                player.HP -= (int)(currentDungeon.levels.IndexOf(currentMap) * 4);
                            woundTimer = TimeSpan.Zero;
                        }
                    }
                    else if (currentMap.mobs[a].sprite.isWithin(player.sprite, 128))
                        currentMap.mobs[a].moveRandomly(currentMap);

                    if (currentMap.mobs[a].HP <= 0 && !currentMap.mobs[a].dead)
                    {
                        currentMap.mobs[a].sprite.color.A = 0;
                        currentMap.mobs[a].sprite.Position = new Vector2(-64, -64);
                        player.XP -= currentDungeon.levels.IndexOf(currentMap) * 20;
                        if (player.XP <= 0 && !currentMap.mobs[a].dead)
                        {
                            player.levelUp();
                            int playerXP = player.XP;
                            message.content = "Congratulations! You are now level " + player.level + ".";
                        }
                        currentMap.mobs[a].dead = true;
                        break;
                    }
                }

                currentMap.origin = Vector2.Add(currentMap.origin, moveVector);
                mouseTarget.Position = Vector2.Add(mouseTarget.Position, moveVector);

                #endregion

                currentMap.updateCoords();
                inventory.grids[0].checkMouseHover(this, cursor.Position);

                if (frameTimer.Milliseconds > 50)
                {
                    player.sprite.tileCode++;
                    if (player.sprite.tileCode > 27)
                        player.sprite.tileCode = 16;
                    frameTimer = TimeSpan.Zero;
                }
                if (attack)
                {
                    if (player.mainEquip.sprite.collidesWith(currentMap.mobs[attackTarget].sprite))
                        currentMap.mobs[attackTarget].Damage(player);

                    if (hitTimer.Milliseconds > 300)
                        attack = false;
                    else
                        hitTimer += gameTime.ElapsedGameTime;
                }

                for (int a = 0; a < player.ability.Count; a++)
                    player.ability[a].update(currentMap, player);

                regenTimer += gameTime.ElapsedGameTime;
                if (player.HP <= 0) //player dies
                {
                    attack = false;
                    gameState = State.gameOver;
                }
                if (player.MP <= 0) //player runs out of mana
                    message.content = "You are out of mana.";
                if (regenTimer.Milliseconds > 500)
                {
                    //health regeneration
                    if (player.HP < player.maxHP)
                        player.HP++;
                    if (player.MP < player.maxMP)
                        player.MP++;
                    regenTimer = TimeSpan.Zero;
                }

                for (int a = 0; a < currentMap.doors.Count; a++)
                {
                    //if (player.sprite.collidesWith(currentMap.doors[a].sprite))
                    if (player.sprite.isWithin(currentMap.doors[a].sprite, 64))
                    {
                        currentMap.doors[a].canUse = true;
                        message.content = "Press " + keyUse.ToString() + " to change level";
                        break;
                    }
                    else
                    {
                        currentMap.doors[a].canUse = false;
                        message.content = "";
                    }
                }

                if (drawCharSheet)
                {
                    string[] charSheetText = { "Level " + player.level, "XP until next Level: " + player.XP, "Strength: " + player.strength, "Agility: " + player.agility, "Vitality: " + player.vitality, "Intellect: " + player.intellect };
                    charSheet.menus[0].setItems(charSheetText);
                }
            }

            if (gameState == State.mainMenu)
                message.content = "ELVIN YUNG PRESENTS";

            message.position.X = GraphicsDevice.Viewport.Width / 2 - spriteFont.MeasureString(message.content).X / 2;

            prevKey = currentKey;
            prevClick = currentClick;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            if (gameState == State.splashScreen)
            {
                darkness.color.A = 255;
                splashInfo.Draw(this.spriteBatch, new Vector2(5, 5));
                logo.Position = new Vector2((int)(this.graphics.PreferredBackBufferWidth / 2 - logo.Size.Width / 2), (int)(this.graphics.PreferredBackBufferHeight / 2 - logo.Size.Height / 2));
                logo.Draw(this.spriteBatch);
            }
            else if (gameState == State.mainMenu || gameState == State.pauseMenu)
            {
                /*if (gameState == State.mainMenu)
                    menu.menus[0].setItems(mainMenuItems);
                else if (gameState == State.pauseMenu)
                    menu.menus[0].setItems(pauseMenuItems);*/

                logo.Position = new Vector2(25, 25);
                background.Draw(this.spriteBatch);
                logo.Scale = 0.5f;
                logo.Draw(this.spriteBatch);
                menu.draw(this.spriteBatch);
                message.Draw(this.spriteBatch);

                if (drawSaveDialog)
                {
                    saveGameDialog.draw(this.spriteBatch);
                }
                if (drawLoadDialog)
                {
                    loadGameDialog.draw(this.spriteBatch);
                }
            }
            else if (gameState == State.controls)
            {
                keyBindings.menus[0].items[0].content = "Up: " + keyUp.ToString();
                keyBindings.menus[0].items[1].content = "Left: " + keyLeft.ToString();
                keyBindings.menus[0].items[2].content = "Down: " + keyDown.ToString();
                keyBindings.menus[0].items[3].content = "Right: " + keyRight.ToString();
                keyBindings.menus[0].items[4].content = "Pause: " + keyPause.ToString();
                keyBindings.menus[0].items[5].content = "Map: " + keyMap.ToString();
                keyBindings.menus[0].items[6].content = "Quest Log: " + keyQLog.ToString();
                keyBindings.menus[0].items[7].content = "Inventory: " + keyInv.ToString();
                keyBindings.menus[0].items[8].content = "Character Sheet: " + keyCharSheet.ToString();
                keyBindings.menus[0].items[9].content = "Use: " + keyUse.ToString();
                keyBindings.menus[0].items[10].content = "Toggle UI: " + keyUItog.ToString();
                keyBindings.menus[0].items[11].content = "Cast magic spells: Right click";

                background.Draw(this.spriteBatch);
                logo.Draw(this.spriteBatch);
                keyBindings.draw(this.spriteBatch);
            }
            else if (gameState == State.gameOver)
            {
                darkness.color.A = 255;
                darkness.Draw(this.spriteBatch);
                deathText.Draw(this.spriteBatch);
                deathText.position.X = (int)(this.graphics.PreferredBackBufferWidth / 2 - spriteFont.MeasureString(deathText.content).X / 2);
                scoreText.content = "Levels delved: " + (currentDungeon.levels.Count - 1);
                scoreText.position.X = (int)(this.graphics.PreferredBackBufferWidth / 2 - spriteFont.MeasureString(scoreText.content).X / 2);
                scoreText.Draw(this.spriteBatch);
                continueText.Draw(this.spriteBatch);
                continueText.position.X = (int)(this.graphics.PreferredBackBufferWidth / 2 - spriteFont.MeasureString(continueText.content).X / 2);
            }
            else if (gameState == State.game)
            {
                /* HOW DRAWING WORKS, in ascending order of rendering:
                 * Layer 0: Map + Buildings/Scenery/Objects
                 * Layer 1: NPCS/Monsters
                 * Layer 2: Player
                 * Layer 3: Interface*/

                currentMap.Draw(this.spriteBatch, false, -1, (int)currentMap.origin.X, (int)currentMap.origin.Y);
                currentMap.Draw(this.spriteBatch, false, 0, (int)currentMap.origin.X, (int)currentMap.origin.Y);
                mouseTarget.Draw(this.spriteBatch);

                currentMap.drawMobs(this.spriteBatch);
                player.sprite.Draw(spriteBatch, player.sprite.tileCode);
                if (attack)
                {
                    player.mainEquip.sprite.Draw(spriteBatch, player.mainEquip.sprite.tileCode);
                }

                darkness.Size = new Rectangle(0, 0, 640, 480);
                darkness.Draw(this.spriteBatch);

                for (int a = 0; a < player.ability.Count; a++)
                {
                    if (player.ability[a].Visible)
                        player.ability[a].spellVisual.Draw(this.spriteBatch);
                }

                //interfaces
                if (map == true)
                {
                    currentMap.Draw(this.spriteBatch, true, -1, (int)(player.sprite.Position.X - (8 * player.x)) + 8, (int)(player.sprite.Position.Y - (8 * player.y)) + 8);
                    currentMap.Draw(this.spriteBatch, true, 0, (int)(player.sprite.Position.X - (8 * player.x)) + 8, (int)(player.sprite.Position.Y - (8 * player.y)) + 8);
                }
                if (drawUI == true) //interface drawing
                {
                    actionBar.Draw(this.spriteBatch);
                    float hMeterHeight = (((float)(player.maxHP - player.HP) / player.maxHP) * hMeter.Size.Height);
                    hMeter.Position.Y = actionBar.Position.Y + 3 + hMeterHeight;
                    hMeter.Draw(this.spriteBatch, new Rectangle(0, (int)hMeterHeight, hMeter.Size.Width, (int)(hMeter.Size.Height - hMeterHeight)), hMeter.color);

                    float mMeterHeight = (((float)(player.maxMP - player.MP) / player.maxMP) * mMeter.Size.Height);
                    mMeter.Position.Y = actionBar.Position.Y + 3 + mMeterHeight;
                    mMeter.Draw(this.spriteBatch, new Rectangle(0, (int)mMeterHeight, mMeter.Size.Width, (int)(mMeter.Size.Height - mMeterHeight)), mMeter.color);
                    //mMeter.Draw(this.spriteBatch, 
                    message.Draw(this.spriteBatch);
                }
                if (drawInv)
                    inventory.draw(this.spriteBatch);
                if (drawQLog)
                    spriteBatch.DrawString(spriteFont, "QUEST LOG IS OPEN", new Vector2(100, 100), Color.White);
                if (drawCharSheet)
                    charSheet.draw(this.spriteBatch);
            }


            cursor.Draw(this.spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
