#version 330 core

layout(location = 0) in vec3 vertex_screenspace;
layout(location = 1) in vec3 vertex_color;

out vec3 vColor;

void main(){
    gl_Position = vec4(vertex_screenspace, 1.0);
    vColor = vertex_color;
}

