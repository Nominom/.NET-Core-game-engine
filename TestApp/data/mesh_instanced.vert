#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout (location = 0) in vec3 inPos;
layout (location = 1) in vec3 inNormal;
layout (location = 2) in vec2 inUV;

layout (location = 3) in vec4 model1;
layout (location = 4) in vec4 model2;
layout (location = 5) in vec4 model3;
layout (location = 6) in vec4 model4;
layout (location = 7) in vec3 normal1;
layout (location = 8) in vec3 normal2;
layout (location = 9) in vec3 normal3;

layout (binding = 0) uniform UBO 
{
	mat4 projection;
	mat4 view;
	vec4 lightDir;
	vec4 cameraPos;
} ubo;

layout (location = 0) out vec3 outNormal;
layout (location = 1) out vec2 outUV;
layout (location = 2) out vec3 transformedLightDir;
layout (location = 3) out vec3 fragPos;

out gl_PerVertex
{
	vec4 gl_Position;
};

void main() 
{
	mat4 model = mat4(model1, model2, model3, model4);
	mat3 normalMatrix = mat3(normal1, normal2, normal3);
	mat4 mvp = ubo.projection * ubo.view * model;

	
	//mat3 normalMatrix = mat3(transpose(inverse(model)));
	
	//transformedLightDir = normalMatrix * ubo.lightDir.xyz;
	//normalMatrix = inverse(normalMatrix); //Slow as heck
	//normalMatrix = transpose(normalMatrix);
	
	fragPos = (model * vec4(inPos, 1.0)).xyz;

	outNormal = normalMatrix * inNormal;
	//outWorldNormal = normalMatrix * inNormal;
	outUV = inUV;
	gl_Position = mvp * vec4(inPos.xyz, 1.0);
}