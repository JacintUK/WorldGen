using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGenerator
{
    class MouseButton
    {
        Point oldPoint;
        OpenTK.Input.MouseButton button;
        bool buttonDown = false;
        bool buttonDownTrigger = false;
        int xDelta;
        int yDelta;
        public delegate void UpdateDelegate(MouseButton button);
        UpdateDelegate handler;
        public int XDelta { get { return xDelta; } }
        public int YDelta { get { return yDelta; } }
        public bool IsDown { get { return buttonDown; } }

        public MouseButton(OpenTK.Input.MouseButton button, UpdateDelegate handler)
        {
            this.button = button;
            this.handler = handler;
        }

        public void Down(OpenTK.Input.MouseButton button)
        {
            if (button == this.button)
            {
                if (!buttonDown)
                    buttonDownTrigger = true;
                buttonDown = true;
            }
        }

        public void Up(OpenTK.Input.MouseButton button)
        {
            if (button == this.button)
            {
                buttonDown = false;
                buttonDownTrigger = false;
            }
        }

        public void Update(Point mousePos)
        {
            if (buttonDown)
            {
                if (buttonDownTrigger)
                {
                    oldPoint = mousePos;
                    buttonDownTrigger = false;
                }
                xDelta = mousePos.X - oldPoint.X;
                yDelta = mousePos.Y - oldPoint.Y;
                oldPoint = mousePos;
                handler(this);
            }
        }
    }
}
