#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 2) in vec2 vertexUV;
layout(location = 1) in vec3 normal;

// Output data
out vec2 UV;
out float depth;
out vec3 worldXY;
smooth out vec3 normPos;
out vec2 logBufDat;


//Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform float ZFar;
uniform float ZNear;

void main()
{
	mat4 WVP = Projection * View * World;

	float FCOEF = 2.0 / log2(ZFar + 1.0);
	logBufDat.x = 0.5f * FCOEF;

	gl_Position = WVP * vec4(position, 1);

	depth = (gl_Position.z * gl_Position.w - ZNear)/(ZFar - ZNear);

	gl_Position.z = log2(max(ZNear, 1.0 + gl_Position.w)) * FCOEF - 1.0;
	logBufDat.y = 1.0 + gl_Position.w;

	normPos = (World * vec4(normal, 0)).xyz;
	worldXY = (World * vec4(position, 1)).xyz;
	UV = vertexUV;
}