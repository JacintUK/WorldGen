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

using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Net;

namespace WorldGen
{
    class MouseButton
    {
        Vector2i point;
        Vector2i downPoint;
        OpenTK.Windowing.GraphicsLibraryFramework.MouseButton button;
        bool buttonDown = false;
        bool buttonDownTrigger = false;
        public class ButtonEventArgs : EventArgs { public MouseButton button { get; set; } };

        public delegate bool TapEvent(ButtonEventArgs button);
        public delegate bool UpdateEvent(ButtonEventArgs button);
        public event TapEvent tapEvent;
        public event UpdateEvent updateEvent;

        public float XDelta { get; private set; }
        public float YDelta { get; private set; }
        public Vector2i Point { get { return point; } }
        public bool IsDown { get { return buttonDown; } }

        public MouseButton(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton button, TapEvent downDelegate, UpdateEvent updateDelegate)
        {
            this.button = button;
            if (downDelegate != null)
            {
                tapEvent += downDelegate;
            }
            if (updateDelegate != null)
            {
                updateEvent += updateDelegate;
            }
        }

        public void Down(MouseButtonEventArgs e, MouseState state)
        {
            if (e.Button == button)
            {
                if (!buttonDown)
                    buttonDownTrigger = true;
                XDelta = 0;
                YDelta = 0;

                downPoint = new Vector2i((int)state.Position.X, (int)state.Position.Y);
                buttonDown = true;
             }
        }

        public void Up(MouseButtonEventArgs e, MouseState state)
        {
            if (e.Button == button)
            {
                buttonDown = false;
                buttonDownTrigger = false;
                var currentPoint = new Vector2i((int)state.Position.X, (int)state.Position.Y);

                if (currentPoint == downPoint)
                {
                    point = currentPoint;
                    tapEvent?.Invoke(new ButtonEventArgs() { button = this });
                }
            }
        }

        public bool Update(MouseMoveEventArgs e)
        {
            bool handled = false;
            if (buttonDown)
            {
                if (buttonDownTrigger)
                {
                    buttonDownTrigger = false;
                }
                XDelta = e.DeltaX;
                YDelta = e.DeltaY;
                point = (Vector2i)e.Position;
                updateEvent?.Invoke(new ButtonEventArgs() { button = this });
            }
            return handled;
        }
    }
}
