#version 120

attribute vec3 inPosition;
attribute vec4 inColor;
attribute vec4 inTextCoords;

varying vec4 vertColor;
varying vec2 vertTexCoords;

uniform vec3 camPosition;
uniform vec3 camUpVector;
uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main(void)
{
    vec3 at     = normalize(camPosition - inPosition);
    vec3 right  = cross(vec3(0.0, 1.0, 0.0), at);
    vec3 up     = cross(at, right);

    vec4 r = vec4(inTextCoords.z * right, 0.0);
    vec4 u = vec4(inTextCoords.w * up, 0.0);
    vec4 direction = r + u;
    gl_Position = proj * view * model * (inPosition + direction);

    // For downstream fragment shader.
    vertColor = inColor;
    vertTexCoords = inTextCoords.xy;
}
