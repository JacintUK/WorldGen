using System;
using System.Collections.Generic;
using OpenTK.Input;
using System.Drawing;
using OpenTK;

namespace WorldGenerator
{
    class EventHandler
    {
        List<MouseButton> pressedButtons = new List<MouseButton>();
        List<KeyHandler> keyHandlers = new List<KeyHandler>();
        World world;
        Scene scene;
        float longitude, attitude;

        public EventHandler(World world, Scene scene)
        {
            this.world = world;
            this.scene = scene;
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Left, UpdateIco));
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Middle, UpdateZoom));
            pressedButtons.Add(new MouseButton(OpenTK.Input.MouseButton.Right, UpdateCameraPos));

            keyHandlers.Add(new KeyHandler(Key.Space, RelaxTriangles));
            keyHandlers.Add(new KeyHandler(Key.D, DistortTriangles));
            keyHandlers.Add(new KeyHandler(Key.R, ResetSphere));
            keyHandlers.Add(new KeyHandler(Key.Number1, DR1));
            keyHandlers.Add(new KeyHandler(Key.C, Recolor));
            keyHandlers.Add(new KeyHandler(Key.I, InitPlates));
            keyHandlers.Add(new KeyHandler(Key.P, GrowPlates));
        }

        public void OnKeyDown(KeyboardKeyEventArgs e)
        {
            foreach (var h in keyHandlers)
            {
                h.OnKeyDown(e.Key);
            }
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
        }

        public void OnMouseUp(MouseButtonEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Up(e.Button);
            }
        }

        public void OnMouseMove(MouseMoveEventArgs e)
        {
            foreach (var button in pressedButtons)
            {
                button.Update(new Point(e.X, e.Y));
            }
        }


        private void ResetSphere(bool down)
        {
            if (down)
            {
                world.ResetSeed();
                world.Initialize();
                scene.Update();
            }
        }

        private void RelaxTriangles(bool down)
        {
            if (down)
            {
                world.RelaxTriangles();
                scene.Update();
            }
        }

        private void DistortTriangles(bool down)
        {
            if (down)
            {
                world.TweakTriangles();
                scene.Update();
            }
        }

        private void DR1(bool down)
        {
            if (down)
            {
                world.Distort();
                world.CreatePlates();
                scene.Update();
            }
        }

        private void Recolor(bool down)
        {
            if (down)
            {
                world.CreatePlates();
                scene.Update();
            }
        }

        private void InitPlates(bool down)
        {
            if (down)
            {
                world.InitPlates();
                scene.Update();
            }
        }

        private void GrowPlates(bool down)
        {
            if (down)
            {
                world.GrowPlates();
                scene.Update();
            }
        }

        void UpdateZoom(MouseButton button)
        {
            if (button.IsDown)
            {
                // drag up and down to change zoom level.
                scene.camera.ChangeFieldOfView(button.YDelta);
            }
        }

        void UpdateCameraPos(MouseButton button)
        {
            if (button.IsDown)
            {
                // drag left and right to change camera Z.
                scene.camera.ChangeZoom(button.XDelta);
            }
        }

        void UpdateIco(MouseButton button)
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
            }
        }
    }
}
