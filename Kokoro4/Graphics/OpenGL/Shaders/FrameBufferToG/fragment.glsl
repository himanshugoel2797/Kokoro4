#version 430 core

// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 2) out vec4 color;

// Values that stay constant for the whole mesh.
uniform sampler2D AlbedoMap;

void main(){
	color = texture2D(AlbedoMap, UV);
}