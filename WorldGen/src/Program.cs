﻿/*
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

namespace WorldGen
{
    partial class Program
    {
        World world;
        Scene scene;
        EventHandler eventHandler;

        IGeometry worldRenderGeometry;
        IGeometry borderGeometry;
        GeometryRenderer<Vertex3DColorUV> worldRenderer;
        GeometryRenderer<Vertex3D> worldCentroidDebugRenderer;
        GeometryRenderer<Vertex3DColor> worldVertsDebugRenderer;
        GeometryRenderer<Vertex3DColorUV> worldPlateSpinDebugRenderer;
        GeometryRenderer<Vertex3DColorUV> worldPlateDriftDebugRenderer;
        GeometryRenderer<Vertex3DColor> borderRenderer;
        GeometryRenderer<Vertex3DColor> equatorRenderer;

        Quaternion rotation = new Quaternion(Vector3.UnitY, 0.0f);

        Vector3 lightPosition = new Vector3(-2, 2, 2);
        Vector3 ambientColor;

        int colorMap;

        void CreateScene(object sender, GameWindow.SceneCreatedEventArgs e)
        {
            world = new World();
            colorMap = (int)world.WorldColor;

            scene = e.scene;
            scene.SceneUpdatedEvent += UpdateScene;

            ambientColor = Math2.ToVec3(Color4.Aquamarine) * 0.25f;

            eventHandler = new EventHandler(world, scene);
            var window = sender as GameWindow;
            if( window != null )
            {
                window.EventHandler = eventHandler;
            }
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.Space, RelaxTriangles));
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.D, TweakTriangles));
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.R, ResetSphere));
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.D1, DistortTriangles));
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.C, Recolor));
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.I, InitPlates));
            eventHandler.keyHandlers.Add(new KeyHandler(Keys.P, GrowPlates));


            Shader quadShader = new Shader(GameWindow.SHADER_PATH + "quadVertShader.glsl", GameWindow.SHADER_PATH + "4ChannelFragShader.glsl");
            Shader shader = new Shader(GameWindow.SHADER_PATH + "Vert3DColorUVShader.glsl", GameWindow.SHADER_PATH + "shadedFragShader.glsl");
            Shader pointShader = new Shader(GameWindow.SHADER_PATH + "pointVertShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
            Shader lineShader = new Shader(GameWindow.SHADER_PATH + "pointColorVertShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
            Shader texShader2 = new Shader(GameWindow.SHADER_PATH + "Vert3DColorUVShader.glsl", GameWindow.SHADER_PATH + "texFragShader.glsl");
            Shader borderShader = new Shader(GameWindow.SHADER_PATH + "Vert3DColorShader.glsl", GameWindow.SHADER_PATH + "pointFragShader.glsl");
            Texture cellTexture = new Texture("Edge.png");
            Texture arrowTexture = new Texture("Arrow.png");

            var node = new Node
            {
                Position = new Vector3(0, 0, -3)
            };
            node.Model = Matrix4.CreateTranslation(node.Position);
            scene.Add(node);

            worldRenderGeometry = world.RegenerateMesh();

            worldRenderer = new GeometryRenderer<Vertex3DColorUV>(worldRenderGeometry as Geometry<Vertex3DColorUV>, shader);
            worldRenderer.renderer.AddUniform(new UniformProperty("lightPosition", lightPosition));
            worldRenderer.renderer.AddUniform(new UniformProperty("ambientColor", ambientColor));
            worldRenderer.renderer.AddTexture(cellTexture);
            worldRenderer.renderer.CullFaceFlag = true;
            node.Add(worldRenderer.renderer);

            borderGeometry = world.plates.GenerateBorderGeometry<Vertex3DColor>();
            borderRenderer = new GeometryRenderer<Vertex3DColor>(borderGeometry as Geometry<Vertex3DColor>, borderShader);
            borderRenderer.renderer.DepthTestFlag = true;
            borderRenderer.renderer.CullFaceFlag = true;
            borderRenderer.renderer.CullFaceMode = CullFaceMode.Back;
            node.Add(borderRenderer.renderer);

            worldVertsDebugRenderer = new GeometryRenderer<Vertex3DColor>(world.geometry as Geometry<Vertex3DColor>, pointShader);
            worldVertsDebugRenderer.renderer.AddUniform(new UniformProperty("color", new Vector4(0, 0.2f, 0.7f, 1)));
            worldVertsDebugRenderer.renderer.AddUniform(new UniformProperty("pointSize", 3f));
            worldVertsDebugRenderer.renderer.AddUniform(new UniformProperty("zCutoff", -2.0f));
            worldVertsDebugRenderer.renderer.Visible = false;
            node.Add(worldVertsDebugRenderer.renderer);

            Geometry<Vertex3D> centroidGeom = new Geometry<Vertex3D>(world.geometry.GenerateCentroidPointMesh())
            {
                PrimitiveType = PrimitiveType.Points
            };
            worldCentroidDebugRenderer = new GeometryRenderer<Vertex3D>(centroidGeom, pointShader);
            worldCentroidDebugRenderer.renderer.DepthTestFlag = false;
            worldCentroidDebugRenderer.renderer.CullFaceFlag = false;
            worldCentroidDebugRenderer.renderer.AddUniform(new UniformProperty("color", new Vector4(0.5f, 0.5f, 0.5f, 1)));
            worldCentroidDebugRenderer.renderer.AddUniform(new UniformProperty("pointSize", 3f));
            worldCentroidDebugRenderer.renderer.AddUniform(new UniformProperty("zCutoff", -2.0f));
            worldCentroidDebugRenderer.renderer.Visible = false;
            node.Add(worldCentroidDebugRenderer.renderer);

            var spinGeom = world.plates.GenerateSpinDriftDebugGeom(true);
            worldPlateSpinDebugRenderer = new GeometryRenderer<Vertex3DColorUV>(spinGeom, texShader2);
            worldPlateSpinDebugRenderer.renderer.DepthTestFlag = false;
            worldPlateSpinDebugRenderer.renderer.CullFaceFlag = false;
            worldPlateSpinDebugRenderer.renderer.BlendingFlag = true;
            worldPlateSpinDebugRenderer.renderer.AddTexture(arrowTexture);
            worldPlateSpinDebugRenderer.renderer.AddUniform(new UniformProperty("color", new Vector4(1, 1, 1, 1.0f)));
            node.Add(worldPlateSpinDebugRenderer.renderer);

            var driftGeom = world.plates.GenerateSpinDriftDebugGeom(false);
            worldPlateDriftDebugRenderer = new GeometryRenderer<Vertex3DColorUV>(driftGeom, texShader2);
            worldPlateDriftDebugRenderer.renderer.DepthTestFlag = false;
            worldPlateDriftDebugRenderer.renderer.CullFaceFlag = false;
            worldPlateDriftDebugRenderer.renderer.BlendingFlag = true;
            worldPlateDriftDebugRenderer.renderer.AddTexture(arrowTexture);
            worldPlateDriftDebugRenderer.renderer.AddUniform(new UniformProperty("color", new Vector4(.75f, 0.75f, 0.0f, 1.0f)));
            node.Add(worldPlateDriftDebugRenderer.renderer);

            var equatorGeom = GeometryFactory.GenerateCircle(Vector3.Zero, Vector3.UnitY, 1.001f, new Vector4(1.0f, 0, 0, 1.0f));
            equatorRenderer = new GeometryRenderer<Vertex3DColor>(equatorGeom, lineShader);
            equatorRenderer.renderer.AddUniform(new UniformProperty("zCutoff", -2.8f));
            node.Add(equatorRenderer.renderer);
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
                world.Distort();
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

        Renderer NewRenderer<TVertex>(IGeometry geometry, Shader shader) where TVertex : struct, IVertex
        {
            var geom = geometry as Geometry<TVertex>;
            VertexBuffer<TVertex> vbo = new VertexBuffer<TVertex>();
            IndexBuffer ibo = new IndexBuffer();
            geom.Upload(vbo, ibo);
            return new Renderer(vbo, ibo, shader);
        }

        private bool debugRenderBorder = true;
        private bool debugRenderSpin = true;
        private bool debugRenderDrift = true;
        private bool debugCentroid = false;
        private bool debugVertex = false;
        private bool debugEquator = true;

        void RenderGui(object sender, EventArgs e)
        {
            // Do IMGui layout here.
            ImGui.Begin("Debug World");

            if (ImGui.CollapsingHeader("Control Geometry"))
            {
                ImGui.SliderInt("Subdivisions", ref numSubdivisions, 1, 5, numSubdivisions.ToString());
                ImGui.SliderInt("Plates", ref numPlates, 5, 40, numPlates.ToString());
                if (ImGui.Button("Reset Sphere"))
                {
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
                if( ImGui.Checkbox("Vertices", ref debugVertex))
                {
                    worldVertsDebugRenderer.renderer.Visible = debugVertex;
                }
                if (ImGui.Checkbox("Centroids", ref debugCentroid))
                {
                    worldCentroidDebugRenderer.renderer.Visible = debugCentroid;
                }
                if( ImGui.Checkbox("Border", ref debugRenderBorder) )
                {
                    borderRenderer.renderer.Visible = debugRenderBorder;
                }
                if( ImGui.Checkbox("Plate Spin", ref debugRenderSpin))
                {
                    worldPlateSpinDebugRenderer.renderer.Visible = debugRenderSpin;
                }
                if (ImGui.Checkbox("Plate Drift", ref debugRenderDrift))
                {
                    worldPlateDriftDebugRenderer.renderer.Visible = debugRenderDrift;
                }
                if(ImGui.Checkbox("Equator", ref debugEquator))
                {
                    equatorRenderer.renderer.Visible = debugEquator;
                }
            }

            if( ImGui.CollapsingHeader("Color Map"))
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
            }

            ImGui.End();
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

        int numSubdivisions = 4;
        int numPlates = 20;

        static void Main(string[] args)
        {
            var program = new Program();
            GameWindow window = new GameWindow(800, 600);
            window.SceneCreatedEvent += program.CreateScene;
            window.ImGuiRenderEvent += program.RenderGui;
            window.ImGuiMouseUpEvent += program.ImGuiMouseUp;
            window.Start();
        }
    }
}
