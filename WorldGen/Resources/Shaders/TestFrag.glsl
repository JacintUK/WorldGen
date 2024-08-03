#version 430

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

// TEST FRAG
in vec4 vColor;
in vec2 vTexCoords;
in float intensity;
in float distanceSq;
in float specular;

in vec3  vAmbientColor;

out vec4 outputColor;
uniform sampler2D sTexture;

void main()
{
	vec4 texColor = vColor * texture(sTexture,vTexCoords);

	vec3 litColor =  vAmbientColor + 
		texColor.rgb * intensity/distanceSq +
		specular / distanceSq;

	outputColor = vec4(texColor.rgb + vAmbientColor, texColor.a);
	//outputColor = vec4(litColor, texColor.a) + vec4(vAmbientColor, 1.0);
}
