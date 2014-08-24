#version 120

varying vec4 vertColor;
varying vec2 vertTexCoords;

void main(void)
{
    gl_FragColor = vertColor;
}
