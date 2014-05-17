using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace WumpusDungeon
{
    abstract class Tile
    {
        public Vector2 Position { get; set; }
        protected Texture2D texture;
        public Vector2 Dimensions { get; set; }

        public Tile(ContentManager content, Vector2 position, Vector2 dimensions)
        {
            LoadContent(content);
            this.Dimensions = dimensions;

            this.Position = new Vector2(position.X * Dimensions.X, position.Y * Dimensions.Y);
        }
        protected abstract void LoadContent(ContentManager content);

        public void Draw(GameTime gameTime, SpriteBatch graphics)
        {
            graphics.Begin();

            graphics.Draw(texture, Position, Color.White);

            graphics.End();
        }
    }
    class EmptyTile : Tile
    {
        public EmptyTile(ContentManager content, Vector2 position, Vector2 dimensions)
            : base(content, position, dimensions)
        {
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/tiles/empty");
        }
    }

    class WallTile : Tile
    {
        public WallTile(ContentManager content, Vector2 position, Vector2 dimensions)
            : base(content, position, dimensions)
        {
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/tiles/wall");
        }
    }
}
