/*
 * Copyright 2018 David Ian Steele
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
