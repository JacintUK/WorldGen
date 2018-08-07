using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    class Scene
    {
        const string SHADER_PATH = "Resources/Shaders/";

        List<Node> nodes = new List<Node>();

        IGeometry worldRenderGeometry;
        IGeometry borderGeometry;
        Renderer worldRenderer;
        Renderer worldCentroidDebugRenderer;
        Renderer worldVertsDebugRenderer;
        Renderer worldPlateSpinDebugRenderer;
        Renderer worldPlateDriftDebugRenderer;

        Renderer borderRenderer;
        Quaternion rotation = new Quaternion(Vector3.UnitY, 0.0f);

        Vector3 lightPosition = new Vector3(-2, 2, 2);
        Vector3 ambientColor;
        World world;
        public Camera camera;

        public Scene(World world, float width, float height)
        {
            this.world = world;

            camera = new Camera
            {
                Position = Vector3.UnitZ * 2.0f,
                Width = width,
                Height = height
            };

            ambientColor = Math2.ToVec3(Color.Aquamarine) * 0.25f;

            Shader quadShader = new Shader(SHADER_PATH + "quadVertShader.glsl", SHADER_PATH + "texFragShader.glsl");
            Shader shader = new Shader(SHADER_PATH + "Vert3DColorUVShader.glsl", SHADER_PATH + "shadedFragShader.glsl");
            Shader pointShader = new Shader(SHADER_PATH + "pointVertShader.glsl", SHADER_PATH + "pointFragShader.glsl");
            Shader lineShader = new Shader(SHADER_PATH + "pointColorVertShader.glsl", SHADER_PATH + "pointFragShader.glsl");
            Shader texShader2 = new Shader(SHADER_PATH + "Vert3DColorUVShader.glsl", SHADER_PATH + "texFragShader.glsl");
            Shader borderShader = new Shader(SHADER_PATH + "Vert3DColorShader.glsl", SHADER_PATH + "pointFragShader.glsl");
            Texture cellTexture = new Texture("Edge.png");
            Texture arrowTexture = new Texture("Arrow.png");

            var node = new Node
            {
                Position = new Vector3(0, 0, -3)
            };
            node.Model = Matrix4.CreateTranslation(node.Position);
            nodes.Add(node);

            worldRenderGeometry = world.geometry.GenerateDualMesh<Vertex3DColorUV>();

            worldRenderer = new Renderer(worldRenderGeometry, shader);
            worldRenderer.AddUniform(new UniformProperty("lightPosition", lightPosition));
            worldRenderer.AddUniform(new UniformProperty("ambientColor", ambientColor));
            worldRenderer.AddTexture(cellTexture);
            worldRenderer.CullFaceFlag = true;
            node.Add(worldRenderer);

            borderGeometry = world.plates.GenerateBorderGeometry<Vertex3DColor>();
            borderGeometry.PrimitiveType = PrimitiveType.Triangles;
            borderRenderer = new Renderer(borderGeometry, borderShader)
            {
                DepthTestFlag = true,
                CullFaceFlag = true,
                CullFaceMode = CullFaceMode.Back
            };
            node.Add(borderRenderer);

            worldVertsDebugRenderer = new Renderer(world.geometry, pointShader);
            worldVertsDebugRenderer.AddUniform(new UniformProperty("color", new Vector4(0, 0.2f, 0.7f, 1)));
            worldVertsDebugRenderer.AddUniform(new UniformProperty("pointSize", 3f));
            //node.Add(worldVertsDebugRenderer);

            Geometry<Vertex3D> centroidGeom = new Geometry<Vertex3D>(world.geometry.GenerateCentroidPointMesh())
            {
                PrimitiveType = PrimitiveType.Points
            };
            worldCentroidDebugRenderer = new Renderer(centroidGeom, pointShader)
            {
                DepthTestFlag = false,
                CullFaceFlag = false
            };
            worldCentroidDebugRenderer.AddUniform(new UniformProperty("color", new Vector4(0.5f, 0.5f, 0.5f, 1)));
            worldCentroidDebugRenderer.AddUniform(new UniformProperty("pointSize", 3f));
            worldCentroidDebugRenderer.AddUniform(new UniformProperty("zCutoff", -2.8f));
            //node.Add(worldCentroidDebugRenderer);

            var spinGeom = world.plates.GenerateSpinDriftDebugGeom(true);
            worldPlateSpinDebugRenderer = new Renderer(spinGeom, texShader2)
            {
                DepthTestFlag = false,
                CullFaceFlag = false,
                BlendingFlag = true
            };
            worldPlateSpinDebugRenderer.AddTexture(arrowTexture);
            worldPlateSpinDebugRenderer.AddUniform(new UniformProperty("color", new Vector4(1, 1, 1, 1.0f)));
            node.Add(worldPlateSpinDebugRenderer);

            var driftGeom = world.plates.GenerateSpinDriftDebugGeom(false);
            worldPlateDriftDebugRenderer = new Renderer(driftGeom, texShader2)
            {
                DepthTestFlag = false,
                CullFaceFlag = false,
                BlendingFlag = true
            };
            worldPlateDriftDebugRenderer.AddTexture(arrowTexture);
            worldPlateDriftDebugRenderer.AddUniform(new UniformProperty("color", new Vector4(.75f, 0.75f, 0.0f, 1.0f)));
            node.Add(worldPlateDriftDebugRenderer);

            var equatorGeom = GeometryFactory.GenerateCircle(Vector3.Zero, Vector3.UnitY, 1.001f, new Vector4(1.0f, 0, 0, 1.0f));
            var equatorRenderer = new Renderer(equatorGeom, lineShader);
            equatorRenderer.AddUniform(new UniformProperty("zCutoff", -2.8f));
            node.Add(equatorRenderer);

            //var plateDistanceGeom = world.plates.GenerateDistanceDebugGeom();
            //var plateDistanceRenderer = new Renderer(plateDistanceGeom, lineShader)
            //{
            //    DepthTestFlag = true;
            //}
            //plateDistanceRenderer.AddUniform(new UniformProperty("zCutoff", -2.8f));
            //node.Add(plateDistanceRenderer);
        }

        public Node GetRootNode()
        {
            return nodes[0];
        }

        public void Update(float width, float height)
        {
            camera.Width = width;
            camera.Height = height;
            Update();
        }

        public void Update()
        {
            camera.Update();

            worldRenderGeometry = world.geometry.GenerateDualMesh<Vertex3DColorUV>();
            borderGeometry = world.plates.GenerateBorderGeometry<Vertex3DColor>();
            borderRenderer.Update(borderGeometry);
            worldRenderer.Update(worldRenderGeometry);
            worldVertsDebugRenderer.Update(world.geometry);

            ComplexGeometry<Vertex3D> centroidGeom = new ComplexGeometry<Vertex3D>(world.geometry.GenerateCentroidPointMesh())
            {
                PrimitiveType = PrimitiveType.Points
            };
            worldCentroidDebugRenderer.Update(centroidGeom);
            var spinGeom = world.plates.GenerateSpinDriftDebugGeom(true);
            worldPlateSpinDebugRenderer.Update(spinGeom);
            spinGeom = world.plates.GenerateSpinDriftDebugGeom(false);
            worldPlateDriftDebugRenderer.Update(spinGeom);
        }

        public void Render()
        {
            Matrix4 view = camera.View;
            Matrix4 projection = camera.Projection;

            foreach (var node in nodes)
            {

                node.Draw(ref view, ref projection);
            }
        }


    }
}
