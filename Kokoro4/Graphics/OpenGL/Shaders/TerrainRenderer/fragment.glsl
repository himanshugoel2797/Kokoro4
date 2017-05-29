// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 normal;
in flat int inst;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

layout (std140) buffer heightmaps
{
	uvec2 HeightMaps[MAX_DRAWS_UBO];
} HeightMapData;


void main(){
	color = texture(sampler2D(HeightMapData.HeightMaps[inst]), UV);
	color.a = 1;
}