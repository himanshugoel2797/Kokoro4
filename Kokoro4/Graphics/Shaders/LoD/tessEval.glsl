#version 430 core

layout(triangles, equal_spacing, ccw) in;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform float ZNear;
uniform float ZFar;


in vec2 UV_o[];
in vec3 worldXY_o[];
in vec3 normPos_o[];

// Output data
out vec2 UV_o2;
out float depth_o2;
out vec3 worldXY_o2;
smooth out vec3 normPos_o2;
out vec2 logBufDat;

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y) * v1 + vec3(gl_TessCoord.z) * v2;
}

void main()
{
UV_o2 = interpolate2D(UV_o[0], UV_o[1], UV_o[2]);
   	normPos_o2 = interpolate3D(normPos_o[0], normPos_o[1], normPos_o[2]);
   	normPos_o2 = normalize(normPos_o2);
   	vec3 position = (interpolate3D(worldXY_o[0], worldXY_o[1], worldXY_o[2]));

	mat4 WVP = Projection * View * World;

	float FCOEF = 2.0 / log2(ZFar + 1.0);
	logBufDat.x = 0.5f * FCOEF;

	gl_Position = WVP * vec4(position, 1);

	depth_o2 = (gl_Position.z * gl_Position.w - ZNear)/(ZFar - ZNear);

	gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * FCOEF - ZNear;
	logBufDat.y = 1.0 + gl_Position.w;
	worldXY_o2 = (World * vec4(position, 1)).xyz;
}