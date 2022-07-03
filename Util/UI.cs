using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace D3D
{
    public class UI
    {
        private TextBox _fpstext;
        private TextBox _counttext;

        public UI()
        {

        }

        public TextBox FpsTextBox
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
        public TextBox CountTextBox
        {
            get { return _counttext; }
            set
            {
                _counttext = value;
            }
        }
    }
}
