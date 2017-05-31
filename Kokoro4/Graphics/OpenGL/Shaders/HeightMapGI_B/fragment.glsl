//Uniforms
uniform vec2 Direction;

layout(std140) uniform V_dash {
	vec4 val[256];
} V_dash_t;

// Interpolated values from the vertex shaders
in vec2 UV;

// Ouput data
layout(location = 0) out vec4 v_sh;
layout(location = 1) out vec4 u_sh;

// Values that stay constant for the whole mesh.
layout(bindless_sampler) uniform sampler2D SrcMap;
layout(bindless_sampler) uniform sampler2D SrcColor;

#define MAX_STEPS (256.0f * STEP)
#define STEP (1.0f / 256.0f)

#define SQRT_HALF 0.707106781
#define SQRT3_BY_SQRT2 1.22474487
#define SQRT2_5_BY_4 1.76776695
#define SQRT2_7_BY_4 2.47487373

vec4 P_z(float z) {
	vec4 P;

	P.x = SQRT_HALF;
	P.y = SQRT3_BY_SQRT2 * z;
	P.z = SQRT2_5_BY_4 * (3 * z * z - 1);
	P.w = SQRT2_7_BY_4 * (5 * z * z * z - 3 * z);

	return P;
}

void main(){

	//For the current point, determine the maximum point along the line
	float height = texture(SrcMap, UV).r;

	vec3 N1, N2, N3, N4;
	N1.xz = UV - vec2(0, STEP);
	N2.xz = UV + vec2(0, STEP);
	N3.xz = UV - vec2(STEP, 0);
	N4.xz = UV + vec2(STEP, 0);

	N1.y = texture(SrcMap, N1.xz).r;
	N2.y = texture(SrcMap, N2.xz).r;
	N3.y = texture(SrcMap, N3.xz).r;
	N4.y = texture(SrcMap, N4.xz).r;

	vec3 normal = normalize(cross(N2 - N3, N1 - N4));

	float wmin = -asin(dot(normal, vec3(Direction.x, Direction.y, 0)) );


	vec2 pos = UV;
	vec2 maxVal_T = vec2(0);
	float maxVal_H = height;
	float wmax = wmin;
	
	vec3 IR = vec3(0);
	int IR_count = 0;

	for(float steps = 0; steps < MAX_STEPS; steps += STEP)
	{
		pos = UV + Direction * steps;
		pos.x = clamp(pos.x, 0.0f, 1.0f);
		pos.y = clamp(pos.y, 0.0f, 1.0f);

		float sampleHeight = texture(SrcMap, pos).r;
		float angle = atan((sampleHeight - height) / steps);

		//Set maxVal_H to the new maximum height
		bool control = sampleHeight > maxVal_H;
		if(control)maxVal_H = sampleHeight;
		if(control)maxVal_T = pos;
		if(control)wmax = angle;

		//Find a visible point and calculate the incoming radiance from that point
		bool IRcontrol = angle >= wmin && angle >= wmax;
		if(IRcontrol) IR += clamp(dot(normal, normalize(vec3(Direction.x, sampleHeight, Direction.y))), 0.0f, 1.0f) * texture(SrcColor, pos).rgb * 1.0f/(10000 * steps * steps + 0.0001f);
		if(IRcontrol) IR_count++;
	}

	//Calculate the angle 
	
	v_sh.rgba = V_dash_t.val[int(PI * 0.5f * 255.0f / (PI * 0.5f))] - V_dash_t.val[int(wmax * 255.0f / (PI * 0.5f ))];
	
	//Store incoming radiance
	u_sh.rgb = IR / float(IR_count);
	u_sh.a = wmax;

	//In the next pass, read the incoming radiance info for bounce lighting

	//Visibility function reconstruction
	//u_sh.r = dot(v_sh.rgba, P_z(cos(angle from zenith)));
}