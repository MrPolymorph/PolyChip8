using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyChip8.UI.Drawing;

namespace PolyChip8.UI.Managers
{
    public class GameStateManager
    {
        private readonly CPU _cpu;
        private GameState _currentState;

        private KeyboardManager _keyboardManager;
        private MainMenuView _mainMenuView;
        private DebugOutputView _debugOutputView;
        private GameView _gameView;

        private float _fps;
        
        public GameStateManager(GraphicsDevice gd, SpriteFont font, CPU cpu)
        {
            _cpu = cpu;
            _keyboardManager = new KeyboardManager();
            _mainMenuView = new MainMenuView(this, font, _keyboardManager, gd);
            _debugOutputView = new DebugOutputView(font, cpu);
            _gameView = new GameView(this, gd, _keyboardManager, _cpu);
            _fps = 0;
            _currentState = GameState.Menu;
        }

        public void Update(GameTime gameTime)
        {
            _fps = (float) (1 / gameTime.ElapsedGameTime.TotalSeconds);
            
            _keyboardManager.Update();

            switch (_currentState)
            {
                case GameState.Menu:
                    break;
                case GameState.Running:
                case GameState.Debug:
                    _gameView.Update();
                    break;
                case GameState.Pause:
                    break;
                
                case GameState.LoadRom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeState(GameState state, string rom)
        {
            var previous = _currentState;
            _currentState = state;

            if (_currentState == GameState.LoadRom)
            {
                _cpu.Reset();
                _cpu.LoadROM(rom);
                _currentState = GameState.Running;
            }

            if (previous != _currentState && _currentState == GameState.Debug)
            {
                _cpu.Reset();
                _cpu.Disassemble();
            }
        }

        public void Draw(SpriteBatch sb)
        {
            switch (_currentState)
            {
                case GameState.Menu:
                    _mainMenuView.Draw(sb);
                    break;
                case GameState.Running:
                    _gameView.Draw(sb, true);
                    break;
                case GameState.Pause:
                    break;
                case GameState.Debug:
                    _gameView.Draw(sb, false);
                    _debugOutputView.DrawAll(sb, _fps);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}