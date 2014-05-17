using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WumpusDungeon
{
    abstract class Entity
    {
        protected Texture2D texture;
        public Vector2 Position;
        public Vector2 Dimensions { get; set; }
        protected List<Effect> effects;

        public Entity(ContentManager content, Vector2 position)
        {
            LoadContent(content);
            this.Position = new Vector2(position.X, position.Y);
            effects = new List<Effect>();
        }

        abstract protected void LoadContent(ContentManager content);

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch graphics)
        {
            graphics.Begin();

            // Draw entity itself
            if (texture != null)
                graphics.Draw(texture, Position.TilesToAbsolute(), Color.White);

            // Draw effects bound to this entity
            foreach (var effect in effects)
                effect.Draw(gameTime, graphics);

            graphics.End();
        }

        public abstract void AffectPlayer(Player player);
    }

    class Pit : Entity
    {
        public Pit(ContentManager content, Vector2 position)
            : base(content, position)
        {
            // Create breeze left right above under
            if(Level.IsPositonValid(Position + new Vector2(1, 0)))
                effects.Add(new Breeze(content, Position + new Vector2(1, 0)));
            if (Level.IsPositonValid(Position + new Vector2(-1, 0)))
                effects.Add(new Breeze(content, Position + new Vector2(-1, 0)));
            if (Level.IsPositonValid(Position + new Vector2(0, 1)))
                effects.Add(new Breeze(content, Position + new Vector2(0, 1)));
            if (Level.IsPositonValid(Position + new Vector2(0, -1)))
                effects.Add(new Breeze(content, Position + new Vector2(0, -1)));
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/entities/pit");
        }
        public override void AffectPlayer(Player player)
        {
            if (player.Position == this.Position)
                player.Die();

            foreach (var breeze in effects)
                if (breeze.Position == player.Position)
                    player.FeelBreeze();
        }
    }

    class Gold : Entity
    {
        public Gold(ContentManager content, Vector2 position)
            : base(content, position)
        {
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/entities/treasure");
        }
        public override void AffectPlayer(Player player)
        {
            if (player.Position == this.Position)
                player.SeeGold();
        }
    }

    class Wumpus : Entity
    {
        public Wumpus(ContentManager content, Vector2 position)
            : base(content, position)
        {
            // Create stench left right above under
            if (Level.IsPositonValid(Position + new Vector2(1, 0)))
                effects.Add(new Stench(content, Position + new Vector2(1, 0)));
            if (Level.IsPositonValid(Position + new Vector2(-1, 0)))
                effects.Add(new Stench(content, Position + new Vector2(-1, 0)));
            if (Level.IsPositonValid(Position + new Vector2(0, 1)))
                effects.Add(new Stench(content, Position + new Vector2(0, 1)));
            if (Level.IsPositonValid(Position + new Vector2(0, -1)))
                effects.Add(new Stench(content, Position + new Vector2(0, -1)));
        }

        protected override void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/entities/wumpus/wumpus");
        }
        public override void AffectPlayer(Player player)
        {
            if (player.Position == this.Position)
                player.Die();

            foreach (var stench in effects)
                if (stench.Position == player.Position)
                    player.SmellStench();
        }
    }
}
