using System;
using Microsoft.Xna.Framework.Graphics;
using PolyChip8.UI.Drawing;

namespace PolyChip8.UI.Managers
{
    public class GameStateManager
    {
        private GameState _currentState;

        private MainMenu _mainMenu;
        
        public GameStateManager()
        {
            _mainMenu = new MainMenu();
        }

        public void Draw(SpriteBatch _sb)
        {
            switch (_currentState)
            {
                case GameState.Menu:
                    break;
                case GameState.Game:
                    break;
                case GameState.Pause:
                    break;
                case GameState.Debug:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}