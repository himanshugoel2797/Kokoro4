﻿// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vs_pos;
layout(location = 1) in vec2 vs_uv;
layout(location = 2) in vec2 vs_normal;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec3 normal;
out flat int inst;

// Values that stay constant for the whole mesh.
uniform mat4 View;
uniform mat4 Projection;

layout (std140) buffer transforms
{ 
  vec4 XYZPosition_WScale[MAX_DRAWS_UBO];
} Transforms;

layout (std140) buffer heightmaps
{
	uvec2 HeightMaps[MAX_DRAWS_UBO];
} HeightMapData;



void main(){

	//Set the view position to zero and update all the vertex positions to be relative to the camera.
	mat4 tmpView = View;
	tmpView[3].xyz = vec3(0);
	mat4 MVP = Projection * tmpView;
	
	vec2 n = vs_normal / 100.0f * PI/180.0f;
	vec3 vnorm = vec3(0);
	vnorm.x = cos(n.x) * sin(n.y);
	vnorm.y = sin(n.x) * sin(n.y);
	vnorm.z = cos(n.y);

	vec3 vpos = vs_pos;
	vpos = vec3(vpos.x * Transforms.XYZPosition_WScale[gl_InstanceID].w + Transforms.XYZPosition_WScale[gl_InstanceID].x, 
							  vpos.y * Transforms.XYZPosition_WScale[gl_InstanceID].w + Transforms.XYZPosition_WScale[gl_InstanceID].y, 
							  vpos.z * Transforms.XYZPosition_WScale[gl_InstanceID].w + Transforms.XYZPosition_WScale[gl_InstanceID].z);

	vpos += vnorm * texture(sampler2D(HeightMapData.HeightMaps[gl_InstanceID]), vs_uv).x;

	gl_Position =  MVP * vec4(vpos.x, vpos.y, vpos.z, 1);

	inst = gl_InstanceID;
	UV = vs_uv;
	normal = vnorm;
}