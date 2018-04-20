// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vs_pos;
layout(location = 1) in vec2 vs_uv;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;
out vec3 vPos;

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	mat4 MVP = Projection * View * World;
	gl_Position =  MVP * vec4(vs_pos.x, vs_pos.y, vs_pos.z, 1);

	vPos = vs_pos;

	// UV of the vertex. No special space for this one.
	UV = vs_uv;
	//UV = (vs_pos.xz/vec2(50)+vec2(1,1))/2.0;

	vec2 n = vs_normal / 100.0f * PI/180.0f;
	normal.x = cos(n.x) * sin(n.y);
	normal.y = sin(n.x) * sin(n.y);
	normal.z = cos(n.y);
}