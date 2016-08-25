
// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 0) out vec4 color;

// Values that stay constant for the whole mesh.
layout(bindless_sampler) uniform sampler2D AlbedoMap;

void main(){
	color = texture(AlbedoMap, UV);
}