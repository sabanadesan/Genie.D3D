using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

using Windows.Gaming.Input;
using System.Threading;
using Windows.System;

namespace D3D
{
    public class Client
    {
        private EventManager _eventManager;

        public Gamepad MainGamepad
        {
            get { return _eventManager.MainGamepad; }
            set { _eventManager.MainGamepad = value; }
        }

        public List<Gamepad> MyGamepads
        {
            get { return _eventManager.MyGamepads; }
        }

        public bool XBOX
        {
            get { return _eventManager.XBOX; }
        }

        public Client(EventManager events = null)
        {
            if (events == null)
            {
                _eventManager = new EventManager();
            }
            else
            {
                _eventManager = events;
            }
        }

        public void Go_Click()
        {
            _eventManager.Go_Click();
        }

        public void OnBaseMouseMove(Mouse mouse, double x, double y)
        {
            _eventManager.OnBaseMouseMove(mouse, x, y);
        }

        public void OnBaseKeys(VirtualKey key, bool isDown)
        {
            _eventManager.OnBaseKeys(key, isDown);
        }

        public void Init(System.Object swapChainPanel, Rect bounds, Action closeApp)
        {
            _eventManager.Init(swapChainPanel, bounds, closeApp);
        }


        public void Resize(Rect bounds)
        {
            _eventManager.Resize(bounds);
        }
    }
}
