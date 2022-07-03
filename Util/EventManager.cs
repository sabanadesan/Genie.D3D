using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.System;

namespace D3D
{
    public class EventManager
    {
        private Object _swapChainPanel;

        private BoxApp _box;

        private List<Gamepad> _myGamepads;
        private Gamepad _mainGamepad;
        private double _width;
        private double _height;

        private Action CloseApp;

        private bool _xbox = false;

        public Gamepad MainGamepad
        {
            get { return _mainGamepad; }
            set { _mainGamepad = value; }
        }

        public List<Gamepad> MyGamepads
        {
            get { return _myGamepads; }
        }

        public bool XBOX
        {
            get { return _xbox; }
        }

        public EventManager()
        {
            
        }

        public void Init(System.Object swapChainPanel, Rect bounds, Action closeApp)
        {
            _swapChainPanel = swapChainPanel;

            _myGamepads = new List<Gamepad>();

            CloseApp = closeApp;

            _box = new BoxApp(_swapChainPanel);

            UI ui = Service.Resolve<UI>();

            _box.FpsTextBox = ui.FpsTextBox;
            _box.CountTextBox = ui.CountTextBox;

            _box.Initialize();
            Resize(bounds);

            Action<CancellationTokenSource> myAction = delegate (CancellationTokenSource token) { Do(token); };

            Process p = new Process("d3d");
            p.Run(myAction);
        }

        private void Do(CancellationTokenSource token)
        {
            bool isDispose = _box.Run(new Action(GamepadMoved), token);

            if (isDispose)
            {
                _box.Dispose();
                CloseApp();
            }
        }

        public void Resize(Rect bounds)
        {
            //Get the current Windows Size
            _height = bounds.Height - 170;
            _width = bounds.Width;

            _box.Resize(_height, _width);
        }

        public void Go_Click()
        {
            _box.Count = _box.Count + 1;
        }

        public void OnBaseMouseMove(Mouse mouse, double x, double y)
        {
            _box.OnBaseMouseMove(mouse, (int)x, (int)y);
        }

        public void OnBaseKeys(VirtualKey key, bool isDown)
        {
            Keys keyCode = Keys.None;

            switch (key)
            {
                case VirtualKey.Escape:
                    keyCode = Keys.Escape;
                    break;
                case VirtualKey.Control:
                    keyCode = Keys.ControlKey;
                    break;
            }

            if (keyCode != Keys.None)
            {
                if (isDown)
                    _box.OnBaseKeyDown(keyCode);
            }
            else
            {
                _box.OnBaseKeyUp(keyCode);
            }
        }

        private void GamepadMoved()
        {
            if (_xbox && _mainGamepad != null)
            {
                GamepadReading reading = _mainGamepad.GetCurrentReading();
                double x = reading.LeftThumbstickX * _width;
                double y = reading.LeftThumbstickY * _height;

                Mouse mouse = new Mouse();

                if (GamepadButtons.X == (reading.Buttons & GamepadButtons.X))
                {
                    VirtualKey key = VirtualKey.Control;
                    OnBaseKeys(key, true);
                }

                if (GamepadButtons.None == (reading.Buttons & GamepadButtons.X))
                {
                    VirtualKey key = VirtualKey.Control;
                    OnBaseKeys(key, false);
                }

                if (GamepadButtons.Y == (reading.Buttons & GamepadButtons.Y))
                {
                    VirtualKey key = VirtualKey.Escape;
                    OnBaseKeys(key, true);
                }

                if (GamepadButtons.None == (reading.Buttons & GamepadButtons.Y))
                {
                    VirtualKey key = VirtualKey.Escape;
                    OnBaseKeys(key, false);
                }

                _box.OnBaseMouseMove(mouse, (int)x, (int)y);
            }
        }

    }
}
