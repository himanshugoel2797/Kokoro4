

// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D FRGBA;
uniform sampler2D FNormal;
uniform sampler2D FDepth;
uniform sampler2D RadiosityPassA;
uniform sampler2D VPLPositions;

vec3 calcPointLightEffect(vec3 lightColor, vec3 lightPos, vec3 pointPos, vec3 normal, vec3 lightNormal)
{
	vec3 lightDir = pointPos - lightPos;
	float dist = length(lightDir);
	lightDir = normalize(lightDir);

	return lightColor * dot(normal, lightNormal);
}

void main(){
	
	const int lights = 5;

	vec3 pointColor = texture2D(FRGBA, UV).rgb;
	vec3 worldPos = texture2D(FDepth, UV).gbr;	//Read in the vertex position
	worldPos.xy = UV;

	vec3 normal = normalize( 2.0f * texture2D(FNormal, UV).rgb - 1.0f );



}