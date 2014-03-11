
#pragma once

#include "..\glew-1.9.0\include\gl\glew.h"
#include "..\glew-1.9.0\include\GL\wglew.h"
#include <gl/GL.h>
#include <gl/GLU.h>
#include <vector>
#include <set>
#include <map>
#include "Contract.h"
#include <gcroot.h>

#define USE_FRAME_BUFFER

#define NONNULL(v, f) {                                         \
    (v) = (f);                                                  \
    if (NULL == (v)) {                                          \
        this->ReportOpenGlErrors("NULL pointer found: " #v);    \
        return false;                                           \
    }                                                           \
}

#define NOTFALSE(f, m) {                                        \
    if (FALSE == (f)) {                                         \
        this->ReportOpenGlErrors(m);                            \
        return false;                                           \
    }                                                           \
}

#define EXTENDANDCOPY(v, p, c)                                  \
{                                                               \
    size_t oldSize = v.size();                                  \
    v.reserve(oldSize + c);                                     \
    for (size_t i = 0; i < c; ++i)                              \
    v.push_back(p[i]);                                          \
}

// #define TRACEMSG TRACE
#define TRACEMSG(x)                             \
    OutputDebugString(L"RS: " x)

#define TRACEMSG2(x, m)                         \
{                                               \
    wchar_t buffer[1024] = { 0 };               \
    swprintf_s(buffer, 1024, L"RS: " x, m);     \
    OutputDebugString(buffer);                  \
}

#define TRACEMSG3(x, m, n)                      \
{                                               \
    wchar_t buffer[1024] = { 0 };               \
    swprintf_s(buffer, 1024, L"RS: " x, m, n);  \
    OutputDebugString(buffer);                  \
}

#define DESTROYOPENGLBUFFER(o)              { if (OpenGL::IsBuffer(o)) OpenGL::DeleteBuffers(1, &(o)); o = 0; }
#define ENSURE_FUNCTION_POINTER_VALID(f)    { if (NULL == (f)) AppendStatus("Function not found: " #f); }

namespace DesignScriptStudio { namespace Renderer {

    class Camera;
    class RenderThread;

    private class ScopedLock
    {
    public:
        ScopedLock(HANDLE hMutex) : mhMutex(hMutex)
        {
            if (NULL != mhMutex)
            {
                if (WAIT_OBJECT_0 != WaitForSingleObject(hMutex, INFINITE))
                    mhMutex = NULL; // Mutex has been destroyed!
            }
        }

        ~ScopedLock()
        {
            if (NULL != mhMutex)
                ReleaseMutex(mhMutex);
        }

        bool AccessGranted(void) const
        {
            return (NULL != mhMutex);
        }

    private:
        HANDLE mhMutex;
    };

    private class OpenGL
    {
    public:
        static bool Initialize(HDC hFrameBufferDC);
        static bool IsInitialized(void);
        static const void* GetErrorString(void);
        static bool ConstructFrameBuffer(HDC hFrameBufferDC,
            HGLRC* phFrameBufferRC, std::string& error);

    public:
        // OpenGL extension functions.
        static PFNWGLMAKECONTEXTCURRENTARBPROC  MakeContextCurrent;
        static PFNWGLCHOOSEPIXELFORMATARBPROC   ChoosePixelFormat;
        static PFNWGLCREATEPBUFFERARBPROC       CreatePbuffer;
        static PFNWGLDESTROYPBUFFERARBPROC      DestroyPbuffer;
        static PFNWGLGETPBUFFERDCARBPROC        GetPbufferDC;
        static PFNWGLRELEASEPBUFFERDCARBPROC    ReleasePbufferDC;
        static PFNWGLQUERYPBUFFERARBPROC        QueryPbuffer;
        static PFNWGLBINDTEXIMAGEARBPROC        BindTexImage;
        static PFNWGLRELEASETEXIMAGEARBPROC     ReleaseTexImage;
        static PFNWGLSETPBUFFERATTRIBARBPROC    SetPbufferAttrib;

        static PFNGLGENBUFFERSPROC              GenBuffers;
        static PFNGLDELETEBUFFERSPROC           DeleteBuffers;
        static PFNGLISBUFFERPROC                IsBuffer;
        static PFNGLBINDBUFFERPROC              BindBuffer;
        static PFNGLBUFFERDATAPROC              BufferData;
        static PFNGLBUFFERSUBDATAPROC           BufferSubData;
        static PFNGLGENRENDERBUFFERSPROC        GenRenderbuffers;
        static PFNGLBINDRENDERBUFFERPROC        BindRenderbuffer;
        static PFNGLRENDERBUFFERSTORAGEPROC     RenderbufferStorage;
        static PFNGLGENFRAMEBUFFERSPROC         GenFramebuffers;
        static PFNGLBINDFRAMEBUFFERPROC         BindFramebuffer;
        static PFNGLFRAMEBUFFERRENDERBUFFERPROC FramebufferRenderbuffer;
        static PFNGLDELETEFRAMEBUFFERSPROC      DeleteFramebuffers;
        static PFNGLDELETERENDERBUFFERSPROC     DeleteRenderbuffers;
        static PFNGLCHECKFRAMEBUFFERSTATUSPROC  CheckFramebufferStatus;

    private:
        static const wchar_t* kInitializationMutex;

        static std::string mStatusString;
        static void PrintExtensionStrings(const char* pExtensions);
        static bool ValidateExtensionStrings(const char* pExtensions);
        static void AppendStatus(const char* pMessage);
    };

    struct PackageId
    {
        int documentId;
        int packageId;
    };

    private class ThumbnailImpl
    {
    public:
        ThumbnailImpl(int width, int height);
        ~ThumbnailImpl();

        PackageId GetIdentifier(void) const;
        void GetThumbnailSize(int& width, int& height) const;
        int BufferSizeInBytes(void) const;
        unsigned char* GetWriteableBuffer(PackageId packageId);
        const unsigned char* GetPixelBuffer(void) const;

    private:
        int mPixelWidth, mPixelHeight;
        unsigned char* mpPixelBuffer;
        PackageId mPackageId;
    };

    private class ThumbnailPool
    {
    public:
        ThumbnailPool(RenderServiceImpl* pRenderService);
        ~ThumbnailPool(void);

        void Initialize(int width, int height);
        void Destroy(void);

        ThumbnailImpl* LockWriteableThumbnail();
        void UnlockWriteableThumbnail(ThumbnailImpl* pThumbnail);

        static const wchar_t* kThumbnailPoolMutex;

    private:
        static unsigned int NotifierThreadProc(void *pContext);
        unsigned int InternalThreadProc(void);
        ThumbnailImpl* GetThumbnailUnsafe(void);
        void NotifyServiceClientUnsafe(void);

        HANDLE mNotifierThread;
        HANDLE mShutdownEvent;
        HANDLE mThumbnailPoolAccess;
        int mThumbWidth, mThumbHeight;
        RenderServiceImpl* mpRenderService;

        std::set<ThumbnailImpl *> mUnusedThumbnails;
        std::set<ThumbnailImpl *> mResultThumbnails;
    };

    private class RenderPackageImpl : public IRenderPackageImpl
    {
    public:
        RenderPackageImpl(int documentId, int packageId);
        ~RenderPackageImpl(void);
        PackageId GetIdentifier(void) const;

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

    public:
        void ComputeBoundingBox(void);
        void GetBoundingBox(float* pCorners) const;

        const std::vector<float>& GetPointVertices(void) const;
        const std::vector<unsigned char>& GetPointColors(void) const;
        const std::vector<float>& GetLineVertices(void) const;
        const std::vector<unsigned char>& GetLineColors(void) const;
        const std::vector<float>& GetLineStripVertices(void) const;
        const std::vector<unsigned char>& GetLineStripColors(void) const;
        const std::vector<size_t>& GetLineStripVertexCount(void) const;
        const std::vector<float>& GetTriangleVertices(void) const;
        const std::vector<float>& GetTriangleNormals(void) const;
        const std::vector<unsigned char>& GetTriangleColors(void) const;

    private:
        void AlterBoundingBox(float x, float y, float z);

        PackageId mPackageId;
        bool mBoundingBoxComputed;
        float mBoundingBox[6];

        // Point related vertex data.
        std::vector<float> mPointVertices;
        std::vector<unsigned char> mPointColors;

        // Line related vertex data.
        std::vector<float> mLineVertices;
        std::vector<unsigned char> mLineColors;
        std::vector<float> mLineStripVertices;
        std::vector<unsigned char> mLineStripColors;
        std::vector<size_t> mLineStripVertexCount;

        // Triangle related vertex data.
        std::vector<float> mTriangleVertices;
        std::vector<float> mTriangleNormals;
        std::vector<unsigned char> mTriangleColors;
    };

    private class RenderPackageWrapper : public IRenderPackageImpl
    {
    public:
        RenderPackageWrapper(Autodesk::DesignScript::Interfaces::IRenderPackage^ rcw);
        ~RenderPackageWrapper(void);

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

        Autodesk::DesignScript::Interfaces::IRenderPackage^ wrapper() { return mRCW; }
    private:
        gcroot<Autodesk::DesignScript::Interfaces::IRenderPackage^> mRCW;
    };

    private ref class RenderPackage : public Autodesk::DesignScript::Interfaces::IRenderPackage
    {
    public:
        RenderPackage(int documentId, int packageId);
        virtual void PushPointVertex(double x, double y, double z);
        virtual void PushPointVertexColor(byte red, byte green, byte blue, byte alpha);

        virtual void PushTriangleVertex(double x, double y, double z);
        virtual void PushTriangleVertexNormal(double x, double y, double z);
        virtual void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha);

        virtual void PushLineStripVertex(double x, double y, double z);
        virtual void PushLineStripVertexCount(int n);
        virtual void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha);

        virtual property System::IntPtr NativeRenderPackage { System::IntPtr get(void); }

        RenderPackageImpl* ReleasePackageOwnership(void);

    private:
        void EnsureNativePackageCreated(void);

        int mDocumentId;
        int mPackageId;
        RenderPackageImpl* mpNativeRenderPackage;
    };

    private class RenderServiceImpl
    {
    public:
        RenderServiceImpl();
        ~RenderServiceImpl();
        bool Initialize(int width, int height);
        void Destroy(void);
        bool Resize(int width, int height);
        void NotifyThumbnailReady(ThumbnailImpl *pThumbnail) const;
        void QueuePackage(const RenderPackageImpl* pPackage);
        const RenderPackageImpl* DequeueNextPackage();

        ThumbnailImpl* LockWriteableThumbnail();
        void UnlockWriteableThumbnail(ThumbnailImpl* pThumbnail);

        // Synchronization object names.
        static const wchar_t* kPackageMutex;
        static const wchar_t* kPackageReadyEvent;
        static const wchar_t* kShutdownEvent;

    private:
        unsigned int GetOptimalThreadCount() const;
        bool CreateRendererWindows();
        bool InitializeGraphics(HWND hWindow) const;
        bool CreateSizeDependentObjects();
        void DestroySizeDependentObjects();

        // Synchornization objects.
        HANDLE mShutdownEvent;
        HANDLE mPackageReadyEvent;
        HANDLE mPackageListAccess;

        int mPixelWidth, mPixelHeight;
        ThumbnailPool* mpThumbnailPool;

        HWND mhWndParent;
        std::vector<HWND> mRendererWindows;
        std::vector<RenderThread *> mRenderThreads;
        std::set<const RenderPackageImpl *> mPendingPackages;
    };

    private class RenderThread
    {
    public:
        RenderThread(RenderServiceImpl *pRenderService,
            HWND hRenderWindow, int width, int height);
        ~RenderThread();
        unsigned int Run();
        HANDLE GetThreadHandle() const;

    private:
        bool SetupThreadContext();
        void DestroyThreadContext();
        void DequeueAndProcessPackage();
        bool ConstructPixelBuffer();
        void RenderScene(const RenderPackageImpl* pPackage);
        void SetupRenderSettings(const RenderPackageImpl* pPackage) const;
        void CreateOrUpdateBuffers(const RenderPackageImpl* pPackage);
        void RenderPointPrimitives(const RenderPackageImpl* pPackage) const;
        void RenderLineStripPrimitives(const RenderPackageImpl* pPackage) const;
        void RenderTrianglePrimitives(const RenderPackageImpl* pPackage) const;

        // Generic helper methods.
        void AppendStatus(const char* pMessage);
        void ReportWin32Error(const char* pMessage) const;
        void ReportOpenGlErrors(const char* pMessage) const;

        int mPixelWidth, mPixelHeight;
        bool mReadyForRendering;
        float mBackgroundColor[4];
        unsigned char* mpLocalBuffer;
        HANDLE mThreadHandle;
        Camera* mpThreadCamera;
        RenderServiceImpl* mpRenderService;

        HWND  mhRenderWindow;
        HDC   mFrameBufferDC;
        HGLRC mFrameBufferRC;

        // Hardware object handles.
        unsigned int mPointVboId;
        unsigned int mLineStripVboId;
        unsigned int mTriangleVboId;

        // Status messages.
        std::string mStatusString;

#ifndef USE_FRAME_BUFFER
        HPBUFFERARB mPixelBuffer;
        HDC   mPixelBufferDC;
        HGLRC mPixelBufferRC;
#else
        unsigned int mColorRenderBufferId;
        unsigned int mDepthRenderBufferId;
        unsigned int mFrameBufferId;
#endif
    };

} }
