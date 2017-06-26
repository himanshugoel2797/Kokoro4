layout(local_size_x = 1, local_size_y = 1) in;

layout(rgba16f, bindless_sampler) uniform sampler3D TransCache;
layout(rgba16f, bindless_image) uniform writeonly image3D ScatterCache;

uniform float Mie;
uniform float Rt;

#define SAMPLE_COUNT 512

bool sphere_dist(in float r, in vec3 pos, in vec3 dir, in float tmax, in float mode, out float t) {

    float b = dot(dir, pos);
    float c = dot(pos, pos) - r * r;
    float tmp = b * b - c;
    if (tmp >= 0.0) t = - b + mode * sqrt(tmp);

    return tmp >= 0.0 && t <= tmax;
}

float expp(float minVal, float maxVal, float erp) {
    return minVal * pow((maxVal / minVal), erp);
}

vec4 T(float startDensity, float endDensity, float dist) {
    return textureLod(TransCache, vec3(startDensity, endDensity, dist / (2 * Rt)), 0);
}

void main(){
    float V = float(gl_GlobalInvocationID.x) / float(gl_NumWorkGroups.x - 1);
    float delta = float(gl_GlobalInvocationID.y) / float(gl_NumWorkGroups.y - 1);
    float theta = float(gl_GlobalInvocationID.z) / float(gl_NumWorkGroups.z - 1);

    //Calculate the maximum angle before intersecting the ground at the given height, use as maximum range for delta and theta, maximizing precision
    float MaxAngle = PI;
    
    theta = theta * MaxAngle;
    delta = delta * MaxAngle;
    V = V * 2 * MaxAngle;
    
    //Calculate the light scattering intergral
    vec3 Dir = vec3(sin(theta), cos(theta), 0);    
    vec3 SunDir = vec3(sin(delta) * cos(V), cos(delta), sin(delta) * sin(V));
    
    //Calculate raylen based on the chord from the view direction
    float rayLen = cos(PI - theta) * Rt * 2;
    float stepLen = rayLen / SAMPLE_COUNT;

    //NOTE: currently working with a sphere, position for all intents and purposes is at the surface of the sphere.
    vec3 Pos = Dir * Rt;
    vec3 SunPos = SunDir * Rt;

    //From the surface, trace a ray through the cloud, calculating scattering at each point

    //Integrate along the length of the View ray, calculating the scattering at each point
    float mie_radiance = float(0);
    for(float i = 0; i < SAMPLE_COUNT; ++i){

        vec3 curPos = Pos + Dir * i * stepLen;

        float baseDensity = exp(- i - 1);
        float destDensity = exp(- i);

        //Calculate the transmittance to this point given the sun's position
        vec4 T_L = T(baseDensity, destDensity, length(curPos - SunPos));

        //Calculate the transmittance to this point given the viewer's position
        vec4 T_V = T(baseDensity, destDensity, i * stepLen);

        mie_radiance += mix(baseDensity, destDensity, i / SAMPLE_COUNT) * exp(- T_L.r - T_V.r) * stepLen;
    }

    vec4 val = vec4(0);

    float mu = dot(SunDir, Dir);
    float g = 0.76f;

    val.rgb = vec3(Mie / 1.1f * mie_radiance);
    val.rgb *= 3.0f / (8.0f * PI) * (1 - g * g) * (1 + mu * mu) / ((2 + g * g)  * pow(1 + g * g - 2 * g * mu, 3.0f / 2.0f));

    //val.rgb *= 

    //val.rgb = vec3(mie_radiance);
    val.a = 1;

    imageStore(ScatterCache, ivec3(gl_GlobalInvocationID.xyz), val);
}