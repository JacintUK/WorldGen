/*
 * Copyright 2019 David Ian Steele
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


using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace WorldGen
{
    partial class Program
    {
        World world;
        Scene scene;

        IGeometry worldRenderGeometry;
        IGeometry borderGeometry;
        GeometryRenderer<Vertex3DColorUV> worldRenderer;
        GeometryRenderer<Vertex3DColorUV> tileRenderer; // Used for tile selection.
        GeometryRenderer<Vertex3D> worldCentroidDebugRenderer;
        GeometryRenderer<Vertex3DColor> worldVertsDebugRenderer;
        GeometryRenderer<Vertex3DColorUV> worldPlateSpinDebugRenderer;
        GeometryRenderer<Vertex3DColorUV> worldPlateDriftDebugRenderer;
        GeometryRenderer<Vertex3DColor> borderRenderer;
        GeometryRenderer<Vertex3DColor> equatorRenderer;
        GeometryRenderer<Vertex3DColor> meridianRenderer;
       

        Node worldNode;
       
        Vector3 lightPosition = new Vector3(-2, 2, 2);
        Vector3 ambientColor;
        System.Numerics.Vector3 centroidDebugColor = new System.Numerics.Vector3(0.5f, 0.5f, 0.5f);
        System.Numerics.Vector3 ambientDebugColor = new System.Numerics.Vector3(0.5f, 0.5f, 0.5f);

        static float INITIAL_NODE_Z = 0.0f;//-3.0f;
        static float RENDERER_Z_CUTOFF = INITIAL_NODE_Z + 0.1f;

        float zCutoff = RENDERER_Z_CUTOFF; 
        float centroidAlpha = 1.0f;

        int colorMap;

        static int DEFAULT_WINDOW_WIDTH = 1200;
        static int DEFAULT_WINDOW_HEIGHT = 800;

        private EventHandler eventHandler;
        void CreateScene(object sender, GameWindow.SceneCreatedEventArgs e)
        {
            world = new World();
            colorMap = (int)world.WorldColor;
            numSubdivisions = world.NumSubDivisions;
            numPlates = world.NumPlates;

            scene = e.scene;
            scene.SceneUpdatedEvent += UpdateScene;

            // TODO: Change to a skybox!
            scene.SetBackground("black-sky-darkness-night-atmosphere-astronomical-object-1563485-pxhere.com.jpg");

            ambientColor = Math2.ToVec3(Color4.Aquamarine);// * 0.25f;

            var window = sender as GameWindow;
            if (window != null)
            {
                window.EventHandler = eventHandler = new EventHandler(window, scene);

                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.Space, RelaxTriangles));
                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.D, TweakTriangles));
                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.R, ResetSphere));
                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.D1, DistortTriangles));
                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.C, Recolor));
                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.I, InitPlates));
                window.EventHandler.keyHandlers.Add(new KeyHandler(Keys.P, GrowPlates));
            }
            Shader quadShader = new Shader(GameWindow.SHADER_PATH + "quadVertShader.glsl", GameWindow.SHADER_PATH + "4ChannelFragShader.glsl");
            Shader shader = new Shader(GameWindow.SHADER_PATH + "Vert3DColorUVShader.glsl", GameWindow.SHADER_PATH + "shadedFragShader.glsl");
            Shader pointShader = new Shader(GameWindow.SHADER_PATH + "pointVertShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
            Shader lineShader = new Shader(GameWindow.SHADER_PATH + "pointColorVertShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
            Shader texShader2 = new Shader(GameWindow.SHADER_PATH + "Vert3DColorUVShader.glsl", GameWindow.SHADER_PATH + "texFragShader.glsl");
            Shader borderShader = new Shader(GameWindow.SHADER_PATH + "Vert3DColorShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
            Texture cellTexture = new Texture("Edge.png");
            Texture arrowTexture = new Texture("Arrow.png");

            worldNode = new Node
            {
                Position = new Vector3(0, 0, INITIAL_NODE_Z),
                Rotation = new Quaternion(Vector3.UnitY, 0.0f),
                Scale = new Vector3(1.0f, 1.0f, 1.0f)
            };
            worldNode.Update();
            scene.Add(worldNode);

            worldRenderGeometry = world.RegenerateMesh();

            worldRenderer = new GeometryRenderer<Vertex3DColorUV>(worldRenderGeometry as Geometry<Vertex3DColorUV>, shader);
            worldRenderer.Renderer.AddUniform(new UniformProperty("lightPosition", lightPosition));
            worldRenderer.Renderer.AddUniform(new UniformProperty("ambientColor", ambientColor));
            worldRenderer.Renderer.AddTexture(cellTexture);
            worldRenderer.Renderer.CullFaceFlag = true;
            worldRenderer.Sensitive = true;
            worldRenderer.touchedEvent += TileTouchedEventHandler;
            worldNode.Add(worldRenderer);

            borderGeometry = world.plates.GenerateBorderGeometry<Vertex3DColor>();
            borderRenderer = new GeometryRenderer<Vertex3DColor>(borderGeometry as Geometry<Vertex3DColor>, borderShader);
            borderRenderer.Renderer.DepthTestFlag = true;
            borderRenderer.Renderer.CullFaceFlag = true;
            borderRenderer.Renderer.CullFaceMode = CullFaceMode.Back;
            borderRenderer.Sensitive = true;
            borderRenderer.touchedEvent += BorderTouchedEventHandler;
            worldNode.Add(borderRenderer);

            worldVertsDebugRenderer = new GeometryRenderer<Vertex3DColor>(world.geometry as Geometry<Vertex3DColor>, pointShader);
            worldVertsDebugRenderer.Renderer.AddUniform(new UniformProperty("color", new Vector4(1, 0.2f, 0.7f, 1)));
            worldVertsDebugRenderer.Renderer.AddUniform(new UniformProperty("pointSize", 3f));
            worldVertsDebugRenderer.Renderer.AddUniform(new UniformProperty("zCutoff", RENDERER_Z_CUTOFF));
            worldVertsDebugRenderer.Renderer.DepthTestFlag = false;
            worldVertsDebugRenderer.Renderer.CullFaceFlag = false;
            worldVertsDebugRenderer.Renderer.BlendingFlag = true;
            worldVertsDebugRenderer.Renderer.Visible = false;
            worldNode.Add(worldVertsDebugRenderer);

            Geometry<Vertex3D> centroidGeom = new Geometry<Vertex3D>(world.geometry.GenerateCentroidPointMesh())
            {
                PrimitiveType = PrimitiveType.Points
            };
            worldCentroidDebugRenderer = new GeometryRenderer<Vertex3D>(centroidGeom, pointShader);
            worldCentroidDebugRenderer.Renderer.DepthTestFlag = false;
            worldCentroidDebugRenderer.Renderer.CullFaceFlag = false;
            worldCentroidDebugRenderer.Renderer.BlendingFlag = true;
            worldCentroidDebugRenderer.Renderer.AddUniform(new UniformProperty("color", new Vector4(0.5f, 0.5f, 0.5f, 1.0f)));
            worldCentroidDebugRenderer.Renderer.AddUniform(new UniformProperty("pointSize", 3f));
            worldCentroidDebugRenderer.Renderer.AddUniform(new UniformProperty("zCutoff", RENDERER_Z_CUTOFF));
            worldCentroidDebugRenderer.Renderer.Visible = false;
            worldNode.Add(worldCentroidDebugRenderer);

            var spinGeom = world.plates.GenerateSpinDriftDebugGeom(true);
            worldPlateSpinDebugRenderer = new GeometryRenderer<Vertex3DColorUV>(spinGeom, texShader2);
            worldPlateSpinDebugRenderer.Renderer.BlendingFlag = true;
            worldPlateSpinDebugRenderer.Renderer.AddTexture(arrowTexture);
            worldPlateSpinDebugRenderer.Renderer.AddUniform(new UniformProperty("color", new Vector4(1, 1, 1, 1.0f)));
            worldPlateSpinDebugRenderer.Renderer.AddUniform(new UniformProperty("zCutoff", RENDERER_Z_CUTOFF));
            worldPlateSpinDebugRenderer.Renderer.Visible = false;
            worldNode.Add(worldPlateSpinDebugRenderer);

            var driftGeom = world.plates.GenerateSpinDriftDebugGeom(false);
            worldPlateDriftDebugRenderer = new GeometryRenderer<Vertex3DColorUV>(driftGeom, texShader2);
            worldPlateDriftDebugRenderer.Renderer.BlendingFlag = true;
            worldPlateDriftDebugRenderer.Renderer.AddTexture(arrowTexture);
            worldPlateDriftDebugRenderer.Renderer.AddUniform(new UniformProperty("color", new Vector4(.75f, 0.75f, 0.0f, 1.0f)));
            worldPlateDriftDebugRenderer.Renderer.AddUniform(new UniformProperty("zCutoff", RENDERER_Z_CUTOFF));
            worldPlateDriftDebugRenderer.Renderer.Visible = false;
            worldNode.Add(worldPlateDriftDebugRenderer);

            var equatorGeom = GeometryFactory.GenerateCircle(Vector3.Zero, Vector3.UnitY, 1.005f, new Vector4(1.0f, 0, 0, 1.0f));
            equatorRenderer = new GeometryRenderer<Vertex3DColor>(equatorGeom, lineShader);
            equatorRenderer.Renderer.AddUniform(new UniformProperty("zCutoff", RENDERER_Z_CUTOFF));
            worldNode.Add(equatorRenderer);

            var meridianGeom = GeometryFactory.GenerateCircle(Vector3.Zero, Vector3.UnitZ, 1.005f, new Vector4(1.0f, 0.0f, 1.0f, 1.0f));
            meridianRenderer = new GeometryRenderer<Vertex3DColor>(meridianGeom, lineShader);
            meridianRenderer.Renderer.AddUniform(new UniformProperty("zCutoff", RENDERER_Z_CUTOFF));
            worldNode.Add(meridianRenderer);
        }

        void TileTouchedEventHandler(object sender, TouchedEventArgs e)
        {
            if (tileRenderer != null)
            {
                tileRenderer.ChangeGeometry(worldRenderer.GetGeometry().GenerateTile(e.VertexIndex) as Geometry<Vertex3DColorUV>);
            }
            else
            {
                Shader shader = new Shader(GameWindow.SHADER_PATH + "Vert3DColorUVShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
                tileRenderer = new GeometryRenderer<Vertex3DColorUV>(worldRenderer.GetGeometry().GenerateTile(e.VertexIndex) as Geometry<Vertex3DColorUV>, shader);
                tileRenderer.Renderer.CullFaceFlag = false;
                worldNode.Add(tileRenderer);
            }
        }

        void BorderTouchedEventHandler(object sender, TouchedEventArgs e)
        {

        }

        void UpdateScene(object sender, EventArgs e)
        {
            worldRenderGeometry = world.RegenerateMesh();
            worldRenderer.Update(worldRenderGeometry as Geometry<Vertex3DColorUV>);

            borderGeometry = world.plates.GenerateBorderGeometry<Vertex3DColor>();
            borderRenderer.Update(borderGeometry as Geometry<Vertex3DColor>);

            worldVertsDebugRenderer.Update(world.geometry as Geometry<Vertex3DColor>);

            Geometry<Vertex3D> centroidGeom = new Geometry<Vertex3D>(world.geometry.GenerateCentroidPointMesh())
            {
                PrimitiveType = PrimitiveType.Points
            };
            worldCentroidDebugRenderer.Update(centroidGeom);

            var spinGeom = world.plates.GenerateSpinDriftDebugGeom(true);
            worldPlateSpinDebugRenderer.Update(spinGeom);

            spinGeom = world.plates.GenerateSpinDriftDebugGeom(false);
            worldPlateDriftDebugRenderer.Update(spinGeom);
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

        private void TweakTriangles(bool down)
        {
            if (down)
            {
                world.TweakTriangles();
                world.CalculatePlateBoundaries();
                scene.Update();
            }
        }

        private void DistortTriangles(bool down)
        {
            if (down)
            {
                world.Distort(numDistortions);
                world.CreatePlates(world.NumPlates);
                scene.Update();
            }
        }

        private void Recolor(bool down)
        {
            if (down)
            {
                world.CreatePlates(world.NumPlates);
                scene.Update();
            }
        }

        private void InitPlates(bool down)
        {
            if (down)
            {
                world.CreatePlates(world.NumPlates);
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

        private bool worldRender = true;
        private bool debugRenderBorder = true;
        private bool debugRenderSpin = false;
        private bool debugRenderDrift = true;
        private bool debugCentroid = false;
        private bool debugVertex = false;
        private bool debugEquator = true;
        private bool debugMeridian = true;
        private int numSubdivisions = 4;
        private int numPlates=5;
        private int numDistortions=6;
        private bool showDemo = false;
        void RenderGui(object sender, EventArgs e)
        {
            // Do IMGui layout here.
            ImGui.Begin("Debug World");

            if (ImGui.CollapsingHeader("Control Geometry"))
            {
                if(ImGui.SliderInt("Subdivisions", ref numSubdivisions, 0, 6, numSubdivisions.ToString()))
                {
                    world.NumSubDivisions = numSubdivisions;
                }
                if (ImGui.SliderInt("Plates", ref numPlates, 5, 40, numPlates.ToString()))
                {
                    world.NumPlates = numPlates;
                }
                if (ImGui.SliderInt("Distortions", ref numDistortions, 0, 12, numDistortions.ToString()))
                {
                    world.NumDistortions = numDistortions;
                }
                
                if (ImGui.Button("Reset Sphere"))
                {
                    world.NumDistortions = numDistortions;
                    world.NumSubDivisions = numSubdivisions;
                    world.NumPlates = numPlates;
                    ResetSphere(true);
                }
                if (ImGui.Button("Relax Triangles"))
                {
                    RelaxTriangles(true);
                }
                if (ImGui.Button("Tweak Triangles"))
                {
                    TweakTriangles(true);
                }
                if (ImGui.Button("Initialize Plates"))
                {
                    InitPlates(true);
                }
                if (ImGui.Button("Grow Plates"))
                {
                    GrowPlates(true);
                }
                if (ImGui.Button("Reassign Plates"))
                {
                    Recolor(true);
                }
                if (ImGui.Button("Distort mesh & Reassign"))
                {
                    DistortTriangles(true);
                }
            }
            if (ImGui.CollapsingHeader("Debug Renderers"))
            {
                if(ImGui.Checkbox("World", ref worldRender))
                {
                    worldRenderer.Renderer.Visible = worldRender;
                }
                if( ImGui.Checkbox("Vertices", ref debugVertex))
                {
                    worldVertsDebugRenderer.Renderer.Visible = debugVertex;
                }
                if (ImGui.Checkbox("Centroids", ref debugCentroid))
                {
                    worldCentroidDebugRenderer.Renderer.Visible = debugCentroid;
                }
                if(debugCentroid)
                {
                    if (ImGui.CollapsingHeader("Centroid detail"))
                    {
                        if (ImGui.SliderFloat("Alpha", ref centroidAlpha, 0.0f, 1.0f))
                        {
                            worldCentroidDebugRenderer.Renderer.SetUniform("color", new Vector4(centroidDebugColor.X, centroidDebugColor.Y, centroidDebugColor.Z, centroidAlpha));
                        }
                        if (ImGui.ColorPicker3("Centroid", ref centroidDebugColor))
                        {
                            worldCentroidDebugRenderer.Renderer.SetUniform("color", new Vector4(centroidDebugColor.X, centroidDebugColor.Y, centroidDebugColor.Z, centroidAlpha));
                        }
                    }
                }
                if( ImGui.Checkbox("Border", ref debugRenderBorder) )
                {
                    borderRenderer.Renderer.Visible = debugRenderBorder;
                }
                if( ImGui.Checkbox("Plate Spin", ref debugRenderSpin))
                {
                    worldPlateSpinDebugRenderer.Renderer.Visible = debugRenderSpin;
                }
                if (ImGui.Checkbox("Plate Drift", ref debugRenderDrift))
                {
                    worldPlateDriftDebugRenderer.Renderer.Visible = debugRenderDrift;
                }
                if (ImGui.Checkbox("Equator", ref debugEquator))
                {
                    equatorRenderer.Renderer.Visible = debugEquator;
                }
                if (ImGui.Checkbox("Meridian", ref debugMeridian))
                {
                    meridianRenderer.Renderer.Visible = debugMeridian;
                }
                if (ImGui.SliderFloat("Z Cutoff", ref zCutoff, -10.0f, 10.0f))
                {
                    SetDebugRendererZcutoff();
                }
            }

            if ( ImGui.CollapsingHeader("Color Map"))
            {
                if( ImGui.RadioButton("Terrain", ref colorMap, (int)World.WorldColorE.Height) )
                {
                    world.WorldColor = World.WorldColorE.Height;
                    scene.Update();
                }
                if (ImGui.RadioButton("PlateDebug", ref colorMap, (int)World.WorldColorE.PlateColor))
                {
                    world.WorldColor = World.WorldColorE.PlateColor;
                    scene.Update();
                }
                if (ImGui.ColorPicker3("ambient Color", ref ambientDebugColor))
                {
                    worldRenderer.Renderer.SetUniform("ambientColor", new Vector3(ambientDebugColor.X, ambientDebugColor.Y, ambientDebugColor.Z));
                }
            }
            if (false)
            {


                const ImGuiTableFlags flags = ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter |
                    ImGuiTableFlags.BordersV;
                ImGui.BeginTable("Table1", 4, flags);
                //ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Camera world pos");
                ImGui.TableSetColumnIndex(1);
                string xPos = $"X: {scene.camera.Position.X:F2}";
                ImGui.Text(xPos);
                ImGui.TableSetColumnIndex(2);
                string yPos = $"Y: {scene.camera.Position.Y:F2}";
                ImGui.Text(yPos);
                ImGui.TableSetColumnIndex(3);
                string zPos = $"Z: {scene.camera.Position.Z:F2}";
                ImGui.Text(zPos);
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Touch point:");
                ImGui.TableNextColumn();
                string scrX = $"X: {eventHandler.Buttons[0].Point.X}";
                ImGui.Text(scrX);
                ImGui.TableNextColumn();
                string scrY = $"Y: {eventHandler.Buttons[0].Point.Y}";
                ImGui.Text(scrY);
                ImGui.EndTable();
            }
            if(ImGui.Button("Demo"))
            {
                showDemo = true;
            }
            if(showDemo)
            {
                ImGui.ShowDemoWindow(ref showDemo);
            }

            ImGui.End();
        }

        private void SetDebugRendererZcutoff()
        {
            worldVertsDebugRenderer.Renderer.SetUniform("zCutoff", zCutoff);
            worldCentroidDebugRenderer.Renderer.SetUniform("zCutoff", zCutoff);
            worldPlateDriftDebugRenderer.Renderer.SetUniform("zCutoff", zCutoff);
            worldPlateSpinDebugRenderer.Renderer.SetUniform("zCutoff", zCutoff);
            meridianRenderer.Renderer.SetUniform("zCutoff", zCutoff);
            equatorRenderer.Renderer.SetUniform("zCutoff", zCutoff);
        }

        void ImGuiMouseUp(object sender, OpenTK.Windowing.Common.MouseButtonEventArgs e)
        {
            if(world.NumPlates != numPlates)
            {
                world.NumPlates = numPlates;
                world.Initialize();
                scene.Update();
            }
            if( world.NumSubDivisions != numSubdivisions )
            {
                world.NumSubDivisions = numSubdivisions;
                world.Initialize();
                scene.Update();
            }
        }

        static void Main(string[] args)
        {
            var program = new Program();
            GameWindow window = new GameWindow(DEFAULT_WINDOW_WIDTH, DEFAULT_WINDOW_HEIGHT);
            window.SceneCreatedEvent += program.CreateScene;
            window.ImGuiRenderEvent += program.RenderGui;
            window.ImGuiMouseUpEvent += program.ImGuiMouseUp;
            window.Start();
        }
    }
}
