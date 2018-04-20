#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 vertexUV;
// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec4 shadowCoord;
out vec3 worldCoord;
out vec3 norm;

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform mat4 sWVP;
uniform float ZNear;
uniform float ZFar;

out float flogz;
uniform float Fcoef;

void main(){
    // Output position of the vertex, in clip space : MVP * position
	
	vec4 tmp = World * vec4(vertexPosition_modelspace, 1);

    gl_Position =  Projection * View * tmp;
    shadowCoord = sWVP * tmp;

	worldCoord = tmp.xyz;
	//worldCoord = (World * vec4(vertexPosition_modelspace, 1)).xyz;
	norm = (World * vec4(normal, 0)).xyz;
	
    gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0;
	flogz = 1.0 + gl_Position.w;

    // UV of the vertex. No special space for this one.
	UV = vertexUV;
}
