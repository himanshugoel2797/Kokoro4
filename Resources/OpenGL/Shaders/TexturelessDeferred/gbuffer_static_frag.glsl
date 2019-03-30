// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 normal;
flat in int drawID;

// Ouput data
layout(location = 0) out vec4 uv_data;
layout(location = 1) out vec4 matIDs;
// Values that stay constant for the whole mesh.

layout(std140) uniform Material_t {
	ivec4 matID[1024];
} Material;

uniform int tile_sz;

void main(){
	double theta = atan(normal.y, normal.x) * 180.0f/PI;
    double phi = acos(normal.z) * 180.0f/PI;
	
	uv_data = vec4(UV, theta * 100, phi * 100);
	matIDs = vec4(Material.matID[drawID/4][drawID % 4] / 4096.0f);// / 4096.0f;
	matIDs.y = (gl_FragCoord.z);
}