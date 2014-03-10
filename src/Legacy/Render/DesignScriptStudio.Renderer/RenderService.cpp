// This is the main DLL file.

#include "stdafx.h"
#include "Contract.h"
#include "Internal.h"

using namespace Autodesk::DesignScript::Interfaces;
using namespace DesignScriptStudio::Renderer;
using namespace System::Runtime::InteropServices;

RenderService::RenderService(IRenderServiceConsumer^ consumer) :
    mpRenderServiceImpl(NULL), mServiceStatus(ServiceStatus::None)
{
    this->mConsumer = consumer;
    RenderService::mRenderService = this;
}

void RenderService::Shutdown()
{
    if (NULL != mpRenderServiceImpl) {
        mpRenderServiceImpl->Destroy();
        delete mpRenderServiceImpl;
        mpRenderServiceImpl = NULL;
    }

    RenderService::mRenderService = nullptr;
}

RenderService::ServiceStatus RenderService::GetServiceStatus(void)
{
    return this->mServiceStatus;
}

IRenderPackage^ RenderService::CreateRenderPackage(unsigned int documentId, unsigned int packageId)
{
    return gcnew RenderPackage(documentId, packageId);
}

bool RenderService::QueueRenderPackage(IRenderPackage^ package)
{
    if (nullptr == package)
        return false;
    if (ServiceStatus::NoDisplayDriver == mServiceStatus)
        return false;

    if (NULL == mpRenderServiceImpl)
    {
        mServiceStatus = ServiceStatus::Running;
        mpRenderServiceImpl = new RenderServiceImpl();
        if (mpRenderServiceImpl->Initialize(64, 64) == false) {
            mServiceStatus = ServiceStatus::NoDisplayDriver;
            return false; // No display driver, no point proceeding.
        }
    }

    // Convert the RenderPackage (managed) to RenderPackageImpl (native),
    // so that it can be queued up for RenderServiceImpl to process.
    // 
    RenderPackage ^renderPackage = dynamic_cast<RenderPackage ^>(package);
    if (nullptr == renderPackage)
        return false;

    RenderPackageImpl* pPackage = renderPackage->ReleasePackageOwnership();
    mpRenderServiceImpl->QueuePackage(pPackage);
    return true;
}

void RenderService::NotifyThumbnailReady(ThumbnailImpl *pThumbnail)
{
    // Get pointer to the native buffer.
    const unsigned char* p = pThumbnail->GetPixelBuffer();
    unsigned char *pNativeBuffer = const_cast<unsigned char *>(p);
    const int bytes = pThumbnail->BufferSizeInBytes();

    int width = 0, height = 0;
    pThumbnail->GetThumbnailSize(width, height);

    // Duplicate the content into managed buffer.
    PackageId id = pThumbnail->GetIdentifier();
    ThumbnailData^ thumbnail = gcnew ThumbnailData(id.documentId, id.packageId);
    thumbnail->width = width;
    thumbnail->height = height;
    thumbnail->pixels = gcnew cli::array<System::Byte>(bytes);
    Marshal::Copy((System::IntPtr) pNativeBuffer, thumbnail->pixels, 0, bytes);

    // Notify the render service consumer.
    this->mConsumer->NotifyThumbnailReady(thumbnail);
}

RenderService^ RenderService::GetInstance(void)
{
    return RenderService::mRenderService;
}


System::IntPtr RenderPackageUtils::CreateNativeRenderPackage(Autodesk::DesignScript::Interfaces::IRenderPackage^ package)
{
    RenderPackageWrapper* pNativeRenderPackage = new RenderPackageWrapper(package);
    return ((System::IntPtr)(pNativeRenderPackage));
}

void RenderPackageUtils::DestroyNativeRenderPackage(System::IntPtr nativePackage)
{
    RenderPackageWrapper* pNativeRenderPackage = static_cast<RenderPackageWrapper*>(nativePackage.ToPointer());
    delete pNativeRenderPackage;
}
