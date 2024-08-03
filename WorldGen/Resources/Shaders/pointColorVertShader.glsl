#version 140

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
 
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat3 mvIT;
uniform float pointSize;
uniform float zCutoff;

in vec3 aPosition;
in vec4 aColor;
out vec4 vColor;

void main()
{
    gl_PointSize = pointSize;

    vec4 pos = model * vec4(aPosition, 1.0);
    vec4 posC = view * pos;
    gl_Position = projection * posC;
    vec3 eyeDirC = normalize(vec3(0,0,0) - posC.xyz);	

    vec3 normal = normalize(aPosition);
    vec3 normalC = mvIT * normal;

    vColor=aColor;
    if(dot(normalC, eyeDirC) < zCutoff)
    {
        vColor.a = 0;
    }
}
