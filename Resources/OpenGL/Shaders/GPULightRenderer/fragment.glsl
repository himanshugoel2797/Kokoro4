layout(early_fragment_tests) in;

struct LightEntry_t{
    vec3 Pos;
    float effectiveRadius;
    float radius;
    float intensity;
    float typeIndex;
    float unused0;
    vec3 color;
    float unused1;
    vec4 unused2;
};

layout(binding = 0, std430) buffer lightData{
    LightEntry_t data[];
} lights;

layout(r32ui, bindless_image) uniform coherent restrict uimage3D LightIndices;

flat in int inst_id;

void main(){
    //Increment the light count in LightIndices for this fragCoord
    uint idx = imageAtomicAdd(LightIndices, ivec3(gl_FragCoord.x, gl_FragCoord.y, 0), 1);

    //Write the inst_id at the index
    imageStore(LightIndices, ivec3(gl_FragCoord.x, gl_FragCoord.y, idx + 1), ivec4(inst_id));
}