#version 130

in vec4 vColor;
out vec4 outputColor;
uniform sampler2D sTexture;

void main()
{
	outputColor = vColor * texture2D(sTexture, gl_PointCoord);
}
