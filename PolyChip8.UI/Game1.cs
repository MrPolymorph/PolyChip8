using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyChip8.UI.Drawing;
using PolyChip8.UI.Managers;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace PolyChip8.UI
{
    public class Game1 : Game
    {
        private CPU _cpu;
        private GraphicsDeviceManager _graphics;
        private SpriteFont _systemFont;
        private SpriteBatch _spriteBatch;
        private GameStateManager _gameStateManager;
        private float _residual;

        private bool _emulate;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _cpu = new CPU();
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _systemFont = Content.Load<SpriteFont>(@"NES_FONT");

            _gameStateManager = new GameStateManager(GraphicsDevice, _systemFont, _cpu);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            _gameStateManager.Update(gameTime);


            if (Keyboard.GetState().IsKeyDown(Keys.F5))
            {
                IsFixedTimeStep = !IsFixedTimeStep;
                _graphics.SynchronizeWithVerticalRetrace = !_graphics.SynchronizeWithVerticalRetrace;
            }
            
            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);
            GraphicsDevice.Textures[0] = null;
            
            _spriteBatch.Begin();
                _gameStateManager.Draw(_spriteBatch);
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}