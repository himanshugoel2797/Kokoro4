#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;
// Ouput data
layout(location = 0) out vec4 map;

//n = normal
//m = half
//a = roughness factor
float ggx(float nDotm, float a)
{
    float top = a * a;
    float pA = nDotm;
    pA = pA * pA * (top - 1) + 1;
    pA = pA * pA * 3.14159;
    return top/pA;
}

//f0 = fresnel factor
//h = half vector
//v = view vector
float fresnel_schlick(float f0,	vec3 h, vec3 v, float vDotH)
{
    return f0 + (1 - f0) * pow((1 - vDotH), 5);
}

float geometric_schlick(vec3 n, vec3 v, float k, float nDotV)
{
	return nDotV / (nDotV * (1 - k) + k);
}

void main(){
	//UV.x = [-1, 1], UV.y = [0, 1]

	map.r = ggx(2 * UV.x - 1, UV.y);
	map.g = fresnel_schlick(UV.y, vec3(0), vec3(0), 2 * UV.x - 1);
	map.b = geometric_schlick(vec3(0), vec3(0), UV.y, 2 * UV.x - 1);
	map.a = 1;
}



