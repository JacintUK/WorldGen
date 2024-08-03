#version 430

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

 // TEST VERT SHADER
in vec3 aPosition;
in vec3 aNormal;
in vec4 aColor;
in vec2 aTexCoords;

out vec4 vColor;
out vec2 vTexCoords;
out float intensity;
out float distanceSq;
out float specular;

out vec3  vAmbientColor;

uniform mat4 modelView;
uniform mat4 projection;
uniform mat4 view;
uniform vec3 lightPosition;

void main()
{
	vec4 vertexPos = vec4(aPosition,1.0);
	vColor = aColor;
	vTexCoords = aTexCoords;
	vec3 posC = (modelView * vertexPos).xyz;
	/*
	vec3 eyeDirC = vec3(0,0,0)-posC;
	vec3 lightPosC = (view * vec4(lightPosition, 1)).xyz;
	float distance = vec3(lightPosition-aPosition).length;
	distanceSq = distance * distance;

	vec3 lightDirection = normalize(lightPosC+eyeDirC);

	vec3 normalC = normalize( modelView*vec4(aNormal, 1.0) ).xyz;

	vec3 E = normalize(eyeDirC);
	vec3 R = reflect(-lightDirection, normalC);
	float specSize = 8.0;
	float specular = pow(clamp(dot(E,R),0.0,1.0),specSize);

	// For Debugging normals	
	intensity=1;
	vColor = vec4(vec3(0.5)+(0.5*aNormal),1);

	intensity = clamp( dot( normalC, lightDirection ), 0.15, 1 );
	*/
	gl_Position = projection * vec4(posC,1.0);
}
