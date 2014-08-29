
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamo::Bloodstone;
using namespace Dynamo::Bloodstone::OpenGL;

// Convert float count to offset.
#define FC2O(x) ((const void *)(x * sizeof(float)))

// ================================================================================
// VertexBuffer
// ================================================================================

VertexBuffer::VertexBuffer() :
    mVertexCount(0),
    mVertexArrayId(0),
    mVertexBufferId(0),
    mPrimitiveType(Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::None)
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
    if (mVertexCount <= 0) // Nothing to render.
        return;

    GL::glBindVertexArray(mVertexArrayId);

    switch (mPrimitiveType)
    {
    case Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::Point:
        GL::glDrawArrays(GL_POINTS, 0, mVertexCount);
        break;
    case Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::LineStrip:
        {
            auto vc = mSegmentVertexCount.begin();
            for (int start = 0; vc != mSegmentVertexCount.end(); ++vc)
            {
                int vertexCount = *vc;
                if (vertexCount > 0) {
                    GL::glDrawArrays(GL_LINE_STRIP, start, vertexCount);
                    start = start + vertexCount;
                }
            }
            break;
        }
    case Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::Triangle:
        GL::glDrawArrays(GL_TRIANGLES, 0, mVertexCount);
        break;
    }
}

IVertexBuffer::PrimitiveType VertexBuffer::GetPrimitiveTypeCore() const
{
    return this->mPrimitiveType;
}

void VertexBuffer::LoadDataCore(const GeometryData& geometries)
{
    EnsureVertexBufferCreation();

    const GeometryData* p = &geometries;
    auto pgd = dynamic_cast<const PointGeometryData *>(p);
    auto lgd = dynamic_cast<const LineStripGeometryData *>(p);
    auto tgd = dynamic_cast<const TriangleGeometryData *>(p);

    if (pgd != nullptr)
        mPrimitiveType = Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::Point;
    else if (lgd != nullptr)
        mPrimitiveType = Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::LineStrip;
    else if (tgd != nullptr)
        mPrimitiveType = Dynamo::Bloodstone::IVertexBuffer::PrimitiveType::Triangle;
 
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

    if (tgd != nullptr)
    {
        // We have normal values when we deal with triangles.
        const float* pNormalCoords = tgd->GetNormalCoords(0);
        for (int vertex = 0; vertex < mVertexCount; ++vertex)
        {
            data[vertex].nx = pNormalCoords[0];
            data[vertex].ny = pNormalCoords[1];
            data[vertex].nz = pNormalCoords[2];
            pNormalCoords = pNormalCoords + 3;
        }
    }
    else if (lgd != nullptr)
    {
        auto segments = lgd->GetSegmentCount();
        auto svc = lgd->GetSegmentVertexCounts();
        for (int segment = 0; segment < segments; ++segment)
            mSegmentVertexCount.push_back(svc[segment]);
    }

    LoadDataInternal(data);
}

void VertexBuffer::GetBoundingBoxCore(BoundingBox* pBoundingBox) const
{
    (*pBoundingBox) = mBoundingBox;
}

void VertexBuffer::BindToShaderProgramCore(IShaderProgram* pShaderProgram)
{
    EnsureVertexBufferCreation();

    GL::glBindVertexArray(mVertexArrayId);
    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);

    GL::glEnableVertexAttribArray(0);   // Position
    GL::glEnableVertexAttribArray(1);   // Normal
    GL::glEnableVertexAttribArray(2);   // Color

    const auto pProgram = dynamic_cast<ShaderProgram *>(pShaderProgram);
    const auto locPosition = pProgram->GetAttributeLocation("inPosition");
    const auto locNormal = pProgram->GetAttributeLocation("inNormal");
    const auto locColor = pProgram->GetAttributeLocation("inColor");

    auto stride = ((int) sizeof(VertexData));
    GL::glVertexAttribPointer(locPosition, 3, GL_FLOAT, GL_FALSE, stride, FC2O(0));
    GL::glVertexAttribPointer(locNormal,   3, GL_FLOAT, GL_FALSE, stride, FC2O(3));
    GL::glVertexAttribPointer(locColor,    4, GL_FLOAT, GL_FALSE, stride, FC2O(6));

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
    GL::glBindVertexArray(0);
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

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
    GL::glBindVertexArray(0);
}

// ================================================================================
// BillboardVertexBuffer
// ================================================================================

BillboardVertexBuffer::BillboardVertexBuffer(const IGraphicsContext* pGraphicsContext) : 
    IBillboardVertexBuffer(pGraphicsContext),
    mVertexCount(0),
    mVertexArrayId(0),
    mVertexBufferId(0)
{
}

BillboardVertexBuffer::~BillboardVertexBuffer(void)
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

void BillboardVertexBuffer::RenderCore(void) const
{
    if (mVertexCount <= 0) // Nothing to render.
        return;

    GL::glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
    GL::glBindVertexArray(mVertexArrayId);
    GL::glDrawArrays(GL_TRIANGLES, 0, mVertexCount);
    GL::glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
}

void BillboardVertexBuffer::UpdateCore(const std::vector<BillboardVertex>& vertices)
{
    if (vertices.size() <= 0)
        return;

    mVertexCount = ((int) vertices.size());
    EnsureVertexBufferCreation();

    GL::glBindVertexArray(mVertexArrayId);
    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);

    const auto bytes = vertices.size() * sizeof(BillboardVertex);
    GL::glBufferData(GL_ARRAY_BUFFER, bytes, &vertices[0], GL_DYNAMIC_DRAW);

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
    GL::glBindVertexArray(0);
}

void BillboardVertexBuffer::BindToShaderProgramCore(IShaderProgram* pShaderProgram)
{
    EnsureVertexBufferCreation();

    GL::glBindVertexArray(mVertexArrayId);
    GL::glBindBuffer(GL_ARRAY_BUFFER, mVertexBufferId);

    GL::glEnableVertexAttribArray(0);   // Position
    GL::glEnableVertexAttribArray(1);   // Color
    GL::glEnableVertexAttribArray(2);   // Texture coordinates

    const auto pProgram = dynamic_cast<ShaderProgram *>(pShaderProgram);
    const auto locPosition = pProgram->GetAttributeLocation("inPosition");
    const auto locTexCoords = pProgram->GetAttributeLocation("inTextCoords");
    const auto locColor = pProgram->GetAttributeLocation("inColor");

    auto stride = ((int) sizeof(BillboardVertex));
    GL::glVertexAttribPointer(locPosition,  3, GL_FLOAT, GL_FALSE, stride, FC2O(0));
    GL::glVertexAttribPointer(locTexCoords, 4, GL_FLOAT, GL_FALSE, stride, FC2O(3));
    GL::glVertexAttribPointer(locColor,     4, GL_FLOAT, GL_FALSE, stride, FC2O(7));

    GL::glBindBuffer(GL_ARRAY_BUFFER, 0);
    GL::glBindVertexArray(0);
}

void BillboardVertexBuffer::EnsureVertexBufferCreation(void)
{
    if (mVertexArrayId == 0)
        GL::glGenVertexArrays(1, &mVertexArrayId);

    if (mVertexBufferId == 0)
        GL::glGenBuffers(1, &mVertexBufferId);
}
