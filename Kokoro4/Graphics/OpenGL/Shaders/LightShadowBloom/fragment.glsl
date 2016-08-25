#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;
// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.
uniform sampler2D LitMap;
uniform sampler2D BloomMap;
uniform sampler2D ShadowMap;
uniform sampler2D DiffuseMap;
uniform sampler2D depthBuffer;
uniform sampler2D giBuffer;
uniform sampler2D AvgColor;
vec3 Uncharted2Tonemap(vec3 x)
{
    const float A = 0.15;
    const float B = 0.50;
    const float C = 0.10;
    const float D = 0.20;
    const float E = 0.02;
    const float F = 0.30;
    return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
}





void main(){
	float weight = min(texture(ShadowMap, UV).a, 1);

    color = (texture(LitMap, UV) * weight + texture(BloomMap, UV));
	color *= 1.25;
    // Hardcoded Exposure Adjustment
	color = color/(1+color);
    gl_FragDepth = texture(depthBuffer, UV).r;
}


