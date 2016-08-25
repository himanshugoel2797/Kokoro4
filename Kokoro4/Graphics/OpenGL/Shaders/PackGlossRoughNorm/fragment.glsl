#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 0) out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D GlossinessMap;
uniform sampler2D SpecularMap;
uniform sampler2D NormalMap;

vec2 encode (vec3 n)
{
    return (vec2(atan(n.y, n.x)/3.1415926536, n.z)+1.0)*0.5;
}

void main(){

	vec3 norm = texture2D(NormalMap, UV).rgb;
	float dx = norm.x / norm.z * 0.5f + 0.5f;
	float dy = norm.y / norm.z * 0.5f + 0.5f;

	color = vec4(dx, dy, texture2D(SpecularMap, UV).r, texture2D(GlossinessMap, UV).r);	//Pack the normal, roughness and glossiness map into one
}