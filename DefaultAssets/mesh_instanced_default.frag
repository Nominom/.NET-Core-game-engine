#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable


layout (binding = 0) uniform UBO 
{
	mat4 projection;
	mat4 view;
	vec4 lightDir;
	vec4 cameraPos;
} ubo;


layout (binding = 1) uniform sampler2D samplerColorMap;

layout (location = 0) in vec3 inNormal;
layout (location = 1) in vec2 inUV;
layout (location = 2) in vec3 transformedLightDir;
layout (location = 3) in vec3 fragPos;

layout (location = 0) out vec4 outFragColor;

void main() 
{
	vec4 color = texture(samplerColorMap, inUV);
	
	vec3 N = normalize(inNormal);
	vec3 L = normalize(ubo.lightDir.xyz);
	vec3 R = reflect(-L, N);
	vec3 V = normalize(fragPos - ubo.cameraPos.xyz);
	
	vec3 diffuse = vec3(max(dot(N, -L), 0.0));
	vec3 specular = pow(max(dot(R, V), 0.0), 16.0) * vec3(0.75);
	
	
	//outFragColor = vec4(diffuse * color.rgb + specular, 1.0);	

	vec3 final = (diffuse + vec3(0.1)) * color.rgb + specular;
	outFragColor = vec4(final, 1.0);
	//outFragColor = vec4(1,1,1,1);
}