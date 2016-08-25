in vec2 UV;
in float depth;
in vec3 normPos;
in vec3 tangent;
in vec3 bitangent;

layout(location = 0) out vec4 RGBA0;
layout(location = 1) out vec4 Depth0;
layout(location = 2) out vec4 Normal0;


uniform sampler2D ColorMap;
uniform sampler2D LightingMap;
uniform sampler2D NormalMap;
uniform float Emissivity;

void main()
{
		Normal0 = vec4(0.5f * normalize(normPos) + 0.5f, 1);
		RGBA0 = texture2D(ColorMap, UV);
		Depth0 = vec4(vec3(depth/50), 1);
}