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
in vec3 vAmbientColor;
in vec3 eyeDirC;
in vec3 posC;
in vec3 posW;
in vec3 lightDirection;
in float intensity;
in float distanceSq;
in float power;
in float specular;
in vec2 vTexCoords;
uniform sampler2D sTexture;
out vec4 outputColor;

void main()
{
	vec3 texColor = vColor.rgb;
	
	vec3 litColor =  vAmbientColor + 
		texColor * intensity *power/distanceSq +
		vec3(1,1,1) * specular * power / distanceSq;

	outputColor = vec4( litColor, vColor.a );
}
