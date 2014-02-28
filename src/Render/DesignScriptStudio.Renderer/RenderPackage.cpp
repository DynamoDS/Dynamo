// This is the main DLL file.

#include "stdafx.h"
#include "Contract.h"
#include "Internal.h"

using namespace System::Runtime::InteropServices;
using namespace DesignScriptStudio::Renderer;

RenderPackage::RenderPackage(int documentId, int packageId) : 
    mDocumentId(documentId),
    mPackageId(packageId),
    mpNativeRenderPackage(NULL)
{
}

void RenderPackage::PushPointVertex(double x, double y, double z)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushPointVertex(((float) x), ((float) y), ((float) z));
}

void RenderPackage::PushPointVertexColor(byte red, byte green, byte blue, byte alpha)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushPointVertexColor(red, green, blue, alpha);
}

void RenderPackage::PushLineStripVertex(double x, double y, double z)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushLineStripVertex(((float) x), ((float) y), ((float) z));
}

void RenderPackage::PushLineStripVertexCount(int n)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushLineStripVertexCount(n);
}

void RenderPackage::PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushLineStripVertexColor(red, green, blue, alpha);
}

System::IntPtr RenderPackage::NativeRenderPackage::get(void)
{
    EnsureNativePackageCreated();
    return ((System::IntPtr)(this->mpNativeRenderPackage));
}

void RenderPackage::PushTriangleVertex(double x, double y, double z)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushTriangleVertex(((float) x), ((float) y), ((float) z));
}

void RenderPackage::PushTriangleVertexNormal(double x, double y, double z)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushTriangleVertex(((float) x), ((float) y), ((float) z));
}

void RenderPackage::PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha)
{
    EnsureNativePackageCreated();
    mpNativeRenderPackage->PushTriangleVertexColor(red, green, blue, alpha);
}

RenderPackageImpl* RenderPackage::ReleasePackageOwnership(void)
{
    RenderPackageImpl* pPackage = mpNativeRenderPackage;
    mpNativeRenderPackage = NULL;
    return pPackage;
}

void RenderPackage::EnsureNativePackageCreated(void)
{
    if (NULL == mpNativeRenderPackage)
        mpNativeRenderPackage = new RenderPackageImpl(mDocumentId, mPackageId);
}
