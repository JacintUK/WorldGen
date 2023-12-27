using System;
using System.Collections.Generic;
using OpenTK.Input;
using OpenTK;
using OpenTK.Mathematics;

using OTKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using OpenTK.Windowing.Common;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace WorldGen
{
    class EventHandler
    {
        List<MouseButton> pressedButtons = new List<MouseButton>();
        public List<KeyHandler> keyHandlers = new List<KeyHandler>();
        Scene scene;
        float longitude = 0.0f;
        float attitude = 0.0f;
        public bool Grabbed { get; private set; } = false;

        public EventHandler(Scene scene)
        {
            this.scene = scene;
            pressedButtons.Add(new MouseButton(OTKMouseButton.Left, UpdateCameraPos));
            pressedButtons.Add(new MouseButton(OTKMouseButton.Middle, UpdateFOV));
            pressedButtons.Add(new MouseButton(OTKMouseButton.Right, UpdateCameraPosZ));
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
                button.Down(e.Button);
            }
            Grabbed = true;
        }

        public void OnMouseUp(MouseButtonEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Up(e.Button);
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
                button.Update(new Vector2i((int)e.X, (int)e.Y));
            }
            return true;
        }

        bool UpdateFOV(MouseButton button)
        {
            bool handled = false;
            if (button.IsDown)
            {
                // drag up and down to change zoom level.
                scene.camera.ChangeFieldOfView(button.YDelta);
                handled = true;
            }
            return handled;
        }

        bool UpdateCameraPos(MouseButton button)
        {
            bool handled = false;
            const float sensitivity = 0.25f;
            if (button.IsDown)
            {
                scene.Yaw += button.XDelta * sensitivity;
                scene.Pitch += button.YDelta * sensitivity;
                scene.UpdateCameraPos();
                handled = true;
            }
            return handled;
        }

        bool UpdateCameraPosZ(MouseButton button)
        {
            bool handled = false;
            if (button.IsDown)
            {
                // drag up and down to change camera Z.
                scene.camera.DistanceFromObject += button.YDelta*0.05f;
                scene.UpdateCameraPos();
                handled = true;
            }
            return handled;
        }


    }
}
