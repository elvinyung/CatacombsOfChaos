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
    class Ability
    {
        public string name; //name of the spell
        public int level; //required level of ability
        public int manaCost; //mana cost of ability from player's mana

        public Vector2 startPosRelativeToMapOrigin, posRelativeToMapOrigin, startPosition, directionUnit; //these will manage the movement of the spell
        public int maxRange; //range of the spell in terms of pixels
        public int duration; //the length of the ability's effect in time
        public bool Visible; //boolean to decide whether or not to draw
        public Sprite spellVisual; //sprite that represents the spell visual

        public int minDmg, maxDmg; //the actual damage will be an integer between this range

        public int strength; //strength effect
        public int agility; //agility effect
        public int vitality; //vitality effect
        public int intellect; //intellect effect
        public int speedModifier; //speed modifier effect

        public void Affect(Entity entity)
        {
            entity.strength += this.strength;
            entity.agility += this.agility;
            entity.vitality += this.vitality;
            entity.intellect += this.intellect;

            entity.speedModifier = this.speedModifier;
        }

        public void endAffect(Entity entity)
        {
            entity.strength += this.strength;
            entity.agility += this.agility;
            entity.vitality += this.vitality;
            entity.intellect += this.intellect;

            entity.speedModifier = 0;
        }

        public void update(Map map, Player player)
        {
            this.posRelativeToMapOrigin = Vector2.Subtract(this.spellVisual.Position, map.origin);
            this.startPosRelativeToMapOrigin = Vector2.Subtract(this.startPosition, map.origin);
            if (Vector2.Distance(posRelativeToMapOrigin, startPosRelativeToMapOrigin) > maxRange)
                Visible = false;

            for (int a = 0; a < map.mobs.Count; a++)
            {
                if (this.spellVisual.collidesWith(map.mobs[a].sprite) && this.Visible && map.mobs[a].sprite.color.A != 0)
                {
                    map.mobs[a].Damage((int)((player.intellect) - (0.25 * map.mobs[a].agility)));
                    this.Visible = false;
                }
            }

            if (Visible)
            {
                this.posRelativeToMapOrigin = Vector2.Add(this.posRelativeToMapOrigin, directionUnit);
                this.startPosition = Vector2.Add(Vector2.Add(this.startPosRelativeToMapOrigin, map.origin), directionUnit);
                this.spellVisual.Position = Vector2.Add(map.origin, this.posRelativeToMapOrigin);
            }


            int x = (int)map.getMapPosition(spellVisual).X;
            int y = (int)map.getMapPosition(spellVisual).Y;

            if (x > 0 && y > 0 &&
                x < map.width - 1 && y < map.height - 1)
            {
                if (map.midTiles[y][x].tileType != 0)
                    Visible = false;
            }
            else
                Visible = false;

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
                spellVisual.Draw(spriteBatch, spellVisual.tileCode, spellVisual.color);            
        }
    }

    class Buff : Ability
    {
        //buff or debuff
    }
}
