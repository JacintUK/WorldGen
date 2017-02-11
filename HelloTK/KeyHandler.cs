using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;

namespace HelloTK
{
    class KeyHandler
    {
        public delegate void Handler(bool down);

        private bool keyDown;
        private Key key;
        private Handler h;
        public KeyHandler( Key key, Handler h)
        {
            this.key = key;
            this.h = h;
        }
        public void OnKeyDown(Key k)
        {
            if( keyDown == false && k == key)
            {
                keyDown = true;
                h(true);
            }
        }
        public void OnKeyUp(Key k)
        {
            if( k == key )
            {
                keyDown = false;
                h(false);
            }
        }
    }
}
