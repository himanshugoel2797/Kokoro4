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
    float delta = float(gl_GlobalInvocationID.y + YOff) / float(YLen - 1);
    float theta = float(Layer) / float(Count - 1);

    height = Rg + height * (Rt - Rg);
    delta = acos(delta * 2 - 1);
    theta = acos(theta * 2 - 1);
    
    //Calculate the light scattering intergral
    vec3 Pos = vec3(0, height, 0);
    vec3 Dir = vec3(sin(theta), cos(theta), 0);
    vec3 SunDir = vec3(sin(delta), cos(delta), 0);
    
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
    
    bool test = false;

    if(theta == acos(-1))
            test = true;

    for(float i = 0; i < SAMPLE_COUNT && doOp; ++i){
        vec3 curPos = Pos + Dir * i * stepLen; 
        float curHeight = length(curPos);

        //sun_intersect = sphere_dist(Rg, curPos, SunDir, Rg * 4, -1, sunRayLen);
        //doOp = !(sun_intersect && sunRayLen >= 0);
    

        float hr = exp(- (curHeight - Rg) / RayleighScaleHeight) * stepLen;
        float hm = exp(- (curHeight - Rg) / MieScaleHeight) * stepLen;

        //Compute the view transmittance
        T_V.xyz += vec3(hr);
        T_V.w += hm;
        
        //Sun transmittance
        vec4 T_L = T(curHeight, cos(delta));

        //Attenuation
        vec3 tau = (T_L.xyz + T_V.xyz) * Rayleigh + (T_L.www + T_V.www) * Mie * 1.1f;
        tau = exp(-tau);

        radiance += hr * tau;
        mie_radiance += hm * tau;
    }

    vec4 val = vec4(1);
    val.rgb = Rayleigh * radiance;
    imageStore(ScatterCache, ivec3(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y + YOff, Layer), val);
    
    val.rgb = Mie * mie_radiance;
    imageStore(MieScatterCache, ivec3(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y + YOff, Layer), val);
}