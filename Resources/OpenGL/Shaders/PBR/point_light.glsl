layout(location = 0) out vec4 accum_data;

struct LightEntry_t{
    vec3 Pos;
    float effectiveRadius;
    float radius;
    float intensity;
    float typeIndex;
    float unused0;
    vec3 color;
    float unused1;
    vec4 unused2;
};

struct PBRMaterial_t{
    uvec2 albedo;
    uvec2 metal_roughness_deriv;
};

layout (binding = 0, std430) buffer MaterialParams
{
    PBRMaterial_t data[];
} materialParams;

layout(binding = 1, std430) buffer LightData{
    LightEntry_t data[];
} lights;

layout(bindless_sampler) uniform sampler2D uvBuf;
layout(bindless_sampler) uniform sampler2D mID_Buf;
layout(r32ui, bindless_image) uniform coherent restrict readonly uimage3D LightIndices;

uniform int mat_type;
uniform float min_intensity;
uniform vec2 im_sz;
uniform mat4 iVP;
uniform mat4 VP;
uniform int tile_sz;

flat in int inst_id;
in vec2 UV;

void main(){
    ivec2 pos = ivec2(gl_FragCoord.xy);
    ivec2 pos_tile = ivec2(pos / tile_sz.xx);
    
    //uvBuf contains (uv.x, uv.y, norm.x, norm.y)
    vec4 uvBuf_dat = texelFetch(uvBuf, pos, 0);
    vec2 uv = uvBuf_dat.xy;
    
    //normal
    vec2 n = uvBuf_dat.zw / 100.0f * PI/180.0f;
	vec3 normal = vec3(cos(n.x) * sin(n.y), sin(n.x) * sin(n.y), cos(n.y));

    //depth
    //material ID
    vec4 matID_data = texelFetch(mID_Buf, pos, 0);
    int matID = int(matID_data.x * 4096.0f);
    float depth = matID_data.y;

    //light ID
    uint lightCnt = imageLoad(LightIndices, ivec3(pos_tile.x, pos_tile.y, 0)).x;
    
    if(lightCnt <= inst_id)
        discard;
    
    uint lightID = imageLoad(LightIndices, ivec3(pos_tile.x, pos_tile.y, inst_id + 1)).x;

    accum_data = vec4(lightCnt / 10.0f,depth,0,1);
    if(matID / 1024 == mat_type){
        matID = matID % 1024;

        //Material data
        vec4 albedo = texture(sampler2D(materialParams.data[matID].albedo), uv);
        vec4 metal_roughness_deriv = texture(sampler2D(materialParams.data[matID].metal_roughness_deriv), uv);
        
        //Compute the world space position of this pixel
        vec4 world_pos = iVP * vec4(pos.x / im_sz.x * 2 - 1, pos.y / im_sz.y * 2 - 1, depth, 1);

        //Compute the lighting

        accum_data = world_pos;
    }
}