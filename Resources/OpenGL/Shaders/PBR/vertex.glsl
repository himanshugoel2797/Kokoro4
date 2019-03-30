
// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 2) in vec2 vertexUV;

uniform float ZNear;
uniform float ZFar;

uniform vec2 im_sz;
uniform int tile_sz;

// Output data ; will be interpolated for each fragment.
flat out int inst_id;
out vec2 UV;

void main(){
	gl_Position = vec4(position, 1);
	
	// UV of the vertex. No special space for this one
	UV = (position.xy+vec2(1,1))/2.0;
	inst_id = gl_InstanceID;
}