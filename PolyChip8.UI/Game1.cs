using System;
using System.Linq;
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

            _cpu.LoadROM("/home/kris/Projects/PolyChip8/ROMS/test_opcode.ch8");
            _cpu.Dissassemble();
            
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

            _emulate = false;
            
            if (_emulate)
            {
                _cpu.Clock();
            }

            if (_currentKeyboardState.IsKeyDown(Keys.F) &&
                _previousKeyboardState.IsKeyUp(Keys.F))
            {
                _cpu.Fetch();
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.E) &&
                _previousKeyboardState.IsKeyUp(Keys.E))
            {
                _cpu.GetOp(_cpu.Instruction)();
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.C) &&
                _previousKeyboardState.IsKeyUp(Keys.C))
            {
                _cpu.Clock();
            }

            if (_cpu.DelayTimer >= 0)
                _cpu.DelayTimer--;
            if (_cpu.SoundTimer >= 0)
                _cpu.SoundTimer--;
            
            _previousKeyboardState = _currentKeyboardState;

            base.Update(gameTime);
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.MidnightBlue);
            GraphicsDevice.Textures[0] = null;

            _screenTexture.SetData(_cpu.Screen, 0, _cpu.DisplayHeight * _cpu.DisplayWidth );
            
            _spriteBatch.Begin();
            
            _spriteBatch.Draw(_screenTexture, new Vector2(120, 20), new Rectangle(0,0, _cpu.DisplayWidth, _cpu.DisplayHeight),
                Color.White, 0.0f, new Vector2(0,0), 10f, SpriteEffects.None, 0f);
            
            DrawRegisters();
            
            _spriteBatch.End();
            

            base.Draw(gameTime);
        }

        private void DrawRegisters()
        {
            var baseX = _cpu.DisplayWidth + 700;
            var baseY = 70;
            
            _spriteBatch.DrawString(_systemFont, $"PC: $0x{_cpu.ProgramCounter:X4}", new Vector2(baseX + 64, baseY + 25), Color.White);
            _spriteBatch.DrawString(_systemFont, $"Stack P: $0x{_cpu.StackPointer:X2}", new Vector2(baseX + 64, baseY + 45), Color.White);
            _spriteBatch.DrawString(_systemFont, $"I: $0x{_cpu.AddressRegister:X4} ", new Vector2(baseX + 64, baseY + 65), Color.White);
            
            _spriteBatch.DrawString(_systemFont, $"X: $0x{_cpu.X:X2} ", new Vector2(baseX + 170, baseY + 65), Color.White);
            _spriteBatch.DrawString(_systemFont, $"Y: $0x{_cpu.Y:X2} ", new Vector2(baseX + 260, baseY + 65), Color.White);
            _spriteBatch.DrawString(_systemFont, $"N: $0x{_cpu.N:X2} ", new Vector2(baseX + 350, baseY + 65), Color.White);
            _spriteBatch.DrawString(_systemFont, $"NN: $0x{_cpu.Nn:X2} ", new Vector2(baseX + 450, baseY + 65), Color.White);
            _spriteBatch.DrawString(_systemFont, $"NNN: $0x{_cpu.Nnn:X4} ", new Vector2(baseX + 550, baseY + 65), Color.White);

            for (int i = 0x0; i < 0x10; i++)
            {
                var y = (baseY + 175) + i * 20;
                _spriteBatch.DrawString(_systemFont, $"V{i}: ${_cpu.VRegisters[i]:X2} ", new Vector2(baseX + 64, y), Color.White);
            }

            _spriteBatch.DrawString(_systemFont, $"Dissassembly:",
                new Vector2(baseX + 250, baseY + 150), Color.White);

            DrawDisassembly();
            DrawRam();
        }

        private void DrawDisassembly()
        {
            var y = 220;
            var x = _cpu.DisplayWidth + 900;
            
            var programCounterHex = _cpu.ProgramCounter.ToString("X4");
            var instruction = _cpu.Dissassembly.FirstOrDefault(x => x.Contains(programCounterHex, StringComparison.OrdinalIgnoreCase));

            if (instruction != null)
            {
                var instructionIndex = _cpu.Dissassembly.IndexOf(instruction);

                if (instructionIndex < 26)
                {
                    var dissPrint = _cpu.Dissassembly.Take(26);

                    foreach (var line in dissPrint)
                    {
                        var color = Color.White;

                        if (line == instruction)
                            color = Color.Cyan;
                        
                        y += 20;
                        PrintDisassemblyLine(line, x, y, color);
                    }
                }
                else
                {
                    var halfLines = 26 / 2;

                    var dissBefore = _cpu.Dissassembly.Skip(instructionIndex - halfLines).Take(halfLines);
                    var dissAfter = _cpu.Dissassembly.Skip(instructionIndex + 1).Take(halfLines);

                    foreach (var line in dissBefore)
                    {
                        y += 20;
                        PrintDisassemblyLine(line, x, y, Color.White);
                    }

                    y += 20;
                    
                    PrintDisassemblyLine(instruction, x, y, Color.Cyan);
                    
                    foreach (var line in dissAfter)
                    {
                        y += 20;
                        PrintDisassemblyLine(line, x, y, Color.White);
                    }
                }
            }
        }
        
        private void PrintDisassemblyLine(string line, int x, int y, Color color)
        {
            _spriteBatch.DrawString(_systemFont, line, new Vector2(x , y), color);
        }

        private void DrawRam()
        {
            int y = 28;
            int count = 0;
            for (int row = 0; row < 128; row++)
            {
                if (count >= _cpu.ProgramSize)
                    break;
                
                for (int col = 0; col < 28; col++)
                {
                    int index = 0x0200 + row *  y + col;

                    var lineColor = Color.White;
                    
                    if(index == _cpu.ProgramCounter || index == _cpu.ProgramCounter+1)
                        lineColor = Color.Red;
                    
                    _spriteBatch.DrawString(_systemFont, _cpu.Ram[index].ToString("X2"), new Vector2(1200 + (col * 25) , 10 + (row * 15)), lineColor
                    , 0.0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0);

                    count++;
                }
                
            }
        }
    }
}