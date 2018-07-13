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

using OpenTK;

namespace WorldGenerator
{
    internal class Plane
    {
        Vector3 normal;
        Vector3 origin;

        public Plane(Vector3 normal, Vector3 origin)
        {
            this.normal = normal;
            this.normal.Normalize();
            this.origin = origin;
        }
        public void Redefine(Vector3 normal, Vector3 origin)
        {
            this.normal = normal;
            this.normal.Normalize();
            this.origin = origin;
        }
        public Vector3 ProjectPoint(Vector3 point)
        {
            Vector3 toOrig = point - origin;
            Vector3 projectedPoint = point - Vector3.Dot(toOrig, normal) * normal;
            return projectedPoint;
        }
    }
}