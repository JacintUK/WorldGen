using System;
using System.Collections.Generic;
using OpenTK.Input;
using OpenTK;
using OpenTK.Mathematics;

using OTKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using OpenTK.Windowing.Common;

namespace WorldGen
{
    class EventHandler
    {
        List<MouseButton> pressedButtons = new List<MouseButton>();
        public List<KeyHandler> keyHandlers = new List<KeyHandler>();
        World world;
        Scene scene;
        float longitude, attitude;
        public bool Grabbed { get; private set; } = false;

        public EventHandler(World world, Scene scene)
        {
            this.world = world;
            this.scene = scene;
            pressedButtons.Add(new MouseButton(OTKMouseButton.Left, UpdateIco));
            pressedButtons.Add(new MouseButton(OTKMouseButton.Middle, UpdateZoom));
            pressedButtons.Add(new MouseButton(OTKMouseButton.Right, UpdateCameraPos));
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

        bool UpdateZoom(MouseButton button)
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
            if (button.IsDown)
            {
                // drag left and right to change camera Z.
                scene.camera.ChangeZoom(button.XDelta);
                handled = true;
            }
            return handled;
        }

        bool UpdateIco(MouseButton button)
        {
            if (button.IsDown)
            {
                // Actually, what you really want to do, is to unproject the mouse pointer onto the sphere, 
                // before and after movement, then slerp to new rotation.

                // Rotate around Y axis
                longitude += button.XDelta / 4.0f;
                longitude %= 360;

                attitude += button.YDelta / 2.0f;
                attitude = Math.Max(Math.Min(90, attitude), -90);

                Quaternion equatorRot = Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI * longitude / 180.0f);
                Quaternion polarAxisRot = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI * attitude / 180.0f);
                Quaternion rotation = equatorRot * polarAxisRot;
                Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);
                var rootNode = scene.GetRootNode();
                Matrix4 tr = Matrix4.CreateTranslation(rootNode.Position);
                rootNode.Model = rotationMatrix * tr;
                return true;
            }
            return false;
        }
    }
}
