
// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 2) in vec2 vertexUV;

uniform float ZNear;
uniform float ZFar;

// Values that stay constant for the whole mesh.
uniform mat4 View;
uniform mat4 Projection;

layout(std140) uniform Material_t {
	uvec2 Albedo;
	vec4 lHandPos;	//xy = hand, zw = tip
	vec4 rHandPos;	//xy = hand, zw = tip
	vec2 frameDim;
	float PointCount;
} Material;

layout(std140) buffer PointCloud_t
{
    vec4 points[MAX_DRAWS_SSBO];
} PointCloud;

void main(){
	gl_Position = Projection * View * vec4(PointCloud.points[gl_VertexID].xyz, 1);
}