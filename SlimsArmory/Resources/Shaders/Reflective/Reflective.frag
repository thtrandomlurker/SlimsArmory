#version 330 core

in vec3 fPosition;
in vec3 fNormal;
in vec2 fUV;
in vec4 fWeights;
in vec4 fIndices;

uniform vec3 uEye;
uniform vec3 uLightPos;

out vec4 oColor;

void main() {
	vec3 lightDir = normalize(vec3(0.f, 0.f, 0.f) - uLightPos);
	vec3 normal = normalize(fNormal);
	vec3 halfwayDir = normalize(lightDir + uEye);
	float nDotV = dot(normal, normalize(uEye));
	float nDotL = dot(normal, lightDir);
	float nDotH = dot(normal, halfwayDir);

	float spec = pow(nDotH, 20.0f);

	oColor = vec4(1.0f, 1.0f, 0.0f, 1.0f);
}