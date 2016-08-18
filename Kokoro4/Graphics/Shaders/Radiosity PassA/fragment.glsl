

// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;
layout(location = 1) out vec4 PointLightPos;

// Values that stay constant for the whole mesh.
uniform sampler2D FRGBA;
uniform sampler2D FNormal;
uniform sampler2D FDepth;


#define AVG_SAMPLES 16
const vec2[] circleSamples = vec2[](
vec2(0.23390828009249556,-0.47285578945428747)
,
vec2(-0.5412048915778925,-0.7136079680414222)
,
vec2(-0.8189258081843398,-0.5049371571438046)
,
vec2(0.17251370806228303,-0.06742766435936354)
,
vec2(0.2672413998809714,-0.24500156426565128)
,
vec2(0.06263778087404967,0.6704992156819559)
,
vec2(0.1712678781718275,0.06631262534992063)
,
vec2(-0.8169431932182069,0.5073133035134255)
,
vec2(-0.9784363186305796,0.18441976788028816)
,
vec2(-0.21759157652213781,0.7674860228521244)
,
vec2(-0.22029976577795682,-0.7677111897721756)
,
vec2(-0.5385021129869465,0.7147615170773114)
,
vec2(0.2348092185916006,0.4708969066830925)
,
vec2(0.06060742356932678,-0.6718671091163962)
,
vec2(0.0015926448368210637,-5.073080190734773e-06)
,
vec2(-0.9791632092305752,-0.18132896245745006)
);

void main(){


	vec3 rndAVG = vec3(0);
	vec3 rndPosAVG = vec3(0);
	for(int i = 0; i < AVG_SAMPLES; i++)
	{
		rndAVG += texture2D(FRGBA, UV + circleSamples[i] * 0.01f).rgb;
		vec3 tmp = texture2D(FDepth, UV + circleSamples[i] * 0.01f).gbr;
		rndPosAVG += tmp;
	}


	rndAVG /= AVG_SAMPLES;

	
	PointLightPos.rgb = rndPosAVG/AVG_SAMPLES;
	PointLightPos.a = 1;

	color.rgb = rndAVG;
	color.a = 1;
}