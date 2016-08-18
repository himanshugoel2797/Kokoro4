
// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec3 normal;
layout(location = 3) in vec3 tans;

// Output data
out vec2 UV;
out float depth;
out vec3 normPos;
out vec3 tangent;
out vec3 bitangent;

//Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform float ZFar;
uniform float ZNear;

void main()
{
	mat4 WVP = Projection * View * World;

	gl_Position = WVP * vec4(position, 1);
	normPos = (World * vec4(normal, 0)).xyz;
	depth = (gl_Position.z * gl_Position.w - ZNear)/(ZFar - ZNear);
	UV = vertexUV;

	tangent = tans;
	bitangent = cross(tangent, normal);
}