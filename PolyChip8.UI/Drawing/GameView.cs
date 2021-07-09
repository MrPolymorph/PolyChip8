using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyChip8.UI.Managers;

namespace PolyChip8.UI.Drawing
{
    public class GameView
    {
        private readonly CPU _cpu;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly GameStateManager _stateManager;
        private readonly KeyboardManager _keyboardManager;
        private Texture2D _screenTexture;

        private bool _emulate;

        public GameView(GameStateManager stateManager, GraphicsDevice gd, KeyboardManager keyboardManager, CPU cpu)
        {
            _cpu = cpu;
            _emulate = true;
            _graphicsDevice = gd;
            _stateManager = stateManager;
            _screenTexture = new Texture2D(gd, CPU.DisplayWidth, CPU.DisplayHeight, false, SurfaceFormat.ColorSRgb);

            _keyboardManager = keyboardManager;
            
            _keyboardManager.SubscribeForKeyDown(Keys.Space, ToggleEmulation);
            _keyboardManager.SubscribeForKeyDown(Keys.Back, () => { _stateManager.ChangeState(GameState.Menu, string.Empty);});
            _keyboardManager.SubscribeForKeyDown(Keys.F, _cpu.Clock);
            _keyboardManager.SubscribeForKeyDown(Keys.F1, () => {_stateManager.ChangeState(GameState.Debug, string.Empty);});
            _keyboardManager.SubscribeForKeyDown(Keys.F, _cpu.Fetch);
            _keyboardManager.SubscribeForKeyDown(Keys.E, () =>
            {
                var op = _cpu.GetOp(_cpu.Instruction);
                op();
            });
        }

        public void Update()
        {
            DealWithKeyboardInput();
            
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
        }

        public void Draw(SpriteBatch sb, bool fullscreen)
        {
            _screenTexture.SetData(_cpu.Screen, 0, CPU.DisplayHeight * CPU.DisplayWidth );
            
            var screenHeight = _graphicsDevice.Viewport.Height;
            var screenWidth = _graphicsDevice.Viewport.Width;
            
            if (fullscreen)
            {

                var scale = screenWidth / CPU.DisplayWidth;
                sb.Draw(_screenTexture, new Vector2(0, 0), new Rectangle(0, 0, screenWidth, screenHeight),
                    Color.White, 0.0f, new Vector2(0, 0), scale, SpriteEffects.None, 0f);
            }
            else
            {
                sb.Draw(_screenTexture, new Vector2(120, 20), new Rectangle(0, 0, CPU.DisplayWidth, CPU.DisplayHeight),
                    Color.White, 0.0f, new Vector2(0, 0), 10f, SpriteEffects.None, 0f);
            }
        }
        
        private void ToggleEmulation()
        {
            _emulate = !_emulate;
        }
        
        private void DealWithKeyboardInput()
        { 
            /* ====================
             * keyboard key mapping
             * ====================
             *
             *  1 2 3 C  -> 1 2 3 4
             *  4 5 6 D  -> Q W E R
             *  7 8 9 E  -> A S D F
             *  A 0 B F  -> Z X C V
             * 
            */
            var currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.D1))
            {
                _cpu.SetKeypress(0x01, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.D1))
            {
                _cpu.SetKeypress(0x01, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.D2))
            {
                _cpu.SetKeypress(0x02, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.D2))
            {
                _cpu.SetKeypress(0x02, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.D3))
            {
                _cpu.SetKeypress(0x03, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.D3))
            {
                _cpu.SetKeypress(0x03, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.D4))
            {
                _cpu.SetKeypress(0x0C, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.D4))
            {
                _cpu.SetKeypress(0x0C, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.Q))
            {
                _cpu.SetKeypress(0x04, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.Q))
            {
                _cpu.SetKeypress(0x04, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.W))
            {
                _cpu.SetKeypress(0x05, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.W))
            {
                _cpu.SetKeypress(0x05, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.E))
            {
                _cpu.SetKeypress(0x06, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.E))
            {
                _cpu.SetKeypress(0x06, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.R))
            {
                _cpu.SetKeypress(0x0D, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.R))
            {
                _cpu.SetKeypress(0x0D, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                _cpu.SetKeypress(0x07, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.A))
            {
                _cpu.SetKeypress(0x07, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.S))
            {
                _cpu.SetKeypress(0x08, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.S))
            {
                _cpu.SetKeypress(0x08, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                _cpu.SetKeypress(0x09, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.D))
            {
                _cpu.SetKeypress(0x09, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.F))
            {
                _cpu.SetKeypress(0x0E, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.F))
            {
                _cpu.SetKeypress(0x0E, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                _cpu.SetKeypress(0x0A, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.Z))
            {
                _cpu.SetKeypress(0x0A, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.X))
            {
                _cpu.SetKeypress(0x00, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.X))
            {
                _cpu.SetKeypress(0x00, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.C))
            {
                _cpu.SetKeypress(0x0B, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.C))
            {
                _cpu.SetKeypress(0x0B, 0);
            }
            
            if (currentKeyboardState.IsKeyDown(Keys.V))
            {
                _cpu.SetKeypress(0x0F, 1);
            }
            if(currentKeyboardState.IsKeyUp(Keys.V))
            {
                _cpu.SetKeypress(0x0F, 0);
            }
        }
        
    }
}