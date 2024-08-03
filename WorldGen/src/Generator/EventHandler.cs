using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

using OTKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using OpenTK.Windowing.Common;

namespace WorldGen
{
    class EventHandler
    {
        List<MouseButton> pressedButtons = new List<MouseButton>();
        public List<KeyHandler> keyHandlers = new List<KeyHandler>();
        Scene scene;
        GameWindow gameWindow;

        public bool Grabbed { get; private set; } = false;
        public List<MouseButton> Buttons { get { return pressedButtons; } }

        public EventHandler(GameWindow window, Scene scene)
        {
            this.scene = scene;
            gameWindow = window;
            pressedButtons.Add(new MouseButton(OTKMouseButton.Left, HitTest, UpdateCameraPos));
            pressedButtons.Add(new MouseButton(OTKMouseButton.Middle, null, UpdateFOV));
            pressedButtons.Add(new MouseButton(OTKMouseButton.Right, null, UpdateCameraPosZ));
        }

        public void OnKeyDown(KeyboardKeyEventArgs e)
        {
            foreach (var h in keyHandlers)
            {
                h.OnKeyDown(e.Key);
            }
            Grabbed = true; //@todo Possibly subject to hit testing
        }

        public void OnKeyUp(KeyboardKeyEventArgs e)
        {
            foreach (var h in keyHandlers)
            {
                h.OnKeyUp(e.Key);
            }
        }

        public void OnMouseDown(MouseButtonEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Down(e, gameWindow.MouseState);
            }
            Grabbed = true;
        }

        public void OnMouseUp(MouseButtonEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Up(e, gameWindow.MouseState);
            }

            bool x = false;
            foreach (var button in pressedButtons)
            {
                if (button.IsDown)
                    x = true;
            }
            Grabbed = x;
        }

        public bool OnMouseMove(MouseMoveEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Update(e);
            }
            return true;
        }

        bool UpdateFOV(MouseButton.ButtonEventArgs e)
        {
            bool handled = false;
            if (e.button.IsDown)
            {
                // drag up and down to change zoom level.
                scene.camera.ChangeFieldOfView(e.button.YDelta);
                handled = true;
            }
            return handled;
        }

        bool HitTest(MouseButton.ButtonEventArgs e)
        {
            scene.HitTest(e.button.Point);
            return true;
        }


        bool UpdateCameraPos(MouseButton.ButtonEventArgs e)
        {
            const float sensitivity = 0.25f;

            scene.Yaw += e.button.XDelta * sensitivity;
            scene.Pitch += e.button.YDelta * sensitivity;
            scene.UpdateCameraPos();
            return true;
        }

        bool UpdateCameraPosZ(MouseButton.ButtonEventArgs e)
        {
            bool handled = false;
            if (e.button.IsDown)
            {
                // drag up and down to change camera Z.
                scene.camera.DistanceFromObject += e.button.YDelta*0.05f;
                scene.UpdateCameraPos();
                handled = true;
            }
            return handled;
        }


    }
}
