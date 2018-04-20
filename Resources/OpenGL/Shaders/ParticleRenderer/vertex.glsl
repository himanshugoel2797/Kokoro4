#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec2 vertexUV;

uniform float ZNear;
uniform float ZFar;
uniform sampler2D PosData;
uniform vec3 Source;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform vec3 EyePos;

// Output data ; will be interpolated for each fragment.
flat out ivec2 UV;
out vec3 worldCoord;
out float flogz;
uniform float Fcoef;

void main(){
	vec4 data = vec4(Source, 0) + texelFetch(PosData, ivec2(position.xy), 0) * 50;

	vec4 tmpPos = World * vec4(data.xyz, 1);

	gl_Position = Projection * View * tmpPos;
	gl_PointSize = 1/length(EyePos - tmpPos.xyz) * (1 - data.w) * 0.1;
	// UV of the vertex. No special space for this one
	UV = ivec2(position.xy);
	worldCoord = tmpPos.xyz;
    gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0;
	flogz = 1.0 + gl_Position.w;
}