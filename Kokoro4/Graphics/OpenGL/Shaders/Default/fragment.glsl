// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;

in float flogz;
uniform float Fcoef;

// Values that stay constant for the whole mesh.
uniform sampler2D AlbedoMap;

void main(){
	color = texture2D(AlbedoMap, UV);
	color = vec4(0,0,0,1);
}