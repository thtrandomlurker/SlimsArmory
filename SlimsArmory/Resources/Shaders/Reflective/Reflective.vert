﻿#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUV;
layout (location = 3) in vec4 aWeights;
layout (location = 4) in vec4 aIndices;

out vec3 fPosition;
out vec3 fNormal;
out vec2 fUV;
out vec4 fWeights;
out vec4 fIndices;

uniform mat4 matView;
uniform mat4 matModel;
uniform mat4 matProj;

void main() {
	vec4 worldPos = matModel * vec4(aPosition, 1.0f);
	fPosition = worldPos.xyz;
	mat3 matNormal = transpose(inverse(mat3(matModel)));
	fNormal = normalize(matNormal * aNormal);
	fUV = aUV;
	fWeights = aWeights;
	fIndices = aIndices;
	gl_Position = matProj * matView * worldPos;
}