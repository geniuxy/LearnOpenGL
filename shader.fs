#version 330 core
out vec4 FragColor;

in vec3 FragPos;  
in vec3 Normal;
in vec2 TexCoords;

struct Material {
    // vec3 ambient; // 环境光照下这个表面反射的是什么颜色，通常与表面的颜色相同
    // vec3 diffuse; // 漫反射光照下表面的颜色
    // vec3 specular; // 表面上镜面高光的颜色

    sampler2D diffuse; // 漫反射光照改成漫反射贴图，同时移除了环境光材质颜色向量，因为环境光颜色在几乎所有情况下都等于漫反射颜色
    sampler2D specular; // 镜面光贴图
    float shininess; // 影响镜面高光的散射/半径
}; 

struct Light {
    vec3 position; // 使用定向光就不再需要了
    vec3 direction; // 使用点光源就不需要了
    float cutOff; // “手电筒”模式时需要用到
    float outerCutOff; // 

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float constant;
    float linear;
    float quadratic;
};

uniform vec3 viewPos;
uniform Material material;
uniform Light light;

void main()
{
    // ambient
    vec3 ambient = light.ambient * texture(material.diffuse, TexCoords).rgb;
    
    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * texture(material.diffuse, TexCoords).rgb;  
    
    // specular
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * texture(material.specular, TexCoords).rgb;  
    
    // spotlight (soft edges)
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = (light.cutOff - light.outerCutOff);
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
    diffuse  *= intensity;
    specular *= intensity;
    
    // attenuation
    float distance    = length(light.position - FragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    ambient  *= attenuation; 
    diffuse   *= attenuation;
    specular *= attenuation;   
        
    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}