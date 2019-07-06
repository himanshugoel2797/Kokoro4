#version 430 core

in vec2 UV;
in float depth;
in vec3 worldXY;
smooth in vec3 normPos;

layout(location = 0) out vec4 ColorOUT;

layout(location = 0, std140) uniform Material {
	uvec2 RGBA0;
	uvec2 Depth0;
	uvec2 Normal0;
	uvec2 Material0;
};

vec3 decode (vec2 enc)
{
    vec2 fenc = enc*4-2;
    float f = dot(fenc,fenc);
    vec2 g = sqrt(1-f/4);
    vec3 n;
    n.xy = fenc*g;
    n.z = 1-f/2;
    return n;
}

void main()
{
//Read in all information from textures
		vec4 albedo = texture2D(RGBA0, UV);
		
		vec4 tmp = texture2D(Depth0, UV);

		float Depth = tmp.r * 50;
		vec2 WorldPos = tmp.gb;
		float Microsurface = tmp.a;

		vec4 Reflectivity = texture2D(Material0, UV);

		tmp = texture2D(Normal0, UV);
		vec3 Normal = decode(tmp.rg); 

		float AO = tmp.b;
		float Cavity = tmp.a;
}