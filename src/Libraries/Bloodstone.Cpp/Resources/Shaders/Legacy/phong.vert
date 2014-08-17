#version 120

attribute vec3 inPosition;
attribute vec3 inNormal;
attribute vec4 inColor;

varying vec3 vertNormal;
varying vec3 vertPosition;
varying vec4 vertColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform mat4 normalMatrix;
uniform vec4 colorOverride;

// Various control parameters merged into a single vector value:
// 
//  controlParams[0]: "1.0" for rendering points/lines
//                    "3.0" for rendering triangles
// 
//  controlParams[1]: "1.0" for overriding color.
// 
uniform vec4 controlParams;

void main(void)
{
    vec4 viewPos = view * model * vec4(inPosition, 1.0);
    gl_Position = proj * viewPos;
    
    // Compute parameters for fragment shader
    vertPosition = vec3(viewPos) / viewPos.w;

    vertColor = inColor;
    if (controlParams[1] > 0.5)
        vertColor = colorOverride;

    vertNormal = vec3(normalMatrix * vec4(inNormal, 0.0));
}
