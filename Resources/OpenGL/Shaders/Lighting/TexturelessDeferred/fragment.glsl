layout(early_fragment_tests) in; 

// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 normal;
in vec3 pos;
flat in uint drawID;

// Ouput data
layout(location = 0) out vec4 uvs;
layout(location = 1) out uint matID;
layout(location = 2) out vec3 wPos;
// Values that stay constant for the whole mesh.

layout(std430, binding = 1) buffer material_t {
	uint MaterialID[MAX_DRAWS_UBO];
} Material;

vec2 encode (vec3 n)
{
    vec2 enc = normalize(n.xy) * (sqrt(-n.z*0.5+0.5));
    enc = enc*0.5+0.5;
    return enc;
}

void main(){
	uvs = vec4(UV, encode(normal));
	matID = Material.MaterialID[drawID];
	wPos = pos;
}