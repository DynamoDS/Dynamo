
#include "stdafx.h"
#include "Internal.h"
#include "Camera.h"

#ifdef _DEBUG
#define ENABLE_THREAD_VISUALIZATION
#endif

using namespace DesignScriptStudio::Renderer;

unsigned int WINAPI RenderThreadRoutine(void *pContext)
{
    return (((RenderThread *) pContext)->Run());
}

#define SetColor(c, r, g, b) { c[0] = r / 255.0f; c[1] = g / 255.0f; c[2] = b / 255.0f; c[3] = 1.0f; }

RenderThread::RenderThread(
    RenderServiceImpl *pRenderService, 
    HWND hRenderWindow, int width, int height) :

mPixelWidth(width), mPixelHeight(height),
    mhRenderWindow(hRenderWindow),
    mFrameBufferDC(NULL), mFrameBufferRC(NULL),
    mPointVboId(-1), mLineStripVboId(-1), mTriangleVboId(-1),

#ifndef USE_FRAME_BUFFER
    mPixelBuffer(NULL),
    mPixelBufferDC(NULL),
    mPixelBufferRC(NULL),
#else
    mColorRenderBufferId(0),
    mDepthRenderBufferId(0),
    mFrameBufferId(0),
#endif

    mReadyForRendering(false),
    mpLocalBuffer(NULL),
    mThreadHandle(NULL),
    mpThreadCamera(NULL),
    mpRenderService(pRenderService)
{
    static int index = 0;
    switch((index++) % 4)
    {
    case 0: SetColor(mBackgroundColor, 200, 191, 231); break;
    case 1: SetColor(mBackgroundColor, 153, 217, 234); break;
    case 2: SetColor(mBackgroundColor, 195, 195, 195); break;
    case 3: SetColor(mBackgroundColor, 239, 228, 176); break;
    }

    mpThreadCamera = new Camera(width, height); // Create default camera.

    int bytes = ((mPixelWidth * 4) * mPixelHeight);
    mpLocalBuffer = new unsigned char[bytes];

    unsigned long threadId = 0;
    LPTHREAD_START_ROUTINE pRoutine = ((LPTHREAD_START_ROUTINE) RenderThreadRoutine);
    mThreadHandle = CreateThread(NULL, 0, pRoutine, this, 0, &threadId);
    TRACEMSG2(L"RenderThread(0x%x): Thread created\n", threadId);
}

RenderThread::~RenderThread()
{
    if (NULL != mpLocalBuffer) {
        delete [] mpLocalBuffer;
        mpLocalBuffer = NULL;
    }

    delete mpThreadCamera;
    mpThreadCamera = NULL;
}

unsigned int RenderThread::Run()
{
    unsigned int threadExitCode = 1200;
    if (SetupThreadContext() != false)
    {
        TRACEMSG2(L"RenderThread(0x%x): Running...\n", GetCurrentThreadId());

        threadExitCode = 0;
        HANDLE packageReadyEvent = OpenEvent(EVENT_ALL_ACCESS, FALSE, RenderServiceImpl::kPackageReadyEvent);
        HANDLE shutdownEvent = CreateEvent(NULL, TRUE, FALSE, RenderServiceImpl::kShutdownEvent);

        bool keepRunning = true;
        while (keepRunning)
        {
            HANDLE handles[] = { packageReadyEvent, shutdownEvent };
            switch(WaitForMultipleObjects(_countof(handles), handles, FALSE, INFINITE))
            {
            case WAIT_OBJECT_0: // Package ready, attempt to process it.
                DequeueAndProcessPackage();
                break;
            case WAIT_OBJECT_0 + 1: // Shutdown event was signaled.
                TRACEMSG2(L"RenderThread(0x%x): Shutdown signaled\n", GetCurrentThreadId());
                keepRunning = false;
                break;

            case WAIT_ABANDONED_0:
            case WAIT_ABANDONED_0 + 1:
                keepRunning = false;
                threadExitCode = 1201;
                break;
            }
        }
    }

    DestroyThreadContext();

    TRACEMSG2(L"OpenGL status message: %S\n", OpenGL::GetErrorString());
    TRACEMSG2(L"Thread status message: %S\n", this->mStatusString.c_str());
    TRACEMSG3(L"RenderThread(0x%x): Exit code %d\n", GetCurrentThreadId(), threadExitCode);
    return threadExitCode;
}

HANDLE RenderThread::GetThreadHandle() const
{
    return mThreadHandle;
}

bool RenderThread::SetupThreadContext()
{
    std::string error;
    this->mFrameBufferDC = ::GetDC(mhRenderWindow);
    if (OpenGL::ConstructFrameBuffer(mFrameBufferDC, &mFrameBufferRC, error))
        return ConstructPixelBuffer();

    return false;
}

void RenderThread::DestroyThreadContext()
{
    if (OpenGL::IsInitialized())
    {
        DESTROYOPENGLBUFFER(mPointVboId);
        DESTROYOPENGLBUFFER(mLineStripVboId);
        DESTROYOPENGLBUFFER(mTriangleVboId);

#ifndef USE_FRAME_BUFFER

        if (NULL != mPixelBufferRC) {
            glDrawBuffer(GL_BACK);  // Draw to the back buffer of frame buffer
            glReadBuffer(GL_FRONT); // Read from the front buffer of the frame buffer
            OpenGL::MakeContextCurrent(mFrameBufferDC, mFrameBufferDC, mFrameBufferRC);
            wglDeleteContext(mPixelBufferRC);
        }

        if (NULL != mPixelBufferDC)
            OpenGL::ReleasePbufferDC(mPixelBuffer, mPixelBufferDC);
        if (NULL != mPixelBuffer)
            OpenGL::DestroyPbuffer(mPixelBuffer);

        mPixelBufferRC = NULL;
        mPixelBufferDC = NULL;
        mPixelBuffer = NULL;

#else

        // Destroy frame buffer and render buffer...
        OpenGL::DeleteFramebuffers(1, &mFrameBufferId);
        OpenGL::DeleteRenderbuffers(1, &mColorRenderBufferId);
        OpenGL::DeleteRenderbuffers(1, &mDepthRenderBufferId);
        mFrameBufferId = 0;
        mColorRenderBufferId = 0;
        mDepthRenderBufferId = 0;

#endif
    }

    glViewport(0, 0, mPixelWidth, mPixelHeight);

    if (NULL != mFrameBufferRC) {
        wglMakeCurrent(mFrameBufferDC, NULL);
        wglDeleteContext(mFrameBufferRC);
        mFrameBufferRC = NULL;
    }

    ReleaseDC(mhRenderWindow, mFrameBufferDC);
    mFrameBufferDC = NULL;
}

void RenderThread::DequeueAndProcessPackage()
{
    const RenderPackageImpl* pPackage = mpRenderService->DequeueNextPackage();
    if (NULL != pPackage)
    {
        PackageId id = pPackage->GetIdentifier();
        TRACEMSG3(L"RenderThread(0x%x): Handling package 0x%.8X\n",
            GetCurrentThreadId(), id.packageId);

        RenderScene(pPackage);
        delete pPackage;
    }
}

bool RenderThread::ConstructPixelBuffer()
{
#ifdef USE_FRAME_BUFFER

    // Initialize render buffer for color storage...
    OpenGL::GenRenderbuffers(1, &mColorRenderBufferId);
    OpenGL::BindRenderbuffer(GL_RENDERBUFFER, mColorRenderBufferId);
    OpenGL::RenderbufferStorage(GL_RENDERBUFFER, GL_RGBA8, mPixelWidth, mPixelHeight);
    OpenGL::BindRenderbuffer(GL_RENDERBUFFER, 0);

    // Initialize render buffer for depth storage...
    OpenGL::GenRenderbuffers(1, &mDepthRenderBufferId);
    OpenGL::BindRenderbuffer(GL_RENDERBUFFER, mDepthRenderBufferId);
    OpenGL::RenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, mPixelWidth, mPixelHeight);
    OpenGL::BindRenderbuffer(GL_RENDERBUFFER, 0);

    // Initialize frame buffer...
    OpenGL::GenFramebuffers(1, &mFrameBufferId);
    OpenGL::BindFramebuffer(GL_FRAMEBUFFER, mFrameBufferId);

    OpenGL::FramebufferRenderbuffer(GL_FRAMEBUFFER,
        GL_COLOR_ATTACHMENT0, GL_RENDERBUFFER, mColorRenderBufferId);
    OpenGL::FramebufferRenderbuffer(GL_FRAMEBUFFER,
        GL_DEPTH_ATTACHMENT_EXT, GL_RENDERBUFFER, mDepthRenderBufferId);

    unsigned int status = OpenGL::CheckFramebufferStatus(GL_FRAMEBUFFER);
    OpenGL::BindFramebuffer(GL_FRAMEBUFFER, 0);

    if (GL_FRAMEBUFFER_COMPLETE != status) {
        ReportOpenGlErrors("'glCheckFramebufferStatus' failed!");
        return false;
    }

#else

    int attributes[] =
    {
        WGL_SUPPORT_OPENGL_ARB,         TRUE,   // pbuffer will be used with gl
        WGL_DRAW_TO_PBUFFER_ARB,        TRUE,   // enable render to pbuffer
        WGL_BIND_TO_TEXTURE_RGBA_ARB,   FALSE,  // pbuffer will not be used as a texture
        WGL_RED_BITS_ARB,               8,      // at least 8 bits for RED channel
        WGL_GREEN_BITS_ARB,             8,      // at least 8 bits for GREEN channel
        WGL_BLUE_BITS_ARB,              8,      // at least 8 bits for BLUE channel
        WGL_ALPHA_BITS_ARB,             8,      // at least 8 bits for ALPHA channel
        WGL_DEPTH_BITS_ARB,             24,     // at least 24 bits for depth buffer
        WGL_DOUBLE_BUFFER_ARB,          FALSE,  // we don’t require double buffering
        0 // zero terminates the list
    };

    unsigned int count = 0;
    int pixelFormat;
    if (!OpenGL::ChoosePixelFormat(mFrameBufferDC, (const int*)attributes, NULL, 1, &pixelFormat, &count)) {
        ReportOpenGlErrors("'OpenGL::ChoosePixelFormat' failed!");
        return false;
    }

    if (0 == count) {
        ReportOpenGlErrors("Pixel format count is zero!");
        return false;
    }

    // Create pixel buffer and its related device/render contexts.
    NONNULL(mPixelBuffer, OpenGL::CreatePbuffer(mFrameBufferDC,
        pixelFormat, mPixelWidth, mPixelHeight, NULL));
    NONNULL(mPixelBufferDC, OpenGL::GetPbufferDC(mPixelBuffer));
    NONNULL(mPixelBufferRC, wglCreateContext(mPixelBufferDC));

    // TODO: Ensure pbuffer is still valid (no display mode changes).
    NOTFALSE(OpenGL::MakeContextCurrent(mPixelBufferDC, 
        mPixelBufferDC, mPixelBufferRC), "OpenGL::MakeContextCurrent");

    // Remember we don't have no double buffering for pbuffer?
    glDrawBuffer(GL_FRONT); // Draw to front buffer.
    glReadBuffer(GL_FRONT); // Read from front buffer.

#endif
    glViewport(0, 0, mPixelWidth, mPixelHeight);
    mReadyForRendering = true;
    return true;
}

void RenderThread::RenderScene(const RenderPackageImpl* pPackage)
{
    SetupRenderSettings(pPackage);
    CreateOrUpdateBuffers(pPackage);
    RenderPointPrimitives(pPackage);
    RenderLineStripPrimitives(pPackage);
    RenderTrianglePrimitives(pPackage);

    ThumbnailImpl* pThumbnail = mpRenderService->LockWriteableThumbnail();
    if (NULL != pThumbnail)
    {
        PackageId packageId = pPackage->GetIdentifier();
        TRACEMSG3(L"RenderThread(0x%x): Processed package 0x%x\n", GetCurrentThreadId(), packageId.packageId);

        glReadPixels(0, 0, mPixelWidth, mPixelHeight, GL_RGBA, GL_UNSIGNED_BYTE, mpLocalBuffer);

        // Place the read pointer at the last row of pixels.
        const int byteWidth = mPixelWidth * 4;
        unsigned char* pReadPtr = mpLocalBuffer;
        pReadPtr += ((mPixelHeight - 1) * byteWidth);

        // Start inverting the image upside-down.
        unsigned char* pBuffer = pThumbnail->GetWriteableBuffer(packageId);
        {
            for (int index = 0; index < mPixelHeight; index++) {
                memcpy(pBuffer, pReadPtr, byteWidth);
                pBuffer = pBuffer + byteWidth;
                pReadPtr = pReadPtr - byteWidth;
            }
        }
        mpRenderService->UnlockWriteableThumbnail(pThumbnail);
    }

#ifdef USE_FRAME_BUFFER
    OpenGL::BindFramebuffer(GL_FRAMEBUFFER, 0);
#endif
}

void RenderThread::SetupRenderSettings(const RenderPackageImpl* pPackage) const
{
#ifdef USE_FRAME_BUFFER
    OpenGL::BindFramebuffer(GL_FRAMEBUFFER, mFrameBufferId);
#endif

    glViewport(0, 0, mPixelWidth, mPixelHeight);

    // Make sure the objects are within the view.
    float boundingBox[6] = { 0 };
    const_cast<RenderPackageImpl *>(pPackage)->ComputeBoundingBox();
    pPackage->GetBoundingBox(&(boundingBox[0]));

    mpThreadCamera->FitToBoundingBox(&(boundingBox[0]));
    mpThreadCamera->SetProjectionMatrix();
    mpThreadCamera->SetModelViewMatrices();

    glEnable(GL_DEPTH_TEST);
    glDepthFunc(GL_LESS);
    glClearDepth(1.0);
    glShadeModel(GL_SMOOTH);
    glDisable(GL_TEXTURE_2D);
    glDisable(GL_BLEND);

#ifdef ENABLE_THREAD_VISUALIZATION
    glClearColor(mBackgroundColor[0], mBackgroundColor[1],
        mBackgroundColor[2], mBackgroundColor[3]);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glColor3f(0.0f, 0.0f, 0.0f);
    Sleep(200);
#else
    glClearColor(0.2039f, 0.2039f, 0.2039f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    glColor3f(1.0f, 1.0f, 1.0f);
#endif

    glEnable(GL_LIGHTING);

    float ambientLevel = 0.2f;
    float ambientColor[] = { ambientLevel, ambientLevel, ambientLevel, 1.0f };
    glLightModelfv(GL_LIGHT_MODEL_AMBIENT, ambientColor);

    glEnable(GL_LIGHTING);
    glEnable(GL_LIGHT0);

    Vector eye = mpThreadCamera->EyePosition();
    Vector direction = mpThreadCamera->ViewDirection();

    float lightLevel = 0.8f;
    float lightPosition[] = { eye.x, eye.y, eye.z, 1.0f };
    float lightDirection[] = { direction.x, direction.y, direction.z };
    float whiteLight[] = { lightLevel, lightLevel, lightLevel, lightLevel };

    glLightfv(GL_LIGHT0, GL_POSITION, lightPosition);
    glLightfv(GL_LIGHT0, GL_SPOT_DIRECTION, lightDirection);
    glLightfv(GL_LIGHT0, GL_DIFFUSE, whiteLight);
    glLightfv(GL_LIGHT0, GL_SPECULAR, whiteLight);

    glEnable(GL_LIGHT1); // Enable front light.

    Vector eye2 = eye + direction * mpThreadCamera->FarClipPlane();
    float lightPosition2[] = { eye2.x, eye2.y, eye2.z, 1.0f };
    float lightDirection2[] = { -direction.x, -direction.y, -direction.z };

    glLightfv(GL_LIGHT1, GL_POSITION, lightPosition2);
    glLightfv(GL_LIGHT1, GL_SPOT_DIRECTION, lightDirection2);
    glLightfv(GL_LIGHT1, GL_DIFFUSE, whiteLight);
    glLightfv(GL_LIGHT1, GL_SPECULAR, whiteLight);

    glEnable(GL_COLOR_MATERIAL);
}

void RenderThread::CreateOrUpdateBuffers(const RenderPackageImpl* pPackage)
{
    const std::vector<float>& points = pPackage->GetPointVertices();
    if (points.size() > 0) // There is at least one point being specified.
    {
        const std::vector<unsigned char>& colors = pPackage->GetPointColors();

        const size_t vertexBytes = points.size() * sizeof(float);
        const size_t colorBytes  = colors.size() * sizeof(unsigned char);
        const size_t totalBytes  = vertexBytes + colorBytes;

        // @TODO(Ben): Move this out and do it only once.
        DESTROYOPENGLBUFFER(mPointVboId);
        OpenGL::GenBuffers(1, &mPointVboId);
        OpenGL::BindBuffer(GL_ARRAY_BUFFER, mPointVboId);
        OpenGL::BufferData(GL_ARRAY_BUFFER, totalBytes, NULL, GL_STATIC_DRAW);

        const float* pVertices = points.data();
        OpenGL::BufferSubData(GL_ARRAY_BUFFER, 0, vertexBytes, pVertices);

        const unsigned char* pColors = colors.data();
        if (NULL != pColors)
            OpenGL::BufferSubData(GL_ARRAY_BUFFER, vertexBytes, colorBytes, pColors);
    }

    const std::vector<float>& lineStrip = pPackage->GetLineStripVertices();
    if (lineStrip.size() > 0)
    {
        const std::vector<unsigned char>& colors = pPackage->GetLineStripColors();

        const size_t vertexBytes = lineStrip.size() * sizeof(float);
        const size_t colorBytes  = colors.size() * sizeof(unsigned char);
        const size_t totalBytes  = vertexBytes + colorBytes;

        // @TODO(Ben): Move this out and do it only once.
        DESTROYOPENGLBUFFER(mLineStripVboId);
        OpenGL::GenBuffers(1, &mLineStripVboId);
        OpenGL::BindBuffer(GL_ARRAY_BUFFER, mLineStripVboId);
        OpenGL::BufferData(GL_ARRAY_BUFFER, totalBytes, NULL, GL_STATIC_DRAW);

        const float* pVertices = lineStrip.data();
        OpenGL::BufferSubData(GL_ARRAY_BUFFER, 0, vertexBytes, pVertices);

        const unsigned char* pColors = colors.data();
        if (NULL != pColors)
            OpenGL::BufferSubData(GL_ARRAY_BUFFER, vertexBytes, colorBytes, pColors);
    }

    const std::vector<float>& triangles = pPackage->GetTriangleVertices();
    if (triangles.size() > 0)
    {
        const std::vector<float>& normals = pPackage->GetTriangleNormals();
        const std::vector<unsigned char>& colors = pPackage->GetTriangleColors();

        const size_t vertexBytes = triangles.size() * sizeof(float);
        const size_t normalBytes = normals.size() * sizeof(float);
        const size_t colorBytes  = colors.size() * sizeof(unsigned char);
        const size_t totalBytes  = vertexBytes + normalBytes + colorBytes;

        // @TODO(Ben): Move this out and do it only once.
        DESTROYOPENGLBUFFER(mTriangleVboId);
        OpenGL::GenBuffers(1, &mTriangleVboId);
        OpenGL::BindBuffer(GL_ARRAY_BUFFER, mTriangleVboId);
        OpenGL::BufferData(GL_ARRAY_BUFFER, totalBytes, NULL, GL_STATIC_DRAW);

        int offset = 0;
        const float* pVertices = triangles.data();
        OpenGL::BufferSubData(GL_ARRAY_BUFFER, offset, vertexBytes, pVertices);
        offset = offset + ((int) vertexBytes);

        const float* pNormals = normals.data();
        if (NULL != pNormals) {
            OpenGL::BufferSubData(GL_ARRAY_BUFFER, offset, normalBytes, pNormals);
            offset = offset + ((int) normalBytes);
        }

        const unsigned char* pColors = colors.data();
        if (NULL != pColors) {
            OpenGL::BufferSubData(GL_ARRAY_BUFFER, offset, colorBytes, pColors);
            offset = offset + ((int) colorBytes);
        }
    }
}

void RenderThread::RenderPointPrimitives(const RenderPackageImpl* pPackage) const
{
    const std::vector<float>& points = pPackage->GetPointVertices();
    if (points.size() <= 0) // There is no point geometry being specified.
        return;

    glDisable(GL_LIGHTING);
    glPointSize(4.0f);
    OpenGL::BindBuffer(GL_ARRAY_BUFFER, mPointVboId);

    const std::vector<unsigned char>& colors = pPackage->GetPointColors();
    if (colors.size() > 0) // This is the case where color information is specified.
    {
        glEnableClientState(GL_COLOR_ARRAY);
        glEnableClientState(GL_VERTEX_ARRAY);

        const size_t vertexBytes = points.size() * sizeof(float);
        glColorPointer(4, GL_UNSIGNED_BYTE, 0, ((void *)vertexBytes));
        glVertexPointer(3, GL_FLOAT, 0, 0);
        glDrawArrays(GL_POINTS, 0, ((int)(points.size() / 3)));

        glDisableClientState(GL_VERTEX_ARRAY);  // disable vertex arrays
        glDisableClientState(GL_COLOR_ARRAY);
    }
    else // There is no color information specified for points.
    {
        glEnableClientState(GL_VERTEX_ARRAY);

        glVertexPointer(3, GL_FLOAT, 0, 0);
        glDrawArrays(GL_POINTS, 0, ((int)(points.size() / 3)));

        glDisableClientState(GL_VERTEX_ARRAY);  // disable vertex arrays
    }

    // it is good idea to release VBOs with ID 0 after use.
    // Once bound with 0, all pointers in gl*Pointer() behave as real
    // pointer, so, normal vertex array operations are re-activated
    OpenGL::BindBuffer(GL_ARRAY_BUFFER, 0);
}

void RenderThread::RenderLineStripPrimitives(const RenderPackageImpl* pPackage) const
{
    const std::vector<float>& lineStrip = pPackage->GetLineStripVertices();
    if (lineStrip.size() <= 0)
        return;

    glDisable(GL_LIGHTING);
    OpenGL::BindBuffer(GL_ARRAY_BUFFER, mLineStripVboId);

    const std::vector<unsigned char>& colors = pPackage->GetLineStripColors();
    const bool colorSpecified = (colors.size() > 0);

    if (false != colorSpecified)
        glEnableClientState(GL_COLOR_ARRAY);
    glEnableClientState(GL_VERTEX_ARRAY);

    const size_t vertexBytes = lineStrip.size() * sizeof(float);

    if (false != colorSpecified)
        glColorPointer(4, GL_UNSIGNED_BYTE, 0, (void *)(vertexBytes));
    glVertexPointer(3, GL_FLOAT, 0, 0);

    const std::vector<size_t>& lineStripVertexCount = pPackage->GetLineStripVertexCount();
    std::vector<size_t>::const_iterator iterator = lineStripVertexCount.begin();
    for (size_t index = 0; iterator != lineStripVertexCount.end(); ++iterator) {
        glDrawArrays(GL_LINE_STRIP, ((int) index), ((int) *iterator));
        index += *iterator;
    }

    glDisableClientState(GL_VERTEX_ARRAY);
    if (false != colorSpecified)
        glDisableClientState(GL_COLOR_ARRAY);

    OpenGL::BindBuffer(GL_ARRAY_BUFFER, 0); // Reset buffer binding.
}

void RenderThread::RenderTrianglePrimitives(const RenderPackageImpl* pPackage) const
{
    const std::vector<float>& triangles = pPackage->GetTriangleVertices();
    if (triangles.size() <= 0)
        return;

    const std::vector<float>& normals = pPackage->GetTriangleNormals();
    const std::vector<unsigned char>& colors = pPackage->GetTriangleColors();

    glEnable(GL_LIGHTING);
    glEnable(GL_COLOR_MATERIAL);

    OpenGL::BindBuffer(GL_ARRAY_BUFFER, mTriangleVboId);

    const size_t vertexBytes = triangles.size() * sizeof(float);
    const size_t normalBytes = normals.size() * sizeof(float);
    const size_t colorBytes = colors.size() * sizeof(unsigned char);

    if (normalBytes > 0)
        glEnableClientState(GL_NORMAL_ARRAY);
    if (colorBytes > 0)
        glEnableClientState(GL_COLOR_ARRAY);
    glEnableClientState(GL_VERTEX_ARRAY);

    glVertexPointer(3, GL_FLOAT, 0, 0);
    if (normalBytes > 0)
        glNormalPointer(GL_FLOAT, 0, (void *)(vertexBytes));
    if (colorBytes > 0)
        glColorPointer(4, GL_UNSIGNED_BYTE, 0, (void *)(vertexBytes + normalBytes));

    glDrawArrays(GL_TRIANGLES, 0, ((int) triangles.size()) / 3);

    glDisableClientState(GL_VERTEX_ARRAY);
    if (colorBytes > 0)
        glDisableClientState(GL_COLOR_ARRAY); 
    if (normalBytes > 0)
        glDisableClientState(GL_NORMAL_ARRAY); 

    OpenGL::BindBuffer(GL_ARRAY_BUFFER, 0); // Reset buffer binding.
    glDisable(GL_LIGHTING);
}

void RenderThread::AppendStatus(const char* pMessage)
{
    if (NULL != pMessage) {
        mStatusString += pMessage;
        mStatusString += "\n";
    }
}

void RenderThread::ReportWin32Error(const char* pMessage) const
{
    char buffer[256] = { 0 };
    sprintf_s(buffer, "RS: Win32 Error (GetLastError): %d", GetLastError());
    const_cast<RenderThread *>(this)->AppendStatus(pMessage);
    const_cast<RenderThread *>(this)->AppendStatus(buffer);
}

void RenderThread::ReportOpenGlErrors(const char* pMessage) const
{
    const char* pErrorMessage = NULL;
    unsigned int errorCode = glGetError();

    if (GL_NO_ERROR != errorCode)
        pErrorMessage = ((const char*) gluGetString(errorCode));

    if (NULL == pErrorMessage)
        pErrorMessage = "None";

    char buffer[256] = { 0 };
    sprintf_s(buffer, "RS: OpenGL Error: %s", pErrorMessage);
    const_cast<RenderThread *>(this)->AppendStatus(pMessage);
    const_cast<RenderThread *>(this)->AppendStatus(buffer);
}
