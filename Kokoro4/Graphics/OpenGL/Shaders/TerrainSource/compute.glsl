layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, bindless_image) uniform writeonly image2DArray terrain_fragment;

#define MULTER 2.0f

uniform vec3 top_left_corner;
uniform float side;
uniform int layer;

uniform int XSide;
uniform int ZSide;

float noise(vec3 seed) {
    return clamp(snoise(seed), 0, 1);
}

float ridged_noise(vec3 seed) {
    return 1.0f - pow(noise(seed), 2);
}

void main(){
    
    vec3 sample_pos = top_left_corner;
    sample_pos[XSide] = top_left_corner[XSide] + float(gl_WorkGroupID.x) / float(gl_NumWorkGroups.x - 1) * side;
    sample_pos[ZSide] = top_left_corner[ZSide] + float(gl_WorkGroupID.y) / float(gl_NumWorkGroups.y - 1) * side; 
	sample_pos = normalize(sample_pos);
	//sample_pos *= MULTER;


    float continent_noise = noise(sample_pos * 0.25f);

    ivec3 img_coords = ivec3(gl_WorkGroupID.x, gl_WorkGroupID.y, layer);

    vec4 val = vec4(1);
    //val.xyz = vec3(max(0, noise(sample_pos * 0.5f) + ridged_noise(sample_pos * 25.0f)) + continent_noise);
    //val.xyz *= continent_noise;
    val.xyz = vec3(snoise(sample_pos) + snoise(sample_pos * 2) * 0.5f + snoise(sample_pos * 4) * 0.25f + snoise(sample_pos * 8) * 0.125f);
    val.xyz += ridged_noise(sample_pos * 5) * 0.03125f;

    imageStore(terrain_fragment, img_coords, val);
}