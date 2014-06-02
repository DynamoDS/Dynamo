
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
    mVertexBufferId(0),
    mPrimitiveType(Dynamorph::IVertexBuffer::PrimitiveType::None)
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

    switch (mPrimitiveType)
    {
    case Dynamorph::IVertexBuffer::PrimitiveType::Point:
        GL::glDrawArrays(GL_POINTS, 0, mVertexCount);
        break;
    case Dynamorph::IVertexBuffer::PrimitiveType::LineStrip:
        GL::glDrawArrays(GL_LINE_STRIP, 0, mVertexCount);
        break;
    case Dynamorph::IVertexBuffer::PrimitiveType::Triangle:
        GL::glDrawArrays(GL_TRIANGLES, 0, mVertexCount);
        break;
    }
}

void VertexBuffer::LoadDataCore(const GeometryData& geometries)
{
    EnsureVertexBufferCreation();

    const GeometryData* p = &geometries;
    if ((dynamic_cast<const PointGeometryData *>(p)) != nullptr)
        mPrimitiveType = Dynamorph::IVertexBuffer::PrimitiveType::Point;
    else if ((dynamic_cast<const LineStripGeometryData *>(p)) != nullptr)
        mPrimitiveType = Dynamorph::IVertexBuffer::PrimitiveType::LineStrip;
    else if ((dynamic_cast<const TriangleGeometryData *>(p)) != nullptr)
        mPrimitiveType = Dynamorph::IVertexBuffer::PrimitiveType::Triangle;

    mVertexCount = geometries.VertexCount();
    std::vector<VertexData> data(mVertexCount);
    for (int vertex = 0; vertex < mVertexCount; ++vertex)
    {
        auto pCoordinates = geometries.GetCoordinates(vertex);
        auto pRgbaColors = geometries.GetRgbaColors(vertex);

        data[vertex].x = pCoordinates[0];
        data[vertex].y = pCoordinates[1];
        data[vertex].z = pCoordinates[2];
        data[vertex].r = pRgbaColors[0];
        data[vertex].g = pRgbaColors[1];
        data[vertex].b = pRgbaColors[2];
        data[vertex].a = pRgbaColors[3];
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
