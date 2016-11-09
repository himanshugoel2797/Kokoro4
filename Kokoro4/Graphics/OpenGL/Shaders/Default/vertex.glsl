// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 2) in vec2 vertexUV;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	mat4 MVP = Projection * View * World;
	gl_Position =  MVP * vec4(vertexPosition_modelspace, 1);

	// UV of the vertex. No special space for this one.
	UV = vertexUV;
}