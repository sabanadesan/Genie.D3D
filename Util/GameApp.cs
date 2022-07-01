using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;

namespace D3D
{
    public class GameApp : D3DApp
    {
        protected InputManager _inputManager;
        protected Log _log;

        public GameApp(System.Object swapChainPanel) : base(swapChainPanel)
        {
            _inputManager = new InputManager();
            _log = new Log();
        }

        public void OnBaseKeyDown(Keys keyCode)
        {
            _inputManager.KeyDown(keyCode);
            KeyEvent(keyCode);
        }

        public void OnBaseKeyUp(Keys keyCode)
        {
            _inputManager.KeyUp(keyCode);
        }

        public bool IsKeyDown(Keys keys)
        {
            return _inputManager.isKeyDown(keys);
        }

        public void OnBaseMouseMove(Mouse mouse, int x, int y)
        {
            OnMouseMove(mouse, new Point(x, y));
        }

        protected virtual void OnMouseMove(Mouse mouse, Point location)
        {

        }

        public void KeyEvent(Keys keys)
        {
            switch (keys)
            {
                case Keys.Escape:
                    _running = false;
                    CancellationTokenSource.Cancel();
                    break;
                case Keys.F2:
                    M4xMsaaState = !M4xMsaaState;
                    break;
            }
        }
    }
}
