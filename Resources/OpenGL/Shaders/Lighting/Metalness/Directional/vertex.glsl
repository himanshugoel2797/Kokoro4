// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vs_pos;
layout(location = 1) in vec2 vs_uv;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;
flat out int drawID;

// Values that stay constant for the whole mesh.
uniform mat4 View;
uniform mat4 Projection;

layout (std140) buffer transforms
{ 
  mat4 World[MAX_DRAWS_UBO];
} Transforms;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	mat4 MVP = Projection * View * Transforms.World[gl_BaseInstance + gl_InstanceID];
	gl_Position =  MVP * vec4(vs_pos.x, vs_pos.y, vs_pos.z, 1);

	// UV of the vertex. No special space for this one.
	UV = vs_uv;
	drawID = gl_DrawID;
	//UV = (vs_pos.xz/vec2(50)+vec2(1,1))/2.0;

	vec2 n = vs_normal / 100.0f * PI/180.0f;
	normal.x = cos(n.x) * sin(n.y);
	normal.y = sin(n.x) * sin(n.y);
	normal.z = cos(n.y);
}