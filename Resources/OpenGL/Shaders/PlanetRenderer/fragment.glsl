// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 normal;
in flat int inst;

// Ouput data
layout(location = 0) out vec4 color;
layout (bindless_sampler) uniform sampler2DArray Cache;

layout (std140) uniform heightmaps
{
	ivec4 HeightMaps[MAX_DRAWS_UBO];
} HeightMapData;

layout(rgba16f, bindless_sampler) uniform sampler2D TransCache;
layout(rgba16f, bindless_sampler) uniform sampler3D ScatterCache;
layout(rgba16f, bindless_sampler) uniform sampler3D MieScatterCache;

uniform float Radius;
uniform vec3 SunDir;
uniform vec3 EyePosition;
uniform float Rt;
uniform float Rg;

bool sphere_dist(in float r, in vec3 pos, in vec3 dir, in float tmax, in float mode, out float t) {

    float b = dot(dir, pos);
    float c = dot(pos, pos) - r * r;
    float tmp = b * b - c;
    if (tmp >= 0.0) t = - b + mode * sqrt(tmp);

    return tmp >= 0.0 && t <= tmax;
}

vec4 T(float h, float CosTheta) {
    return textureLod(TransCache, vec2(acos(CosTheta) / PI, (h - Rg) / (Rt - Rg)), 0);
}

float RayleighPhase(float mu)
{
    return 3.0f / (16.0f * PI) * (1 + mu * mu);
}

#define g 0.76f
float MiePhase(float mu)
{
    float numerator = (1 - g * g) * (1 + mu * mu);
    float denom = (2 + g * g) * pow(1 + g * g - 2 * g * mu, 3.0f / 2.0f);

    return 3.0f / (8.0f * PI) * numerator / denom;
}

void Scatter(float height, float sunAngle, float eyeAngle, vec3 sunDir, vec3 eyeDir, out vec4 Ray, out vec4 Mie) {
	vec3 GndTanDir = normalize(vec3(Rg, -height, 0));
    
	vec3 coord = vec3(0);
	coord.x = (height - Rg) / (Rt - Rg);
	coord.y = acos(sunAngle) / PI;
	coord.z = acos(eyeAngle) / PI;

	Ray = texture(ScatterCache, coord);
	Mie = texture(MieScatterCache, coord);

	float mu = dot(sunDir, eyeDir);
	Ray.rgb *= RayleighPhase(mu);
	Mie.rgb *= MiePhase(mu);

    //float mu = eyeAngle;
    //float g = 0.76f;
	//Ray.rgb *= 3.0f / (16.0f * PI) * (1 + mu * mu);
	//Mie.rgb *= 3.0f / (8.0f * PI) * (1 - g * g) * (1 + mu * mu) / ((2 + g * g)  * pow(1 + g * g - 2 * g * mu, 3.0f / 2.0f));
	
	//float nDL = dot(normalize(normal), sunDir);
	//Ray.rgb *= nDL;
	//Mie.rgb *= nDL;
	//Ray.a = nDL;
	//Mie.a = nDL;
}

void WithoutHeightField(vec3 eP, vec3 P_g, vec3 eD, vec3 sunDir, float h, out vec4 Ray, out vec4 Mie) {
	//Height is limited to the atmosphere
	float height = clamp(length(eP), Rg, Rt);		//h
	
	//Eye angle is based on vertex position
	float eyeAngle = dot(normalize(eP), eD);		//theta

	//Calculate the view angle, sun angle and eye angle.
	float sunAngle = dot(normalize(eP), SunDir);	//sigma

	Scatter(height, sunAngle, eyeAngle, sunDir, eD, Ray, Mie);
}

void WithHeightField(vec3 eP, vec3 P_g, vec3 eD, vec3 sunDir, float h, out vec4 Ray, out vec4 Mie) {

	//Height is limited to the atmosphere
	float height = clamp(Rg + h, Rg, Rt);

	//Eye angle is based on vertex position
	float eyeAngle = dot(normalize(eP), eD);	//theta
	
	//Calculate the view angle, sun angle and eye angle.
	float sunAngle = dot(normalize(P_g), SunDir);	//sigma
	
	Scatter(height, sunAngle, eyeAngle, sunDir, eD, Ray, Mie);
}

void Sunlight(vec3 eP, vec3 P_g, vec3 eD, vec3 sunDir, vec4 col, out vec4 I_r_gv, out vec4 I_m_gv)
{
	float height = clamp(length(P_g), Rg, Rt);
	float eyeHeight = clamp(length(eP), Rg, Rt);

	float sunAngle = dot(normalize(P_g), sunDir);	//sigma

	vec4 T_gc = T(height, sunAngle);
	
	float angle_GtoV = dot(normalize(P_g), eD);
	vec4 T_ga = T(height, angle_GtoV);
	vec4 T_va = T(eyeHeight, angle_GtoV);

	vec4 T_gv = T_ga - T_va;

	//line 17
	vec4 T_net = exp(-T_gc - T_gv);
	I_r_gv = col * T_net;
	I_m_gv = col * T_net.aaaa;

	//T_r_net *= exp( -(height - Rg) / 8 );
	//T_m_net *= exp( -(height - Rg) / 1.2 );

	//T_r_net.rgb *= vec3(5.8e-3f, 1.35e-2f, 3.31e-2f);
	//T_m_net *= 20e-3f;

	float mu = dot(sunDir, eD);
	I_r_gv *= RayleighPhase(mu);
	I_m_gv *= MiePhase(mu);
    //T_r_net *= 3.0f / (16.0f * PI) * (1 + mu * mu);
	//T_m_net *= 3.0f / (8.0f * PI) * (1 - g * g) * (1 + mu * mu) / ((2 + g * g)  * pow(1 + g * g - 2 * g * mu, 3.0f / 2.0f));
	

	//color.rgb = vec3(sunAngle * 0.5f + 0.5f);
}

void main(){

	float h_orig = texture(Cache, vec3(UV.x, UV.y, HeightMapData.HeightMaps[inst / 4][inst % 4])).r; 
	float h = clamp(exp(h_orig * 2), 0, Rt - Rg);
	
	vec3 P_g = normalize(normal) * (Rg + h);
	vec3 eD = normalize(P_g - EyePosition);	//R_v
	vec3 eP = EyePosition;
	
	float rayDist = 0;
	bool raySphere = sphere_dist(Rt, eP, eD, Rt * 4, -1, rayDist);
	if(raySphere && rayDist > 0)
		eP = eP + eD * rayDist;
		
	float nDL = dot(normalize(normal), SunDir);
	color.rgb = mix(vec3(0, 0, 0.6f), mix(vec3(0.2f, 0.8f, 0.2f), vec3(0.9f, 0.9f, 0.9f), smoothstep(0.85f, 1.0f, h_orig)) , step(0.7f, h_orig));// + vec3(1.0f, 1.0f, 1.0f) * smoothstep(0.88f, 1.0f, h);
	
	vec4 r_data = vec4(0);
	vec4 m_data = vec4(0);
	WithoutHeightField(eP, P_g, eD, SunDir, h, r_data, m_data);

	vec4 off_r_data = vec4(0);
	vec4 off_m_data = vec4(0);
	WithHeightField(eP, P_g, eD, SunDir, h, off_r_data, off_m_data);
	
	vec4 I_r_gv = vec4(0);
	vec4 I_m_gv = vec4(0);
	Sunlight(eP, P_g, eD, SunDir, color, I_r_gv, I_m_gv);

	vec3 rayleigh_f = r_data.rgb - off_r_data.rgb + I_r_gv.rgb * nDL;
	vec3 mie_f = m_data.rgb - off_m_data.rgb + I_m_gv.rgb * nDL;
	
	color.rgb = vec3(1, 0.96f, 0.949f) * 15 * (rayleigh_f + mie_f);
	//color.rgb *= nDL;

	color.a = 1;
}