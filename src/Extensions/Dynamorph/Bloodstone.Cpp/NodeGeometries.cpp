
#include "stdafx.h"
#include "Interfaces.h"
#include "NodeGeometries.h"

using namespace Bloodstone;

NodeGeometries::NodeGeometries(const std::wstring& nodeId) : mNodeId(nodeId)
{
    mNodeRgbaColor[0] = mNodeRgbaColor[1] = 1.0f;
    mNodeRgbaColor[2] = mNodeRgbaColor[3] = 1.0f;
}

NodeGeometries::~NodeGeometries(void)
{
    ClearVertexBuffers();
}

const std::wstring NodeGeometries::GetNodeId(void) const
{
    return this->mNodeId;
}

void NodeGeometries::ClearVertexBuffers(void)
{
    auto iterator = mVertexBuffers.begin();
    for (; iterator != mVertexBuffers.end(); ++iterator) {
        auto pVertexBuffer = *iterator;
        delete pVertexBuffer;
    }

    mVertexBuffers.clear();
    mBoundingBox.Invalidate();
}

void NodeGeometries::GetColor(float* pRgbaColor) const
{
    pRgbaColor[0] = mNodeRgbaColor[0];
    pRgbaColor[1] = mNodeRgbaColor[1];
    pRgbaColor[2] = mNodeRgbaColor[2];
    pRgbaColor[3] = mNodeRgbaColor[3];
}

void NodeGeometries::SetColor(float red, float green, float blue, float alpha)
{
    mNodeRgbaColor[0] = red;
    mNodeRgbaColor[1] = green;
    mNodeRgbaColor[2] = blue;
    mNodeRgbaColor[3] = alpha;
}

void NodeGeometries::AppendVertexBuffer(IVertexBuffer* pVertexBuffer)
{
#ifdef _DEBUG

    auto iterator = mVertexBuffers.begin();
    for (; iterator != mVertexBuffers.end(); ++iterator)
    {
        if (pVertexBuffer == *iterator)
            throw new std::exception("IVertexBuffer inserted twice");
    }

#endif

    mVertexBuffers.push_back(pVertexBuffer);

    BoundingBox boundingBox;
    pVertexBuffer->GetBoundingBox(&boundingBox);
    this->mBoundingBox.EvaluateBox(boundingBox);
}

void NodeGeometries::Render(IGraphicsContext* pGraphicsContext, Dimensionality dimensionality) const
{
    auto iterator = mVertexBuffers.begin();
    for (; iterator != mVertexBuffers.end(); ++iterator)
    {
        auto pVertexBuffer = *iterator;
        switch (pVertexBuffer->GetPrimitiveType())
        {
        case IVertexBuffer::PrimitiveType::Point:
        case IVertexBuffer::PrimitiveType::LineStrip:
            if (dimensionality == Dimensionality::High)
                return;
            break;
        case IVertexBuffer::PrimitiveType::Triangle:
            if (dimensionality == Dimensionality::Low)
                return;
            break;
        }

        pGraphicsContext->RenderVertexBuffer(pVertexBuffer);
    }
}

void NodeGeometries::GetBoundingBox(BoundingBox* pBoundingBox) const
{
    (*pBoundingBox) = mBoundingBox;
}
