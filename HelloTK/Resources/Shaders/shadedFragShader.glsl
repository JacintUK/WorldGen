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

out vec4 outputColor;

void main()
{
	// DO stuff with textures!
	vec3 litColor =  vAmbientColor + vColor.xyz * ( intensity + specular ) *power/distanceSq;
	outputColor = vec4( litColor, vColor.a );
}
