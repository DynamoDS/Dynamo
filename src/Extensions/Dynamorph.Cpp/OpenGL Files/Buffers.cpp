
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;
using namespace Dynamorph::OpenGL;

// Convert float count to offset.
#define FC2O(x) ((const void *)(x * sizeof(float)))

struct VertexData
{
    float x, y, z;
    float nx, ny, nz;
    float a, r, g, b;
};

VertexBuffer::VertexBuffer() : mVertexBufferId(0)
{
}

VertexBuffer::~VertexBuffer()
{
    if (mVertexBufferId != 0) {
        GL::glDeleteBuffers(1, &mVertexBufferId);
        mVertexBufferId = 0;
    }
}

void VertexBuffer::LoadDataCore(const std::vector<float>& positions)
{
    // TODO(Ben): Ensure that we only get positions in 3D coordinates.

    EnsureVertexBufferCreation();

    int vertices = ((int) std::floor(positions.size() / 3.0));

    std::vector<VertexData> intermediate(vertices);
    for (int vertex = 0; vertex < vertices; ++vertex)
    {
        int offset = vertex * 3;

        VertexData data = {
            positions[offset + 0],  // Position
            positions[offset + 1],
            positions[offset + 2],
            1.0, 1.0, 1.0,          // Normal
            1.0, 1.0, 1.0, 1.0      // Color (a, r, g, b)
        };

        intermediate.push_back(data);
    }

    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);
    GL::glBufferData(GL_ARRAY_BUFFER,
        intermediate.size() * sizeof(VertexData),
        ((const void *) &intermediate[0]), GL_STATIC_DRAW);

    GL::glEnableVertexAttribArray(0);   // Position
    GL::glEnableVertexAttribArray(1);   // Normal
    GL::glEnableVertexAttribArray(2);   // Color
    GL::glDisableVertexAttribArray(3);  // Disabled

    GL::glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(0));
    GL::glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(3));
    GL::glVertexAttribPointer(2, 4, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(6));

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
}

void VertexBuffer::LoadDataCore(const std::vector<float>& positions,
                                const std::vector<float>& colors)
{
    EnsureVertexBufferCreation();
}

void VertexBuffer::EnsureVertexBufferCreation(void)
{
    if (mVertexBufferId == 0)
        GL::glGenBuffers(1, &mVertexBufferId);
}
