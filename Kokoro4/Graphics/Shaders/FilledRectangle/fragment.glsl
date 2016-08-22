
// Interpolated values from the vertex shaders
in vec2 UV;


// Ouput data
layout(location = 0) out vec4 color;

uniform vec2 WindowSize;

// Values that stay constant for the whole mesh.
uniform vec4 AlbedoColor;
uniform vec2 Position;
uniform vec2 Size;

void main(){

	vec2 p = Position / WindowSize;
	vec2 s = Size / WindowSize;

	if(UV.x >= p.x && UV.y >= p.y && UV.x <= (p.x + s.x) && UV.y <= (p.y + s.y))
		color = AlbedoColor;
	else
		color = vec4(1);

}