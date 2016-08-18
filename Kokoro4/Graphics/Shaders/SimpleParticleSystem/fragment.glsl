#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 0) out vec4 c_posData;
layout(location = 1) out vec4 c_dVData;


// Values that stay constant for the whole mesh.
uniform vec2 MassRange;
uniform sampler2D PosData;
uniform sampler2D dVData;
uniform vec3 Impulse;
uniform float DeltaTime;
uniform float AgeRate;
uniform vec3 EmitterPosition;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

vec3 sphericalRand(vec3 a)
{
	float theta = 2 * 3.14159 * rand(a.xy);
    float phi = acos(2 * rand(a.xz) - 1.0);
    return vec3(cos(theta) * sin(phi), sin(theta) * sin(phi), cos(phi));
}

void main(){
	c_dVData = texture2D(dVData, UV);
	c_posData = texture2D(PosData, UV);

	c_posData.xyz = c_posData.xyz + c_dVData.xyz * DeltaTime;
	c_dVData.xyz = Impulse * c_dVData.w * DeltaTime;
	c_posData.w = c_posData.w + AgeRate;
	if(c_posData.w > 1){
		c_posData.xyz = EmitterPosition + sphericalRand(gl_FragCoord.zyx)  * 0.0025;
		c_posData.w = -rand(c_posData.xy);
		c_dVData.xyz = sphericalRand(gl_FragCoord.xyz) * 0.0025;
		c_dVData.w = 0.2 + rand(c_dVData.xy);

	}
}