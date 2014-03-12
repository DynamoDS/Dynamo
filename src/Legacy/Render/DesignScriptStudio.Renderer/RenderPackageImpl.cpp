
#include "stdafx.h"
#include "Internal.h"

using namespace DesignScriptStudio::Renderer;

RenderPackageImpl::RenderPackageImpl(int documentId, int packageId) : 
    mBoundingBoxComputed(false)
{
    mPackageId.documentId = documentId;
    mPackageId.packageId = packageId;
}

RenderPackageImpl::~RenderPackageImpl()
{
}

PackageId RenderPackageImpl::GetIdentifier(void) const
{
    return mPackageId;
}

void RenderPackageImpl::PushPointVertex(float x, float y, float z)
{
    mPointVertices.push_back(x);
    mPointVertices.push_back(y);
    mPointVertices.push_back(z);
}

void RenderPackageImpl::PushPointVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    mPointColors.push_back(red);
    mPointColors.push_back(green);
    mPointColors.push_back(blue);
    mPointColors.push_back(alpha);
}

void RenderPackageImpl::PushLineVertex(float x, float y, float z)
{
    mLineVertices.push_back(x);
    mLineVertices.push_back(y);
    mLineVertices.push_back(z);
}

void RenderPackageImpl::PushLineVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    mLineColors.push_back(red);
    mLineColors.push_back(green);
    mLineColors.push_back(blue);
    mLineColors.push_back(alpha);
}

void RenderPackageImpl::PushLineStripVertex(float x, float y, float z)
{
    mLineStripVertices.push_back(x);
    mLineStripVertices.push_back(y);
    mLineStripVertices.push_back(z);
}

void RenderPackageImpl::PushLineStripVertexCount(int n)
{
    mLineStripVertexCount.push_back(n);
}

void RenderPackageImpl::PushLineStripVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    mLineStripColors.push_back(red);
    mLineStripColors.push_back(green);
    mLineStripColors.push_back(blue);
    mLineStripColors.push_back(alpha);
}

void RenderPackageImpl::PushTriangleVertex(float x, float y, float z)
{
    mTriangleVertices.push_back(x);
    mTriangleVertices.push_back(y);
    mTriangleVertices.push_back(z);
}

void RenderPackageImpl::PushTriangleVertexNormal(float x, float y, float z)
{
    mTriangleNormals.push_back(x);
    mTriangleNormals.push_back(y);
    mTriangleNormals.push_back(z);
}

void RenderPackageImpl::PushTriangleVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    mTriangleColors.push_back(red);
    mTriangleColors.push_back(green);
    mTriangleColors.push_back(blue);
    mTriangleColors.push_back(alpha);
}

void RenderPackageImpl::PushPointVertexPtr(const float *pBuffer, size_t floatCount)
{
    EXTENDANDCOPY(mPointVertices, pBuffer, floatCount);
}

void RenderPackageImpl::PushPointVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    EXTENDANDCOPY(mPointColors, pBuffer, ucharCount);
}

void RenderPackageImpl::PushLineVertexPtr(const float *pBuffer, size_t floatCount)
{
    EXTENDANDCOPY(mLineVertices, pBuffer, floatCount);
}

void RenderPackageImpl::PushLineVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    EXTENDANDCOPY(mLineColors, pBuffer, ucharCount);
}


void RenderPackageImpl::PushLineStripVertexPtr(const float *pBuffer, size_t floatCount)
{
    EXTENDANDCOPY(mLineStripVertices, pBuffer, floatCount);
}

void RenderPackageImpl::PushLineStripVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    EXTENDANDCOPY(mLineStripColors, pBuffer, ucharCount);
}

void RenderPackageImpl::PushTriangleVertexPtr(const float *pBuffer, size_t floatCount)
{
    EXTENDANDCOPY(mTriangleVertices, pBuffer, floatCount);
}

void RenderPackageImpl::PushTriangleVertexNormalPtr(const float *pBuffer, size_t floatCount)
{
    EXTENDANDCOPY(mTriangleNormals, pBuffer, floatCount);
}

void RenderPackageImpl::PushTriangleVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    EXTENDANDCOPY(mTriangleColors, pBuffer, ucharCount);
}

void RenderPackageImpl::ComputeBoundingBox(void)
{
    // Reset the bounding box.
    mBoundingBoxComputed = false;

    const float* p = mPointVertices.data();
    size_t floatCount = mPointVertices.size();
    for (size_t index = 0; index < floatCount; index += 3)
        AlterBoundingBox(p[index + 0], p[index + 1], p[index + 2]);

    p = mLineStripVertices.data();
    floatCount = mLineStripVertices.size();
    for (size_t index = 0; index < floatCount; index += 3)
        AlterBoundingBox(p[index + 0], p[index + 1], p[index + 2]);

    p = mTriangleVertices.data();
    floatCount = mTriangleVertices.size();
    for (size_t index = 0; index < floatCount; index += 3)
        AlterBoundingBox(p[index + 0], p[index + 1], p[index + 2]);

    if (false == mBoundingBoxComputed) {
        memset(&mBoundingBox[0], 0, sizeof(mBoundingBox));
        mBoundingBoxComputed = true;
    }
}

void RenderPackageImpl::GetBoundingBox(float* pCorners) const
{
    if (false == mBoundingBoxComputed)
        throw new std::exception("'RenderPackageImpl::ComputeBoundingBox' not called!");

    if (NULL != pCorners)
        memcpy(pCorners, &(mBoundingBox[0]), sizeof(mBoundingBox));
}

const std::vector<float>& RenderPackageImpl::GetPointVertices(void) const
{
    return mPointVertices;
}

const std::vector<unsigned char>& RenderPackageImpl::GetPointColors(void) const
{
    return mPointColors;
}

const std::vector<float>& RenderPackageImpl::GetLineVertices(void) const
{
    return mLineVertices;
}

const std::vector<unsigned char>& RenderPackageImpl::GetLineColors(void) const
{
    return mLineColors;
}

const std::vector<float>& RenderPackageImpl::GetLineStripVertices(void) const
{
    return mLineStripVertices;
}

const std::vector<unsigned char>& RenderPackageImpl::GetLineStripColors(void) const
{
    return mLineStripColors;
}

const std::vector<size_t>& RenderPackageImpl::GetLineStripVertexCount(void) const
{
    return mLineStripVertexCount;
}

const std::vector<float>& RenderPackageImpl::GetTriangleVertices(void) const
{
    return mTriangleVertices;
}

const std::vector<float>& RenderPackageImpl::GetTriangleNormals(void) const
{
    return mTriangleNormals;
}

const std::vector<unsigned char>& RenderPackageImpl::GetTriangleColors(void) const
{
    return mTriangleColors;
}

void RenderPackageImpl::AlterBoundingBox(float x, float y, float z)
{
    if (false != mBoundingBoxComputed)
    {
        // Compare and adjust the minimum corner.
        if (x < mBoundingBox[0]) mBoundingBox[0] = x;
        if (y < mBoundingBox[1]) mBoundingBox[1] = y;
        if (z < mBoundingBox[2]) mBoundingBox[2] = z;

        // Compare and adjust the maximum corner.
        if (x > mBoundingBox[3]) mBoundingBox[3] = x;
        if (y > mBoundingBox[4]) mBoundingBox[4] = y;
        if (z > mBoundingBox[5]) mBoundingBox[5] = z;
    }
    else
    {
        // This is the first point we encountered, 
        // set the bounding box to be as big as it.
        mBoundingBox[0] = x;
        mBoundingBox[1] = y;
        mBoundingBox[2] = z;
        mBoundingBox[3] = x;
        mBoundingBox[4] = y;
        mBoundingBox[5] = z;
        mBoundingBoxComputed = true;
    }
}


/// Implementation of RenderPackageWrapper

RenderPackageWrapper::RenderPackageWrapper(Autodesk::DesignScript::Interfaces::IRenderPackage^ rcw) : 
    mRCW(rcw)
{
}

RenderPackageWrapper::~RenderPackageWrapper()
{
}

void RenderPackageWrapper::PushPointVertex(float x, float y, float z)
{
    wrapper()->PushPointVertex(x, y, z);
}

void RenderPackageWrapper::PushPointVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    wrapper()->PushPointVertexColor(red, green, blue, alpha);
}

void RenderPackageWrapper::PushLineVertex(float x, float y, float z)
{
    //wrapper()->PushLineStripVertex(x, y, z);
}

void RenderPackageWrapper::PushLineVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    //wrapper()->PushLineStripVertexColor(red, green, blue, alpha);
}

void RenderPackageWrapper::PushLineStripVertex(float x, float y, float z)
{
    wrapper()->PushLineStripVertex(x, y, z);
}

void RenderPackageWrapper::PushLineStripVertexCount(int n)
{
    wrapper()->PushLineStripVertexCount(n);
}

void RenderPackageWrapper::PushLineStripVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    wrapper()->PushLineStripVertexColor(red, green, blue, alpha);
}

void RenderPackageWrapper::PushTriangleVertex(float x, float y, float z)
{
    wrapper()->PushTriangleVertex(x, y, z);
}

void RenderPackageWrapper::PushTriangleVertexNormal(float x, float y, float z)
{
    wrapper()->PushTriangleVertexNormal(x, y, z);
}

void RenderPackageWrapper::PushTriangleVertexColor(
    unsigned char red, unsigned char green,
    unsigned char blue, unsigned char alpha)
{
    wrapper()->PushTriangleVertexColor(red, green, blue, alpha);
}

void RenderPackageWrapper::PushPointVertexPtr(const float *pBuffer, size_t floatCount)
{
    for(size_t i = 0; i < floatCount; i += 3)
    {
        float x = pBuffer[i];
        float y = pBuffer[i+1];
        float z = pBuffer[i+2];
        this->PushPointVertex(x, y, z);
    }
}

void RenderPackageWrapper::PushPointVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    for(size_t i = 0; i < ucharCount; i += 4)
    {
        unsigned char red = pBuffer[i];
        unsigned char green = pBuffer[i+1];
        unsigned char blue = pBuffer[i+2];
        unsigned char alpha = pBuffer[i+3];
        this->PushPointVertexColor(red, green, blue, alpha);
    }
}

void RenderPackageWrapper::PushLineVertexPtr(const float *pBuffer, size_t floatCount)
{
    for(size_t i = 0; i < floatCount; i += 3)
    {
        float x = pBuffer[i];
        float y = pBuffer[i+1];
        float z = pBuffer[i+2];
        this->PushLineVertex(x, y, z);
    }
}

void RenderPackageWrapper::PushLineVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    for(size_t i = 0; i < ucharCount; i += 4)
    {
        unsigned char red = pBuffer[i];
        unsigned char green = pBuffer[i+1];
        unsigned char blue = pBuffer[i+2];
        unsigned char alpha = pBuffer[i+3];
        this->PushLineVertexColor(red, green, blue, alpha);
    }
}


void RenderPackageWrapper::PushLineStripVertexPtr(const float *pBuffer, size_t floatCount)
{
    for(size_t i = 0; i < floatCount; i += 3)
    {
        float x = pBuffer[i];
        float y = pBuffer[i+1];
        float z = pBuffer[i+2];
        this->PushLineStripVertex(x, y, z);
    }
}

void RenderPackageWrapper::PushLineStripVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    for(size_t i = 0; i < ucharCount; i += 4)
    {
        unsigned char red = pBuffer[i];
        unsigned char green = pBuffer[i+1];
        unsigned char blue = pBuffer[i+2];
        unsigned char alpha = pBuffer[i+3];
        this->PushLineStripVertexColor(red, green, blue, alpha);
    }
}

void RenderPackageWrapper::PushTriangleVertexPtr(const float *pBuffer, size_t floatCount)
{
    for(size_t i = 0; i < floatCount; i += 3)
    {
        float x = pBuffer[i];
        float y = pBuffer[i+1];
        float z = pBuffer[i+2];
        this->PushTriangleVertex(x, y, z);
    }
}

void RenderPackageWrapper::PushTriangleVertexNormalPtr(const float *pBuffer, size_t floatCount)
{
    for(size_t i = 0; i < floatCount; i += 3)
    {
        float x = pBuffer[i];
        float y = pBuffer[i+1];
        float z = pBuffer[i+2];
        this->PushTriangleVertexNormal(x, y, z);
    }
}

void RenderPackageWrapper::PushTriangleVertexColorPtr(const unsigned char *pBuffer, size_t ucharCount)
{
    for(size_t i = 0; i < ucharCount; i += 4)
    {
        unsigned char red = pBuffer[i];
        unsigned char green = pBuffer[i+1];
        unsigned char blue = pBuffer[i+2];
        unsigned char alpha = pBuffer[i+3];
        this->PushTriangleVertexColor(red, green, blue, alpha);
    }
}