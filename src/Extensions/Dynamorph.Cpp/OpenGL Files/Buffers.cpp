
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
    // float nx, ny, nz;
    // float a, r, g, b;

    VertexData() : x(0), y(0), z(0)
    {
    }
};

VertexBuffer::VertexBuffer() :
    mVertexCount(0),
    mVertexArrayId(0),
    mVertexBufferId(0)
{
}

VertexBuffer::~VertexBuffer()
{
    if (mVertexBufferId != 0) {
        GL::glDeleteBuffers(1, &mVertexBufferId);
        mVertexBufferId = 0;
    }

    if (mVertexArrayId != 0) {
        GL::glDeleteVertexArrays(1, &mVertexArrayId);
        mVertexArrayId = 0;
    }
}

void VertexBuffer::Render(void) const
{
    GL::glBindVertexArray(mVertexArrayId);
    GL::glDrawArrays(GL_TRIANGLES, 0, mVertexCount);
}

void VertexBuffer::LoadDataCore(const std::vector<float>& positions)
{
    // TODO(Ben): Ensure that we only get positions in 3D coordinates.

    EnsureVertexBufferCreation();

    mVertexCount = ((int) std::floor(positions.size() / 3.0));

    std::vector<VertexData> data(mVertexCount);
    const auto bytes = data.size() * sizeof(VertexData);

    for (int vertex = 0; vertex < mVertexCount; ++vertex)
    {
        int offset = vertex * 3;
        data[vertex].x = positions[offset + 0];
        data[vertex].y = positions[offset + 1];
        data[vertex].z = positions[offset + 2];
    }

    GL::glBindVertexArray(mVertexArrayId);
    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);
    GL::glBufferData(GL_ARRAY_BUFFER, bytes, &data[0], GL_STATIC_DRAW);

    GL::glEnableVertexAttribArray(0);   // Position
    // GL::glEnableVertexAttribArray(1);   // Normal
    // GL::glEnableVertexAttribArray(2);   // Color
    // GL::glDisableVertexAttribArray(3);  // Disabled

    GL::glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, FC2O(0));
    // GL::glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(3));
    // GL::glVertexAttribPointer(2, 4, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(6));

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
    GL::glBindVertexArray(0);
}

void VertexBuffer::LoadDataCore(const std::vector<float>& positions,
                                const std::vector<float>& colors)
{
    EnsureVertexBufferCreation();
}

void VertexBuffer::EnsureVertexBufferCreation(void)
{
    if (mVertexArrayId == 0)
        GL::glGenVertexArrays(1, &mVertexArrayId);

    if (mVertexBufferId == 0)
        GL::glGenBuffers(1, &mVertexBufferId);
}
