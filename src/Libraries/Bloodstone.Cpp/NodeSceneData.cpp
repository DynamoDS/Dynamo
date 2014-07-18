
#include "stdafx.h"
#include "Bloodstone.h"
#include "NodeSceneData.h"

using namespace Dynamo::Bloodstone;

NodeSceneData::NodeSceneData(const std::wstring& nodeId) :
    mRenderMode(RenderMode::Shaded),
    mNodeId(nodeId),
    mNodeSelected(false)
{
    mNodeRgbaColor[0] = mNodeRgbaColor[1] = 0.0f;
    mNodeRgbaColor[2] = mNodeRgbaColor[3] = 0.0f;
}

NodeSceneData::~NodeSceneData(void)
{
    ClearVertexBuffers();
}

const std::wstring NodeSceneData::GetNodeId(void) const
{
    return this->mNodeId;
}

void NodeSceneData::GetBoundingBox(BoundingBox* pBoundingBox) const
{
    (*pBoundingBox) = mBoundingBox;
}

bool NodeSceneData::GetSelected(void) const
{
    return this->mNodeSelected;
}

void NodeSceneData::SetSelected(bool selected)
{
    this->mNodeSelected = selected;
}

void NodeSceneData::GetColor(float* pRgbaColor) const
{
    pRgbaColor[0] = mNodeRgbaColor[0];
    pRgbaColor[1] = mNodeRgbaColor[1];
    pRgbaColor[2] = mNodeRgbaColor[2];
    pRgbaColor[3] = mNodeRgbaColor[3];
}

void NodeSceneData::SetColor(float red, float green, float blue, float alpha)
{
    mNodeRgbaColor[0] = red;
    mNodeRgbaColor[1] = green;
    mNodeRgbaColor[2] = blue;
    mNodeRgbaColor[3] = alpha;
}

RenderMode NodeSceneData::GetRenderMode(void) const
{
    return this->mRenderMode;
}

void NodeSceneData::SetRenderMode(RenderMode renderMode)
{
    this->mRenderMode = renderMode;
}

void NodeSceneData::ClearVertexBuffers(void)
{
    auto iterator = mVertexBuffers.begin();
    for (; iterator != mVertexBuffers.end(); ++iterator) {
        auto pVertexBuffer = *iterator;
        delete pVertexBuffer;
    }

    mVertexBuffers.clear();
    mBoundingBox.Invalidate();
}

void NodeSceneData::AppendVertexBuffer(IVertexBuffer* pVertexBuffer)
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

void NodeSceneData::Render(IGraphicsContext* pGraphicsContext, Dimensionality dimensionality) const
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
