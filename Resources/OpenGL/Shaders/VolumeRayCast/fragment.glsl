// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 vPos;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

uniform sampler2D Albedo;


void main(){
	color.rgb = vPos * 0.5f + 0.5f;
	color.a = 1;
}