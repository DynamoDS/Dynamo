
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
                                const std::vector<float>& rgbaColors)
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
        data[vertex].r = rgbaColors[colorOffset + 0];
        data[vertex].g = rgbaColors[colorOffset + 1];
        data[vertex].b = rgbaColors[colorOffset + 2];
        data[vertex].a = rgbaColors[colorOffset + 3];
    }

    LoadDataInternal(data);
}

void VertexBuffer::GetBoundingBoxCore(BoundingBox* pBoundingBox)
{
    (*pBoundingBox) = mBoundingBox;
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

    std::size_t count = vertices.size();
    if (count <= 0)
        mBoundingBox.Reset(0.0f, 0.0f, 0.0f);
    else
    {
        const VertexData* p = &vertices[0];
        mBoundingBox.Reset(p[0].x, p[0].y, p[0].z);
        for (std::size_t index = 0; index < count; ++index)
            mBoundingBox.EvaluatePoint(p[index].x, p[index].y, p[index].z);
    }

    GL::glBindVertexArray(mVertexArrayId);
    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);
    GL::glBufferData(GL_ARRAY_BUFFER, bytes, &vertices[0], GL_STATIC_DRAW);

    GL::glEnableVertexAttribArray(0);   // Position
    GL::glEnableVertexAttribArray(1);   // Color

    GL::glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(0));
    GL::glVertexAttribPointer(1, 4, GL_FLOAT, GL_FALSE, sizeof(VertexData), FC2O(3));

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
    GL::glBindVertexArray(0);
}
