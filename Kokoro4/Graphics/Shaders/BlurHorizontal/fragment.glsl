#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;
in vec2 UV_blur[14];

// Ouput data
layout(location = 0) out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D AlbedoMap;
uniform float blurSize;

void main(){
	color = vec4(0);
    color += texture2D(AlbedoMap, UV_blur[ 0])*0.0044299121055113265;
    color += texture2D(AlbedoMap, UV_blur[ 1])*0.00895781211794;
    color += texture2D(AlbedoMap, UV_blur[ 2])*0.0215963866053;
    color += texture2D(AlbedoMap, UV_blur[ 3])*0.0443683338718;
    color += texture2D(AlbedoMap, UV_blur[ 4])*0.0776744219933;
    color += texture2D(AlbedoMap, UV_blur[ 5])*0.115876621105;
    color += texture2D(AlbedoMap, UV_blur[ 6])*0.147308056121;
    color += texture2D(AlbedoMap, UV         )*0.159576912161;
    color += texture2D(AlbedoMap, UV_blur[ 7])*0.147308056121;
    color += texture2D(AlbedoMap, UV_blur[ 8])*0.115876621105;
    color += texture2D(AlbedoMap, UV_blur[ 9])*0.0776744219933;
    color += texture2D(AlbedoMap, UV_blur[10])*0.0443683338718;
    color += texture2D(AlbedoMap, UV_blur[11])*0.0215963866053;
    color += texture2D(AlbedoMap, UV_blur[12])*0.00895781211794;
    color += texture2D(AlbedoMap, UV_blur[13])*0.0044299121055113265;
}