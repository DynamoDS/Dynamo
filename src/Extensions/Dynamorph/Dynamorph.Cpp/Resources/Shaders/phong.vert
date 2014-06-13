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

void main(void)
{
    vec4 modelPos = vec4(inPosition, 1.0);
    gl_Position = proj * view * model * modelPos;
    
    // Compute varying parameters
    vec4 viewPos = view * model * modelPos;
    vertPosition = vec3(viewPos) / viewPos.w;
    vertColor = inColor;
    vertNormal = normalize(inNormal);
}
