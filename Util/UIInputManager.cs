using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Input;

namespace D3D
{
    public class UIInputManager
    {
        private List<Gamepad> _myGamepads;
        private Gamepad _mainGamepad;
        private double _width;
        private double _height;

        private bool _xbox = false;

        private BoxApp _box;

        private Action _closeApp;

        private Rect _bounds;

        public Rect Bounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }


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

        public Action CloseApp
        {
            get { return _closeApp;  }
            set { _closeApp = value; }
        }

        public UIInputManager()
        {
            _myGamepads = new List<Gamepad>();
            _box = Service.Resolve<BoxApp>();
        }

        public void GamepadRemoved(Gamepad e)
        {
            int indexRemoved = MyGamepads.IndexOf(e);

            if (indexRemoved > -1)
            {
                if (MainGamepad == MyGamepads[indexRemoved])
                {
                    MainGamepad = null;
                }

                MyGamepads.RemoveAt(indexRemoved);
            }
        }

        public void GamepadAdded(Gamepad e)
        {
            MyGamepads.Add(e);
            MainGamepad = MyGamepads[0];
        }

        public void PointerMoved(PointerPoint point)
        {
            double x = point.Position.X;
            double y = point.Position.Y;

            Mouse mouse = new Mouse();

            if (point.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {

                if (point.Properties.IsLeftButtonPressed)
                {
                    mouse.left = true;
                }
                if (point.Properties.IsMiddleButtonPressed)
                {
                    mouse.mid = true;
                }
                if (point.Properties.IsRightButtonPressed)
                {
                    mouse.right = true;
                }
            }

            OnBaseMouseMove(mouse, x, y);
        }

        public void Resize()
        {
            //Get the current Windows Size
            _height = _bounds.Height - 170;
            _width = _bounds.Width;

            _box.Resize(_height, _width);
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

        public void GamepadMoved()
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

        public void OnBaseMouseMove(Mouse mouse, double x, double y)
        {
            _box.OnBaseMouseMove(mouse, (int)x, (int)y);
        }
    }
}
