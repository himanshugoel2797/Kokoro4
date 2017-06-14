layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba8, bindless_image) uniform image2DArray terrain_fragment;

uniform vec3 top_left_corner;
uniform float side;
uniform int layer;

void main(){
    
    vec3 sample_pos = vec3(0);
    sample_pos.x = top_left_corner.x + gl_GlobalInvocationID.x / gl_WorkGroupSize.x * side;
    sample_pos.y = top_left_corner.y + gl_GlobalInvocationID.y / gl_WorkGroupSize.y * side;

    ivec3 img_coords = ivec3(gl_GlobalInvocationID.x, gl_GlobalInvocationID.y, layer);

    imageStore(terrain_fragment, img_coords, vec4(snoise(sample_pos)));
}