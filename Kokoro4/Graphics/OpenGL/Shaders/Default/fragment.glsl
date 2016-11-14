// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 normal;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.
uniform sampler2D AlbedoMap;

void main(){
	vec3 n = normal * 0.5f + 0.5f;
	color = vec4(n.x,n.y,n.z,1);
}