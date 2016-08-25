#version 430 core

in vec2 UV;

layout(location = 0) out vec4 reflection;

uniform sampler2D colorMap;
uniform sampler2D normData;
uniform sampler2D specularData;
uniform sampler2D worldData;
uniform sampler2D envMap;
uniform sampler2D depthMap;
uniform vec3 giLightDirection;
uniform vec3 EyePos;
uniform mat4 View;
uniform mat4 Projection;


vec3 decode (vec2 enc)
{
	vec2 ang = enc*2-1;
    vec2 scth;
    scth.x = sin(ang.x * 3.1415926536f);
	scth.y = cos(ang.x * 3.1415926536f);
    vec2 scphi = vec2(sqrt(1.0 - ang.y*ang.y), ang.y);
    return vec3(scth.y*scphi.x, scth.x*scphi.x, scphi.y);
}

float rand(vec4 seed4)
{
    float dot_product = dot(seed4, vec4(12.9898,78.233,45.164,94.673));
    return fract(sin(dot_product) * 43758.5453);
}

void main(){
vec2 poissonDisk[64];
poissonDisk[0] = vec2(-0.613392, 0.617481);
poissonDisk[1] = vec2(0.170019, -0.040254);
poissonDisk[2] = vec2(-0.299417, 0.791925);
poissonDisk[3] = vec2(0.645680, 0.493210);
poissonDisk[4] = vec2(-0.651784, 0.717887);
poissonDisk[5] = vec2(0.421003, 0.027070);
poissonDisk[6] = vec2(-0.817194, -0.271096);
poissonDisk[7] = vec2(-0.705374, -0.668203);
poissonDisk[8] = vec2(0.977050, -0.108615);
poissonDisk[9] = vec2(0.063326, 0.142369);
poissonDisk[10] = vec2(0.203528, 0.214331);
poissonDisk[11] = vec2(-0.667531, 0.326090);
poissonDisk[12] = vec2(-0.098422, -0.295755);
poissonDisk[13] = vec2(-0.885922, 0.215369);
poissonDisk[14] = vec2(0.566637, 0.605213);
poissonDisk[15] = vec2(0.039766, -0.396100);
poissonDisk[16] = vec2(0.751946, 0.453352);
poissonDisk[17] = vec2(0.078707, -0.715323);
poissonDisk[18] = vec2(-0.075838, -0.529344);
poissonDisk[19] = vec2(0.724479, -0.580798);
poissonDisk[20] = vec2(0.222999, -0.215125);
poissonDisk[21] = vec2(-0.467574, -0.405438);
poissonDisk[22] = vec2(-0.248268, -0.814753);
poissonDisk[23] = vec2(0.354411, -0.887570);
poissonDisk[24] = vec2(0.175817, 0.382366);
poissonDisk[25] = vec2(0.487472, -0.063082);
poissonDisk[26] = vec2(-0.084078, 0.898312);
poissonDisk[27] = vec2(0.488876, -0.783441);
poissonDisk[28] = vec2(0.470016, 0.217933);
poissonDisk[29] = vec2(-0.696890, -0.549791);
poissonDisk[30] = vec2(-0.149693, 0.605762);
poissonDisk[31] = vec2(0.034211, 0.979980);
poissonDisk[32] = vec2(0.503098, -0.308878);
poissonDisk[33] = vec2(-0.016205, -0.872921);
poissonDisk[34] = vec2(0.385784, -0.393902);
poissonDisk[35] = vec2(-0.146886, -0.859249);
poissonDisk[36] = vec2(0.643361, 0.164098);
poissonDisk[37] = vec2(0.634388, -0.049471);
poissonDisk[38] = vec2(-0.688894, 0.007843);
poissonDisk[39] = vec2(0.464034, -0.188818);
poissonDisk[40] = vec2(-0.440840, 0.137486);
poissonDisk[41] = vec2(0.364483, 0.511704);
poissonDisk[42] = vec2(0.034028, 0.325968);
poissonDisk[43] = vec2(0.099094, -0.308023);
poissonDisk[44] = vec2(0.693960, -0.366253);
poissonDisk[45] = vec2(0.678884, -0.204688);
poissonDisk[46] = vec2(0.001801, 0.780328);
poissonDisk[47] = vec2(0.145177, -0.898984);
poissonDisk[48] = vec2(0.062655, -0.611866);
poissonDisk[49] = vec2(0.315226, -0.604297);
poissonDisk[50] = vec2(-0.780145, 0.486251);
poissonDisk[51] = vec2(-0.371868, 0.882138);
poissonDisk[52] = vec2(0.200476, 0.494430);
poissonDisk[53] = vec2(-0.494552, -0.711051);
poissonDisk[54] = vec2(0.612476, 0.705252);
poissonDisk[55] = vec2(-0.578845, -0.768792);
poissonDisk[56] = vec2(-0.772454, -0.090976);
poissonDisk[57] = vec2(0.504440, 0.372295);
poissonDisk[58] = vec2(0.155736, 0.065157);
poissonDisk[59] = vec2(0.391522, 0.849605);
poissonDisk[60] = vec2(-0.620106, -0.328104);
poissonDisk[61] = vec2(0.789239, -0.419965);
poissonDisk[62] = vec2(-0.545396, 0.538133);
poissonDisk[63] = vec2(-0.178564, -0.596057);

    vec3 n = decode(texture2D(normData, UV).xy);
    n = normalize(n);
    vec3 worldCoord = texture2D(worldData, UV).rgb;
    vec3 v = EyePos - worldCoord;
    v = normalize(-v);
	
	float ao = 0;

	for(int i = 0; i < 8; i++)
	{
		float d_a = texture2D(depthMap, UV + poissonDisk[i] * 0.025).r;
		vec3 n_a = decode(texture2D(normData, UV + poissonDisk[i] * 0.025).xy);
		ao += dot(n_a, n);

	}

	ao = ao / 8;

	reflection = vec4(ao);

	/*
	float ao = depth;
	vec3 active_samples = vec3(0);
	for(int i = 0; i < 16; i++)
	{
		vec2 uv_c = UV + poissonDisk[i] * 0.007;

		float d_a = texture2D(depthMap, uv_c).r;
		vec3 n_a = decode(texture2D(normData, uv_c).xy);
		active_samples += max(0.3, dot(n_a, -giLightDirection)) * step(depth, d_a);
	}*/

	//reflection = vec4(active_samples/8, 1);
}





