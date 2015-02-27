
#include "stdafx.h"
#include "stdio.h"
#include "Internal.h"

using namespace DesignScriptStudio::Renderer;

static int CountSetBits(ULONG_PTR bitMask)
{
    int totalBits = 0;
    ULONG_PTR testBit = 1;
    for (int bit = 0; bit < (sizeof(ULONG_PTR) * 8); ++bit)
    {
        if ((testBit & bitMask) != 0)
            totalBits = totalBits + 1;

        testBit = testBit << 1;
    }

    return totalBits;
}

const wchar_t* RenderServiceImpl::kPackageMutex       = L"PackageListMutex";
const wchar_t* RenderServiceImpl::kPackageReadyEvent  = L"PackageReadyEvent";
const wchar_t* RenderServiceImpl::kShutdownEvent      = L"RenderThreadsShutdownEvent";

RenderServiceImpl::RenderServiceImpl() : 
    mPixelWidth(0), mPixelHeight(0),
    mpThumbnailPool(NULL),
    mhWndParent(NULL),
    mShutdownEvent(NULL),
    mPackageReadyEvent(NULL),
    mPackageListAccess(NULL)
{
}

RenderServiceImpl::~RenderServiceImpl()
{
    Destroy();
}

bool RenderServiceImpl::Initialize(int width, int height)
{
    TRACEMSG3(L"RenderServiceImpl: Initialized to %dx%d!\n", width, height);

    // TODO: Ensure parameters are of values 2^X.
    mPixelWidth = width;
    mPixelHeight = height;

    mPackageListAccess = CreateMutex(NULL, FALSE, RenderServiceImpl::kPackageMutex);
    if (NULL == mPackageListAccess)
        return false;

    mPackageReadyEvent = CreateEvent(NULL, FALSE, FALSE, RenderServiceImpl::kPackageReadyEvent);
    if (NULL == mPackageReadyEvent)
        return false;

    mShutdownEvent = CreateEvent(NULL, TRUE, FALSE, RenderServiceImpl::kShutdownEvent);
    if (NULL == mShutdownEvent)
        return false;

    if (CreateRendererWindows() == false)
        return false;

    return CreateSizeDependentObjects();
}

void RenderServiceImpl::Destroy(void)
{
    // Service is already shut-down.
    if (NULL == mPackageListAccess)
        return;

    TRACEMSG(L"RenderServiceImpl: Shutting down...\n");
    DestroySizeDependentObjects();

    if (NULL != mPackageListAccess)
    {
        unsigned int result = WaitForSingleObject(mPackageListAccess, INFINITE);
        if (WAIT_OBJECT_0 == result)
        {
            // If there are packages left in the queue when shutdown is initiated, 
            // then go through each of them and free up the memory they consume.
            std::set<const RenderPackageImpl *>::iterator iterator = mPendingPackages.begin();
            for (; iterator != mPendingPackages.end(); ++iterator)
                delete *iterator;

            mPendingPackages.clear();
        }

        TRACEMSG(L"RenderServiceImpl: Package list cleared\n");
        CloseHandle(mPackageListAccess);
    }

    if (mRendererWindows.size() > 0)
    {
        std::vector<HWND>::iterator iterator = mRendererWindows.begin();
        for (; iterator != mRendererWindows.end(); ++iterator)
            DestroyWindow(*iterator);

        mRendererWindows.clear();
    }

    if (NULL != mPackageReadyEvent)
        CloseHandle(mPackageReadyEvent);
    if (NULL != mShutdownEvent)
        CloseHandle(mShutdownEvent);

    mShutdownEvent = NULL;
    mPackageReadyEvent = NULL;
    mPackageListAccess = NULL;
    TRACEMSG(L"RenderServiceImpl: Shut down!\n");
}

bool RenderServiceImpl::Resize(int width, int height)
{
    mPixelWidth = width;
    mPixelHeight = height;
    DestroySizeDependentObjects();
    return CreateSizeDependentObjects();
}

void RenderServiceImpl::NotifyThumbnailReady(ThumbnailImpl *pThumbnail) const
{
    if (NULL == pThumbnail)
        return;

    RenderService^ renderService = RenderService::GetInstance();
    if (nullptr != renderService)
        renderService->NotifyThumbnailReady(pThumbnail);
}

void RenderServiceImpl::QueuePackage(const RenderPackageImpl* pPackage)
{
    if (NULL == pPackage)
        return;

    HANDLE handles[] = { mPackageListAccess, mShutdownEvent };
    switch (WaitForMultipleObjects(_countof(handles), handles, FALSE, INFINITE))
    {
    case WAIT_OBJECT_0:
        {
            mPendingPackages.insert(pPackage);

            PackageId id = pPackage->GetIdentifier();
            TRACEMSG2(L"RenderServiceImpl: Package 0x%.8X queued\n", id.packageId);
            ReleaseMutex(mPackageListAccess); // Release exclusive access.

            // Signal that package being ready, so if there's any renderer 
            // thread that is waiting, then it will resume work immediately
            // 
            SetEvent(mPackageReadyEvent);
            break;
        }

    default:
    case WAIT_OBJECT_0 + 1: // Shutdown event.
        break;
    }
}

const RenderPackageImpl* RenderServiceImpl::DequeueNextPackage()
{
    const RenderPackageImpl* pPackage = NULL;

    HANDLE handles[] = { mPackageListAccess, mShutdownEvent };
    switch (WaitForMultipleObjects(_countof(handles), handles, FALSE, INFINITE))
    {
    case WAIT_OBJECT_0:
        {
            if (mPendingPackages.size() <= 0)
            {
                TRACEMSG(L"RenderServiceImpl: No package to dequeue\n");
            }
            else
            {
                pPackage = *(mPendingPackages.begin());
                mPendingPackages.erase(pPackage);

                PackageId id = pPackage->GetIdentifier();
                TRACEMSG2(L"RenderServiceImpl: Package 0x%.8X dequeued\n", id.packageId);
            }

            // Fix: IDE-1619: If there's still package left in the package list, 
            // then signal the "RenderServiceImpl::kPackageReadyEvent" event so 
            // any waiting thread will attempt to dequeue from the list again.
            // 
            const bool somePackagesLeft = (mPendingPackages.size() > 0);
            ReleaseMutex(mPackageListAccess);

            if (false != somePackagesLeft)
                SetEvent(mPackageReadyEvent);
            break;
        }

    case WAIT_OBJECT_0 + 1:
        break;
    }

    return pPackage;
}

ThumbnailImpl* RenderServiceImpl::LockWriteableThumbnail()
{
    if (NULL != mpThumbnailPool)
        return mpThumbnailPool->LockWriteableThumbnail();

    return NULL;
}

void RenderServiceImpl::UnlockWriteableThumbnail(ThumbnailImpl* pThumbnail)
{
    if (NULL != mpThumbnailPool)
        mpThumbnailPool->UnlockWriteableThumbnail(pThumbnail);
}

unsigned int RenderServiceImpl::GetOptimalThreadCount() const
{
#if 0 // Experimental for now.

    unsigned long processorCores = 0;
    unsigned long logicalProcessors = 0;

    PSYSTEM_LOGICAL_PROCESSOR_INFORMATION pBuffer = NULL;
    PSYSTEM_LOGICAL_PROCESSOR_INFORMATION pReadPtr = NULL;

    unsigned long length = 0;
    if (!GetLogicalProcessorInformation(NULL, &length)) // This will fail.
    {
        if (GetLastError() == ERROR_INSUFFICIENT_BUFFER) {
            pBuffer = ((PSYSTEM_LOGICAL_PROCESSOR_INFORMATION) malloc(length));
            GetLogicalProcessorInformation(pBuffer, &length);

            int startOffset = 0;
            pReadPtr = pBuffer;

            while (startOffset + sizeof(PSYSTEM_LOGICAL_PROCESSOR_INFORMATION) <= length)
            {
                switch(pReadPtr->Relationship)
                {
                case RelationProcessorCore:
                    // A hyperthreaded core supplies more than one logical processor.
                    logicalProcessors += CountSetBits(pReadPtr->ProcessorMask);
                    processorCores++;
                    break;
                }

                pReadPtr++; // Move on to point to the next block.
                startOffset += sizeof(PSYSTEM_LOGICAL_PROCESSOR_INFORMATION);
            }

            free(pBuffer);
        }
    }

    return ((processorCores > 0) ? processorCores : 4);

#else

    return 4;

#endif
}

bool RenderServiceImpl::CreateRendererWindows()
{
    if (mRendererWindows.size() > 0)
        throw new std::exception("'RenderServiceImpl::CreateRendererWindows' called twice!");

    WNDCLASSEX wcx;
    ZeroMemory(&wcx, sizeof(WNDCLASSEX));

    HINSTANCE hInstance = NULL; // AfxGetInstanceHandle();
    const wchar_t* rendererClassName = L"RenderThreadWindow";

    wcx.cbSize = sizeof(wcx);
    wcx.style = CS_HREDRAW | CS_VREDRAW;
    wcx.lpfnWndProc = DefWindowProc;
    wcx.hInstance = hInstance;
    wcx.hbrBackground = (HBRUSH) GetStockObject(WHITE_BRUSH);
    wcx.lpszClassName = rendererClassName;
    wcx.cbWndExtra = sizeof(void *);
    RegisterClassEx(&wcx);

    mhWndParent = CreateWindow(rendererClassName, NULL, WS_POPUP,
        CW_USEDEFAULT, CW_USEDEFAULT, mPixelWidth, mPixelHeight,
        NULL, ((HMENU) NULL), hInstance, ((LPVOID) NULL));

    if (this->InitializeGraphics(mhWndParent) == false)
        return false;

    unsigned int threads = GetOptimalThreadCount();
    for (unsigned int index = 0; index < threads; ++index)
    {
        HWND hWnd = CreateWindow(rendererClassName, NULL, WS_CHILDWINDOW,
            CW_USEDEFAULT, CW_USEDEFAULT, mPixelWidth, mPixelHeight,
            mhWndParent, ((HMENU) NULL), hInstance, ((LPVOID) NULL));

        ::UpdateWindow(hWnd);
        mRendererWindows.push_back(hWnd);
    }

    return true;
}

bool RenderServiceImpl::InitializeGraphics(HWND hWindow) const
{
    bool oglInitialized = false;
    std::string error;
    HGLRC hFrameBufferRC = NULL;
    HDC hFrameBufferDC = ::GetDC(hWindow);

    if (OpenGL::ConstructFrameBuffer(hFrameBufferDC, &hFrameBufferRC, error)) {
        if (OpenGL::Initialize(hFrameBufferDC))
            oglInitialized = true;
    }

    if (NULL != hFrameBufferRC) {
        wglMakeCurrent(hFrameBufferDC, NULL);
        wglDeleteContext(hFrameBufferRC);
        hFrameBufferRC = NULL;
    }

    ::ReleaseDC(hWindow, hFrameBufferDC);
    hFrameBufferDC = NULL;
    return oglInitialized;
}

bool RenderServiceImpl::CreateSizeDependentObjects()
{
    if (mRenderThreads.size() > 0 || (NULL != mpThumbnailPool))
        throw new std::exception("'RenderServiceImpl::Initialize' called twice!");

    // New thumbnail pool to write thumbnails to.
    mpThumbnailPool = new ThumbnailPool(this);
    mpThumbnailPool->Initialize(mPixelWidth, mPixelHeight);

    // @TODO(Ben): Please resize all the renderer windows as well!

    unsigned int threads = GetOptimalThreadCount();
    if (threads != mRendererWindows.size())
        throw new std::exception("Not matching thread count!");

    for (unsigned int index = 0; index < threads; ++index)
    {
        mRenderThreads.push_back(new RenderThread(this,
            mRendererWindows[index], mPixelWidth, mPixelHeight));
    }

    return true;
}

void RenderServiceImpl::DestroySizeDependentObjects()
{
    if (NULL != mShutdownEvent) {
        TRACEMSG(L"RenderServiceImpl: Signaling shut down\n");
        SetEvent(mShutdownEvent);
    }

    if (mRenderThreads.size() > 0)
    {
        unsigned int count = 0;
        HANDLE* pThreadHandles = new HANDLE[mRenderThreads.size()];
        std::vector<RenderThread *>::iterator iterator = mRenderThreads.begin();
        for (; iterator != mRenderThreads.end(); ++iterator)
        {
            RenderThread* pRenderer = *iterator;
            pThreadHandles[count++] = pRenderer->GetThreadHandle();
        }

        // Wait for all the running threads to be shutdown.
        WaitForMultipleObjects(count, pThreadHandles, TRUE, INFINITE);
        TRACEMSG(L"RenderServiceImpl: All threads shut down\n");

        iterator = mRenderThreads.begin();
        for (; iterator != mRenderThreads.end(); ++iterator) {
            RenderThread* pRenderThread = *iterator;
            delete pRenderThread;
        }

        mRenderThreads.clear();
        delete [] pThreadHandles;
    }

    if (NULL != mpThumbnailPool) {
        mpThumbnailPool->Destroy();
        delete mpThumbnailPool;
        mpThumbnailPool = NULL;
    }

    if (NULL != mShutdownEvent)
        ResetEvent(mShutdownEvent);
}

