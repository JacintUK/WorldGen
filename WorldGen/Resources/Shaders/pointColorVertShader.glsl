#version 130

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
