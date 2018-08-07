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
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ImGuiNET;

namespace WorldGenerator
{
    class GameWindow : OpenTK.GameWindow
    {
        EventHandler eventHandler;
        Scene scene;
        World world;
        Color backgroundColor = Color.Aquamarine;

        public GameWindow(int w, int h)
           : base(w, h, new OpenTK.Graphics.GraphicsMode(32, 8, 0, 0), 
                 "WorldGen",
                  GameWindowFlags.Default, DisplayDevice.Default,
                  // ask for an OpenGL 3.0 forward compatible context
                   3, 0, GraphicsContextFlags.ForwardCompatible)
        {

            Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeGL();
            
            world = new World();
            scene = new Scene(world, Width, Height);
            eventHandler = new EventHandler(world, scene);
        }


        //private bool _mainWindowOpened;
        void DrawGUI()
        {
            //ImGui.BeginWindow("ImGUI.NET Sample Program", ref _mainWindowOpened, WindowFlags.NoResize | WindowFlags.NoTitleBar | WindowFlags.NoMove);
            //ImGui.Text("Hello,");
            //ImGui.EndWindow();
        }



        private void InitializeGL()
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            GL.ClearColor(System.Drawing.Color.Aquamarine);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.VertexProgramPointSize);
        }



        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Coords relative to drawing frame
            // var mousePos = base.PointToClient(new Point(Mouse.X, Mouse.Y));
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.ClearColor(backgroundColor);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            scene.Render();
            DrawGUI();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            scene.Update(Width, Height);
        }


        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                base.Exit();
            }
            eventHandler.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            eventHandler.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            eventHandler.OnMouseDown(e);
        }
 
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            eventHandler.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            eventHandler.OnMouseMove(e);
        }
    }
}
