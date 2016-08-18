#version 430 core

// Interpolated values from the vertex shaders
flat in ivec2 UV;
in vec3 worldCoord;

// Ouput data
layout(location = 0) out vec4 worldPos;
layout(location = 1) out vec4 norm;
layout(location = 2) out vec4 color;
layout(location = 3) out vec4 bloom;

// Values that stay constant for the whole mesh.
uniform sampler2D AlbedoMap;
uniform sampler2D PosData;
uniform float bloomFactor;

in float flogz;
uniform float Fcoef;

void main(){
	worldPos.xyz = worldCoord;
	worldPos.a = 0;
	color = texture2D(AlbedoMap, gl_PointCoord);
	color.a  *= 1.0 - texelFetch(PosData, UV, 0).a;
	norm = vec4(0, 0, 0, 1);
	bloom = color * bloomFactor;
    gl_FragDepth = Fcoef * 0.5 * log2(flogz);
}