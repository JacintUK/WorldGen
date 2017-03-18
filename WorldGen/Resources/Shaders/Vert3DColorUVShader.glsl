#version 130

in vec3 aPosition;
in vec3 aNormal;
in vec4 aColor;
in vec2 aTexCoords;
out vec4 vColor;

out float intensity;
out float distanceSq;
out float specular;
out float power;
out vec3  vAmbientColor;
out vec2  vTexCoords;

uniform mat4 modelView;
uniform mat3 mvIT;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform vec3 lightPosition;
uniform vec3 ambientColor;

void main()
{
	vec4 vertexPos = vec4(aPosition,1.0);
	gl_Position = projection * (modelView * vertexPos);
	vTexCoords = aTexCoords;

	vec3 posW = (model * vertexPos).xyz;
	vec3 posC = (modelView * vertexPos).xyz;
	vec3 eyeDirC = vec3(0,0,0)-posC;
	vec3 lightPosC = (view * vec4(lightPosition, 1)).xyz;
	float distance = vec3(lightPosition-aPosition).length;
	distanceSq = distance * distance;

	//lightDirection = normalize((view*vec4(lightPosC + eyeDirC, 1)).xyz);
	vec3 lightDirection = normalize(lightPosC+eyeDirC);
	mat4 viewIT = view;

	vec3 normalC = normalize( mvIT*aNormal );
	//vec3 normalC = normalize( (modelView * vec4(aNormal,1)).xyz );

	vec3 E = normalize(eyeDirC);
	vec3 R = reflect(-lightDirection, normalC);
	float specSize = 8.0;
	float specular = pow(clamp(dot(E,R),0.0,1.0),specSize);
	power = 10; //20;

	// For Debugging normals	
	//intensity=1;
	//vColor = vec4(vec3(0.5)+(0.5*aNormal),1);g

	intensity = clamp( dot( normalC, lightDirection ), 0, 1 );
	vColor = aColor;
	if(posW.z < -2.8)
	{
	  vColor.a = 0;// *= 0.5;
	}
	vAmbientColor = ambientColor;
}
