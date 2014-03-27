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

namespace prjTileRPG_1
{
    class Item
    {
        public Random r = new Random();
        public string name;
        public int cost;
        public int currentDurability, maxDurability;
        public Sprite sprite;
        public enum Quality 
        {
            Junk = 0, //appears grey
            Mundane = 1, //appears white
            Enchanted = 2, //appears green
            Epic = 3, //appears blue
            Legendary = 4 //appears orange
        }
        public Quality tier;
    }

    class Weapon : Item
    {
        public enum weaponType { Sword, Axe, Staff, Dagger }
        public weaponType type;
        public int baseMinDmg, baseMaxDmg;
        
        public void Generate(Game1 mainGame, string module, int level, Quality tier, weaponType type)
        {
            this.tier = tier;
            this.type = type;
            List<string> nameListData = new List<string>();
            List<List<string>> nameLists = new List<List<string>>();

            int nameListNumber = 0;
            if ((int)tier > 3 && r.Next(1,100) > 60)
                nameListNumber = 2;
            else
                nameListNumber = 1;

            StreamReader nameListReader = new StreamReader("Content\\data\\modules\\" + module + "\\names\\" + type.ToString().ToLower() + nameListNumber + ".txt");
            int a = -1;
            while (nameListReader.Peek() != -1)
            {
                if (nameListReader.ReadLine() == "*" || nameListReader.Read() == '*')
                {
                    nameLists.Add(new List<string>());
                    a++;
                }
                else
                    nameLists[a].Add(nameListReader.ReadLine());
            }
            nameListReader.Close();

            for (int b = 0; b < r.Next(2, nameLists.Count); b++)
                this.name += nameLists[b][r.Next(0, nameLists[b].Count)];

            this.baseMinDmg = r.Next((int)(0.65 * (level + (int)tier - 2)), (int)(0.85 * (level + (int)tier + 2)));
            this.baseMaxDmg = r.Next((int)(0.70 * (level + (int)tier - 2)), (int)(0.95 * (level + (int)tier + 2)));

            this.sprite = new Sprite();
            this.sprite.LoadContent(mainGame.Content, "items\\swords");
            this.sprite.tileCode = r.Next(0, (this.sprite.Size.Width / 32) * (this.sprite.Size.Height / 32));
            this.sprite.Size = new Rectangle(0, 0, 32, 32);
        }
    }

    class Armor : Item
    {
        
    }
}
