
// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 0) out vec4 color;

uniform vec2 WindowSize;

// Values that stay constant for the whole mesh.
layout(bindless_sampler) uniform sampler2D AlbedoMap;
uniform vec2 Position;
uniform vec2 Size;

void main(){

	vec2 p = Position / WindowSize;
	vec2 s = Size / WindowSize;

	p.y = 1 - p.y - s.y;

	if(UV.x >= p.x && UV.y >= p.y && UV.x <= (p.x + s.x) && UV.y <= (p.y + s.y))
		color = texture(AlbedoMap, (UV - p)/s);
	else
		color = vec4(0);

}