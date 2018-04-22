layout(local_size_x = 1, local_size_y = 1) in;

layout(rgba16f, bindless_image) uniform writeonly image2D TransCache;

uniform vec3 Rayleigh;
uniform vec3 RayleighExtinction;
uniform float RayleighScaleHeight;

uniform float Mie;
uniform float MieExtinction;
uniform float MieScaleHeight;

uniform float Rg;
uniform float Rt;


#define SAMPLE_COUNT 1024

bool sphere_dist(in float r, in vec3 pos, in vec3 dir, in float tmax, in float mode, out float t) {

    float b = dot(dir, pos);
    float c = dot(pos, pos) - r * r;
    float tmp = b * b - c;
    if (tmp >= 0.0) t = - b + mode * sqrt(tmp);

    return tmp >= 0.0 && t <= tmax;
}

void main(){
    float theta = float(gl_GlobalInvocationID.x) / float(gl_NumWorkGroups.x - 1);
    float height = float(gl_GlobalInvocationID.y) / float(gl_NumWorkGroups.y - 1);

    theta = acos(theta * 2 - 1);
    height = Rg + height * (Rt - Rg);

    //Calculate the ray vector
    vec3 Pos = vec3(0, height, 0);
    vec3 Dir = vec3(sin(theta), cos(theta), 0);

    //Calculate the ray length to the atmosphere
    float rayLen = 0;
    sphere_dist(Rt, Pos, Dir, Rt * 4, +1, rayLen);

    float g_rayLen = 0;
    bool g_intersect = sphere_dist(Rg, Pos, Dir, Rt * 4, -1, g_rayLen);
    if(g_intersect && g_rayLen > 0)
        rayLen = g_rayLen;

    float stepLen = abs(rayLen) / SAMPLE_COUNT;
    
	//Integration over the ray length
    float rayleigh_g = 0;
    float mie_g = 0;

    for(float i = 0; i < SAMPLE_COUNT; ++i){
        vec3 curPos = Pos + Dir * i * stepLen; 
        float curHeight = length(curPos) - Rg;

		float rayleigh_l = exp(- curHeight / RayleighScaleHeight);
		float mie_l = exp(- curHeight / MieScaleHeight);

        rayleigh_g += rayleigh_l * stepLen;
        mie_g += mie_l * stepLen;
    }

    //Multiply the sums with the scattering coefficients
    vec4 val = vec4(0);
    val.rgb = vec3(exp(-(rayleigh_g * RayleighExtinction + mie_g * MieExtinction)));
    val.a = 1;

    imageStore(TransCache, ivec2(gl_GlobalInvocationID.xy), val);
}