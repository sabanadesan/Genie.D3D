using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace D3D
{
    public static class UI
    {
        private static TextBox _fpstext;
        private static TextBox _counttext;

        public static TextBox FpsTextBox
        {
            get
            {
                return _fpstext;
            }
            set
            {
                _fpstext = value;
            }
        }
        public static TextBox CountTextBox
        {
            get { return _counttext; }
            set
            {
                _counttext = value;
            }
        }
    }
}
