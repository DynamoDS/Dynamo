
#pragma once

namespace DesignScriptStudio { namespace Renderer {

    __interface IRenderPackageImpl
    {
        void PushPointVertex(float x, float y, float z);
        void PushPointVertexColor(
            unsigned char red, unsigned char green,
            unsigned char blue, unsigned char alpha);

        void PushLineVertex(float x, float y, float z);
        void PushLineVertexColor(
            unsigned char red, unsigned char green,
            unsigned char blue, unsigned char alpha);

        void PushLineStripVertex(float x, float y, float z);
        void PushLineStripVertexCount(int n);
        void PushLineStripVertexColor(
            unsigned char red, unsigned char green,
            unsigned char blue, unsigned char alpha);

        void PushTriangleVertex(float x, float y, float z);
        void PushTriangleVertexNormal(float x, float y, float z);
        void PushTriangleVertexColor(
            unsigned char red, unsigned char green,
            unsigned char blue, unsigned char alpha);

        void PushPointVertexPtr(const float *pBuffer, size_t floatCount);
        void PushPointVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount);

        void PushLineVertexPtr(const float *pBuffer, size_t floatCount);
        void PushLineVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount);

        void PushLineStripVertexPtr(const float *pBuffer, size_t floatCount);
        void PushLineStripVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount);

        void PushTriangleVertexPtr(const float *pBuffer, size_t floatCount);
        void PushTriangleVertexNormalPtr(const float *pBuffer, size_t floatCount);
        void PushTriangleVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount);
    };

} }
