
#include "stdafx.h"
#include "Interfaces.h"
#include "NodeGeometries.h"

using namespace Dynamorph;

NodeGeometries::NodeGeometries(const std::wstring& nodeId) : mNodeId(nodeId)
{
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

void NodeGeometries::Render(IGraphicsContext* pGraphicsContext) const
{
    auto iterator = mVertexBuffers.begin();
    for (; iterator != mVertexBuffers.end(); ++iterator)
        pGraphicsContext->RenderVertexBuffer(*iterator);
}

void NodeGeometries::GetBoundingBox(BoundingBox* pBoundingBox) const
{
    (*pBoundingBox) = mBoundingBox;
}
