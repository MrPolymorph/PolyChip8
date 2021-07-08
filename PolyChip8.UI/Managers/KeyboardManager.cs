using Microsoft.Xna.Framework.Input;

namespace PolyChip8.UI.Managers
{
    public class KeyboardManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        
        public void Update(KeyboardState keyboardState, CPU cpu, bool emulationPaused = false)
        {
            _currentKeyboardState = Keyboard.GetState();

            if (emulationPaused)
            {
                if (_currentKeyboardState.IsKeyDown(Keys.F) &&
                    _previousKeyboardState.IsKeyUp(Keys.F))
                {
                    cpu.Fetch();
                }

                if (_currentKeyboardState.IsKeyDown(Keys.E) &&
                    _previousKeyboardState.IsKeyUp(Keys.E))
                {
                    cpu.GetOp(cpu.Instruction)();
                }

                if (_currentKeyboardState.IsKeyDown(Keys.C) &&
                    _previousKeyboardState.IsKeyUp(Keys.C))
                {
                    cpu.Clock();
                }
            }
            
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
            if (_currentKeyboardState.IsKeyDown(Keys.D1))
            {
                cpu.SetKeypress(0x01, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.D1))
            {
                cpu.SetKeypress(0x01, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.D2))
            {
                cpu.SetKeypress(0x02, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.D2))
            {
                cpu.SetKeypress(0x02, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.D3))
            {
                cpu.SetKeypress(0x03, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.D3))
            {
                cpu.SetKeypress(0x03, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.D4))
            {
                cpu.SetKeypress(0x0C, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.D4))
            {
                cpu.SetKeypress(0x0C, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.Q))
            {
                cpu.SetKeypress(0x04, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.Q))
            {
                cpu.SetKeypress(0x04, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.W))
            {
                cpu.SetKeypress(0x05, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.W))
            {
                cpu.SetKeypress(0x05, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.E))
            {
                cpu.SetKeypress(0x06, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.E))
            {
                cpu.SetKeypress(0x06, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.R))
            {
                cpu.SetKeypress(0x0D, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.R))
            {
                cpu.SetKeypress(0x0D, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.A))
            {
                cpu.SetKeypress(0x07, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.A))
            {
                cpu.SetKeypress(0x07, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.S))
            {
                cpu.SetKeypress(0x08, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.S))
            {
                cpu.SetKeypress(0x08, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.D))
            {
                cpu.SetKeypress(0x09, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.D))
            {
                cpu.SetKeypress(0x09, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.F))
            {
                cpu.SetKeypress(0x0E, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.F))
            {
                cpu.SetKeypress(0x0E, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.Z))
            {
                cpu.SetKeypress(0x0A, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.Z))
            {
                cpu.SetKeypress(0x0A, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.X))
            {
                cpu.SetKeypress(0x00, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.X))
            {
                cpu.SetKeypress(0x00, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.C))
            {
                cpu.SetKeypress(0x0B, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.C))
            {
                cpu.SetKeypress(0x0B, 0);
            }
            
            if (_currentKeyboardState.IsKeyDown(Keys.V))
            {
                cpu.SetKeypress(0x0F, 1);
            }
            if(_currentKeyboardState.IsKeyUp(Keys.V))
            {
                cpu.SetKeypress(0x0F, 0);
            }

            _previousKeyboardState = _currentKeyboardState;
        }
    }
}