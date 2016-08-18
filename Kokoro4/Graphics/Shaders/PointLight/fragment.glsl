#version 430 core

// Interpolated values from the vertex shaders
// Interpolated values from the vertex shaders
in vec2 UV;
in float flogz;
// Ouput data
layout(location = 0) out vec4 lit;
// Values that stay constant for the whole mesh.
uniform sampler2D colorMap;
uniform sampler2D normData;
uniform sampler2D worldData;
uniform sampler2D ssrMap;
uniform sampler2D depthBuffer;
uniform vec3 lPos;
uniform vec4 lColor;
uniform vec3 EyePos;
uniform mat4 Projection;
uniform mat4 View;
uniform mat4 InvProjection;
uniform mat4 InvView;
uniform float Fcoef;
uniform float ZFar;
uniform vec2 ScreenSize;
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

vec4 o_cooktorr(vec3 n, vec3 v, vec3 l, float f0, float r, vec4 spec, vec4 dif, float dist, float atten)
{
    vec3 h = normalize((l + v));
    float nDotV = abs(dot(n, v)) + 1e-6;
    float nDotL = max(0, min(1, dot(n, l)));
    float vDotH = max(0, min(1, dot(v, h)));
    float nDotH = max(0, min(1, dot(n, h)));
    float top = r * r;
    float pA = dot(n, h) * dot(n, h) * (top - 1) + 1;
    float roughness = top/(pA * pA);
    vec4 fresnel = (vec4(1 - f0)) * vec4(pow((1 - vDotH), 5)) + vec4(f0);
    float k = r * 0.797884;
	//fresnel *= 0.04;

	float geometric = 0;//ggx_t(nDotH, nDotV, r) * ggx_t(nDotL, nDotL, r);
	geometric = 1 / mix(nDotV, 1, k) * 1/ mix(nDotL, 1, k);

    vec4 rs = spec * fresnel * roughness * geometric * 0.101321184;
    // 0.101321184 = 1/(PI * PI)
	//rs = max(vec4(0), rs);
	//return vec4(roughness);
	//return spec;
	return vec4((rs + dif * fresnel)/(atten * (dist + 1) * (dist + 1)));
}

void main(){

	vec2 uv_coord = gl_FragCoord.xy/ScreenSize;
    vec3 worldCoord = texture2D(worldData, uv_coord).rgb;
    vec3 l = normalize(worldCoord - lPos.xyz);
    vec3 v = EyePos - worldCoord;
    v = normalize(-v);

	float dist = distance(worldCoord, lPos.xyz);
	//dist += 0.001f;

    vec4 tmp = texture2D(normData, UV);
	
	vec3 n = normalize(decode(tmp.rg));
    vec4 dif = texture2D(colorMap, uv_coord);
    const float f0 = 1;
	
    lit = vec4(lColor.rgb, 1) * o_cooktorr(n, normalize(worldCoord.xyz - EyePos), l, 1 - tmp.b, tmp.a * tmp.a, textureLod(ssrMap, UV, mix(9, 0, tmp.a * tmp.a)), dif, dist, lColor.a);

    gl_FragDepth = Fcoef * 0.5 * log2(flogz);
}

