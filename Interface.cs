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
    class Window
    {
        public Vector2 origin;
        Sprite pixel;
        public List<Text> text = new List<Text>();
        public List<Grid> grids = new List<Grid>();
        public List<Menu> menus = new List<Menu>();
        public Sprite background = new Sprite();
        public Color drawColor;
        bool drawBG = false;

        public Window(Game mainGame, int originX, int originY, int width, int height, Color drawColor)
        {
            pixel = new Sprite();
            pixel.LoadContent(mainGame.Content, "interface\\whitepixel");
            pixel.Size = new Rectangle(0, 0, width, height);
            pixel.Position = new Vector2(originX, originY);
            this.origin = new Vector2(originX, originY);
            this.drawColor = drawColor;
        }

        public Window(Game mainGame, string bgImage, int originX, int originY, int width, int height, Color drawColor)
        {
            //the pixel is a 1x1 sprite, stretched and colored to fit according to specifications
            pixel = new Sprite();
            pixel.LoadContent(mainGame.Content, "interface\\whitepixel");
            pixel.Size = new Rectangle(0, 0, width, height);
            pixel.Position = new Vector2(originX, originY);
            background.LoadContent(mainGame.Content, bgImage);
            this.drawColor = drawColor;
            pixel.color = drawColor;
            this.origin = new Vector2(originX, originY);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            pixel.Draw(spriteBatch, new Rectangle(0, 0, pixel.Size.Width, pixel.Size.Height), drawColor);
            
            for (int a = 0; a < text.Count; a++)
                text[a].Draw(spriteBatch);
            for (int a = 0; a < grids.Count; a++)
                grids[a].Draw(spriteBatch, new Vector2(this.origin.X + 10, this.origin.Y + 10));
            for (int a = 0; a < menus.Count; a++)
                menus[a].Draw(spriteBatch, new Vector2(this.origin.X + 10, this.origin.Y + 10));
        }
    }

    class Button
    {
        public Vector2 origin;
        Sprite pixel;
        public Text text;
        public Color drawColor;

        public Button(Game mainGame, SpriteFont font, int originX, int originY, int width, int height, string Text, Color drawColor)
        {
            pixel.LoadContent(mainGame.Content, "interface\\whitepixel");
            pixel.Size = new Rectangle(0, 0, width, height);
            pixel.Position = new Vector2(originX, originY);
            this.text = new Text(font, Text, Vector2.Add(pixel.Position, new Vector2(5, 5)));
            this.drawColor = drawColor;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            pixel.Draw(spriteBatch);
            text.Draw(spriteBatch);
        }
    }

    class Grid
    {
        int gridWidth, gridHeight, tileSize;
        List<List<Sprite>> items = new List<List<Sprite>>();
        Game mainGame;
        Color drawColor, defaultColor, hoverColor;

        public Grid(Game mainGame, float originX, float originY, int gridWidth, int gridHeight, int tileSize, float spacing, Color defaultColor, Color hoverColor)
        {
            this.mainGame = mainGame;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.tileSize = tileSize;
            this.drawColor = defaultColor;
            this.defaultColor = defaultColor;
            this.hoverColor = hoverColor;

            Vector2 origin = new Vector2(originX, originY);

            for (int y = 0; y < gridHeight; y++)
            {
                items.Add(new List<Sprite>());
                for (int x = 0; x < gridWidth; x++)
                {
                    items[y].Add(new Sprite());
                    items[y][x].LoadContent(mainGame.Content, "interface\\whitepixel");
                    items[y][x].Size = new Rectangle(0, 0, 32, 32);
                    items[y][x].Scale = 32.0f;
                    items[y][x].Position = new Vector2(((x % gridWidth) * tileSize) + (((x % gridWidth) - 1) * spacing) + origin.X, (y * tileSize) + ((y - 1) * spacing) + origin.Y);
                    items[y][x].color = drawColor;
                }
            }
        }

        public void checkMouseHover(Game mainGame, Vector2 mousePosition)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Rectangle itemRect = new Rectangle((int)items[y][x].Position.X, (int)items[y][x].Position.Y, items[y][x].Size.Width, items[y][x].Size.Height);
                    if (itemRect.Contains(new Point((int)mousePosition.X, (int)mousePosition.Y)))
                    {
                        items[y][x].color = hoverColor;
                        mouseHover(items[y][x]);
                    }
                    else
                        items[y][x].color = defaultColor;
                }
            }
        }

        public void checkMouseClick(Game mainGame, Vector2 mousePosition, Item item)
        {

        }

        public void mouseHover(Sprite sprite)
        {

        }

        public void mouseClick()
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 origin)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    items[y][x].Draw(spriteBatch, new Rectangle(0, 0, this.tileSize, this.tileSize), items[y][x].color);
                }
            }
        }
    }

    class Menu
    {
        Keys toggleKey;
        Sprite background = new Sprite();
        public List<Text> items = new List<Text>();
        public int selectedItem;
        int spacing;
        SpriteFont mainFont;

        public Menu (Game mainGame, string[] items, int spacing)
        {            
            int currentX = 0, currentY = 0;
            this.spacing = spacing;
            mainFont = mainGame.Content.Load<SpriteFont>("mainFont");
            for (int a = 0; a < items.Length; a++)
            {
                this.items.Add(new Text(mainFont, items[a], new Vector2(0, currentY)));
                currentY += spacing;
            }
        }

        public void setItems(string[] items)
        {
            this.items.Clear();
            int currentY = 0;
            for (int a = 0; a < items.Length; a++)
            {
                this.items.Add(new Text(mainFont, items[a], new Vector2(0, currentY)));
                currentY += spacing;
            }
        }

        public void checkHover(Vector2 menuOrigin, Point mousePosition, SpriteFont font)
        {
            for (int a = 0; a < items.Count; a++)
            {
                Rectangle textRect = new Rectangle((int)(menuOrigin.X + items[a].position.X), (int)(menuOrigin.Y + items[a].position.Y), (int)font.MeasureString(items[a].content).X, (int)font.MeasureString(items[a].content).Y);
                if (textRect.Contains(mousePosition))
                {
                    items[a].color = Color.Yellow;
                    items[a].selected = true;
                }
                else
                {
                    items[a].color = Color.White;
                    items[a].selected = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 startPosition)
        {
            
            float originX = startPosition.X, originY = startPosition.Y;
            for (int a = 0; a < items.Count; a++)
            {
                Vector2 drawPosition = new Vector2(originX + items[a].position.X, originY + items[a].position.Y);
                spriteBatch.DrawString(items[a].font, items[a].content, drawPosition, items[a].color);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 startPosition, Rectangle boundingBox)
        {
            float originX = startPosition.X, originY = startPosition.Y;
            for (int a = 0; a < items.Count; a++)
            {
                Vector2 drawPosition = new Vector2(originX + items[a].position.X, originY + items[a].position.Y);
                if ((drawPosition.X >= boundingBox.Left) && (drawPosition.X <= boundingBox.Right) && (drawPosition.Y >= boundingBox.Top) && (drawPosition.Y <= boundingBox.Bottom))
                {
                    spriteBatch.DrawString(items[a].font, items[a].content, drawPosition, items[a].color);
                }
            }
        }
    }

    class Text
    {
        public SpriteFont font;
        public string content;
        public Vector2 position;
        public Color color = Color.White;
        public bool selected = false;

        public Text(SpriteFont font, string message, Vector2 position)
        {
            this.font = font;
            this.content = message;
            this.position = position;
        }


        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, content, position, color);
        }


    }

    class TextTree : Text
    {
        public List<Text> ChildText = new List<Text>();
        public int spacing;

        public TextTree(SpriteFont font, string rootText, string[] childText, Vector2 position) : base(font, rootText, position)
        {
            this.font = font;
            this.content = rootText;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, content, position, color);
            for (int a = 0; a < ChildText.Count; a++)
            {
                ChildText[a].position = new Vector2(this.position.X + 5, this.position.Y + (spacing * a));
                ChildText[a].Draw(spriteBatch);
            }
        }
    }
}
