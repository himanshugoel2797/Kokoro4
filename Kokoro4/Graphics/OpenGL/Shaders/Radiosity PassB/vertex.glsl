

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_modelspace;
layout(location = 1) in vec2 vertexUV;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

// Values that stay constant for the whole mesh.
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform sampler2D FRGBA;



void main(){

	//Sample in a circle around the rendered image and use the average color of these points (using mipmapping) to light the scene in a quick radiosity pass
	// Output position of the vertex, in clip space : MVP * position
	gl_Position = vec4(vertexPosition_modelspace,1);

	// UV of the vertex. No special space for this one
	UV = (vertexPosition_modelspace.xy+vec2(1,1))/2.0;
}