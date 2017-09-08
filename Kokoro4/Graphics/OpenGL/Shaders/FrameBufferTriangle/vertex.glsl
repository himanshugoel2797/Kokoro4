﻿
// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec2 vertexUV;

uniform float ZNear;
uniform float ZFar;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

void main(){
	gl_Position = vec4(position, 1);
	
	// UV of the vertex. No special space for this one
	UV = vertexUV;
}