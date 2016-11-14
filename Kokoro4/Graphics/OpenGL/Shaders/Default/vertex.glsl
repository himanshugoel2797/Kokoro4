// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

layout (std140) uniform transforms
{ 
  mat4 World[MAX_DRAWS_UBO];
} Transforms;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	mat4 MVP = Projection * View * Transforms.World[gl_DrawIDARB];
	gl_Position =  MVP * vec4(vertexPosition_modelspace, 1);

	// UV of the vertex. No special space for this one.
	UV = vertexUV;

	vec2 n = vs_normal / 100.0f * PI/180.0f;
	normal.x = cos(n.x) * sin(n.y);
	normal.y = sin(n.x) * sin(n.y);
	normal.z = cos(n.y);
}