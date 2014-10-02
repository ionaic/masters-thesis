// Performed once
GLuint triangleVAO;
GLuint triangleVBO[2];
GLuint triangleProgram;
GLfloat positions = { 0.0, 1.0, 0.0, 1.0, -1.0, 0.0, -1.0, -1.0, 0.0 };
GLfloat colors = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };

glGenVertexArrays(1, &triangleVAO);
glBindVertexArray(triangleVAO);
glGenBuffers(2, &triangleVBO[0]);

glBindBuffer(GL_ARRAY_BUFFER, triangleVBO[0]);
glBufferData(GL_ARRAY_BUFFER, sizeof(positions), positions, GL_STATIC_DRAW);
glBindBuffer(GL_ARRAY_BUFFER, triangleVBO[1]);
glBufferData(GL_ARRAY_BUFFER, sizeof(colors), colors, GL_STATIC_DRAW);

// Performed every loop
glUseProgram(triangleProgram);
glEnableVertexAttribArray(0);
glBindBuffer(GL_ARRAY_BUFFER, triangleVBO[0]);
glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, (void *)0);
glEnableVertexAttribArray(1);
glBindBuffer(GL_ARRAY_BUFFER, triangleVBO[1]);
glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE< 0, (void *)0);

glDrawArrays(GL_TRIANGLES, 0, 3);

glDisableVertexAttribArray(0);
glDisableVertexAttribArray(1);
