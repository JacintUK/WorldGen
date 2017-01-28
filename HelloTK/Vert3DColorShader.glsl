#version 130

in vec3 aPosition;
in vec4 aColor;
out vec4 vColor;
uniform mat4 modelView;
uniform mat4 projection;

void main()
{
	gl_Position = projection * (modelView * vec4(aPosition,1.0));
	vColor = aColor;
}
