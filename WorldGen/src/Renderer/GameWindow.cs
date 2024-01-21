/*
 * Copyright 2022 David Ian Steele
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
using OpenTK.Graphics.OpenGL;
using ImGuiNET;
using System.IO;
using OpenTK.Mathematics;
using System.Drawing;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WorldGen
{
    internal class Colour
    {
        Color4 _color;
        public Color4 Color { get { return _color; }  set { _color = value; } }

        public Colour(System.Drawing.Color c)
        {
            _color.R = c.R;
            _color.G = c.G;
            _color.B = c.B;
            _color.A = c.A;
        }
    }
    internal class GameWindow : OpenTK.Windowing.Desktop.GameWindow
    {
        public EventHandler EventHandler { get; set; }

        Scene scene;
        Colour backgroundColor = new Colour(Color.Aquamarine);
        public const string SHADER_PATH = "Resources/Shaders/";
        public const string IMAGE_PATH = "Resources/Images/";

        public bool IsAlive = false;

        private ImGuiController imGuiController;

        public GameWindow(int w, int h)
            : base(new GameWindowSettings { UpdateFrequency=60, RenderFrequency=60, IsMultiThreaded=false },
                   new NativeWindowSettings { Title = "WorldGen", Size = new Vector2i(w, h), API= ContextAPI.OpenGL, Flags = ContextFlags.ForwardCompatible })
        {
            Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
        }

        public unsafe void Start()
        {
            if (!File.Exists("imgui.ini"))
                File.WriteAllText("imgui.ini", "");

            Run();
        }

        public class SceneCreatedEventArgs : EventArgs
        {
            public Scene scene;
            public SceneCreatedEventArgs(Scene s)
            {
                scene = s;
            }
        }

        public event EventHandler<SceneCreatedEventArgs> SceneCreatedEvent;
        private void RaiseSceneCreatedEvent(Scene s)
        {
            SceneCreatedEvent?.Invoke(this, new SceneCreatedEventArgs(scene));
        }

        public event EventHandler<EventArgs> ImGuiRenderEvent;
        private void RaiseImGuiRenderEvent()
        {
            ImGuiRenderEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler<MouseButtonEventArgs> ImGuiMouseUpEvent;
        private void RaiseImGuiMouseUpEvent(MouseButtonEventArgs e)
        {
            ImGuiMouseUpEvent?.Invoke(this, e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            // Which thread is this on?
            InitializeGL();
            imGuiController = new ImGuiController(new Vector2i(ClientSize.X, ClientSize.Y));
            imGuiController.Initialize();
            scene = new Scene(ClientSize.X, ClientSize.Y);
            OnUpdateFrame(new FrameEventArgs());
            RaiseSceneCreatedEvent(scene);
        }
        
        private void InitializeGL()
        {
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            GL.ClearColor(backgroundColor.Color);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.VertexProgramPointSize);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            imGuiController.Update(this, (float)e.Time);
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(backgroundColor.Color);
            GL.ClearDepth(1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            scene.Render();

            imGuiController.FrameStart((float)e.Time);
            RaiseImGuiRenderEvent();
            imGuiController.Render();

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            scene.Update(ClientSize.X, ClientSize.Y);
            imGuiController.WindowResized(ClientSize.X, ClientSize.Y);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!ImGui.GetIO().WantCaptureKeyboard)
            {
                if (e.Key == Keys.Escape)
                {
                    base.Close();
                }
                EventHandler.OnKeyDown(e);
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!ImGui.GetIO().WantCaptureKeyboard)
                EventHandler.OnKeyUp(e);
        }


        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if(ImGui.GetIO().WantCaptureMouse && !EventHandler.Grabbed)
                imGuiController.OnMouseDown(e);
            else
                EventHandler.OnMouseDown(e);
        }
 
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (ImGui.GetIO().WantCaptureMouse && !EventHandler.Grabbed)
            {
                imGuiController.OnMouseUp(e);
                RaiseImGuiMouseUpEvent(e);
            }
            else
            {
                EventHandler.OnMouseUp(e);
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
             if (!ImGui.GetIO().WantCaptureMouse || EventHandler.Grabbed )
                EventHandler.OnMouseMove(e);
        }
    }
}

