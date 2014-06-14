#version 330

in vec3 vertNormal;
in vec3 vertPosition;
in vec4 vertColor;

out vec4 outColor;

uniform float alpha;

const vec3 lightPosition = vec3(5000.0, 55000.0, 10000.0);
const vec3 ambientColor  = vec3(0.3, 0.3, 0.3);
const vec3 diffuseColor  = vec3(0.5, 0.5, 0.5);
const vec3 specularColor = vec3(1.0, 1.0, 1.0);

void main(void)
{
    vec3 normal = normalize(vertNormal);
    vec3 lightDir = normalize(lightPosition);
    
    float specular = 0.0;
    float lambertian = max(dot(lightDir, normal), 0.0);
    
    if(lambertian > 0.0) {
        vec3 reflectDir = reflect(-lightDir, normal);
        vec3 viewDir = normalize(-vertPosition);
        float specAngle = max(dot(reflectDir, viewDir), 0.0);
        specular = pow(specAngle, 4.0);
        // specular = pow(specAngle, 16.0);
        // specular *= lambertian;
    }
    
    vec3 a = ambientColor * vertColor.rgb;
    outColor = vec4(a +
        lambertian * diffuseColor +
        specular * specularColor, alpha);
}
