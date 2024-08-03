using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using TKEventType = WorldGen.TKEvent.Type;

namespace WorldGen
{
    internal class ImGuiController
    {
        private Shader shader;
        private Vector2i size;
        private Texture fontTexture;

        public ImGuiController(Vector2i size)
        {
            this.size = size;

            shader = new Shader(GameWindow.SHADER_PATH + "Vert2DColorUVShader.glsl", GameWindow.SHADER_PATH + "ImguiFragShader.glsl");
        }

        public unsafe void Initialize()
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGuiIOPtr io = ImGui.GetIO();

            BindKeymaps();

            io.Fonts.AddFontDefault();

            // Build texture atlas
            //io.Fonts.GetTexDataAsAlpha8(out IntPtr texData, out int width, out int height);
            io.Fonts.GetTexDataAsRGBA32(out IntPtr texData, out int width, out int height, out int bytesPerPixel);
            fontTexture = new Texture("ImGui Text Atlas", width, height, bytesPerPixel, texData);
            fontTexture.SetMagFilter(TextureMagFilter.Linear);
            fontTexture.SetMinFilter(TextureMinFilter.Linear);

            // Store the texture identifier in the ImFontAtlas substructure.
            io.Fonts.SetTexID((IntPtr)fontTexture.Handle);

            // Cleanup (don't clear the input data if you want to append new fonts later)
            io.Fonts.ClearTexData();
        }

        public void WindowResized(int w, int h)
        {
            size.X = w; size.Y = h;
        }

        public unsafe void Update(GameWindow window, float deltaTime)
        {
            UpdateImGuiInput(window);
        }

        public unsafe void FrameStart(float deltaTime)
        {
            var io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(size.X, size.Y);
            io.DisplayFramebufferScale = System.Numerics.Vector2.One;

            io.DeltaTime = deltaTime;
            ImGui.NewFrame();
        }

        public unsafe void Render()
        {
            ImGui.Render();
            RenderDrawData(ImGui.GetDrawData(), size.X, size.Y);
        }


        public unsafe void RenderDrawData(ImDrawDataPtr drawData, int displayW, int displayH)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers.

            int last_texture;
            GL.GetInteger(GetPName.TextureBinding2D, out last_texture);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            var io = ImGui.GetIO();
            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            shader.Use();
            Matrix4 modelView = Matrix4.Identity;
            Matrix4 projection = new Matrix4();
            OrthoCamera(io.DisplaySize.X / io.DisplayFramebufferScale.X, io.DisplaySize.Y / io.DisplayFramebufferScale.Y, ref projection);

            shader.SetUniformMatrix4("modelView", modelView);
            shader.SetUniformMatrix4("projection", projection);
            shader.SetSamplerUniform(0, 0);

            GL.ActiveTexture(TextureUnit.Texture0);

            // Render command lists

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = drawData.CmdListsRange[n];
                byte* vtx_buffer = (byte*)cmd_list.VtxBuffer.Data;
                ushort* idx_buffer = (ushort*)cmd_list.IdxBuffer.Data;
                Vertex2DColorUV vertex = new Vertex2DColorUV();
                var sz = sizeof(Vertex2DColorUV);

                var buffer = new VertexBuffer<Vertex2DColorUV>(vtx_buffer, cmd_list.VtxBuffer.Size * sizeof(Vertex2DColorUV), vertex.GetVertexFormat());
                buffer.Bind(shader);

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.BindTexture(TextureTarget.Texture2D, pcmd.TextureId.ToInt32());
                        GL.Scissor((int)pcmd.ClipRect.X,
                                    (int)(io.DisplaySize.Y - pcmd.ClipRect.W),
                                    (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                                    (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        GL.DrawElements(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, new IntPtr(idx_buffer));
                    }
                    idx_buffer += pcmd.ElemCount;
                }
            }

            // Restore modified state
            GL.BindTexture(TextureTarget.Texture2D, last_texture);
            GL.Disable(EnableCap.ScissorTest);
        }

        private void OrthoCamera(float w, float h, ref Matrix4 projection)
        {
            projection = Matrix4.Identity;
            projection.M11 = 2.0f / w;
            projection.M22 = -2.0f / h;
            projection.M33 = -1.0f;
            projection.M41 = -1.0f;
            projection.M42 = 1.0f;
            projection.M43 = 0.0f;
        }

        public static unsafe void UpdateImGuiInput(GameWindow window)
        {
            var io = ImGui.GetIO();

            var screenPoint = new Vector2i((int)window.MouseState.X, (int)window.MouseState.Y);
            io.MousePos = new System.Numerics.Vector2(screenPoint.X, screenPoint.Y);

            // So, used to have problem with unfocused window, so used to set MousePos to zero... is this still an issue?

            io.MouseDown[0] = window.MouseState[OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left];
            io.MouseDown[1] = window.MouseState[OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right];
            io.MouseDown[2] = window.MouseState[OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle];
            
            io.MouseWheel += window.MouseState.ScrollDelta.Y;
            io.MouseWheelH += window.MouseState.ScrollDelta.X;

            var keyboardState = window.KeyboardState;
            io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = keyboardState.IsKeyDown(key);
            }
        }

        public void OnMouseDown(MouseButtonEventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            switch (e.Button)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left:
                    io.MouseDown[0] = true;
                    break;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle:
                    io.MouseDown[2] = true;
                    break;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right:
                    io.MouseDown[1] = true;
                    break;
            }
        }

        public void OnMouseUp(MouseButtonEventArgs e)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            switch (e.Button)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left:
                    io.MouseDown[0] = false;
                    break;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle:
                    io.MouseDown[2] = false;
                    break;
                case OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right:
                    io.MouseDown[1] = false;
                    break;
            }

        }

        private void BindKeymaps()
        {
            var io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;

        }
    }
}   
