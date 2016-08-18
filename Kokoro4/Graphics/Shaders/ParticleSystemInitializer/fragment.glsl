#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 0) out vec4 c_posData;
layout(location = 1) out vec4 c_dVData;

uniform sampler2D PosData;
uniform sampler2D dVData;

// Values that stay constant for the whole mesh.
uniform vec2 MassRange;

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main(){
	c_posData = vec4(0, 0, 0, -rand(gl_FragCoord.zx));
	c_dVData = vec4(rand(gl_FragCoord.xy), rand(gl_FragCoord.yz), rand(gl_FragCoord.yx), 1);	
}