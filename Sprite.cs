using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace prjTileRPG_1
{
    class Sprite
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //states
        public enum State
        {
            stand,
            attack,
            magic,
            dead,
            walkN,
            walkNE,
            walkE,
            walkSE,
            walkS,
            walkSW,
            walkW,
            walkNW,
            building
        }

        public State currentState = State.stand;

        public int tileCode;

        //The current position of the Sprite
        public Vector2 Position = new Vector2(0, 0);

        //The texture object used when drawing the sprite
        private Texture2D mSpriteTexture;

        //color used to draw sprite
        public Color color;

        //The size of the Sprite
        public Rectangle Size;

        //Used to size the Sprite up or down from the original image
        public float Scale = 1.0f;

        //roatation float
        public float RotationAngle = 0.0f;

        //effect
        public SpriteEffects effect = SpriteEffects.None;

        //Load the texture for the sprite using the Content Pipeline
        public void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            //loads the content from the content manager
            mSpriteTexture = theContentManager.Load<Texture2D>(theAssetName);
            Size = new Rectangle(0, 0, (int)(mSpriteTexture.Width * Scale), (int)(mSpriteTexture.Height * Scale));
            this.color = Color.White;
        }

        //pixel based collision algorithm
        public bool collidesWith(Sprite sprite)
        {
            Rectangle thisRect = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            Rectangle thisSource = new Rectangle((int)(this.tileCode % 16) * this.Size.Width, (int)(this.tileCode / 16) * this.Size.Height, this.Size.Width, this.Size.Height);
            Rectangle spriteRect = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, sprite.Size.Width, sprite.Size.Height);
            Rectangle spriteSource = new Rectangle((int)(sprite.tileCode % 16) * sprite.Size.Width, (int)(sprite.tileCode / 16) * sprite.Size.Height, sprite.Size.Width, sprite.Size.Height);
            Rectangle overlapRect = Rectangle.Intersect(thisRect, spriteRect);

            //if (thisRect.Right > spriteRect.Left && thisRect.Left < spriteRect.Right && thisRect.Bottom > spriteRect.Top && thisRect.Top < spriteRect.Bottom)

            if (overlapRect != Rectangle.Empty)
            {
                Rectangle thisCollideRect = new Rectangle();
                Rectangle spriteCollideRect = new Rectangle();

                thisCollideRect = new Rectangle(thisSource.Left + (spriteRect.Left - thisRect.Left), thisSource.Top + (spriteSource.Top - thisSource.Top), overlapRect.Width, overlapRect.Height);
                spriteCollideRect = new Rectangle(spriteSource.Left, spriteSource.Top, overlapRect.Width, overlapRect.Height);

                Color[] thisColors = new Color[this.mSpriteTexture.Width * this.mSpriteTexture.Height];
                Color[] spriteColors = new Color[sprite.mSpriteTexture.Width * sprite.mSpriteTexture.Height];
                this.mSpriteTexture.GetData<Color>(thisColors);
                sprite.mSpriteTexture.GetData<Color>(spriteColors);

                for (int y = overlapRect.Top; y < overlapRect.Bottom; y++)
                {
                    for (int x = overlapRect.Left; x < overlapRect.Right; x++)
                    {
                        Color colorA = thisColors[(x - thisRect.Left + thisSource.Left) * (y - thisRect.Top + thisSource.Top)];
                        Color colorB = spriteColors[(x - spriteRect.Left + spriteSource.Left) * (y - spriteRect.Top + spriteSource.Top)];

                        if ((colorA.A != 0) && (colorB.B != 0))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool overlapsWith(Sprite sprite)
        {
            Rectangle thisRect = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            Rectangle spriteRect = new Rectangle((int)sprite.Position.X, (int)sprite.Position.Y, sprite.Size.Width, sprite.Size.Height);

            Rectangle overlapRect = Rectangle.Intersect(thisRect, spriteRect);
            if (overlapRect != Rectangle.Empty)
                return true;

            return false;
        }

        //check if sprite is within a distance of anothe sprite
        public bool isWithin(Sprite sprite, double distance)
        {
            if (Math.Sqrt(Math.Pow(sprite.Position.X - this.Position.X, 2.0) + Math.Pow(sprite.Position.Y - this.Position.Y, 2.0)) <= distance)
                return true;

            return false;
        }

        //Draw the sprite to the screen
        public void Draw(SpriteBatch theSpriteBatch)
        {
            theSpriteBatch.Draw(mSpriteTexture, Position,
                new Rectangle(0, 0, this.Size.Width, this.Size.Height), this.color,
                RotationAngle, Vector2.Zero, Scale, this.effect, 0);
        }

        public void Draw(SpriteBatch theSpriteBatch, int tileCode)
        {
            theSpriteBatch.Draw(mSpriteTexture, Position,
                new Rectangle(32 * (tileCode % 16), 32 * Convert.ToInt16(tileCode / 16), this.Size.Width, this.Size.Height), this.color,
                RotationAngle, Vector2.Zero, Scale, this.effect, 0);
        }

        public void Draw(SpriteBatch theSpriteBatch, int tileCode, Color drawColor)
        {
            theSpriteBatch.Draw(mSpriteTexture, Position,
                new Rectangle(32 * (tileCode % 16), 32 * Convert.ToInt16(tileCode / 16), this.Size.Width, this.Size.Height), drawColor,
                RotationAngle, Vector2.Zero, Scale, this.effect, 0);
        }

        public void Draw(SpriteBatch theSpriteBatch, Rectangle sourceRect, Color drawColor)
        {
            theSpriteBatch.Draw(mSpriteTexture, Position,
                sourceRect, drawColor,
                RotationAngle, Vector2.Zero, 1.0f, this.effect, 0);
        }
    }
}