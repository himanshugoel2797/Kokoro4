#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 2) in vec2 vertexUV;
layout(location = 3) in vec4 lightData;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 lPos;
out vec4 lColor;	//.a stores the attenuation factor

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform mat4 sWVP;
uniform sampler2D ShadowMap;
uniform sampler2D ReflectiveColMap;
uniform sampler2D ReflectivePosMap;

out float flogz;
uniform float Fcoef;

void main(){

	vec3 vertPos = vec3(0);
	vec3 v = vertexPosition_modelspace;

	// Output position of the vertex, in clip space : MVP * position
	mat4 MVP = Projection * View * World;
	gl_Position =  MVP * vec4(vertexPosition_modelspace, 1);
	
    gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0;
	flogz = 1.0 + gl_Position.w;

	// UV of the vertex. No special space for this one.
	UV = vertexUV;
}