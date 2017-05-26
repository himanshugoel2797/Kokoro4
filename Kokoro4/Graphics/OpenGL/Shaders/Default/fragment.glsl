// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

layout(std140) uniform Material_t {
	uvec2 Albedo;
} Material;

void main(){
	color = texture(sampler2D(Material.Albedo), UV);
	color.a = 1;
}