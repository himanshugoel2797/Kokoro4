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

void Scatter(float height, float sunAngle, float eyeAngle, vec3 sunDir, vec3 eyeDir, out vec4 Ray, out vec4 Mie) {
	vec3 GndTanDir = normalize(vec3(Rg, -height, 0));
    
	Ray = texture(ScatterCache, vec3((height - Rg) / (Rt - Rg), sunAngle * 0.5f + 0.5f, eyeAngle * 0.5f + 0.5f));
	Mie = texture(MieScatterCache, vec3((height - Rg) / (Rt - Rg), sunAngle * 0.5f + 0.5f, eyeAngle * 0.5f + 0.5f));

    float mu = dot(sunDir, eyeDir);
    float g = 0.76f;
	Ray.rgb *= 3.0f / (16.0f * PI) * (1 + mu * mu);
	Mie.rgb *= 3.0f / (8.0f * PI) * (1 - g * g) * (1 + mu * mu) / ((2 + g * g)  * pow(1 + g * g - 2 * g * mu, 3.0f / 2.0f));
	
	float nDL = dot(normalize(normal), sunDir);
	Ray.rgb *= nDL;
	Mie.rgb *= nDL;
	Ray.a = nDL;
	Mie.a = nDL;
}

void WithoutHeightField(float h, out vec4 Ray, out vec4 Mie) {
	vec3 P_g = normalize(normal) * (Rg + h);
	vec3 eD = normalize(P_g - EyePosition);	//R_v
	vec3 eP = EyePosition;

	float rayDist = 0;
	bool raySphere = sphere_dist(Rt, eP, eD, Rt * 4, -1, rayDist);
	if(raySphere && rayDist > 0)
		eP = eP + eD * rayDist;

	//Height is limited to the atmosphere
	float height = clamp(length(eP), Rg, Rt);

	//Calculate the view angle, sun angle and eye angle.
	float sunAngle = dot(normalize(eP), SunDir);

	//Eye angle is based on vertex position
	float eyeAngle = dot(normalize(eP), eD);

	Scatter(height, sunAngle, eyeAngle, SunDir, eD, Ray, Mie);
}

void WithHeightField(float h, out vec4 Ray, out vec4 Mie) {
	
	vec3 P_g = normalize(normal) * (Rg + h);
	vec3 eD = normalize(P_g - EyePosition);	//R_v
	vec3 eP = EyePosition;

	float rayDist = 0;
	bool raySphere = sphere_dist(Rt, eP, eD, Rt * 4, -1, rayDist);
	if(raySphere && rayDist > 0)
		eP = eP + eD * rayDist;

	//Height is limited to the atmosphere
	float height = clamp(Rg + h, Rg, Rt);

	//Calculate the view angle, sun angle and eye angle.
	float sunAngle = dot(normalize(P_g), SunDir);

	//Eye angle is based on vertex position
	float eyeAngle = dot(normalize(eP), eD);

	//color.rgb = vec3(sunAngle * 0.5f + 0.5f);
	Scatter(height, sunAngle, eyeAngle, SunDir, eD, Ray, Mie);
}

void main(){

	float h = texture(Cache, vec3(UV.x, UV.y, HeightMapData.HeightMaps[inst / 4][inst % 4])).r;
	
	vec4 r_data = vec4(0);
	vec4 m_data = vec4(0);
	WithoutHeightField(h, r_data, m_data);

	vec4 off_r_data = vec4(0);
	vec4 off_m_data = vec4(0);
	WithHeightField(h, off_r_data, off_m_data);

	color.rgb = ((r_data.rgb) + (m_data.rgb)) * 20;
	color.a = 1;
}