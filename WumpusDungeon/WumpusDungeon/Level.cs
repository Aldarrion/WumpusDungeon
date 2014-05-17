using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace WumpusDungeon
{
    class Level
    {
        public bool[][] GetMapVisited() { return mapVisited; }
        bool[][] mapVisited;
        static int mapWidth;
        static int mapHeight;
        public static int MapWidth { get { return mapWidth; } }
        public static int MapHeight { get { return mapHeight; } }
        Vector2 tileDimensions;
        
        // List of belonging entities
        List<Tile> tiles;
        List<Entity> entities;
        Player player;

        Texture2D notVisited;
        ContentManager content;
        SpriteFont arial;

        // Game logic
        public bool IsGameFinished { get; set; }
        public bool IsGameOver { get; set; }
        private int levelNumber;

        #region Load
        public Level(ContentManager content, int levelNumber)
        {
            this.content = content;
            this.levelNumber = levelNumber;
            this.tileDimensions = new Vector2(64, 64);
            tiles = new List<Tile>();
            entities = new List<Entity>();

            LoadContent(content);
        }
        private void LoadContent(ContentManager content)
        {
            notVisited = content.Load<Texture2D>("textures/tiles/notVisited");
            arial = content.Load<SpriteFont>("fonts/Arial");
            LoadMap();
        }
        private void InitializeMapVisited()
        {
            mapVisited = new bool[mapWidth][];
            for (int i = 0; i < mapWidth; i++)
                mapVisited[i] = new bool[mapHeight]; // Filled with default -> false (whole map is unvisited)
        }
        private void LoadMap()
        {
            StreamReader reader = new StreamReader(String.Format("Content/maps/map{0}.txt", levelNumber));
            
            mapWidth = Int32.Parse(reader.ReadLine());
            mapHeight = Int32.Parse(reader.ReadLine());

            InitializeMapVisited();

            string line = reader.ReadLine();
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    tiles.Add(new EmptyTile(content, new Vector2(x, y), tileDimensions));
                    LoadEntity(line[x], new Vector2(x, y));
                }
                line = reader.ReadLine();
            }
        }
        private void LoadEntity(char code, Vector2 position)
        {
            switch (code)
            {
                case 'P':
                    LoadPlayer(position);
                    break;
                case '_':
                    entities.Add(new Pit(content, position));
                    break;
                case 'W':
                    entities.Add(new Wumpus(content, position));
                    break;
                case 'X':
                    entities.Add(new Gold(content, position));
                    break;
                default:
                    break;
            }
        }
        private void LoadPlayer(Vector2 position)
        {
            if (player != null)
                throw new FormatException("Two players detected");

            player = new Player(content, position, this, new Inventory(score: 0, spearCount: levelNumber, torchCount: levelNumber + 1));
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            foreach (var e in entities)
                e.Update(gameTime);

            player.Update(gameTime);

            foreach (var e in entities)
                e.AffectPlayer(player);

            mapVisited[(int)player.Position.X][(int)player.Position.Y] = true;

            // If player died end the game -> Game Over
            if (!player.State.IsAlive)
                IsGameOver = true;
            // If player found gold and did not die -> Game Finished
            else if (player.State.HasGold)
            {
                IsGameFinished = true;
                RevealWholeMap();
            }

        }
        private void RevealWholeMap()
        {
            for (int i = 0; i < mapVisited.Length; i++)
                for (int j = 0; j < mapVisited[i].Length; j++)
                    mapVisited[i][j] = true;
        }
        #endregion

        #region Draw
        public void Draw(GameTime gameTime, SpriteBatch graphics)
        {
            // Tiles
            foreach (var t in tiles)
                t.Draw(gameTime, graphics);

            // Entities
            foreach (var e in entities)
                e.Draw(gameTime, graphics);

            // Player
            player.Draw(gameTime, graphics);
            player.DrawStatus(graphics, new Vector2(mapWidth, 0).TilesToAbsolute(), arial, Color.Black);
            player.DrawHUD(graphics, Game1.HUDPosition, arial, Color.Black);

            graphics.Begin();

            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    if (!mapVisited[x][y])
                        graphics.Draw(notVisited, new Vector2(x, y).TilesToAbsolute(), Color.White);

            graphics.End();
        }
        #endregion

        public static bool IsPositonValid(Vector2 position)
        {
            return position.X >= 0 && position.X < mapWidth &&
                position.Y >= 0 && position.Y < mapHeight;
        }
        public void SpearAt(Vector2 target)
        {
            foreach (var item in entities)
                if (item.GetType() == typeof(Wumpus) && item.Position == target)
                {
                    KillWumpus((Wumpus)item);
                    break;
                }
        }
        private void KillWumpus(Wumpus wumpus)
        {
            entities.Remove(wumpus);
        }
        public void TorchAt(Vector2 target)
        {
            Reveal(target);
        }
        public bool IsVisited(Vector2 target)
        {
            return mapVisited[(int)target.X][(int)target.Y];
        }
        public void Visit(Vector2 target)
        {
            Reveal(target);
        }
        private void Reveal(Vector2 target)
        {
            mapVisited[(int)target.X][(int)target.Y] = true;
        }
    }
}
