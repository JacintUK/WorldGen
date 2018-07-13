#version 130


uniform vec4 color;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float pointSize;
uniform float zCutoff;

in vec3 aPosition;
out vec4 vColor;

void main()
{
	gl_PointSize = pointSize;

	vec4 world = model * vec4(aPosition, 1.0);
	gl_Position = projection * view * world;
	
	vec4 newColor=color;
	if( world.z < zCutoff )
	{
	  newColor.a = 0;
	}
	vColor=newColor;
}
