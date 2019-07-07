// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vs_pos;
layout(location = 1) in vec2 vs_uv;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;
out vec3 pos;
flat out uint drawID;

// Values that stay constant for the whole mesh.
uniform mat4 View;
uniform mat4 Projection;

layout (std430, binding = 0) buffer transforms_t
{ 
  mat4 World[MAX_DRAWS_UBO];
} Transforms;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	vec4 worldPos = Transforms.World[gl_BaseInstance + gl_InstanceID] * vec4(vs_pos.x, vs_pos.y, vs_pos.z, 1);
	gl_Position =  Projection * View * worldPos;

	// UV of the vertex. No special space for this one.
	UV = vs_uv;
	drawID = gl_DrawID;
	pos = worldPos.xyz;
	
	vec2 n = vs_normal / 100.0f * PI/180.0f;
	normal.x = cos(n.x) * sin(n.y);
	normal.y = sin(n.x) * sin(n.y);
	normal.z = cos(n.y);
}