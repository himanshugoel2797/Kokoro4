#version 430 core

layout(vertices = 3) out;

in vec2 UV[];
in vec3 worldXY[];
smooth in vec3 normPos[];

uniform mat4 World;

uniform float ZFar;
uniform float ZNear;

uniform vec3 EyePos;


out vec2 UV_o[];
out vec3 worldXY_o[];
out vec3 normPos_o[];

float GetTessLevel(float Distance0, float Distance1)
{
    float AvgDistance = (Distance0 + Distance1) / 2.0;

    return 10 - clamp(AvgDistance/10, 1, 10);
}

void main()
{
    // Set the control points of the output patch
    UV_o[gl_InvocationID] = UV[gl_InvocationID];
    normPos_o[gl_InvocationID] = normPos[gl_InvocationID];
    worldXY_o[gl_InvocationID] = worldXY[gl_InvocationID];

	// Calculate the distance from the camera to the three control points
    float EyeToVertexDistance0 = distance(EyePos, (World * vec4(worldXY[0], 1)).xyz );
    float EyeToVertexDistance1 = distance(EyePos, (World * vec4(worldXY[1], 1)).xyz);
    float EyeToVertexDistance2 = distance(EyePos, (World * vec4(worldXY[2], 1)).xyz);

    // Calculate the tessellation levels
    gl_TessLevelOuter[0] = GetTessLevel(EyeToVertexDistance1, EyeToVertexDistance2);
    gl_TessLevelOuter[1] = GetTessLevel(EyeToVertexDistance2, EyeToVertexDistance0);
    gl_TessLevelOuter[2] = GetTessLevel(EyeToVertexDistance0, EyeToVertexDistance1);
    gl_TessLevelInner[0] = gl_TessLevelOuter[2];
}