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

in vec4 vColor;
in vec2 vTexCoords;
out vec4 outputColor;
uniform sampler2D sTexture;
uniform vec4 color;

void main()
{
	// DO stuff with textures!
	outputColor = color * vColor * texture2D(sTexture,vTexCoords);
	//outputColor = vec4(1.0,0.0,1.0,1.0);
}
