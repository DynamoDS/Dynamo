// DesignScriptStudio.Renderer.h

#pragma once

#include "NativeContract.h"

namespace DesignScriptStudio { namespace Renderer {

    class ThumbnailImpl;
    class RenderServiceImpl;

    public ref struct ThumbnailData
    {
        ThumbnailData(int documentId, int packageId)
        {
            this->documentId = documentId;
            this->packageId = packageId;
        }

        int width, height;
        int documentId, packageId;
        cli::array<System::Byte>^ pixels;
    };

    public interface class IRenderServiceConsumer
    {
        void NotifyThumbnailReady(ThumbnailData^ thumbnail);
    };

    public ref class RenderService
    {
    public:
        enum class ServiceStatus
        {
            None, NoDisplayDriver, Running
        };

    public:
        RenderService(IRenderServiceConsumer^ consumer);
        void Shutdown();

        ServiceStatus GetServiceStatus(void);
        Autodesk::DesignScript::Interfaces::IRenderPackage^ CreateRenderPackage(unsigned int documentId, unsigned int packageId);
        bool QueueRenderPackage(Autodesk::DesignScript::Interfaces::IRenderPackage^ package);
        void NotifyThumbnailReady(ThumbnailImpl *pThumbnail);
        static RenderService^ GetInstance(void);

    private:
        ServiceStatus mServiceStatus;
        IRenderServiceConsumer^ mConsumer;
        RenderServiceImpl* mpRenderServiceImpl;
        static RenderService^ mRenderService;
    };

    public ref class RenderPackageUtils
    {
    public:
        static System::IntPtr CreateNativeRenderPackage(Autodesk::DesignScript::Interfaces::IRenderPackage^ package);
        static void DestroyNativeRenderPackage(System::IntPtr nativePackage);
    };
} }
