#version 130

in vec2 aPosition;
in vec2 aTexCoords;
in vec4 aColor;
out vec4 vColor;
uniform mat4 modelView;
uniform mat4 projection;

void main()
{
	gl_Position = projection * (modelView * vec4(aPosition, 0.0,1.0));
	vColor = aColor;
}
