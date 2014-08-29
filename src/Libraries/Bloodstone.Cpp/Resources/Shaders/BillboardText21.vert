#version 120

attribute vec3 inPosition;
attribute vec4 inColor;
attribute vec4 inTextCoords;

varying vec4 vertColor;
varying vec2 vertTexCoords;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform vec2 screenSize;

void main(void)
{
    vec2 spriteSize = vec2(
        (inTextCoords.z / screenSize.x) * 2.0,
        (inTextCoords.w / screenSize.y) * 2.0 );

    vec4 ndcPosition = proj * view * model * vec4(inPosition, 1.0);
    vec4 ndcOffsetted = ndcPosition / ndcPosition.w;

    gl_Position = vec4(ndcOffsetted.xy + spriteSize, 0.0, 1.0);

    // For downstream fragment shader.
    vertColor = inColor;
    vertTexCoords = inTextCoords.xy;
}
