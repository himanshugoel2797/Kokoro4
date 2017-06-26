// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vs_pos;
layout(location = 1) in vec2 vs_uv;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;
out vec3 EyeDir;

// Values that stay constant for the whole mesh.
uniform mat4 View;
uniform mat4 Projection;

uniform float Rt;
uniform vec3 EyePosition;

layout (std140) buffer transforms
{ 
  mat4 World[MAX_DRAWS_UBO];
} Transforms;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	mat4 MVP = Projection * View;// * Transforms.World[gl_InstanceID];
	gl_Position =  MVP * vec4(vs_pos.x * Rt, vs_pos.y * Rt, vs_pos.z * Rt, 1);

	// UV of the vertex. No special space for this one.
	UV = vs_uv;
	EyeDir = normalize(vs_pos);

	vec2 n = vs_normal / 100.0f * PI/180.0f;
	normal.x = cos(n.x) * sin(n.y);
	normal.y = sin(n.x) * sin(n.y);
	normal.z = cos(n.y);
}