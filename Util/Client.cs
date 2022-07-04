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

        public void Init()
        {
            _eventManager.Init();
        }
    }
}
