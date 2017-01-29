#version 130

in vec4 vColor;
in vec2 vTexCoords;
out vec4 outputColor;
uniform sampler2D sTexture;

void main()
{
	// DO stuff with textures!
	outputColor = vColor * texture2D(sTexture,vTexCoords);
	//outputColor = vec4(1.0,0.0,1.0,1.0);
}
