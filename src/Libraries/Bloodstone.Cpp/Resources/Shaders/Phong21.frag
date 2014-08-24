#version 120

varying vec3 vertNormal;
varying vec3 vertPosition;
varying vec4 vertColor;

uniform float alpha;

// Various control parameters merged into a single vector value:
// 
//  controlParams[0]: "1.0" for rendering points/lines
//                    "3.0" for rendering triangles
// 
//  controlParams[1]: "1.0" for overriding color.
// 
uniform vec4 controlParams;

const vec3 lightPosition = vec3(5000.0, 55000.0, 10000.0);
const vec3 ambientColor  = vec3(0.3, 0.3, 0.3);
const vec3 diffuseColor  = vec3(0.5, 0.5, 0.5);
const vec3 specularColor = vec3(0.8, 0.8, 0.8);

void main(void)
{
    // Rendering primitives of lower dimensionality (e.g. points and lines)
    // will not require shading to be done, just take their current colors.
    // 
    if (controlParams[0] < 3.0)
    {
        gl_FragColor = vec4(vertColor.rgb, alpha);
        return;
    }

    vec3 normal = normalize(vertNormal);
    vec3 finalColor = vec3(0.0, 0.0, 0.0);
    
    // BEGIN - For multiple lights
    vec3 lightDir = normalize(lightPosition - vertPosition);
    vec3 viewDir = normalize(-vertPosition);
    vec3 reflectDir = normalize(-reflect(lightDir, normal));
    
    // Calculate diffuse term.
    vec3 diffuse = vertColor.rgb * max(dot(normal, lightDir), 0.0);
    diffuse = clamp(diffuse, 0.0, 1.0); 

    // Calculate specular term.
    float lambertian = max(dot(reflectDir, viewDir), 0.0);
    vec3 specular = specularColor * pow(lambertian, 4.0);
    specular = clamp(specular, 0.0, 1.0); 

    vec3 ambient = ambientColor * vertColor.rgb;
    finalColor += ambient + diffuse + specular;
    // END - For multiple lights

    gl_FragColor = vec4(finalColor.rgb, 1.0);
}
