using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PolyChip8.UI.Managers;

namespace PolyChip8.UI.Drawing
{
    public class MainMenuView
    {
        private readonly SpriteFont _systemFont;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly GameStateManager _stateManager;
        private readonly IList<string> _roms;

        private int _selectorIndex;

        public MainMenuView(GameStateManager stateManager, SpriteFont font, KeyboardManager keyboardManager, GraphicsDevice gd)
        {
            _systemFont = font;
            _graphicsDevice = gd;
            _selectorIndex = 20;
            _stateManager = stateManager;
#if DEBUG
            var romsDir = "../../../../ROMS/";
#endif
            
            if (Directory.Exists(romsDir))
            {
                _roms = Directory.GetFiles(romsDir);
            }

            keyboardManager.SubscribeForKeyDown(Keys.Down, MoveSelectorDown);
            keyboardManager.SubscribeForKeyUp(Keys.Up, MoveSelectorUp);
            keyboardManager.SubscribeForKeyDown(Keys.Enter, GameSelect);
        }
        
        public void Update()
        {
            
        }

        public void Draw(SpriteBatch sb)
        {
            var windowWidth = _graphicsDevice.Viewport.Width;

            var startY = 20;
            foreach (var rom in _roms)
            {
                var fileName = Path.GetFileName(rom);
                sb.DrawString(_systemFont, $"{fileName}", new Vector2(windowWidth / 2 - rom.Length, startY),
                    Color.White);
                startY += 20;
            }
            
            sb.DrawString(_systemFont, $">>>", new Vector2(windowWidth / 2 - 100, _selectorIndex),
                Color.White);

            DrawLegend(sb);
        }

        private void DrawLegend(SpriteBatch sb)
        {
            sb.DrawString(_systemFont, $"Up    Arrow : Move Selector Up", new Vector2(100, 100), Color.White);
            sb.DrawString(_systemFont, $"Down  Arrow : Move Selector Down", new Vector2(100, 120), Color.White);
            sb.DrawString(_systemFont, $"Enter       : Select Game", new Vector2(100, 140), Color.White);
            sb.DrawString(_systemFont, $"Backspace   : Return to Menu", new Vector2(100, 180), Color.White);
            sb.DrawString(_systemFont, $"F1          : Debug Mode", new Vector2(100, 210), Color.White);
            sb.DrawString(_systemFont, $"F5          : Toggle FPS Limit", new Vector2(100, 250), Color.White);
            
        }

        private void MoveSelectorDown()
        {
            var gameSelectorIndex = (_selectorIndex / 20);
            
            if(gameSelectorIndex < _roms.Count())
                _selectorIndex += 20;
        }

        private void MoveSelectorUp()
        {
            var gameSelectorIndex = (_selectorIndex / 20);
            
            if(gameSelectorIndex > 1)
                _selectorIndex -= 20;
        }

        private void GameSelect()
        {
            var gameSelectorIndex = (_selectorIndex / 20);
            var selectedRom = _roms[gameSelectorIndex - 1];
            _stateManager.ChangeState(GameState.LoadRom, selectedRom);
        }
    }
}