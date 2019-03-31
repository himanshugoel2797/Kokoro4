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

layout(location = 0) in vec3 position;

uniform mat4 ViewProj;

flat out int inst_id;

void main(){
	gl_Position = ViewProj * vec4(lights.data[gl_InstanceID].Pos + position * lights.data[gl_InstanceID].effectiveRadius, 1);
	inst_id = gl_InstanceID;
}