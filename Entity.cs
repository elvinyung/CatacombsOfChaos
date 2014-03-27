using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace prjTileRPG_1
{
    class Entity //the class for a being
    {
        public Random r = new Random();
        public string name; //obviously, entity's name
        public string assetName; //
        public Sprite sprite = new Sprite(); //the Sprite object representing the player character
        public int x, y; //represents coordinates on the screen
        public int XP; //experience points
        public Vector2 positionRelativeToMapOrigin;
        
        public int HP, maxHP; //health points
        public int MP, maxMP; //magic points

        //stats
        public int level;
        public int strength;
        public int agility;
        public int vitality;
        public int intellect;

        //temporary stat modifiers
        public int speedModifier;

        public List<Ability> ability = new List<Ability>();

        //gameplay stuff
        public List<Quest> quests = new List<Quest>();
        public List<Item> inventory = new List<Item>();
        public Weapon mainEquip = new Weapon(), sideEquip = new Weapon();


        public void moveRandomly(Map map)
        {
            Vector2 oldPos = this.sprite.Position;
            int moveX = r.Next(-6, 6), moveY = r.Next(-6, 6);
            this.sprite.Position.X += moveX;
            this.sprite.Position.Y += moveY;

            if ((int)map.getMapPosition(this.sprite).X > 0 &&
                (int)map.getMapPosition(this.sprite).X < map.width - 1 &&
                (int)map.getMapPosition(this.sprite).Y > 0 &&
                (int)map.getMapPosition(this.sprite).Y < map.height - 1)
            {
                if (this.sprite.collidesWith(map.midTiles[(int)map.getMapPosition(this.sprite).Y][(int)map.getMapPosition(this.sprite).X].sprite))
                    this.sprite.Position = oldPos;
            }
            else
            {
                this.sprite.Position = oldPos;
            }
        }

        public void Damage(Entity attacker)
        {
            if (r.Next(1, 100 - attacker.agility) > 40)
            {
                int minDmg = (int)(((attacker.strength) * 0.15) * (attacker.mainEquip.baseMinDmg));
                int maxDmg = (int)(((attacker.strength + attacker.agility) * 0.35) * (attacker.mainEquip.baseMaxDmg)) + minDmg;
                int damage = r.Next(minDmg, maxDmg);

                this.HP -= damage;
                if (this.HP <= 0)
                {
                    this.sprite.color.A = 0;
                    this.sprite.Position = new Vector2(-64, -64);
                }
            }
        }

        public void Damage(int dmg)
        {
            this.HP -= dmg;

            if (this.HP <= 0)
            {
                this.sprite.color.A = 0;
                this.sprite.Position = new Vector2(-64, -64);
            }
        }

        public void levelUp()
        {
            level++;

            this.XP = 125 * (8 + (level - 1));

            this.strength = (int)(this.strength * 1.075);
            this.agility = (int)(this.agility * 1.075);
            this.vitality = (int)(this.vitality * 1.075);
            this.intellect = (int)(this.intellect * 1.075);

            maxHP = (int)((vitality * level) * (1.5));
            HP = maxHP;
            maxMP = (int)((intellect * level) * ((0.25 * Math.Pow(level, 2)) + 0.25 * level));
            MP = maxMP;

            this.XP = 125 * (8 + (level - 1));
        }

        public Object getMemberwiseClone()
        {
            return (Object)this.MemberwiseClone();
        }
    }

    class Player : Entity
    {
        string prefix, suffix; //titles for quests
        public List<Ability> spells = new List<Ability>();
        public int XP;
        
        public void Generate()
        {
            this.level = 1;
            this.XP = 125 * (20 + (level));

            speedModifier = 1;
            do
            {
                this.strength = r.Next(10, 25);
                this.agility = r.Next(10, 25);
                this.vitality = r.Next(10, 25);
                this.intellect = 50 - this.strength;
            }
            while (strength + agility + vitality + intellect < 70);

            maxHP = (int)((vitality * level) * (1.5));
            HP = maxHP;
            maxMP = (int)((intellect * level) * ((0.25 * Math.Pow(level, 2)) + 0.25 * level));
            MP = maxMP;
        }

        public string save() //saves the game, and returns the save file name
        {
            //save player data
            DateTime now = DateTime.Now;
            string saveGameName = now.ToString().Replace("/", "").Replace(" ", "").Replace(":", "");
            Directory.CreateDirectory("saves\\" + saveGameName);
            StreamWriter playerData = new StreamWriter("saves\\" + saveGameName + "\\player.sav");

            //writes in the player stats
            playerData.WriteLine(level + " " + strength + " " + agility + " " + vitality + " " + intellect);
            
            //writes in the player weapon stats
            playerData.WriteLine(mainEquip.name + " " + mainEquip.sprite.tileCode + " " + mainEquip.baseMinDmg + " " + mainEquip.baseMaxDmg);

            playerData.Close();

            return saveGameName;
        }

        public void load(Game mainGame, string fileName)
        {
            StreamReader playerData = new StreamReader(fileName);
            List<string[]> readData = new List<string[]>();
            while (playerData.Peek() != -1)
                readData.Add(playerData.ReadLine().Split(' '));

            //read in player stats
            level = Convert.ToInt16(readData[0][0]);
            strength = Convert.ToInt16(readData[0][1]);
            agility = Convert.ToInt16(readData[0][2]);
            vitality = Convert.ToInt16(readData[0][3]);
            intellect = Convert.ToInt16(readData[0][4]);

            //read in player weapon stats
            mainEquip = new Weapon();
            mainEquip.sprite = new Sprite();
            mainEquip.sprite.LoadContent(mainGame.Content, "items\\swords");
            mainEquip.sprite.Size = new Rectangle(0, 0, 32, 32);
            mainEquip.name = readData[1][0];
            mainEquip.sprite.tileCode = Convert.ToInt16(readData[1][1]);
            mainEquip.baseMinDmg = Convert.ToInt16(readData[1][2]);
            mainEquip.baseMaxDmg = Convert.ToInt16(readData[1][3]);

            playerData.Close();
        }
    }

    class Mob : Entity
    {
        public string prefix;
        public bool dead;

        //when generating, these are the scales to the level
        public double strengthRatio = 0;
        public double agilityRatio = 0;
        public double vitalityRatio = 0;
        public double intellectRatio = 0;
        public int XP = 0;

        public enum Type { Humanoid, Animal, Goblin, Eldritch, Demon, Dragon, Undead, Elemental, Mechanical }

        public void Generate(int scaleLevel)
        {
            this.level = scaleLevel;

            this.strength = (int)(scaleLevel * strengthRatio);
            this.agility = (int)(scaleLevel * agilityRatio);
            this.vitality = (int)(scaleLevel * vitalityRatio);
            this.intellect = (int)(scaleLevel * intellectRatio);

            maxHP = Math.Max(5, (int)((vitality * level) * (0.75)));
            HP = maxHP;
            maxMP = (int)((intellect * level) * ((0.25 * Math.Pow(level, 2)) + 0.25 * level));
            MP = maxMP;

            XP = (int)((scaleLevel * scaleLevel * 1.25) + (scaleLevel * 2.0));
        }
        
        public void Die()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            this.sprite.Draw(spriteBatch);
        }
    }

    class Boss : Mob
    {
        Sprite sprite = new Sprite();

        public string genName(string nameOfList)
        {
            //this generates the name of the boss.
            Random r = new Random();
            string listPath = "data\\names\\";
            listPath += nameOfList;
            listPath += ".txt";
            List<List<string>> nameParts = new List<List<string>>();
            List<string> title = new List<string>();
            StreamReader nameList = new StreamReader(listPath);

            //gets the list of names from the name list file.
            //
            while (nameList.Peek() != -1)
            {
                int listNumber = -1;
                if (nameList.ReadLine().Equals(""))
                {
                    //if line is blank, adds 1 more List<string> to nameList
                    nameParts.Add(new List<string>());
                    listNumber++;
                }
                else
                {
                    nameParts[listNumber].Add(nameList.ReadLine());
                }
            }

            string bossName = "";
            for (int a = 0; a < r.Next(2, nameParts.Count); a++)
            {
                bossName += nameParts[a][r.Next(0, nameParts[a].Count)];
            }
            return bossName;
        }
    }

    class NPC : Entity
    {
        public string name;
        public Sprite sprite;
        //public List<Quest> questsOffered = new List<Quest>();

        //dialogue
        public List<string> greetDialogue = new List<string>();
        public List<string> attackDialogue = new List<string>();

        //stats

    }
}
