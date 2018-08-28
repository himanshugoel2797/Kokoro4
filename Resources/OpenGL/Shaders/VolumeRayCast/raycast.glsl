// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

layout(bindless_sampler) uniform sampler3D VolumeData;
layout(bindless_sampler) uniform sampler2D BackFaces;
layout(bindless_sampler) uniform sampler2D FrontFaces;

#define SAMPLE_CNT 128

void main(){
	vec3 rayStart = texture(FrontFaces, UV).rgb * 2 - 1;
	vec3 rayEnd = texture(BackFaces, UV).rgb * 2 - 1;

	vec3 rayVec = rayEnd - rayStart;
	
	float rayLen = length(rayVec);
	vec3 rayDir = normalize(rayVec);

	vec3 stepVec = rayVec / SAMPLE_CNT;
	vec4 tmp = vec4(0);
	rayVec = rayStart;

	color = vec4(0);
	if(rayLen == 0)
		return;

	for(int i = 0; i < SAMPLE_CNT; i++)
	{
		tmp = texture(VolumeData, rayVec + 0.5f).rrrr;
		tmp.a *= 0.5f;
		tmp.rgb *= tmp.a;
		color = (1.0f - color.a) * tmp + color;

		if(color.a >= 0.95f)
			return;

		rayVec += stepVec;
	}
}