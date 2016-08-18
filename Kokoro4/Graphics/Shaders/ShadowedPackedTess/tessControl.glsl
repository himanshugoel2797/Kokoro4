#version 430 core

layout(vertices = 3) out;

uniform vec3 EyePos;

in vec3 vPos_cs[];
in vec3 norm_cs[];
in vec2 UV_cs[];

out vec3 vPos_es[];
out vec3 norm_es[];
out vec2 UV_es[];

float GetTessLevel(float Distance0, float Distance1)
{
    float AvgDistance = (Distance0 + Distance1) / 2.0;

    if (AvgDistance <= 100) {
        return 7.0;
    }
    else if (AvgDistance <= 200) {
        return 5.0;
    }
    else {
        return 1.0;
    }
}

void main()
{
	vPos_es[gl_InvocationID] = vPos_cs[gl_InvocationID];
	norm_es[gl_InvocationID] = norm_cs[gl_InvocationID];
	UV_es[gl_InvocationID] = UV_cs[gl_InvocationID];

	float dist0 = distance(vPos_cs[0], EyePos);
	float dist1 = distance(vPos_cs[1], EyePos);
	float dist2 = distance(vPos_cs[2], EyePos);

	gl_TessLevelOuter[0] = GetTessLevel(dist1, dist2);
	gl_TessLevelOuter[1] = GetTessLevel(dist2, dist0);
	gl_TessLevelOuter[2] = GetTessLevel(dist0, dist1);
	gl_TessLevelInner[0] = gl_TessLevelOuter[2];
}