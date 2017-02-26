#version 130


uniform vec4 color;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float pointSize;
in vec3 aPosition;

out vec4 vColor;

void main()
{
	gl_PointSize = pointSize;

	vec4 world = model * vec4(aPosition, 1.0);
	gl_Position = projection * view * world;
	
	vColor=color;
	if(world.z < -2.8)
	{
	  vColor.a = 0;// *= 0.5;
	}
}
