using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3D
{
    public class InputManager
    {
        private Dictionary<Keys, bool> keyMap;

        public InputManager()
        {
            keyMap = new Dictionary<Keys, bool>();
        }

        public void KeyDown(Keys keys)
        {
            keyMap[keys] = true;
        }

        public void KeyUp(Keys keys)
        {
            keyMap[keys] = false;
        }

        public bool isKeyDown(Keys keys)
        {
            bool found = false;

            if (keyMap.ContainsKey(keys))
            {
                found = keyMap[keys];
            }

            return found;
        }
    }
}
