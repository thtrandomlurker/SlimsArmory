#version 330 core

in vec3 fPosition;
in vec3 fNormal;
in vec2 fUV;
in vec4 fWeights;
in vec4 fIndices;

uniform sampler2D uTexture;
uniform vec3 uEye;
uniform vec3 uLightPos;

out vec4 oColor;

void main() {
	vec3 lightDir = normalize(vec3(0.f, 0.f, 0.f) - uLightPos);
	vec3 normal = normalize(fNormal);
	float nDotV = dot(normal, normalize(uEye));
	float nDotL = dot(normal, lightDir);

	vec4 diffuse = texture(uTexture, fUV.xy);

	vec3 diffuseLighting = (diffuse.rgb * 0.5) + (diffuse.rgb * (nDotL * 0.75 + 0.25));

	oColor = vec4(diffuseLighting, diffuse.a);
}