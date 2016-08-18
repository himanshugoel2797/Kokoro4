

// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D RGBA0;
uniform sampler2D Depth0;
uniform sampler2D Normal0;
uniform sampler2D HemisphereSample;
uniform sampler2D RadiositySample;
uniform sampler2D LightMap;

void main(){

	// Output color = color of the texture at the specified UV
	color = texture2D( HemisphereSample, UV );
	color *= texture2D(RGBA0, UV);

	//color = texture2D(LightMap, UV);

	color.a = 1;
}