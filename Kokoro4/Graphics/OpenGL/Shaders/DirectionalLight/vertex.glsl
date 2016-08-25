#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 vertexUV;
// Output data ; will be interpolated for each fragment.
out vec2 UV;

// Values that stay constant for the whole mesh.

void main(){
	gl_Position = vec4(position, 1);

	// UV of the vertex. No special space for this one
	UV = (position.xy+vec2(1,1))/2.0;
}
