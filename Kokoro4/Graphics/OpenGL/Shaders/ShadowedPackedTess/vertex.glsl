#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 vertexUV;
// Output data ; will be interpolated for each fragment.

out vec3 vPos_cs;
out vec3 norm_cs;
out vec2 UV_cs;

uniform mat4 World;

void main(){

	vPos_cs = (World * vec4(vertexPosition_modelspace, 1)).xyz;
	norm_cs = (World * vec4(normal, 0)).xyz;
	UV_cs = vertexUV;
}
