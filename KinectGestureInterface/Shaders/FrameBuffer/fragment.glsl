// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 color;
// Values that stay constant for the whole mesh.

layout(std140) uniform Material_t {
	uvec2 Albedo;
	vec4 lHandPos;	//xy = hand, zw = tip
	vec4 rHandPos;	//xy = hand, zw = tip
	vec2 frameDim;
	float PointCount;
} Material;

void drawDetection(vec2 HandLocal, vec2 HandTipLocal, vec2 iUV, float depth) {

	vec2 HandDiff = abs(HandLocal - iUV);
	vec2 HandTipDiff = abs(HandTipLocal - iUV);

	vec2 HandCenter = (HandLocal + HandTipLocal) * 0.5f;
	if(distance(iUV, HandCenter) < distance(HandTipLocal, HandCenter) * 2.5f)
		color.b = 1;

	if(HandDiff.x <= 0.025f && HandDiff.y <= 0.025f)
		color.r = 1;

	if(HandTipDiff.x <= 0.025f && HandTipDiff.y <= 0.025f)
		color.g = 1;
}

void main(){
	vec2 iUV = vec2(UV.x, 1.0f - UV.y);

	uvec4 tex = texture(usampler2D(Material.Albedo), iUV);
	
	color.rgb = vec3(tex.r / 65535.0f);

	vec2 lHandLocal = (Material.lHandPos.xy / Material.frameDim.xy);
	vec2 lHandTipLocal = (Material.lHandPos.zw / Material.frameDim.xy);
	
	vec2 rHandLocal = (Material.rHandPos.xy / Material.frameDim.xy);
	vec2 rHandTipLocal = (Material.rHandPos.zw / Material.frameDim.xy);
	
	drawDetection(lHandLocal, lHandTipLocal, iUV, tex.r);
	drawDetection(rHandLocal, rHandTipLocal, iUV, tex.r);
		
	color.a = 1;
}