#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;
// Ouput data
layout(location = 0) out vec4 lit;
// Values that stay constant for the whole mesh.
uniform sampler2D colorMap;
uniform sampler2D normData;
uniform sampler2D worldData;
uniform sampler2D ssrMap;
uniform sampler2D depthBuffer;
uniform vec3 lDir;
uniform vec3 lColor;
uniform vec3 EyePos;
uniform mat4 Projection;
uniform mat4 View;
uniform mat4 InvProjection;
uniform mat4 InvView;
uniform float Fcoef;
uniform float ZFar;
uniform vec3 EyeDir;

vec3 decode (vec2 enc)
{
    vec2 ang = enc*2-1;
    vec2 scth;
    scth.x = sin(ang.x * 3.1415926536f);
    scth.y = cos(ang.x * 3.1415926536f);
    vec2 scphi = vec2(sqrt(1.0 - ang.y*ang.y), ang.y);
    return vec3(scth.y*scphi.x, scth.x*scphi.x, scphi.y);
}




/*
//n = normal
//m = half
//a = roughness factor
float ggx(vec3 n, vec3 m, float a)
{
    float top = a * a;
    float pA = dot(n, m);
    pA = pA * pA * (top - 1) + 1;
    pA = pA * pA * 3.1415926536f;
    return top/pA;
}


//f0 = fresnel factor
//h = half vector
//v = view vector
float fresnel_schlick(float f0,	vec3 h, vec3 v, float vDotH)
{
    return mix(pow((1 - vDotH), 5), 1, f0);
}


float geometric_schlick(vec3 n, vec3 v, float k, float nDotV)
{
    return nDotV / mix(max(0.000000001, nDotV), 1, k);
}


float geometric_smith_schlick(vec3 l, vec3 v, vec3 h, vec3 n, float r, float nDotL, float nDotV, float vDotH)
{
    float k = r * 0.797884;
    return geometric_schlick(n, l, k, nDotL) * geometric_schlick(n, v, k, nDotV);
}


vec4 cooktorr(vec3 n, vec3 v, vec3 l, float f0, float r, vec4 spec, vec4 dif)
{
    vec3 h = normalize((l + v));
    float nDotV = dot(n, v);
    float nDotL = dot(n, l);
    float vDotH = dot(v, h);
    float roughness = ggx(n, h, r * r);
    float fresnel = fresnel_schlick(f0, h, v, vDotH);
    float geometric = geometric_smith_schlick(l, v, h, n, r * r, nDotL, nDotV, vDotH);
    float rs = fresnel * roughness * geometric/(3.1415926536f * nDotV * nDotL);
    //rs = max(0.0, rs);
    return vec4(spec * rs + dif * fresnel);
}

float ggx_t(float vDotH, float nDotV, float r)
{
	return 1/(nDotV + sqrt( (nDotV - nDotV * r) * nDotV + r ));
}

*/


vec4 o_cooktorr(vec3 n, vec3 v, vec3 l, float f0, float r, vec4 spec, vec4 dif)
{
    vec3 h = normalize(l + v);
    float nDotV = abs(dot(n, v)) + 1e-6;
    float nDotL = clamp(dot(n, l), 0, 1);
    float vDotH = clamp(dot(v, h), 0, 1);
    
	float nDotH = dot(n, h);
    nDotH = nDotH * nDotH;
	
	float top = r * r;
    float pA = nDotH * (top - 1) + 1;
    float roughness = top/(pA * pA);
    float fresnel = (1 - f0) * pow((1 - vDotH), 5) + f0;
    float k = r * 0.797884;
	float geometric = 1 / ((1 - k) * nDotV + k) * 1/ ((1 - k) * nDotL + k);

    vec4 rs = spec * fresnel * roughness * geometric * 0.101321184;
    // 0.101321184 = 1/(PI * PI)
	return vec4(dif * fresnel + rs);
}





void main(){
    vec4 dif = texture(colorMap, UV);
    vec3 worldCoord = texture(worldData, UV).rgb;
    vec3 l = lDir;
	vec4 tmp = texture(normData, UV);
	
	//rg = normals, b = specular factor, a = glossiness
    vec3 n = decode(tmp.rg);
    lit = vec4(lColor.rgb, 1) * o_cooktorr(n, normalize(worldCoord.xyz - EyePos), l, 1 - tmp.b, tmp.a * tmp.a, textureLod(ssrMap, UV, mix(9, 0, tmp.a * tmp.a)), dif);
}







