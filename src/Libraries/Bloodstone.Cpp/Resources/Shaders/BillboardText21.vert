#version 120

attribute vec3 inPosition;
attribute vec4 inColor;
attribute vec4 inTextCoords;

varying vec4 vertColor;
varying vec2 vertTexCoords;

uniform vec3 camPosition;
uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main(void)
{
    vec3 at     = normalize(camPosition - inPosition);
    vec3 right  = cross(vec3( 0.0, 1.0, 0.0 ),  at);
    vec3 up     = cross(at, right);

    mat4 bbt = mat4( vec4(right,      0.0 ),
                     vec4(up,         0.0 ),
                     vec4(at,         0.0 ),
                     vec4(inPosition, 0.0 ) );

    vec4 offset = vec4(inTextCoords.zw, 0.0, 1.0);
    gl_Position = proj * view * model *  vec4(offset, 1.0);

    // For downstream fragment shader.
    vertColor = inColor;
    vertTexCoords = inTextCoords.xy;
}
