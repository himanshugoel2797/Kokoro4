layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

uniform mat4 VP;
uniform mat4 iVP;
uniform vec3 viewPos;

layout(rgb32f, bindless_sampler) uniform sampler2D worldPosMap;
layout(rgba16f, bindless_sampler) uniform sampler2D uvNormMap;
layout(r32ui, bindless_sampler) uniform usampler2D colorMap;
layout(r32ui, bindless_image) uniform writeonly uimage2D reflectionMap_matID;
layout(rgba16f, bindless_image) uniform writeonly image2D reflectionMap_uv;

vec3 decode (vec2 enc)
{
    vec4 nn = vec4(enc, 0, 0)*vec4(2,2,0,0) + vec4(-1,-1,1,-1);
    float l = dot(nn.xyz,-nn.xyw);
    nn.z = l;
    nn.xy *= sqrt(l);
    return nn.xyz * 2 + vec3(0,0,-1);
}

void main(){
    ivec2 coord = ivec2(gl_GlobalInvocationID.xy);
    
    //for each pixel, read the normal
    //compute the view dir
    //compute the reflected direction, convert it to screen coordinates
    //trace the reflected direction through the scene, looking for a point that's near or greater than the depth of the read pixel

    vec2 dim = vec2(WIDTH, HEIGHT);
    vec3 p_norm = normalize(decode(textureLod(uvNormMap, (coord + 0.5f) / dim, 0).ba));
    vec3 p_pos = textureLod(worldPosMap, (coord + 0.5f) / dim, 0).xyz;
    vec3 view_dir = normalize(viewPos - p_pos);

    vec3 ray_dir = -normalize(reflect(view_dir, p_norm));
    uint color = 0;
    vec2 uv = vec2(0);
    float dist = 0;
    //for(int bounce = 0; bounce < BOUNCE_CNT; bounce++){
        for(int samplePos = 1; samplePos <= 64; samplePos++){
            //int samplePos = 5;
            vec3 ray_pix_pos = p_pos + samplePos / 16.0f * ray_dir;
            vec4 ss_pos = VP * vec4(ray_pix_pos, 1);
            ss_pos /= ss_pos.w;
            
            vec4 sample_p = VP * vec4(textureLod(worldPosMap, ss_pos.xy * 0.5f + 0.5f, 0).xyz, 1);
            sample_p /= sample_p.w;
            if(sample_p.z >= ss_pos.z){
                dist = length(ray_pix_pos - p_pos);
                color = textureLod(colorMap, ss_pos.xy * 0.5f + 0.5f, 0).x;
                uv = textureLod(uvNormMap, ss_pos.xy * 0.5f + 0.5f, 0).xy;
                break;
            }
        }
    //}
            
    imageStore(reflectionMap_matID, coord, uvec4(color));//vec4(ss_pos.xy * 0.5f + 0.5f, 1, 1));
    imageStore(reflectionMap_uv, coord, vec4(uv, dist, 0));//vec4(ss_pos.xy * 0.5f + 0.5f, 1, 1));
}