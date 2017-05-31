//Uniforms
uniform vec2 Direction;
uniform float SrcRadianceMultiplier;

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
layout(bindless_sampler) uniform sampler2D SrcRadiance;

#define DIV (128.0f)
#define MAX_STEPS (DIV * STEP)
#define STEP (1.0f / DIV)

#define TABLE_INDEX(x) (int(( (x)) * 255.0f / (PI * 0.5f)))

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

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void main(){

	//For the current point, determine the maximum point along the line
	float height = texture(SrcMap, UV).r;

	//Calculate the surface normal
	vec3 N1, N2, N3, N4;
	N1.xz = UV - vec2(0, STEP);
	N2.xz = UV + vec2(0, STEP);
	N3.xz = UV - vec2(STEP, 0);
	N4.xz = UV + vec2(STEP, 0);

	N1.y = texture(SrcMap, N1.xz).r;
	N2.y = texture(SrcMap, N2.xz).r;
	N3.y = texture(SrcMap, N3.xz).r;
	N4.y = texture(SrcMap, N4.xz).r;

	vec3 normal = normalize(cross(N2 - N1, N4 - N3));

	vec2 pos = UV;
	
	vec4 IR = SrcRadianceMultiplier * texture(SrcRadiance, UV);
	float wmin = -asin(dot(normal, vec3(Direction.x, 0, Direction.y)) );
	float wmax = wmin;
	vec2 prevUV = vec2(-1, -1);

	vec4 prevRadiance = vec4(0);
	float prevAngle = 0;

	for(float steps = 0; steps < MAX_STEPS; steps += STEP)
	{
		pos = UV + Direction * steps;
		pos.x = clamp(pos.x, 0.0f, 1.0f);
		pos.y = clamp(pos.y, 0.0f, 1.0f);

		float sampleHeight = texture(SrcMap, pos).r;
		float angle = clamp(atan((sampleHeight - height) / steps), 0, PI * 0.5);

		//Maximize the blocking angle, prevent duplicates
		bool control = angle >= wmax && prevUV != pos;
		prevUV = pos;
		
		//Set maxVal_H to the new maximum height
		if(control)wmax = angle;

		//Find a visible point and calculate the incoming radiance from that point
		vec4 radiance = clamp(dot(normal, normalize(vec3(Direction.x, sampleHeight - height, Direction.y))), 0, 1) * texture(SrcColor, pos).rgba * 1.0f/(steps * steps + 1.0f);
		if(control) IR += clamp(prevRadiance - radiance, vec4(0), vec4(1)) * V_dash_t.val[TABLE_INDEX(prevAngle)];
		if(control) prevRadiance = radiance;
		if(control) prevAngle = angle;
	}

	IR += clamp(prevRadiance - 0, vec4(0), vec4(1)) * V_dash_t.val[TABLE_INDEX(prevAngle)];

	//IR = clamp(dot(normal, normalize(vec3(Direction.x, texture(SrcMap, pos).r - height, Direction.y))), 0, 1) * texture(SrcColor, UV).rgba * 1.0f/(1 + 0.0001f);

	//Store v's basis vector
	v_sh.rgba = V_dash_t.val[TABLE_INDEX(PI * 0.5f)] - V_dash_t.val[TABLE_INDEX(wmax)];
	
	//Store incoming radiance
	u_sh.rgba = IR;
	//u_sh.a = wmax;

	//In the next pass, read the incoming radiance info for bounce lighting
	
	//Visibility function reconstruction
	u_sh.rgba = texture(SrcColor, UV) * (clamp(dot(v_sh.rgba, P_z(cos(PI * 0.6f))), 0, 0.8) + 0.2 );
	
	
	//Lighting function reconstruction
	//u_sh.rgba = texture(SrcColor, UV) * (clamp(dot(u_sh.rgba, P_z(cos(PI * 0.5f))), 0, 1));
}