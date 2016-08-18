#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 vertexUV;

out vec3 norm;
out vec3 pos;
out vec2 UV;

//Uniforms
uniform mat4 sWVP;
uniform mat4 World;

uniform float ZFar;
uniform float ZNear;

out float flogz;
uniform float Fcoef;

void main()
{
	gl_Position = sWVP * World * vec4(position, 1);
	pos = (World * vec4(position, 1)).xyz;
	norm = (World * vec4(normal, 0)).xyz;
	
    //gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0;
	//flogz = 1.0 + gl_Position.w;

	UV = vertexUV;
}