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
        private Texture2D _screenTexture;
        private KeyboardManager _keyboardManager;
        private DebugOutputManager _debug;
        private float _residual;

        private bool _emulate;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _keyboardManager = new KeyboardManager();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _cpu = new CPU();
            // _graphics.SynchronizeWithVerticalRetrace = false;
            // IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            _screenTexture = new Texture2D(GraphicsDevice, CPU.DisplayWidth, CPU.DisplayHeight, false, SurfaceFormat.ColorSRgb);
            _systemFont = Content.Load<SpriteFont>(@"NES_FONT");

            _cpu.LoadROM("/home/kris/Projects/PolyChip8/ROMS/BLINKY");
            _cpu.Disassemble();
            
            _debug = new DebugOutputManager(_systemFont, _cpu);
            
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
            
            _keyboardManager.Update(Keyboard.GetState(), _cpu);
            
            _emulate = true;
            
            if (_emulate)
            {
                _cpu.Clock();
            }
            
            if (_cpu.DelayTimer >= 0)
                _cpu.DelayTimer--;
            if (_cpu.SoundTimer >= 0)
            {
                Console.Beep();
                _cpu.SoundTimer--;
            }
            
            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);
            GraphicsDevice.Textures[0] = null;

            _screenTexture.SetData(_cpu.Screen, 0, CPU.DisplayHeight * CPU.DisplayWidth );
            
            var fps = (float) (1 / gameTime.ElapsedGameTime.TotalSeconds);
            
            _spriteBatch.Begin();
            
            _spriteBatch.Draw(_screenTexture, new Vector2(120, 20), new Rectangle(0,0, CPU.DisplayWidth, CPU.DisplayHeight),
                Color.White, 0.0f, new Vector2(0,0), 10f, SpriteEffects.None, 0f);
            
            _debug.DrawDisassembly(_spriteBatch);
            _debug.DrawFps(_spriteBatch, fps);
            _debug.DrawRam(_spriteBatch);
            _debug.DrawRegisters(_spriteBatch);
            
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}