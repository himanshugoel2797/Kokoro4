#version 430 core

in vec3 norm;
in vec3 pos;
in vec2 UV;

in float flogz;
uniform float Fcoef;

void main()
{
    //gl_FragDepth = Fcoef * 0.5 * log2(flogz);
}