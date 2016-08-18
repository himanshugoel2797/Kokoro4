#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec4 shadowCoord;
in vec3 worldCoord;
in vec3 norm;
// Ouput data
layout(location = 0) out vec4 worldPos;
layout(location = 1) out vec4 normDat;
layout(location = 2) out vec4 color;
layout(location = 3) out vec4 bloom;
// Values that stay constant for the whole mesh.
uniform sampler2D AlbedoMap;
uniform sampler2D PackedMap;
uniform sampler2DShadow ShadowMap;
uniform sampler2D EmissionMap;
uniform sampler2D ReflectivePosMap;
uniform vec3 EyePos;

uniform mat4 View;
uniform mat4 World;

in float flogz;
uniform float Fcoef;
vec2 encode (vec3 n)
{
    return (vec2(atan(n.y, n.x)/3.1415926536, n.z)+1.0)*0.5;
}
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
mat3 cotangent_frame( vec3 N, vec3 p, vec2 uv )
{
    // get edge vectors of the pixel triangle
    vec3 dp1 = dFdx( p );
    vec3 dp2 = dFdy( p );
    vec2 duv1 = dFdx( uv );
    vec2 duv2 = dFdy( uv );
 
    // solve the linear system
    vec3 dp2perp = cross( dp2, N );
    vec3 dp1perp = cross( N, dp1 );
    vec3 T = dp2perp * duv1.x + dp1perp * duv2.x;
    vec3 B = dp2perp * duv1.y + dp1perp * duv2.y;
 
    // construct a scale-invariant frame 
    float invmax = inversesqrt( max( dot(T,T), dot(B,B) ) );
    return mat3( T * invmax, B * invmax, N );
}

vec3 perturb_normal( vec3 N, vec3 V, vec3 pert, vec2 texcoord )
{
    // assume N, the interpolated vertex normal and 
    // V, the view vector (vertex to eye)
    mat3 TBN = cotangent_frame( N, -V, texcoord );
    return normalize( TBN * pert );
}*/

float chain_rule(float dhdu, float dhdv, float dud, float dvd)
{
	return dhdu * dud + dhdv * dvd;
}

vec3 CalculateSurfaceGradient(vec3 n, vec3 dpdx, vec3 dpdy, float dhdx, float dhdy)
{
    vec3 r1 = cross(dpdy, n);
	vec3 r2 = cross(n, dpdx);
 
    return (r1 * dhdx + r2 * dhdy) / dot(dpdx, r1);
}


vec3 perturb_normal(vec3 N, vec3 p, vec2 d, vec2 texcoord)
{
	vec3 dpdx = dFdx(p);
	vec3 dpdy = dFdy(p);
	vec2 duv1 = dFdx(texcoord);
	vec2 duv2 = dFdy(texcoord); 

	float dhdx = chain_rule(d.x, d.y, duv1.x, duv1.y);
	float dhdy = chain_rule(d.x, d.y, duv2.x, duv2.y);

	return normalize(N - CalculateSurfaceGradient(N, dpdx, dpdy, dhdx, dhdy));
}


void main(){
    vec3 shad = shadowCoord.xyz / shadowCoord.w;
    shad = 0.5 * shad + 0.5;
    shad = max(vec3(0), min(vec3(1), shad));
	float vis = mix(0.1f, 1.0f, texture(ShadowMap, shad, 0.001));
	
	vec3 v = normalize(worldCoord - EyePos);
	vec4 tmp = texture2D(PackedMap, UV);
	vec4 n = World * vec4(perturb_normal(normalize(norm), worldCoord, tmp.rg * 2.0f + 1.0f, UV), 0);

    normDat.rg = encode(normalize(n.xyz));
    normDat.ba = tmp.ba;
	bloom = texture(EmissionMap, UV);
    worldPos.rgb = worldCoord;
    worldPos.a = vis;
    color = texture2D(AlbedoMap, UV);
    gl_FragDepth = Fcoef * 0.5 * log2(flogz);
}
