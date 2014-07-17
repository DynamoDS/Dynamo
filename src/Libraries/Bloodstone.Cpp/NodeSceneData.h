
#ifndef _NODESCENEDATA_H_
#define _NODESCENEDATA_H_

#include "Interfaces.h"

namespace Dynamo { namespace Bloodstone {

    class IGraphicsContext;

    enum class Dimensionality
    {
        Low, High
    };

    class NodeSceneData
    {
    public:
        NodeSceneData(const std::wstring& nodeId);
        ~NodeSceneData(void);

        const std::wstring GetNodeId(void) const;
        void ClearVertexBuffers(void);
        bool GetSelected(void) const;
        void SetSelected(bool selected);
        void GetColor(float* pRgbaColor) const;
        void SetColor(float red, float green, float blue, float alpha);
        void AppendVertexBuffer(IVertexBuffer* pVertexBuffer);
        void Render(IGraphicsContext* pGraphicsContext, Dimensionality dimensionality) const;
        void GetBoundingBox(BoundingBox* pBoundingBox) const;

    private:
        bool mNodeSelected;
        float mNodeRgbaColor[4];
        BoundingBox mBoundingBox;
        std::wstring mNodeId;
        std::vector<IVertexBuffer *> mVertexBuffers;
    };
} }

#endif
