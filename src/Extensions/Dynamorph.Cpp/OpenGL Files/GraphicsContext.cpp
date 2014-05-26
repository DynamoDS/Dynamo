
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;
using namespace Dynamorph::OpenGL;

INITGLPROC(PFNGLGETSTRINGPROC,                  glGetString);
INITGLPROC(PFNGLGETINTEGERVPROC,                glGetIntegerv);
INITGLPROC(PFNGLENABLEPROC,                     glEnable);
INITGLPROC(PFNGLDISABLEPROC,                    glDisable);
INITGLPROC(PFNGLPOLYGONMODEPROC,                glPolygonMode);
INITGLPROC(PFNGLCREATESHADERPROC,               glCreateShader);
INITGLPROC(PFNGLSHADERSOURCEPROC,               glShaderSource);
INITGLPROC(PFNGLCOMPILESHADERPROC,              glCompileShader);
INITGLPROC(PFNGLGETSHADERIVPROC,                glGetShaderiv);
INITGLPROC(PFNGLGETSHADERINFOLOGPROC,           glGetShaderInfoLog);
INITGLPROC(PFNGLCREATEPROGRAMPROC,              glCreateProgram);
INITGLPROC(PFNGLDELETEPROGRAMPROC,              glDeleteProgram);
INITGLPROC(PFNGLATTACHSHADERPROC,               glAttachShader);
INITGLPROC(PFNGLDETACHSHADERPROC,               glDetachShader);
INITGLPROC(PFNGLBINDATTRIBLOCATIONPROC,         glBindAttribLocation);
INITGLPROC(PFNGLLINKPROGRAMPROC,                glLinkProgram);
INITGLPROC(PFNGLUSEPROGRAMPROC,                 glUseProgram);
INITGLPROC(PFNGLGETPROGRAMIVPROC,               glGetProgramiv);
INITGLPROC(PFNGLGETPROGRAMINFOLOGPROC,          glGetProgramInfoLog);
INITGLPROC(PFNGLDELETESHADERPROC,               glDeleteShader);
INITGLPROC(PFNGLGENBUFFERSPROC,                 glGenBuffers);
INITGLPROC(PFNGLDELETEBUFFERSPROC,              glDeleteBuffers);
INITGLPROC(PFNGLBUFFERDATAPROC,                 glBufferData);
INITGLPROC(PFNGLGENVERTEXARRAYSPROC,            glGenVertexArrays);
INITGLPROC(PFNGLDELETEVERTEXARRAYSPROC,         glDeleteVertexArrays);
INITGLPROC(PFNGLBINDVERTEXARRAYPROC,            glBindVertexArray);
INITGLPROC(PFNGLGETATTRIBLOCATIONPROC,          glGetAttribLocation);
INITGLPROC(PFNGLGETUNIFORMLOCATIONPROC,         glGetUniformLocation);
INITGLPROC(PFNGLUNIFORMMATRIX4FVPROC,           glUniformMatrix4fv);
INITGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,    glEnableVertexAttribArray);
INITGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,   glDisableVertexAttribArray);
INITGLPROC(PFNGLBINDBUFFERPROC,                 glBindBuffer);
INITGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,        glVertexAttribPointer);
INITGLPROC(PFNGLVIEWPORTPROC,                   glViewport);
INITGLPROC(PFNGLDRAWARRAYSPROC,                 glDrawArrays);
INITGLPROC(PFNGLCLEARPROC,                      glClear);
INITGLPROC(PFNGLCLEARCOLORPROC,                 glClearColor);

GraphicsContext::GraphicsContext() : 
    mRenderWindow(nullptr),
    mhRenderContext(nullptr),
    mpDefaultCamera(nullptr)
{
}

void GraphicsContext::InitializeCore(HWND hWndOwner)
{
    if (mhRenderContext != nullptr) {
        auto message = L"'GraphicsContext::InitializeCore' called twice";
        throw gcnew InvalidOperationException(gcnew String(message));
    }

    PIXELFORMATDESCRIPTOR descriptor = { 0 };
    descriptor.nSize = sizeof(PIXELFORMATDESCRIPTOR);
    descriptor.nVersion = 1;
    descriptor.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
    descriptor.iPixelType = PFD_TYPE_RGBA;
    descriptor.cColorBits = 32;
    descriptor.cDepthBits = 24;
    descriptor.cStencilBits = 8;
    descriptor.iLayerType = PFD_MAIN_PLANE;

    mRenderWindow = hWndOwner;
    HDC hDeviceContext = ::GetDC(mRenderWindow);

    // Get the best available match of pixel format for the device context   
    int format = ::ChoosePixelFormat(hDeviceContext, &descriptor);
    ::SetPixelFormat(hDeviceContext, format, &descriptor);

    HGLRC tempContext = ::wglCreateContext(hDeviceContext);
    ::wglMakeCurrent(hDeviceContext, tempContext);

    GL::Initialize(); // Initialize OpenGL extension.

    int major = -1, minor = -1;
    GL::glGetIntegerv(GL_MAJOR_VERSION, &major);
    GL::glGetIntegerv(GL_MINOR_VERSION, &minor);

    int attributes[] =
    {
        WGL_CONTEXT_MAJOR_VERSION_ARB, major,
        WGL_CONTEXT_MINOR_VERSION_ARB, minor, 
        WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
        0
    };

    auto proc = wglGetProcAddress("wglCreateContextAttribsARB");
    PFNWGLCREATECONTEXTATTRIBSARBPROC wglCreateContextAttribsARB = nullptr;
    wglCreateContextAttribsARB = ((PFNWGLCREATECONTEXTATTRIBSARBPROC) proc);

    if (wglCreateContextAttribsARB == nullptr)
    {
        mhRenderContext = tempContext;
    }
    else
    {
        mhRenderContext = wglCreateContextAttribsARB(hDeviceContext, 0, attributes);
        wglMakeCurrent(hDeviceContext, mhRenderContext);
        wglDeleteContext(tempContext); // Discard temporary context.
    }

    ::ReleaseDC(mRenderWindow, hDeviceContext); // Done with device context.

    // Create the default camera.
    mpDefaultCamera = new Camera();
}

void GraphicsContext::UninitializeCore(void)
{
    if (mhRenderContext == nullptr)
        return;

    HDC hDeviceContext = ::GetDC(mRenderWindow);
    ::wglMakeCurrent(hDeviceContext, nullptr);
    ::ReleaseDC(mRenderWindow, hDeviceContext); // Done with device context.

    // Now that the default context is reset, destroy the render context.
    ::wglDeleteContext(mhRenderContext);
    mhRenderContext = nullptr;

    if (mpDefaultCamera != nullptr) {
        delete mpDefaultCamera;
        mpDefaultCamera = nullptr;
    }
}

ICamera* GraphicsContext::GetDefaultCameraCore(void) const
{
    return mpDefaultCamera;
}

IVertexShader* GraphicsContext::CreateVertexShaderCore(const std::string& content) const
{
    VertexShader* pVertexShader = new VertexShader(this);
    pVertexShader->LoadFromContent(content);
    return pVertexShader;
}

IFragmentShader* GraphicsContext::CreateFragmentShaderCore(const std::string& content) const
{
    FragmentShader* pFragmentShader = new FragmentShader(this);
    pFragmentShader->LoadFromContent(content);
    return pFragmentShader;
}

IShaderProgram* GraphicsContext::CreateShaderProgramCore(
    IVertexShader* pVertexShader, IFragmentShader* pFragmentShader)
{
    auto pvs = dynamic_cast<VertexShader *>(pVertexShader);
    auto pfs = dynamic_cast<FragmentShader *>(pFragmentShader);
    ShaderProgram* pShaderProgram = new ShaderProgram(pvs, pfs);
    return pShaderProgram;
}

IVertexBuffer* GraphicsContext::CreateVertexBufferCore(void) const
{
    return new VertexBuffer();
}

void GraphicsContext::BeginRenderFrameCore(HDC deviceContext) const
{
    RECT rcClient;
    ::GetClientRect(this->mRenderWindow, &rcClient);

    GL::glViewport(0, 0, rcClient.right, rcClient.bottom);
    GL::glClearColor(0.5f, 1.0f, 0.5f, 1.0f);
    GL::glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
}

void GraphicsContext::ActivateShaderProgramCore(IShaderProgram* pShaderProgram) const
{
    ShaderProgram* pProgram = dynamic_cast<ShaderProgram *>(pShaderProgram);
    if (pProgram == nullptr)
        return;

    pProgram->Activate();
}

void GraphicsContext::RenderVertexBufferCore(IVertexBuffer* pVertexBuffer) const
{
    auto pBuffer = dynamic_cast<VertexBuffer *>(pVertexBuffer);
    if (pBuffer != nullptr)
        pBuffer->Render();
}

void GraphicsContext::EndRenderFrameCore(HDC deviceContext) const
{
    ::SwapBuffers(deviceContext);
}
