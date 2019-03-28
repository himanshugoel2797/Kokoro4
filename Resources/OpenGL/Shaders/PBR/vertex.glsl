
// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 positionO;
layout(location = 2) in vec2 vertexUV;

uniform float ZNear;
uniform float ZFar;

uniform vec2 im_sz;
uniform int tile_sz;

// Output data ; will be interpolated for each fragment.
out vec2 UV;

void main(){
	vec3 position = positionO;//vec3(0);//positionO.xyz * 0.0005f;//vec2(tile_sz) / im_sz;// + bbox_min / im_sz;
	//position.xy = positionO.xy * vec2(tile_sz) / im_sz + (bbox_min * 2) / im_sz - vec2(1.0f);
	gl_Position = vec4(position, 1);
	
	// UV of the vertex. No special space for this one
	UV = (position.xy+vec2(1,1))/2.0;
}