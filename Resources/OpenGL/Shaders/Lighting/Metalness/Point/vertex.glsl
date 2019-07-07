// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vs_pos;
layout(location = 1) in vec2 vs_uv;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;
flat out int drawID;

// Values that stay constant for the whole mesh.
uniform mat4 VP;

struct light_t {
    vec3 pos;
    float radius;
    vec3 color;
    float intensity;
};

layout(std430, binding = 1) buffer Lights_t {
    light_t v[];
} Light;

void main(){
    vec3 pos = Light.v[gl_BaseInstance + gl_InstanceID].pos;
    float rad = Light.v[gl_BaseInstance + gl_InstanceID].radius;

	gl_Position = VP * vec4(vs_pos.x * rad + pos.x, vs_pos.y * rad + pos.y, vs_pos.z * rad + pos.z, 1);
	UV = vs_uv;
	drawID = gl_BaseInstance + gl_InstanceID;
	vec2 n = vs_normal / 100.0f * PI/180.0f;
	normal.x = cos(n.x) * sin(n.y);
	normal.y = sin(n.x) * sin(n.y);
	normal.z = cos(n.y);
}