layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, bindless_image) uniform writeonly image2DArray terrain_fragment;

#define MULTER 25.0f

uniform vec3 top_left_corner;
uniform float side;
uniform int layer;

uniform int XSide;
uniform int ZSide;

void main(){
    
    vec3 sample_pos = top_left_corner;
    sample_pos[XSide] = top_left_corner[XSide] + float(gl_WorkGroupID.x) / float(gl_NumWorkGroups.x - 1) * side;
    sample_pos[ZSide] = top_left_corner[ZSide] + float(gl_WorkGroupID.y) / float(gl_NumWorkGroups.y - 1) * side; 
	sample_pos = normalize(sample_pos);
	sample_pos *= MULTER;


    ivec3 img_coords = ivec3(gl_WorkGroupID.x, gl_WorkGroupID.y, layer);

    vec4 val = vec4(1);
    val.xyz = vec3(snoise(sample_pos));

    imageStore(terrain_fragment, img_coords, val);
}