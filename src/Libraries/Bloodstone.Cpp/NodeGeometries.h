
#ifndef _NODEGEOMETRIES_H_
#define _NODEGEOMETRIES_H_

#include "Interfaces.h"

namespace Dynamo { namespace Bloodstone {

    class IGraphicsContext;

    enum class Dimensionality
    {
        Low, High
    };

    class NodeGeometries
    {
    public:
        NodeGeometries(const std::wstring& nodeId);
        ~NodeGeometries(void);

        const std::wstring GetNodeId(void) const;
        void ClearVertexBuffers(void);
        void GetColor(float* pRgbaColor) const;
        void SetColor(float red, float green, float blue, float alpha);
        void AppendVertexBuffer(IVertexBuffer* pVertexBuffer);
        void Render(IGraphicsContext* pGraphicsContext, Dimensionality dimensionality) const;
        void GetBoundingBox(BoundingBox* pBoundingBox) const;

    private:
        float mNodeRgbaColor[4];
        BoundingBox mBoundingBox;
        std::wstring mNodeId;
        std::vector<IVertexBuffer *> mVertexBuffers;
    };
} }

#endif
