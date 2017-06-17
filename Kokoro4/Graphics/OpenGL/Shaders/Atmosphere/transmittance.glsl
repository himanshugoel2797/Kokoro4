layout(local_size_x = 1, local_size_y = 1) in;

layout(rgba16f, bindless_image) uniform writeonly image2D TransCache;

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

void main(){
    
    float theta = float(gl_WorkGroupID.x) / float(gl_NumWorkGroups.x - 1);
    float height = float(gl_WorkGroupID.y) / float(gl_NumWorkGroups.y - 1);

    theta = acos(theta * 2 - 1);
    height = Rg + height * (Rt - Rg);

    //Calculate the ray vector
    vec3 Pos = vec3(0, height, 0);
    vec3 Dir = vec3(sin(theta), cos(theta), 0);

    //Calculate the ray length to the atmosphere or ground
    float rayLen = 0;
    sphere_dist(Rt, Pos, Dir, Rt * 4, +1, rayLen);

    //Check for intersection with ground
    float g_rayLen = 0;
    bool g_intersect = sphere_dist(Rg, Pos, Dir, Rg * 4, -1, g_rayLen);

    float stepLen = rayLen / SAMPLE_COUNT;

    //Analytical integration over the ray length
    float rayleigh_rhoSum = 0;
    float mie_rhoSum = 0;
    for(float i = 0; i < SAMPLE_COUNT; ++i){
        vec3 curPos = Pos + Dir * i * stepLen; 
        float curHeight = length(curPos) - Rg;

        float aboveGnd = float(curHeight >= 0);

        rayleigh_rhoSum += exp(- curHeight / RayleighScaleHeight) * stepLen * aboveGnd;
        mie_rhoSum += exp(- curHeight / MieScaleHeight) * stepLen * aboveGnd;
    }

    //Multiply the sums with the scattering coefficients
    vec4 val = vec4(0);
    val.rgb = Rayleigh * rayleigh_rhoSum;
    val.a =  Mie * mie_rhoSum;
    
    if(g_intersect && g_rayLen >= 0)
        val *= 0;

    imageStore(TransCache, ivec2(gl_WorkGroupID.xy), val);
}