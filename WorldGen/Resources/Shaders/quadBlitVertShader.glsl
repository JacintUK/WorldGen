﻿#version 130

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

in vec3 aPosition;
in vec2 aTexCoords;
in vec4 aColor;
out vec4 vColor;
out vec2 vTexCoords;
uniform mat4 modelView;
uniform mat4 projection;

void main()
{
	gl_Position = vec4(aPosition.xy*2.0,0.0,1.0);
	vColor = aColor;
	vTexCoords = aTexCoords;
}
