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

        private BoxApp _box;

        private UIInputManager _uim;

        public EventManager()
        {
            _uim = Service.Resolve<UIInputManager>();
        }

        public void Init()
        {
            _box = Service.Resolve<BoxApp>();

            _box.Initialize();
            _uim.Resize();

            Action<CancellationTokenSource> myAction = delegate (CancellationTokenSource token) { Do(token); };

            Process p = new Process("d3d");
            p.Run(myAction);
        }

        private void Do(CancellationTokenSource token)
        {
            bool isDispose = _box.Run(new Action(_uim.GamepadMoved), token);

            if (isDispose)
            {
                _box.Dispose();
                _uim.CloseApp();
            }
        }
    }
}
