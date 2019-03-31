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

layout(r32ui, bindless_image) uniform coherent restrict writeonly uimage3D LightIndices;

void main(){
    imageStore(LightIndices, ivec3(gl_FragCoord.x, gl_FragCoord.y, 0), ivec4(0));
}