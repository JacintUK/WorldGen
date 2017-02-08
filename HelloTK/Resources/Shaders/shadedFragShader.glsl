#version 130

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
	vec3 texColor =  texture2D(sTexture,vTexCoords).xyz;
	texColor=vColor.xyz;//vec3(.5,.5,.5);
	vec3 litColor =  vAmbientColor + 
		texColor * intensity *power/distanceSq +
		vec3(1,1,1) * specular * power / distanceSq;
	outputColor = vec4( litColor, vColor.a );
}
