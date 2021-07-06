using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        private bool _emulate;

        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _cpu = new CPU();

            _screenTexture = new Texture2D(GraphicsDevice, _cpu.DisplayWidth, _cpu.DisplayHeight, false, SurfaceFormat.ColorSRgb);
            _systemFont = Content.Load<SpriteFont>(@"NES_FONT");

            _cpu.LoadROM("/home/kris/Projects/PolyChip8/ROMS/IBM Logo.ch8");
            
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

            _currentKeyboardState = Keyboard.GetState();
            
            if (_emulate)
            {
                _cpu.Clock();
            }

            if (_currentKeyboardState.IsKeyDown(Keys.C) &&
                _previousKeyboardState.IsKeyUp(Keys.C))
            {
                _cpu.Clock();
            }

            _previousKeyboardState = _currentKeyboardState;

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);
            GraphicsDevice.Textures[0] = null;
            
            _screenTexture.SetData(_cpu.Screen, 0, _cpu.DisplayHeight * _cpu.DisplayWidth );
            
            _spriteBatch.Begin();
            
            _spriteBatch.Draw(_screenTexture, new Vector2(0,0), new Rectangle(0,0, _cpu.DisplayWidth, _cpu.DisplayHeight),
                Color.White, 0.0f, new Vector2(0,0), 5f, SpriteEffects.None, 0f);
            
            DrawRegisters();
            
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }

        private void DrawRegisters()
        {
            var baseX = _cpu.DisplayWidth + 650;
            var baseY = 20;
            
            _spriteBatch.DrawString(_systemFont, $"PC: ${_cpu.ProgramCounter:X4}", new Vector2(baseX + 64, baseY + 20), Color.White);
            _spriteBatch.DrawString(_systemFont, $"Stack P: ${_cpu.StackPointer:X2}", new Vector2(baseX + 64, baseY + 40), Color.White);
            _spriteBatch.DrawString(_systemFont, $"I: ${_cpu.AddressRegister:X4} ", new Vector2(baseX + 64, baseY + 60), Color.White);
            
            _spriteBatch.DrawString(_systemFont, $"X: ${_cpu.X:X2} ", new Vector2(baseX + 134, baseY + 60), Color.White);
            _spriteBatch.DrawString(_systemFont, $"Y: ${_cpu.Y:X2} ", new Vector2(baseX + 204, baseY + 60), Color.White);
            _spriteBatch.DrawString(_systemFont, $"N: ${_cpu.N:X2} ", new Vector2(baseX + 274, baseY + 60), Color.White);
            _spriteBatch.DrawString(_systemFont, $"NN: ${_cpu.Nn:X2} ", new Vector2(baseX + 344, baseY + 60), Color.White);
            _spriteBatch.DrawString(_systemFont, $"NNN: ${_cpu.Nnn:X4} ", new Vector2(baseX + 414, baseY + 60), Color.White);
            
            _spriteBatch.DrawString(_systemFont, $"Current Instruction: ${_cpu.CurrentInstruction.Name} ", new Vector2(baseX + 64, baseY + 80), Color.White);

            for (int i = 0x0; i < 0x10; i++)
            {
                var y = (baseY + 100) + i * 20;
                _spriteBatch.DrawString(_systemFont, $"V{i}: ${_cpu.VRegisters[i]} ", new Vector2(baseX + 64, y), Color.White);
            }
            
            _spriteBatch.DrawString(_systemFont, $"Dissassembly: {_cpu.Dissasembly()} ", new Vector2(baseX + 200, baseY + 150), Color.White);
        }
    }
}