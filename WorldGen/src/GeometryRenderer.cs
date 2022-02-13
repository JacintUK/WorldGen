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


using OpenTK;
using OpenTK.Mathematics;

namespace WorldGen
{
    class GeometryRenderer<TVertex> where TVertex : struct, IVertex
    {
        public Geometry<TVertex> geometry;
        public Renderer renderer;
        private VertexBuffer<TVertex> vbo;
        private IndexBuffer ibo;

        public static GeometryRenderer<Vertex> NewQuad(Shader shader)
        {
            Vertex[] quad = new Vertex[4]
            {
                // Texture coords - top is 0, bottom is 1
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0,1), Color4.White),
                new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(1,1), Color4.White),
                new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0,0), Color4.White),
                new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1,0), Color4.White),
            };
            uint[] indices = new uint[6]
            {
                0, 1, 2, 2, 1, 3
            };
            var m = new Mesh<Vertex>(quad);
            var g = new Geometry<Vertex>(m, indices);
            var gr = new GeometryRenderer<Vertex>(g, shader);
            return gr;
        }

        public GeometryRenderer(Geometry<TVertex> geometry, Shader shader)
        {
            this.geometry = geometry;
            vbo = new VertexBuffer<TVertex>((geometry.Mesh as Mesh<TVertex>).vertices);
            if(geometry.Indices != null)
                ibo = new IndexBuffer((geometry.Indices as Indices).IndexArray);
            renderer = new Renderer(vbo, ibo, shader);
            renderer.PrimitiveType = geometry.PrimitiveType;
        }

        public void Update()
        {
            geometry.Upload(vbo, ibo);
        }

        public void Update(Geometry<TVertex> geometry)
        {
            this.geometry = geometry;
            Update();
        }
    }
}

