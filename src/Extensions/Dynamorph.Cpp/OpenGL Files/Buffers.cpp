
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;
using namespace Dynamorph::OpenGL;

// Convert float count to offset.
#define FC2O(x) ((const void *)(x * sizeof(float)))

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
    for (int vertex = 0; vertex < mVertexCount; ++vertex)
    {
        int offset = vertex * 3;
        data[vertex].x = positions[offset + 0];
        data[vertex].y = positions[offset + 1];
        data[vertex].z = positions[offset + 2];
    }

    LoadDataInternal(data);
}

void VertexBuffer::LoadDataCore(const std::vector<float>& positions,
                                const std::vector<float>& argbColors)
{
    EnsureVertexBufferCreation();

    mVertexCount = ((int) std::floor(positions.size() / 3.0));

    std::vector<VertexData> data(mVertexCount);
    for (int vertex = 0; vertex < mVertexCount; ++vertex)
    {
        int posOffset = vertex * 3;
        int colorOffset = vertex * 4;
        data[vertex].x = positions[posOffset + 0];
        data[vertex].y = positions[posOffset + 1];
        data[vertex].z = positions[posOffset + 2];
        data[vertex].a = positions[colorOffset + 0];
        data[vertex].r = positions[colorOffset + 1];
        data[vertex].g = positions[colorOffset + 2];
        data[vertex].b = positions[colorOffset + 3];
    }

    LoadDataInternal(data);
}

void VertexBuffer::EnsureVertexBufferCreation(void)
{
    if (mVertexArrayId == 0)
        GL::glGenVertexArrays(1, &mVertexArrayId);

    if (mVertexBufferId == 0)
        GL::glGenBuffers(1, &mVertexBufferId);
}

void VertexBuffer::LoadDataInternal(const std::vector<VertexData>& vertices)
{
    const auto bytes = vertices.size() * sizeof(VertexData);

    GL::glBindVertexArray(mVertexArrayId);
    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);
    GL::glBufferData(GL_ARRAY_BUFFER, bytes, &vertices[0], GL_STATIC_DRAW);

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
