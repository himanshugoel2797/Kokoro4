#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;

// Output data ; will be interpolated for each fragment.
out vec2 UV;
out vec2 UV_blur[14];

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	gl_Position =  vec4(vertexPosition_modelspace, 1);
	const int div = 2;
	vec2 vertexUV = (vertexPosition_modelspace.xy+vec2(1,1))/2.0;
	// UV of the vertex. No special space for this one.
	UV = vertexUV;
	UV_blur[ 0] = vertexUV + vec2(0.0, -0.028/div);
    UV_blur[ 1] = vertexUV + vec2(0.0, -0.024/div);
    UV_blur[ 2] = vertexUV + vec2(0.0, -0.020/div);
    UV_blur[ 3] = vertexUV + vec2(0.0, -0.016/div);
    UV_blur[ 4] = vertexUV + vec2(0.0, -0.012/div);
    UV_blur[ 5] = vertexUV + vec2(0.0, -0.008/div);
    UV_blur[ 6] = vertexUV + vec2(0.0, -0.004/div);
    UV_blur[ 7] = vertexUV + vec2(0.0,  0.004/div);
    UV_blur[ 8] = vertexUV + vec2(0.0,  0.008/div);
    UV_blur[ 9] = vertexUV + vec2(0.0,  0.012/div);
    UV_blur[10] = vertexUV + vec2(0.0,  0.016/div);
    UV_blur[11] = vertexUV + vec2(0.0,  0.020/div);
    UV_blur[12] = vertexUV + vec2(0.0,  0.024/div);
    UV_blur[13] = vertexUV + vec2(0.0,  0.028/div);


}