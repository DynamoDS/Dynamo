
#include "stdafx.h"
#include "Internal.h"

using namespace DesignScriptStudio::Renderer;

////////////////////////////////////////////////////////////////////////////////

ThumbnailImpl::ThumbnailImpl(int width, int height) : 
    mPixelWidth(width), mPixelHeight(height),
    mpPixelBuffer(NULL)
{
    // Pixel buffer stores ARGB data, which is four bytes 
    // per pixel, so we need to multiply the dimension by 4.
    int bytesInBuffer = mPixelWidth * mPixelHeight * 4;
    mpPixelBuffer = new unsigned char[bytesInBuffer];
}

ThumbnailImpl::~ThumbnailImpl()
{
    if (NULL != mpPixelBuffer) {
        delete [] mpPixelBuffer;
        mpPixelBuffer = NULL;
    }
}

PackageId ThumbnailImpl::GetIdentifier(void) const
{
    return mPackageId;
}

void ThumbnailImpl::GetThumbnailSize(int& width, int& height) const
{
    width = this->mPixelWidth;
    height = this->mPixelHeight;
}

int ThumbnailImpl::BufferSizeInBytes(void) const
{
    return mPixelWidth * mPixelHeight * 4;
}

unsigned char* ThumbnailImpl::GetWriteableBuffer(PackageId packageId)
{
    mPackageId = packageId;
    return mpPixelBuffer;
}

const unsigned char* ThumbnailImpl::GetPixelBuffer(void) const
{
    return mpPixelBuffer;
}

////////////////////////////////////////////////////////////////////////////////

const wchar_t* ThumbnailPool::kThumbnailPoolMutex = L"ThumbnailPoolMutex";

ThumbnailPool::ThumbnailPool(RenderServiceImpl* pRenderService) : 
mpRenderService(pRenderService),
mThumbWidth(0),
mThumbHeight(0),
mNotifierThread(NULL),
mShutdownEvent(NULL),
mThumbnailPoolAccess(NULL)
{
}

ThumbnailPool::~ThumbnailPool(void)
{
    Destroy();
}

void ThumbnailPool::Initialize(int width, int height)
{
    mThumbWidth = width;
    mThumbHeight = height;
    mShutdownEvent = CreateEvent(NULL, TRUE, FALSE, RenderServiceImpl::kShutdownEvent);
    mThumbnailPoolAccess = CreateMutex(NULL, FALSE, ThumbnailPool::kThumbnailPoolMutex);

    unsigned long threadId = 0;
    LPTHREAD_START_ROUTINE pRoutine = ((LPTHREAD_START_ROUTINE) NotifierThreadProc);
    mNotifierThread = CreateThread(NULL, 0, pRoutine, this, 0, &threadId);
}

void ThumbnailPool::Destroy(void)
{
    if (NULL != mNotifierThread)
    {
        // Waiting for the notifier thread to return here...
        WaitForSingleObject(mNotifierThread, INFINITE);
        CloseHandle(mNotifierThread);
        mNotifierThread = NULL;
    }

    if (NULL != mThumbnailPoolAccess) {
        CloseHandle(mThumbnailPoolAccess);
        mThumbnailPoolAccess = NULL;
    }

    std::set<ThumbnailImpl *>::iterator iterator = mUnusedThumbnails.begin();
    for (; iterator != mUnusedThumbnails.end(); ++iterator)
        delete (*iterator);

    iterator = mResultThumbnails.begin();
    for (; iterator != mResultThumbnails.end(); ++iterator)
        delete (*iterator);

    mUnusedThumbnails.clear();
    mResultThumbnails.clear();
    mShutdownEvent = NULL; // Owned by 'RenderServiceImpl'.
}

ThumbnailImpl* ThumbnailPool::LockWriteableThumbnail()
{
    if (NULL == mShutdownEvent)
        throw new std::exception("'ThumbnailPool::Initialize' was not called!");

    HANDLE handles[] = { mThumbnailPoolAccess, mShutdownEvent };
    switch (WaitForMultipleObjects(_countof(handles), handles, FALSE, INFINITE))
    {
        case WAIT_OBJECT_0:
        {
            // We have gained exclusive access to the pool internal now, but 
            // do note that we do not call 'ReleaseMutex(mThumbnailPoolAccess)' 
            // here because it will be done in 'UnlockWriteableThumbnail' later. 
            return GetThumbnailUnsafe();
        }

        case WAIT_OBJECT_0 + 1: // Shutdown event...
            return NULL;
    }

    return NULL; // Mutex was abandoned?
}

void ThumbnailPool::UnlockWriteableThumbnail(ThumbnailImpl* pThumbnail)
{
    // Place the populated thumbnail into the result list...
    mResultThumbnails.insert(pThumbnail);

    // Finally, release the exclusive access.
    ReleaseMutex(mThumbnailPoolAccess);
}

unsigned int ThumbnailPool::NotifierThreadProc(void *pContext)
{
    ThumbnailPool* pThumbnailPool = ((ThumbnailPool *) pContext);
    return (pThumbnailPool->InternalThreadProc());
}

unsigned int ThumbnailPool::InternalThreadProc(void)
{
    bool threadLoopRunning = true;
    while(threadLoopRunning)
    {
        // Warning: the 'mShutdownEvent' has to be the first since this thread is 
        // very likely to gain access to 'mThumbnailPoolAccess' when all other 
        // render threads are already terminated (when 'mShutdownEvent' is signaled).
        HANDLE handles[] = { mShutdownEvent, mThumbnailPoolAccess };
        switch (WaitForMultipleObjects(_countof(handles), handles, FALSE, INFINITE))
        {
            case WAIT_OBJECT_0: // Shutdown event...
                threadLoopRunning = false;
                TRACEMSG(L"ThumbnailPool: Notifier thread shutting down...");
                break;

            case WAIT_OBJECT_0 + 1:
            {
                // We have gained exclusive access to the pool internal now.
                bool resultObtained = false;
                if (mResultThumbnails.size() > 0) {
                    NotifyServiceClientUnsafe();
                    resultObtained = true;
                }

                // Unlock before going to sleep.
                ReleaseMutex(mThumbnailPoolAccess);

                // If the thread did get a thumbnail result, which potentially 
                // means that there might be more in the list. In such case it 
                // should just briefly rest and then carry on looking. If there 
                // wasn't a result, then it can afford to sleep for longer.
                Sleep(resultObtained ? 10 : 200);
                break;
            }
        }
    }

    return 0;
}

ThumbnailImpl* ThumbnailPool::GetThumbnailUnsafe(void)
{
    // This method call needs to be guarded with a mutex.
    if (mUnusedThumbnails.size() > 0)
    {
        std::set<ThumbnailImpl *>::iterator first;
        first = mUnusedThumbnails.begin();
        ThumbnailImpl* pThumbnail = *first;
        mUnusedThumbnails.erase(first);
        return pThumbnail;
    }

    return (new ThumbnailImpl(mThumbWidth, mThumbHeight));
}

void ThumbnailPool::NotifyServiceClientUnsafe(void)
{
    // This method call needs to be guarded with a mutex.
    std::set<ThumbnailImpl *>::iterator nextResult;
    nextResult = mResultThumbnails.begin();
    ThumbnailImpl* pThumbnail = *nextResult;

    // Notify the render service client of the new result.
    mpRenderService->NotifyThumbnailReady(pThumbnail);

    // Remove from the result list, place it inside unused list.
    mResultThumbnails.erase(nextResult);
    mUnusedThumbnails.insert(pThumbnail);
}
