// Interpolated values from the vertex shaders
in vec2 UV;
in vec3 EyeDir;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

layout(rgba16f, bindless_sampler) uniform sampler2D TransCache;
layout(rgba16f, bindless_sampler) uniform sampler3D ScatterCache;
layout(rgba16f, bindless_sampler) uniform sampler3D MieScatterCache;

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
}

void main(){

	vec3 eD = normalize(normalize(EyeDir) * Rt - EyePosition);	//R_v
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


	vec4 r_data = vec4(0);
	vec4 m_data = vec4(0);
	Scatter(height, sunAngle, eyeAngle, SunDir, eD, r_data, m_data);

	color.rgb = (r_data.rgb + m_data.rgb) * 20;
	//color.rgb = r_data.rgb * 20;
	color.a = 1;
}