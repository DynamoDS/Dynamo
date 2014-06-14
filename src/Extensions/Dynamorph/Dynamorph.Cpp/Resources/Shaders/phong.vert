#version 330

layout (location = 0) in vec3 inPosition;
layout (location = 1) in vec3 inNormal;
layout (location = 2) in vec4 inColor;

out vec3 vertNormal;
out vec3 vertPosition;
out vec4 vertColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;
uniform mat4 normalMatrix;

void main(void)
{
    vec4 viewPos = view * model * vec4(inPosition, 1.0);
    gl_Position = proj * viewPos;
    
    // Compute parameters for fragment shader
    vertPosition = vec3(viewPos) / viewPos.w;
    vertColor = inColor;
    vertNormal = vec3(normalMatrix * vec4(inNormal, 0.0));
}
