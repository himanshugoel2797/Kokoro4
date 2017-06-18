layout(local_size_x = 1, local_size_y = 1) in;

layout(rgba16f, bindless_sampler) uniform sampler2D TransCache;
layout(rgba16f, bindless_image) uniform writeonly image3D ScatterCache;

uniform vec3 Rayleigh;
uniform float Mie;

uniform float Rg;
uniform float Rt;

uniform float RayleighScaleHeight;
uniform float MieScaleHeight;

#define SAMPLE_COUNT 512

bool sphere_dist(in float r, in vec3 pos, in vec3 dir, in float tmax, in float mode, out float t) {

    float b = dot(dir, pos);
    float c = dot(pos, pos) - r * r;
    float tmp = b * b - c;
    if (tmp >= 0.0) t = - b + mode * sqrt(tmp);

    return tmp >= 0.0 && t <= tmax;
}

vec4 T(float h, float CosTheta) {
    return textureLod(TransCache, vec2(CosTheta * 0.5f + 0.5f, (h - Rg) / (Rt - Rg)), 0);
}

void main(){


    float height = float(gl_GlobalInvocationID.x) / float(gl_NumWorkGroups.x - 1);
    float delta = float(gl_GlobalInvocationID.y) / float(gl_NumWorkGroups.y - 1);
    float theta = float(gl_GlobalInvocationID.z) / float(gl_NumWorkGroups.z - 1);

    height = Rg + height * (Rt - Rg);

    //Calculate the maximum angle before intersecting the ground at the given height, use as maximum range for delta and theta, maximizing precision
    vec3 Pos = vec3(0, height, 0);
    vec3 GndTan = vec3(Rg, 0, 0);
    vec3 GndTanDir = normalize(GndTan - Pos);
    float MaxAngle = dot(vec3(0, 1, 0), GndTanDir);
    
    theta = mix(theta, 0, MaxAngle);
    delta = mix(delta, 0, MaxAngle);
    
    //Calculate the light scattering intergral
    vec3 Dir = vec3(sin(theta), cos(theta), 0);
    
    vec3 SunDir = vec3(sin(delta), cos(delta), 0);
    vec3 SunPos = SunDir * Rt;  //The sun, as far as the calculation is concerned, is at the surface of the atmosphere.

    //Calculate the ray length to the atmosphere
    float rayLen = 0;
    sphere_dist(Rt, Pos, Dir, Rt * 4, +1, rayLen);

    //Check for intersection with ground
    float g_rayLen = 0;
    bool g_intersect = sphere_dist(Rg, Pos, SunDir, Rg * 4, -1, g_rayLen);

    float stepLen = rayLen / SAMPLE_COUNT;

    //Integrate along the length of the View ray, calculating the scattering at each point
    vec3 radiance = vec3(0);
    vec3 mie_radiance = vec3(0);
    for(float i = 0; i < SAMPLE_COUNT; ++i){
        vec3 curPos = Pos + Dir * i * stepLen; 
        float curHeight = length(curPos);

        //Calculate the sun's direction relative to the current point.
        vec4 T_L = T(curHeight, cos(delta));
        vec4 T_V = T(curHeight, cos(theta));

        float aboveGnd = float(curHeight >= Rg);

        radiance += exp(-(curHeight - Rg) / RayleighScaleHeight - T_L.xyz - T_V.xyz) * stepLen * aboveGnd;
        mie_radiance += exp(-(curHeight -Rg) / MieScaleHeight - T_L.www - T_V.www) * stepLen * aboveGnd;
    }

    vec4 val = vec4(0);

    float mu = dot(SunDir, Dir);
    float g = 0.76f;

    radiance = Rayleigh * radiance;
    radiance *= 3.0f / (16.0f * PI) * (1 + mu * mu);



    val.rgb = Mie / 1.1f * mie_radiance;
    val.rgb *= 3.0f / (8.0f * PI) * (1 - g * g) * (1 + mu * mu) / ((2 + g * g)  * pow(1 + g * g - 2 * g * mu, 3.0f / 2.0f));

    val.rgb += radiance;
    val.a = 1;

    imageStore(ScatterCache, ivec3(gl_GlobalInvocationID.xyz), val);
}