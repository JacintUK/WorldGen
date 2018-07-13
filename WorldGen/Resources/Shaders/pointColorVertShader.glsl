#version 130

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


uniform float zCutoff;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float pointSize;
in vec3 aPosition;
in vec4 aColor;
out vec4 vColor;

void main()
{
	gl_PointSize = pointSize;

	vec4 world = model * vec4(aPosition, 1.0);
	gl_Position = projection * view * world;
	
	vColor=aColor;
	if(world.z < zCutoff)
	{
	  vColor.a = 0;
	}
}
