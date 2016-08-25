
// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 pos;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec3 normal;
layout(location = 3) in vec3 tans;

// Output data
out vec2 UV;
out float depth;
out vec3 worldXY;
smooth out vec3 normPos;
out vec3 tangent;
out vec3 bitangent;

//Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform float ZFar;
uniform float ZNear;

uniform float layer;

void main()
{
	mat4 WVP = Projection * View * World;

	vec3 position = pos + normal * layer * 0.05f;
	//position.z += max(0, layer - 5) * sin(layer * 0.05);
	gl_Position = WVP * vec4(position, 1);	//expand the model along its normal for every layer
	normPos = (World * vec4(normal, 0)).xyz;
	depth = (gl_Position.z * gl_Position.w - ZNear)/(ZFar - ZNear);
	worldXY = (World * vec4(position, 1)).xyz;
	UV = vertexUV;

	tangent = tans;
	bitangent = cross(tangent, normal);
}