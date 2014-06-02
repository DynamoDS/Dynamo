
#ifndef _NODEGEOMETRIES_H_
#define _NODEGEOMETRIES_H_

#include "Interfaces.h"

namespace Dynamorph
{
    class IGraphicsContext;

    class NodeGeometries
    {
    public:
        NodeGeometries(const std::wstring& nodeId);
        ~NodeGeometries(void);

        const std::wstring GetNodeId(void) const;
        void ClearVertexBuffers(void);
        void AppendVertexBuffer(IVertexBuffer* pVertexBuffer);
        void Render(IGraphicsContext* pGraphicsContext) const;
        void GetBoundingBox(BoundingBox* pBoundingBox) const;

    private:
        BoundingBox mBoundingBox;
        std::wstring mNodeId;
        std::vector<IVertexBuffer *> mVertexBuffers;
    };
}

#endif
