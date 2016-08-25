#version 430 core

layout(triangles, equal_spacing, ccw) in;

out vec2 UV;
out vec4 shadowCoord;
out vec3 worldCoord;
out vec3 norm;

in vec3 vPos_es[];
in vec3 norm_es[];
in vec2 UV_es[];

// Values that stay constant for the whole mesh.

uniform mat4 View;
uniform mat4 Projection;
uniform mat4 sWVP;
uniform float ZNear;
uniform float ZFar;
uniform sampler2D PackedMap;

out float flogz;
uniform float Fcoef;

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
    return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
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

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
    return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y) * v1 + vec3(gl_TessCoord.z) * v2;
}


void main()
{
    // Interpolate the attributes of the output vertex using the barycentric coordinates
   	UV = interpolate2D(UV_es[0], UV_es[1], UV_es[2]);
    norm = interpolate3D(norm_es[0], norm_es[1], norm_es[2]);
    norm = normalize(norm);
    worldCoord = interpolate3D(vPos_es[0], vPos_es[1], vPos_es[2]) + -decode(texture2D(PackedMap, UV).rg).y * norm;

    gl_Position =  Projection * View * vec4(worldCoord, 1);
    shadowCoord = sWVP * vec4(worldCoord, 1);
	
    gl_Position.z = log2(max(1e-6, 1.0 + gl_Position.w)) * Fcoef - 1.0;
	flogz = 1.0 + gl_Position.w;
}
