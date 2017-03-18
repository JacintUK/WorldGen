#version 130

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
in vec3 aPosition;
in vec3 aTangent;
out vec4 vColor;

void main()
{
	gl_PointSize = clamp(length(aTangent), 0, 20);

	vec4 world = model * vec4(aPosition, 1.0);
	gl_Position = projection * view * world;
	
	vColor=vec4(1,1,1,1);
	if(world.z < -2.8)
	{
	  vColor.a = 0;// *= 0.5;
	}
}
