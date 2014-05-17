using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Frms = System.Windows.Forms;
using System.IO;

namespace WumpusDungeon
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Graphics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static int screenWidth;
        public static int screenHeight;
        public static Vector2 HUDPosition
        {
            get
            {
                return new Vector2(16, screenHeight - 64);
            }
        }

        Level level;
        int levelNumber;
        int levelCount;

        string settingsFile = "settings.ini";
        const string LVEVEL_COUNT = "LevelCount";

        static KeyboardState currKeyboard;
        static KeyboardState recKeyboard;

        static MouseState currMouseState;
        static MouseState recMouseState;
        static Vector2 cursorPosition;
        static Texture2D cursorTexture;
        static Texture2D spear;
        static Texture2D torch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            currKeyboard = Keyboard.GetState();

            levelNumber = 1;
            
            screenWidth = 1280;
            screenHeight = 720;
            
            LoadSettings();
            
            // Graphics settings - window size
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();

            base.Initialize();
        }
        private void LoadSettings()
        {
            StreamReader reader = new StreamReader(settingsFile);
            Dictionary<string, string> settings = new Dictionary<string,string>();
            
            ReadSettingsFromFile(reader, settings);

            int lvlCount;
            if (Int32.TryParse(settings[LVEVEL_COUNT], out lvlCount))
                levelCount = lvlCount;
        }
        private void ReadSettingsFromFile(StreamReader reader, Dictionary<string, string> settings)
        {
            const char equals = '=';
            char[] spaceQuotes = new char[2] {' ', '\"'};
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] lineSplit = line.Split(equals);
                if (lineSplit.Length == 2)
                {
                    string key = lineSplit[0].Trim();
                    string value = lineSplit[1].Trim(spaceQuotes);
                    settings.Add(key, value);
                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spear = Content.Load<Texture2D>("textures/pointers/spear");
            torch = Content.Load<Texture2D>("textures/pointers/torch");

            level = new Level(Content, levelNumber);
        }
        private void RestartGame()
        {
            LoadContent();
        }
        private void NextLevel()
        {
            levelNumber = ++levelNumber % (levelCount + 1);
            if (levelNumber == 0)
                levelNumber = 1;
            LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            UpdateMouse();

            recKeyboard = currKeyboard;
            currKeyboard = Keyboard.GetState();

            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (level.IsGameOver)
            {
                Frms.MessageBox.Show("Game over, you died!");
                RestartGame();
            }
            else if (level.IsGameFinished)
            {
                Frms.MessageBox.Show("Victory, you have found the gold!");
                NextLevel();
            }

            level.Update(gameTime);

            base.Update(gameTime);
        }
        private void UpdateMouse()
        {
            recMouseState = currMouseState;
            currMouseState = Mouse.GetState();
            cursorPosition.X = currMouseState.X;
            cursorPosition.Y = currMouseState.Y;

            if (cursorTexture == null)
                this.IsMouseVisible = true;
            else
                this.IsMouseVisible = false;
        }
        
        

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            level.Draw(gameTime, spriteBatch);

            spriteBatch.Begin();
            if (cursorTexture != null)
                spriteBatch.Draw(cursorTexture, cursorPosition, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static bool IsNewKey( Keys key )
        {
            return currKeyboard . IsKeyDown(key) && recKeyboard . IsKeyUp(key);
        }
        public static bool IsLeftMousePressed()
        {
            return currMouseState.LeftButton == ButtonState.Pressed && recMouseState.LeftButton == ButtonState.Released;
        }
        public static Vector2 GetMousePosition()
        {
            return cursorPosition;
        }
        public static void DrawSpear()
        {
            cursorTexture = spear;
        }
        internal static void DrawTorch()
        {
            cursorTexture = torch;
        }
        public static void DefaultCursor()
        {
            cursorTexture = null;
        }

        
    }
}
