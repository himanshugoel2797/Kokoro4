

// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D FRGBA;
uniform sampler2D FNormal;
uniform sampler2D FDepth;

void main(){

	vec3 normal = 2.0f * texture2D(FNormal, UV).rgb - 1.0f;		//Read in the front facing normal
	normal = normalize(normal);


	//Reconstruct current position
	vec3 currentPos = UV.xyx;
	currentPos.z = texture2D(FDepth, UV).r; 

	vec3 random = normalize(vec3(rand(UV), rand(normal.xy), rand(currentPos.xz)));



	color = vec4(clamp(ao, 0.0, 1.0));
	color.a = 1;
}