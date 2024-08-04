#version 130

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

in vec2 aPosition;
in vec2 aTexCoords;
in vec4 aColor;

out vec2 vTexCoords;
out vec4 vColor;

uniform mat4 modelView;
uniform mat4 projection;

void main()
{
	vec4 vertexPos = vec4(aPosition,0.0,1.0);
	vTexCoords = aTexCoords;
	vColor = aColor;
	gl_Position = projection * (modelView * vertexPos);
}
