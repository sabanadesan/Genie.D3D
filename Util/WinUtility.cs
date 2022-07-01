using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace D3D
{
    public class WinUtility
    {

        public WinUtility()
        {

        }

        public void OnUiThread(Action action)
        {
            Task m_t = m_t = new Task(action);
            m_t.Start();

            //IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync((workItem) => action());
        }
    }
}
