layout(early_fragment_tests) in;

in vec2 UV;
in float depth;
in vec3 worldXY;
smooth in vec3 normPos;
in vec3 tangent;
in vec3 bitangent;

layout(location = 0) out vec4 RGBA0;
layout(location = 1) out vec4 Depth0;
layout(location = 2) out vec4 Normal0;


uniform sampler2D ColorMap;
uniform sampler2D LightingMap;
uniform sampler2D NormalMap;
uniform float Emissivity;
uniform sampler2D GrassMap;
uniform sampler2D GrassHeightMap;


uniform float layer;
uniform float density;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main()
{
		float drawn = texture2D(GrassMap, UV/10).r + texture2D(GrassMap, UV/10).g + texture2D(GrassMap, UV/10).b;
		drawn /= 3;
		if(drawn < 0.3)drawn = 0;
		else drawn = 1;


		vec3 finalNormal = normalize(normPos) + cross( normalize(bitangent), normalize(2.0f * texture2D(NormalMap, UV).rgb - 1.0f));
		Normal0 = vec4(normalize(0.5f * finalNormal + 0.5f), 1);
		RGBA0 = texture2D(GrassHeightMap, UV/10) * normalize(worldXY.y + 11.0f) * texture2D(GrassMap, UV/20);
		//RGBA0 = vec4(0, 1, 0, 1) * layer/20;
		RGBA0.a = drawn;
		Depth0.r = depth/50;
		Depth0.gb = worldXY.xy;
		Depth0.a = drawn;
		Normal0.a = drawn;
}