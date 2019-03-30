layout(location = 0) out vec4 accum_data;

layout(bindless_sampler) uniform sampler2D uvBuf;
layout(bindless_sampler) uniform sampler2D mID_Buf;

flat in int inst_id;

uniform int mat_idx;
uniform float min_intensity;
uniform vec2 im_sz;
uniform mat4 iVP;
uniform mat4 VP;
uniform int tile_sz;


layout (binding = 0, std430) buffer MaterialParams
{ 
    uvec2 albedo[MAT_CNT];
    uvec2 metal_roughness_deriv[MAT_CNT];
} materialParams;

layout (binding = 1, std430) buffer Lights
{ 
    uvec4 pos_r[MAX_TOTAL_LIGHT_CNT];
    uvec2 col_i[MAX_TOTAL_LIGHT_CNT];
} lights;

layout (binding = 2, std430) buffer LightIndices{
    uint light_indices[];
} lightIndices;

void main(){
    ivec2 pos = ivec2(gl_FragCoord.xy - 0.5f);
    ivec2 pos_tile = ivec2((pos - pos % tile_sz) / tile_sz.xx);
    
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
    float depth = matID_data.y * 100.0f;

    //light ID
    int lightAccessIdx = (pos_tile.y * (int(im_sz.x) / tile_sz) + pos_tile.x);
    uint lightCnt = lightIndices.light_indices[lightAccessIdx];//, (lightAccessIdx % 2) * 16, 16);
    //int lightID = lightIndices.light_indices[] inst_id;

    /*if(matID == mat_idx){
        matID = matID % 1024;

        //Read the accumulator
        vec4 accum_val = imageLoad(accum, pos);

        //Material data (TODO: compute mipmap level with derivatives)
        vec4 albedo = texelFetch(sampler2D(materialParams.albedo[matID]), pos, 0);
        vec4 metal_roughness_deriv = texelFetch(sampler2D(materialParams.metal_roughness_deriv[matID]), pos, 0);
        
        //Compute the world space position of this pixel
        vec4 world_pos = vec4(depth);//iVP * vec4(pos.x / im_sz.x * 2 - 1, pos.y / im_sz.y * 2 - 1, depth, 1);

        //Compute the lighting at the calculated world space position

        //Update the accumulator
        memoryBarrier();
        imageStore(accum, pos, accum_val + world_pos);
    }*/
    //imageStore(accum, pos, uvBuf_dat);
    accum_data = vec4(uv.x,uv.y,1,1);//(bbox_min.xyxy / tile_sz) / (im_sz.xyxy / tile_sz);
}