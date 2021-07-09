using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace PolyChip8.UI.Managers
{
    public class KeyboardManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        private Dictionary<Keys, List<Action>> _keyDownObservers;
        private Dictionary<Keys, List<Action>> _keyUpObservers;

        public KeyboardManager()
        {
            _keyDownObservers = new Dictionary<Keys, List<Action>>();
            _keyUpObservers = new Dictionary<Keys, List<Action>>();
        }
        
        public void Update()
        {
            _currentKeyboardState = Keyboard.GetState();

            foreach (var kvp in _keyDownObservers)
            {
                var key = kvp.Key;

                if (_currentKeyboardState.IsKeyDown(key) &&
                    _previousKeyboardState.IsKeyUp(key))
                {
                    foreach (var callback in kvp.Value)
                    {
                        callback();
                    }
                }
            }
            
            foreach (var kvp in _keyUpObservers)
            {
                var key = kvp.Key;

                if (_currentKeyboardState.IsKeyUp(key) &&
                    _previousKeyboardState.IsKeyDown(key))
                {
                    foreach (var callback in kvp.Value)
                    {
                        callback();
                    }
                }
            }
            


            _previousKeyboardState = _currentKeyboardState;
        }

        public void SubscribeForKeyDown(Keys key, Action callback)
        {
            if(!_keyDownObservers.ContainsKey(key))
                _keyDownObservers.Add(key, new List<Action>(){callback});
            else
                _keyDownObservers[key].Add(callback);
        }

        public void SubscribeForKeyUp(Keys key, Action callback)
        {
            if(!_keyUpObservers.ContainsKey(key))
                _keyUpObservers.Add(key, new List<Action>(){callback});
            else
                _keyUpObservers[key].Add(callback);
        }
    }
}