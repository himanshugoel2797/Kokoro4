#version 430 core

in vec2 UV_o2;
in float depth_o2;
in vec3 worldXY_o2;
smooth in vec3 normPos_o2;
in vec2 logBufDat;

layout(location = 0) out vec4 RGBA0;
layout(location = 1) out vec4 Depth0;
layout(location = 2) out vec4 Normal0;
layout(location = 3) out vec4 Material0;

uniform sampler2D AlbedoMap;
uniform sampler2D AOMap;
uniform sampler2D CavityMap;
uniform sampler2D ReflectivityMap;
uniform sampler2D DerivativeAOCavityMicrosurfaceMap;

vec2 encode (vec3 n)
{
    float p = sqrt(n.z*8+8);
    return vec2(n.xy/p + 0.5);
}

void main()
{
		vec4 data = texture2D(DerivativeAOCavityMicrosurfaceMap, UV_o2);

		vec3 dpdx = dFdx(worldXY_o2);
		vec3 dpdy = dFdy(worldXY_o2);

		float dhdx = dFdx(data.r);
		float dhdy = dFdy(data.r);

		vec3 r1 = cross(dpdy, normPos_o2);
		vec3 r2 = cross(normPos_o2, dpdx);

		vec3 finalNormal = normalize(normPos_o2 - (r1 * dhdx + r2 * dhdy)/dot(dpdx, r1) );

		Normal0.rg = encode(finalNormal);	//Compress the normal data so we can eliminate one texture
		Normal0.b = data.g;	//AO Map
		Normal0.a = data.b;	//Cavity Map


		RGBA0 = texture2D(AlbedoMap, UV_o2);

		Material0 = texture2D(ReflectivityMap, UV_o2);

		Depth0.r = depth_o2/50;
		Depth0.gb = worldXY_o2.xy;
		Depth0.a = data.a;	//Microsurface Map

		//We might have to write to gl_Fragdepth_o2 here, but for now I think it's fine
		gl_FragDepth = log2(logBufDat.y) * logBufDat.x;
}