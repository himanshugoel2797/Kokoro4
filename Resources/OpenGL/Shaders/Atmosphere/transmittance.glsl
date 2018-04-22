layout(local_size_x = 1, local_size_y = 1) in;

layout(rgba16f, bindless_image) uniform writeonly image2D TransCache;

uniform vec3 Rayleigh;
uniform float Mie;

uniform float Rg;
uniform float Rt;

uniform float RayleighScaleHeight;
uniform float MieScaleHeight;

#define SAMPLE_COUNT 256

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
    bool g_intersect = sphere_dist_tan(Rg, Pos, Dir, Rt * 4, -1, g_rayLen, g_tan);
    if(g_intersect && g_rayLen >= 0 && g_rayLen < rayLen)
        rayLen = g_rayLen;

    return rayLen;
}

void main(){
    float theta = float(gl_GlobalInvocationID.x) / float(gl_NumWorkGroups.x - 1);
    float height = float(gl_GlobalInvocationID.y) / float(gl_NumWorkGroups.y - 1);

    theta = theta * PI;
    height = Rg + height * (Rt - Rg);

    //Calculate the ray vector
    vec3 Pos = vec3(0, height, 0);
    vec3 Dir = vec3(sin(theta), cos(theta), 0);
    
    vec3 DirUp = vec3(sin(theta + BIAS), cos(theta + BIAS), 0);
    vec3 DirDown = vec3(sin(theta - BIAS), cos(theta - BIAS), 0);

    vec4 result = vec4(0);
    //float rayLenUp = getRayLen(Pos, DirUp);
    //float rayLenDown = getRayLen(Pos, DirDown);
    //float rayLenMid = getRayLen(Pos, Dir);

    //float rayLen = (rayLenUp + rayLenDown + rayLenMid) / 3.0f;
    //if(rayLenMid - rayLenDown > 10)
    //    rayLen = (rayLenUp + rayLenDown) / 2.0f;
    
    //if(g_intersect && !g_tan && g_rayLen >= 0)
    //    rayLen = g_rayLen;


    //if(g_tan){
    //    rayLen = g_rayLen;
    //}

    float samples = 0;

    for(float i0 = -BIAS_SAMPLE_COUNT / 2; i0 < BIAS_SAMPLE_COUNT / 2; ++i0 ){

        float theta_p = clamp(theta + i0 * BIAS_STEP, 0, PI);

    vec3 DirCur = vec3(sin(theta_p), cos(theta_p), 0);
    float rayLen = getRayLen(Pos, DirCur);
    float stepLen = abs(rayLen) / SAMPLE_COUNT;
    
	//Integration over the ray length
    float rayleigh_g = 0;
    float mie_g = 0;

    for(float i = 0; i < SAMPLE_COUNT; ++i){
        vec3 curPos = Pos + DirCur * i * stepLen; 
        float curHeight = length(curPos) - Rg;

		float rayleigh_l = exp(- curHeight / RayleighScaleHeight);
		float mie_l = exp(- curHeight / MieScaleHeight);

        rayleigh_g += rayleigh_l * stepLen;
        mie_g += mie_l * stepLen;
    }

    //Multiply the sums with the scattering coefficients
    vec4 val = vec4(0);
    val.rgb = vec3(exp(-(rayleigh_g * Rayleigh + mie_g * Mie * 1.1f)));
    val.a = 1;

        //if(all(lessThan(result.rgb, val.rgb)) && all(greaterThan(val.rgb, vec3(0.01f, 0.01f, 0.01f)))){
        //    result = val;
        //}

        if(all(greaterThan(val.rgb, vec3(0.01f, 0.01f, 0.01f)))){
            result += val;
            samples++;
        }
    }
    //result = result / BIAS_SAMPLE_COUNT;
    result = result / samples;

//    if(g_intersect && g_rayLen > 0)
//        val = vec4(1);
//    else
//        val = vec4(0);

    //val.rgb = vec3(rayLen / 6000);

    imageStore(TransCache, ivec2(gl_GlobalInvocationID.xy), result);
}