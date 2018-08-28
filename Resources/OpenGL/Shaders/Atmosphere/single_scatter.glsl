layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

layout(rgba16f, bindless_sampler) uniform sampler2D TransCache;
layout(rgba16f, bindless_image) uniform writeonly image3D ScatterCache;
layout(rgba16f, bindless_image) uniform writeonly image3D MieScatterCache;

uniform vec3 Rayleigh;
uniform float Mie;

uniform float Rg;
uniform float Rt;
uniform int Layer;
uniform int Count;
uniform int YOff;
uniform int YLen;

uniform float RayleighScaleHeight;
uniform float MieScaleHeight;

#define SAMPLE_COUNT 2048

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

float getRayLen(vec3 Pos, vec3 Dir){

    //Calculate the ray length to the atmosphere
    float rayLen = 0;
    sphere_dist(Rt, Pos, Dir, Rt * 4, +1, rayLen);

    float g_rayLen = 0;
    bool g_intersect = sphere_dist(Rg, Pos, Dir, Rt * 4, -1, g_rayLen);
    if(g_intersect && g_rayLen >= 0 && g_rayLen < rayLen)
        rayLen = g_rayLen;

    return rayLen;
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

void ComputeScattering(vec3 P_v, float sigma, vec3 R_v, out vec3 rayleigh_color, out vec3 mie_color)
{
    float rayLen = getRayLen(P_v, R_v);
    float stepLen = rayLen / SAMPLE_COUNT;
    vec3 sunDir = vec3(sin(sigma), cos(sigma), 0);

    float mie_Tv = 0;
    float rayleigh_Tv = 0;

    vec3 rayleigh_scatter = vec3(0);
    vec3 mie_scatter = vec3(0);

    float sunRayLen = 0;
    bool sun_intersect = sphere_dist(Rg, P_v, sunDir, Rg * 4, -1, sunRayLen);
    bool doOp = !(sun_intersect && sunRayLen >= 0);

    for(int i = 0; i < SAMPLE_COUNT && doOp; i++)
    {
        vec3 Ray = P_v + R_v * i * stepLen;
        float h = length(Ray);

        float rayleigh_rho = exp(-(h - Rg) / RayleighScaleHeight);
        float mie_rho = exp(-(h - Rg) / MieScaleHeight);

        rayleigh_Tv += rayleigh_rho * stepLen;
        mie_Tv += mie_rho * stepLen;

        vec3 rayleigh_Tv_tmp = rayleigh_Tv * Rayleigh;
        float mie_Tv_tmp = mie_Tv * Mie * 1.1f;

        vec4 T_L = T(h, cos(sigma));//dot(sunDir, normalize(Ray)));

        rayleigh_scatter += rayleigh_rho * exp( - rayleigh_Tv_tmp - T_L.rgb);
        mie_scatter += mie_rho * exp( - mie_Tv_tmp - T_L.a);
    }

    rayleigh_scatter *= stepLen;
    mie_scatter *= stepLen;

    //float mu = PI - dot(R_v, sunDir);
    //rayleigh_scatter *= RayleighPhase(mu);
    //mie_scatter *= MiePhase(mu);

    rayleigh_color = Rayleigh * rayleigh_scatter;
    mie_color = vec3(Mie * mie_scatter);
}

void main(){
    
    float height = float(gl_GlobalInvocationID.x) / float(gl_NumWorkGroups.x - 1);
    float delta = float(gl_GlobalInvocationID.y + YOff) / float(YLen - 1);
    float theta = float(Layer) / float(Count - 1);

    height = Rg + height * (Rt - Rg);
    delta = delta * PI;
    theta = theta * PI;
    
    //Calculate the light scattering intergral
    vec3 Pos = vec3(0, height, 0);
    vec3 Dir = vec3(sin(theta), cos(theta), 0);
    
    vec3 rayleigh_color = vec3(0);
    vec3 mie_color = vec3(0);
    ComputeScattering(Pos, delta, Dir, rayleigh_color, mie_color);

/*
    //Calculate the ray length to the atmosphere
    float rayLen = 0;
    sphere_dist(Rt, Pos, Dir, Rt * 4, +1, rayLen);

    //Check if the ray intersects the ground, if so, adjust the length of the ray to go up till the ground
    float gndRayLen = 0;
    bool gnd_intersect = sphere_dist(Rg, Pos, Dir, Rg * 4, -1, gndRayLen);
    if(gnd_intersect && gndRayLen >= 0)
        rayLen = gndRayLen;
        
    float stepLen = abs(rayLen) / SAMPLE_COUNT;

    float sunRayLen = 0;
    bool sun_intersect = sphere_dist(Rg, Pos, SunDir, Rg * 4, -1, sunRayLen);
    bool doOp = !(sun_intersect && sunRayLen >= 0);
        
    //Integrate along the length of the View ray, calculating the scattering at each point
    vec3 radiance = vec3(0);
    vec3 mie_radiance = vec3(0);
    vec4 T_V = vec4(0);

    for(float i = 0; i < SAMPLE_COUNT && doOp; ++i){
        vec3 curPos = Pos + Dir * i * stepLen; 
        float curHeight = length(curPos);
    
        float hr = exp(- (curHeight - Rg) / RayleighScaleHeight) * stepLen;
        float hm = exp(- (curHeight - Rg) / MieScaleHeight) * stepLen;

        //Compute the view transmittance
        T_V.xyz += vec3(hr);
        T_V.w += hm;
        
        //Sun transmittance
        vec4 T_L = T(curHeight, cos(delta));

        vec3 cur_T = exp(-(T_V.xyz * Rayleigh + T_V.w * Mie * 1.1f));

        //Attenuation
        vec3 tau = (T_L.xyz * cur_T);
        
        radiance += tau * hr;
        mie_radiance += tau * hm;
    }*/

    vec4 val = vec4(1);
    val.rgb = rayleigh_color;
    imageStore(ScatterCache, ivec3(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y + YOff, Layer), val);
    
    val.rgb = mie_color;
    imageStore(MieScatterCache, ivec3(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y + YOff, Layer), val);
}