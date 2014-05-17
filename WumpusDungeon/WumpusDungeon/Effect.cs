using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WumpusDungeon
{
    abstract class Effect
    {
        protected Texture2D texture;
        public Vector2 Position { get; set; }

        public Effect(ContentManager content, Vector2 position)
        {
            LoadContent(content);
            this.Position = position;
        }
        protected abstract void LoadContent(ContentManager content);

        public virtual void Draw(GameTime gameTime, SpriteBatch graphics)
        {
            graphics.Draw(texture, Position.TilesToAbsolute(), Color.White);
        }
    }

    class Stench : Effect
    {
        public Stench(ContentManager content, Vector2 position)
            : base(content, position)
        {
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/entities/stench");
        }
    }

    class Breeze : Effect
    {
        public Breeze(ContentManager content, Vector2 position)
            : base(content, position)
        {
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/entities/breeze");
        }
    }
}
