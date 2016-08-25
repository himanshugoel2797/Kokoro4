#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 vDirection;

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

void main(){
	gl_Position =  vec4(vertexPosition_modelspace, 1);

	const int div = 2;
	// UV of the vertex. No special space for this one.
	vec2 vertexUV = (vertexPosition_modelspace.xy+vec2(1,1))/2.0;
	UV = vertexUV;
	vDirection = vertexPosition_modelspace;
}