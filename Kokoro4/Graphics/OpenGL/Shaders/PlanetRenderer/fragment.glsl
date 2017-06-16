// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 normal;
in flat int inst;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

layout (bindless_sampler) uniform sampler2DArray Cache;

layout (std140) uniform heightmaps
{
	ivec4 HeightMaps[MAX_DRAWS_UBO];
} HeightMapData;


void main(){
	color = texture(Cache, vec3(UV.x, UV.y, HeightMapData.HeightMaps[inst / 4][inst % 4]));
	color.a = 1;
}