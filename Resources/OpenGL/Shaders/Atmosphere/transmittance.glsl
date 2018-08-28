layout(local_size_x = 1, local_size_y = 1) in;

layout(rgba16f, bindless_image) uniform writeonly image2D TransCache;

uniform vec3 Rayleigh;
uniform float Mie;

uniform float Rg;
uniform float Rt;

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

bool sphere_dist_tan(in float r, in vec3 pos, in vec3 dir, in float tmax, in float mode, out float t, out bool isTan) {

    float b = dot(dir, pos);
    float c = dot(pos, pos) - r * r;
    float tmp = b * b - c;
    if (tmp >= 0.0) t = - b + mode * sqrt(tmp);
    isTan = (abs(tmp) > b * b);

    return tmp >= 0.0 && t <= tmax;
}

#define BIAS (6.35f / 127.0f * PI)
#define BIAS_SAMPLE_COUNT 64
#define BIAS_STEP (BIAS / (BIAS_SAMPLE_COUNT * 2))

float getRayLen(vec3 Pos, vec3 Dir){

    //Calculate the ray length to the atmosphere
    float rayLen = 0;
    sphere_dist(Rt, Pos, Dir, Rt * 4, +1, rayLen);

    float g_rayLen = 0;
    bool g_tan = false;
    bool g_intersect = sphere_dist(Rg, Pos, Dir, Rt * 4, -1, g_rayLen);
    if(g_intersect && g_rayLen >= 0)
        rayLen = g_rayLen;

    return rayLen;
}

void main(){
    float theta = float(gl_GlobalInvocationID.x) / float(gl_NumWorkGroups.x - 1);
    float height = float(gl_GlobalInvocationID.y) / float(gl_NumWorkGroups.y - 1);

    theta = theta * PI;//acos(2 * theta - 1);
    height = Rg + height * (Rt - Rg);

    //Calculate the ray vector
    vec3 Pos = vec3(0, height, 0);
    vec3 Dir = vec3(sin(theta), cos(theta), 0);
    
    float rayleigh_rho = 0;
    float mie_rho = 0;

    rayleigh_rho = exp(- (height / RayleighScaleHeight));
    mie_rho = exp(- (height / MieScaleHeight));


    float rayLen = getRayLen(Pos, Dir);
    float stepLen = rayLen / (SAMPLE_COUNT + 1);

    for(int i = 1; i < SAMPLE_COUNT; i++)
    {
        vec3 Ray = Pos + Dir * i * stepLen;
        float RayHeight = length(Ray) - Rg;

        rayleigh_rho += exp(- (RayHeight / RayleighScaleHeight)) * 2;
        mie_rho += exp(- (RayHeight / MieScaleHeight)) * 2;
    }

    vec3 Ray = Pos + Dir * SAMPLE_COUNT * stepLen;
    float RayHeight = length(Ray) - Rg;

    rayleigh_rho += exp(- (RayHeight / RayleighScaleHeight));
    mie_rho += exp(- (RayHeight / MieScaleHeight));

    rayleigh_rho *= stepLen * 0.5f;
    mie_rho *= stepLen * 0.5f;

    vec4 result = vec4(0);
    result.rgb = rayleigh_rho * Rayleigh;
    result.a = mie_rho * Mie * 1.1f;
    
    imageStore(TransCache, ivec2(gl_GlobalInvocationID.xy), result);
}