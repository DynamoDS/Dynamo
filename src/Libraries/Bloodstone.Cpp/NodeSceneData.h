
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

        // Read-only property accessor methods.
        const std::wstring GetNodeId(void) const;
        void GetBoundingBox(BoundingBox* pBoundingBox) const;

        // Read-write properties accessor methods.
        bool GetSelected(void) const;
        void SetSelected(bool selected);
        void GetColor(float* pRgbaColor) const;
        void SetColor(float red, float green, float blue, float alpha);
        RenderMode GetRenderMode(void) const;
        void SetRenderMode(RenderMode renderMode);

        // Generic class operational methods.
        void ClearVertexBuffers(void);
        void AppendVertexBuffer(IVertexBuffer* pVertexBuffer);
        void Render(IGraphicsContext* pGraphicsContext, Dimensionality dimensionality) const;

    private:
        bool mNodeSelected;
        float mNodeRgbaColor[4];
        RenderMode mRenderMode;
        BoundingBox mBoundingBox;
        std::wstring mNodeId;
        std::vector<IVertexBuffer *> mVertexBuffers;
    };
} }

#endif
