using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace WumpusDungeon
{
    class Player
    {
        // Position
        public Vector2 Position { get; set; }
        public Vector2 Dimensions { get; set; }

        // State
        private PlayerState state;
        public PlayerState State { get { return state; } set { state = value; } }
        private PlayerState defaultState;
        private bool spearOut;
        private bool torchOut;

        // Inventory
        private Inventory inventory;
        
        // Visuals
        private Texture2D texture;

        // Pathfinding
        int?[][] distanceMap;
        Vector2[] directionVectors; // Must be in same order as directions are = left, right, top, bot
        Stack<Direction> path;

        // Game logic
        private Level level;
        float idle; // Time in miliseconds to in wich player does not respond

        #region Load
        public Player(ContentManager content, Vector2 position, Level level, Inventory inventory)
        {
            this.inventory = inventory;
            this.level = level;
            this.idle = 0.0f;
            
            Initialize();

            LoadContent(content);
            
            // Constructor specific + post LoadContent actions
            Dimensions = new Vector2(texture.Width, texture.Height);
            this.Position = new Vector2(position.X, position.Y);

            level.Visit(Position);

            directionVectors = new Vector2[4];
            directionVectors[0] = new Vector2(-1, 0);
            directionVectors[1] = new Vector2(1, 0);
            directionVectors[2] = new Vector2(0, -1);
            directionVectors[3] = new Vector2(0, 1);
        }
        private void LoadContent(ContentManager content)
        {
            texture = content.Load<Texture2D>("textures/entities/player/idle");
        }
        private void Initialize()
        {
            defaultState = new PlayerState
            {
                FeelBreeze = false,
                SmellStench = false,
                HasGold = false,
                IsAlive = true
            };
            State = defaultState;
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            // Reset state to be able to be influenced by environment
            state = defaultState;

            // Dont do nothing if need to wait
            if (idle > 0)
            {
                idle -= gameTime.ElapsedGameTime.Milliseconds;
                return;
            }
            // Try to follow current path
            if (path != null && path.Count != 0)
            {
                MakeMove();
                return;
            }

            // Space draws or sheds the spear - if player has one
            if (Game1.IsNewKey(Keys.Space) && inventory.SpearCount > 0)
                ToggleSpear();
            if (Game1.IsNewKey(Keys.Q) && inventory.TorchCount > 0)
                ToggleTorch();

            if (spearOut || torchOut)
            {
                if (spearOut && Game1.IsLeftMousePressed())
                    ThrowSpear(Game1.GetMousePosition().AbsoluteToTiles());
                if (torchOut && Game1.IsLeftMousePressed())
                    ThrowTorch(Game1.GetMousePosition().AbsoluteToTiles());
            }
            else // If ready to throw -> can not move
            {
                if(Game1.IsLeftMousePressed())
                    MoveTo(Game1.GetMousePosition().AbsoluteToTiles());
                else if (Game1.IsNewKey(Keys.Up))
                    MoveUp();
                else if (Game1.IsNewKey(Keys.Down))
                    MoveDown();
                else if (Game1.IsNewKey(Keys.Left))
                    MoveLeft();
                else if (Game1.IsNewKey(Keys.Right))
                    MoveRight();
            }

            // Mark current location as visited
            level.Visit(Position);
        }
        // Movement
        private void MoveUp()
        {
            Vector2 newPosition = Position + new Vector2(0, -1);
            if(Level.IsPositonValid(newPosition))
                Position = newPosition;
        }
        private void MoveDown()
        {
            Vector2 newPosition = Position + new Vector2(0, 1);
            if(Level.IsPositonValid(newPosition))
                Position = newPosition;
        }
        private void MoveLeft()
        {
            Vector2 newPosition = Position + new Vector2(-1, 0);
            if(Level.IsPositonValid(newPosition))
                Position = newPosition;
        }
        private void MoveRight()
        {
            Vector2 newPosition = Position + new Vector2(1, -0);
            if(Level.IsPositonValid(newPosition))
                Position = newPosition;
        }
        private void MakeMove()
        {
            Direction d = path.Pop();
            switch (d)
            {
                case Direction.Left:
                    MoveLeft();
                    break;
                case Direction.Right:
                    MoveRight();
                    break;
                case Direction.Up:
                    MoveUp();
                    break;
                case Direction.Down:
                    MoveDown();
                    break;
                case Direction.Stay:
                    break;
                default:
                    break;
            }
            if(path.Count != 0)
                idle = 200;
        }
        // When player clicks, this method is called
        private void MoveTo(Vector2 destination)
        {
            if (destination == Position)
                return;
            
            if(Level.IsPositonValid(destination))
                path = FindPathTo(destination);
        }
        private Stack<Direction> FindPathTo(Vector2 destination)
        {
            ComputeDistanceMap();

            Stack<Direction> path = new Stack<Direction>();

            int? distance = null;
            Direction nextDirection = Direction.Stay;
            Vector2 nextDestination = Vector2.Zero;

            // Going backwards from destination to my current position (where distance == 0)
            while (distance != 0)
            {
                for (int i = 0; i < directionVectors.Length; ++i)
                {
                    Vector2 next = destination + directionVectors[i];
                    if (Level.IsPositonValid(next))
                        if (distanceMap[(int)next.X][(int)next.Y] < distance || distance == null && distanceMap[(int)next.X][(int)next.Y] != null)
                        {
                            distance = distanceMap[(int)next.X][(int)next.Y];
                            nextDirection = OppositeDireciton((Direction)i); // Going backwards now but player needs to go forward
                            nextDestination = next;
                        }
                }

                if (distance == null) // Path does not exist
                    return null;

                path.Push(nextDirection);
                destination = nextDestination;
            }

            return path;
        }
        private Direction OppositeDireciton(Direction directon)
        {
            switch (directon)
            {
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Stay:
                    return Direction.Stay;
                default:
                    return Direction.Stay;
            }
        }
        private void ComputeDistanceMap() // Uses standard wave algorithm with queue
        {
            InitializeDistanceMap();
            Queue<Vector2> toProcess = new Queue<Vector2>();
            distanceMap[(int)Position.X][(int)Position.Y] = 0;
            toProcess.Enqueue(Position);

            while (toProcess.Count != 0)
            {
                Vector2 v = toProcess.Dequeue();
                int distance = (int)distanceMap[(int)v.X][(int)v.Y] + 1;
                
                // Try left right top bot tiles
                for (int i = 0; i < directionVectors.Length; i++)
                {
                    // Apply the offset
                    Vector2 next = v + directionVectors[i];

                    // Position is inside the map and visited but not processed yet -> assign distance and enqueue to processing
                    if (Level.IsPositonValid(next) &&
                     level.IsVisited(next) &&
                     distanceMap[(int)next.X][(int)next.Y] == null) 
                    {
                        distanceMap[(int)next.X][(int)next.Y] = distance;
                        toProcess.Enqueue(next);
                    }
                }
            }
        }
        private void InitializeDistanceMap()
        {
            distanceMap = new int?[Level.MapWidth][];
            for (int i = 0; i < distanceMap.Length; ++i)
                distanceMap[i] = new int?[Level.MapHeight];
        }
        // Weapon
        private void ToggleSpear()
        {
            // Toggle
            spearOut = !spearOut;
            torchOut = false;
            if (spearOut)
                Game1.DrawSpear();
            else
                Game1.DefaultCursor();
        }
        private void ThrowSpear(Vector2 target)
        {
            level.SpearAt(target);
            ToggleSpear();
            inventory.SpearCount--;
        }
        // Torch
        private void ToggleTorch()
        {
            torchOut = !torchOut;
            spearOut = false;
            if (torchOut)
                Game1.DrawTorch();
            else
                Game1.DefaultCursor();
        }
        private void ThrowTorch(Vector2 target)
        {
            level.TorchAt(target);
            ToggleTorch();
            inventory.TorchCount--;
        }
        // Public API to change state
        public void SeeGold()
        {
            state.HasGold = true;
        }
        public void SmellStench()
        {
            state.SmellStench = true;
        }
        public void FeelBreeze()
        {
            state.FeelBreeze = true;
        }
        public void Die()
        {
            state.IsAlive = false;
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime, SpriteBatch graphics)
        {
            graphics.Begin();
            graphics.Draw(texture, Position.TilesToAbsolute(), Color.White);
            graphics.End();
        }
        public void DrawStatus(SpriteBatch spriteBatch, Vector2 position, SpriteFont font, Color color)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(font, String.Format("Smell stench: {0}\nFeel breeze: {1}\nIs alive: {2}\nHas gold: {3}", state.SmellStench, state.FeelBreeze, state.IsAlive, state.HasGold), position, color);

            spriteBatch.End();
        }
        public void DrawHUD(SpriteBatch spriteBatch, Vector2 position, SpriteFont font, Color color)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(font, String.Format("Spears: {0}  Torches: {1}", inventory.SpearCount, inventory.TorchCount), position, color);

            spriteBatch.End();
        }
        #endregion
    }

    struct PlayerState
    {
        public bool SmellStench { get; set; }
        public bool FeelBreeze { get; set; }
        public bool IsAlive { get; set; }
        public bool HasGold { get; set; }
    }

    enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        Stay
    }
}
