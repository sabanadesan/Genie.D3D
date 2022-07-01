using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3D
{
    public struct Mouse
    {
        public bool left;
        public bool mid;
        public bool right;

        public Mouse(bool left, bool mid, bool right)
        {
            this.left = left;
            this.mid = mid;
            this.right = right;
        }
    }
}
