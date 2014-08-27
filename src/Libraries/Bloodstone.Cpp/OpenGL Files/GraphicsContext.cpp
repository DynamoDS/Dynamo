
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamo::Bloodstone;
using namespace Dynamo::Bloodstone::OpenGL;

// Legacy OpenGL APIs.
INITGLPROC(PFNGLBINDTEXTUREPROC,                 glBindTexture);
INITGLPROC(PFNGLCLEARPROC,                       glClear);
INITGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);
INITGLPROC(PFNGLDISABLEPROC,                     glDisable);
INITGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
INITGLPROC(PFNGLENABLEPROC,                      glEnable);
INITGLPROC(PFNGLGENTEXTURESPROC,                 glGenTextures);
INITGLPROC(PFNGLGETINTEGERVPROC,                 glGetIntegerv);
INITGLPROC(PFNGLGETSTRINGPROC,                   glGetString);
INITGLPROC(PFNGLPOINTSIZEPROC,                   glPointSize);
INITGLPROC(PFNGLPOLYGONMODEPROC,                 glPolygonMode);
INITGLPROC(PFNGLTEXIMAGE2DPROC,                  glTexImage2D);
INITGLPROC(PFNGLTEXPARAMETERFPROC,               glTexParameterf);
INITGLPROC(PFNGLTEXPARAMETERIPROC,               glTexParameteri);
INITGLPROC(PFNGLVIEWPORTPROC,                    glViewport);

// Modern OpenGL APIs.
INITGLPROC(PFNGLACTIVETEXTUREPROC,               glActiveTexture);
INITGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
INITGLPROC(PFNGLBINDATTRIBLOCATIONPROC,          glBindAttribLocation);
INITGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
INITGLPROC(PFNGLBINDVERTEXARRAYPROC,             glBindVertexArray);
INITGLPROC(PFNGLBLENDEQUATIONSEPARATEPROC,       glBlendEquationSeparate);
INITGLPROC(PFNGLBLENDFUNCSEPARATEPROC,           glBlendFuncSeparate);
INITGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
INITGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
INITGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
INITGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
INITGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
INITGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
INITGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
INITGLPROC(PFNGLDELETEVERTEXARRAYSPROC,          glDeleteVertexArrays);
INITGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
INITGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
INITGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
INITGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
INITGLPROC(PFNGLGENVERTEXARRAYSPROC,             glGenVertexArrays);
INITGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
INITGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
INITGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
INITGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
INITGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
INITGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
INITGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
INITGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
INITGLPROC(PFNGLUNIFORM1FPROC,                   glUniform1f);
INITGLPROC(PFNGLUNIFORM1IPROC,                   glUniform1i);
INITGLPROC(PFNGLUNIFORM2FPROC,                   glUniform2f);
INITGLPROC(PFNGLUNIFORM2IPROC,                   glUniform2i);
INITGLPROC(PFNGLUNIFORM3FPROC,                   glUniform3f);
INITGLPROC(PFNGLUNIFORM3IPROC,                   glUniform3i);
INITGLPROC(PFNGLUNIFORM4FPROC,                   glUniform4f);
INITGLPROC(PFNGLUNIFORM4IPROC,                   glUniform4i);
INITGLPROC(PFNGLUNIFORMMATRIX4FVPROC,            glUniformMatrix4fv);
INITGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
INITGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
INITGLPROC(PFNWGLCHOOSEPIXELFORMATARBPROC,       wglChoosePixelFormatARB);
INITGLPROC(PFNWGLCREATECONTEXTATTRIBSARBPROC,    wglCreateContextAttribsARB);

GraphicsContext::GraphicsContext() : 
    mMajorVersion(0),
    mMinorVersion(0),
    mRenderWindow(nullptr),
    mhRenderContext(nullptr),
    mpDefaultCamera(nullptr)
{
}

bool GraphicsContext::InitializeCore(HWND hWndOwner)
{
    if (mhRenderContext != nullptr) {
        auto message = L"'GraphicsContext::InitializeCore' called twice";
        throw gcnew InvalidOperationException(gcnew String(message));
    }

    if (InitializeWithDummyContext(hWndOwner) == false)
        return false; // Context creation failed.

    HDC hDeviceContext = ::GetDC(hWndOwner);
    if (SelectBestPixelFormat(hDeviceContext) == false) {
        ::ReleaseDC(hWndOwner, hDeviceContext);
        return false;
    }

    int attributes[] =
    {
        WGL_CONTEXT_MAJOR_VERSION_ARB, mMajorVersion,
        WGL_CONTEXT_MINOR_VERSION_ARB, mMinorVersion, 
        WGL_CONTEXT_FLAGS_ARB, WGL_CONTEXT_FORWARD_COMPATIBLE_BIT_ARB,
        WGL_CONTEXT_PROFILE_MASK_ARB, WGL_CONTEXT_CORE_PROFILE_BIT_ARB,
        0
    };

    mhRenderContext = GL::wglCreateContextAttribsARB(hDeviceContext, 0, attributes);
    wglMakeCurrent(hDeviceContext, mhRenderContext);

    ::ReleaseDC(hWndOwner, hDeviceContext); // Done with device context.
    mRenderWindow = hWndOwner;

    // Create the default camera.
    mpDefaultCamera = new Camera(this);

    // Default states of our renderer.
    GL::glEnable(GL_DEPTH_TEST);
    GL::glPointSize(4.0f);
    return true;
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

void GraphicsContext::GetDisplayPixelSizeCore(int& width, int& height) const
{
    width = height = 0;

    if (::IsWindow(this->mRenderWindow)) {
        RECT rcClient;
        ::GetClientRect(this->mRenderWindow, &rcClient);
        width  = rcClient.right - rcClient.left;
        height = rcClient.bottom - rcClient.top;
    }
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

IShaderProgram* GraphicsContext::CreateShaderProgramCore(ShaderName shaderName) const
{
    GetResourceIdentifiersParam params;
    params.openGlVersion = GetOpenGLVersion(mMajorVersion, mMinorVersion);
    params.shaderName = shaderName;
    GetResourceIdentifiers(&params);

    std::string vs, fs;
    Utils::LoadShaderResource(params.vertexShaderId, vs);
    Utils::LoadShaderResource(params.fragmentShaderId, fs);

    // Create shaders and their program.
    auto pvs = dynamic_cast<VertexShader *>(this->CreateVertexShader(vs));
    auto pfs = dynamic_cast<FragmentShader *>(this->CreateFragmentShader(fs));
    return new ShaderProgram(pvs, pfs);
}

IVertexBuffer* GraphicsContext::CreateVertexBufferCore(void) const
{
    return new VertexBuffer();
}

IBillboardVertexBuffer* GraphicsContext::CreateBillboardVertexBufferCore(void) const
{
    return new BillboardVertexBuffer(this);
}

void GraphicsContext::BeginRenderFrameCore(HDC deviceContext) const
{
    RECT rcClient;
    ::GetClientRect(this->mRenderWindow, &rcClient);

    GL::glViewport(0, 0, rcClient.right, rcClient.bottom);
    GL::glClearColor(0.941176f, 0.941176f, 0.941176f, 1.0f); // #F0F0F0
    GL::glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);

    // If the camera is animating, this is the right time to update it.
    if (mpDefaultCamera->IsInTransition())
        mpDefaultCamera->UpdateFrame();
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

bool GraphicsContext::EndRenderFrameCore(HDC deviceContext) const
{
    ::SwapBuffers(deviceContext);
    return mpDefaultCamera->IsInTransition(); // Request frame update if needed.
}

void GraphicsContext::EnableAlphaBlendCore(void) const
{
    GL::glEnable(GL_BLEND);
    GL::glBlendEquationSeparate(GL_FUNC_ADD, GL_FUNC_ADD);
    GL::glBlendFuncSeparate(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA, GL_ONE, GL_ZERO);
}

void GraphicsContext::ClearDepthBufferCore(void) const
{
    GL::glClear(GL_DEPTH_BUFFER_BIT);
}

bool GraphicsContext::InitializeWithDummyContext(HWND hWndOwner)
{
    wchar_t wndClassName[128] = { 0 };
    ::GetClassName(hWndOwner, wndClassName, _countof(wndClassName));
    HWND hWndParent = ::GetParent(hWndOwner);

    HWND hWndTemporary = CreateWindowEx(0, wndClassName, nullptr,
        WS_CHILD, 0, 0, 100, 100, hWndParent, nullptr, nullptr, 0);

    if (nullptr == hWndTemporary)
        return false;

    PIXELFORMATDESCRIPTOR descriptor = { 0 };
    descriptor.nSize = sizeof(PIXELFORMATDESCRIPTOR);
    descriptor.nVersion = 1;
    descriptor.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
    descriptor.iPixelType = PFD_TYPE_RGBA;
    descriptor.cColorBits = 32;
    descriptor.cDepthBits = 24;
    descriptor.cStencilBits = 8;
    descriptor.iLayerType = PFD_MAIN_PLANE;

    bool contextCreated = false;

    // Get the best available match of pixel format for the device context
    HDC hDeviceContext = ::GetDC(hWndTemporary);
    int format = ::ChoosePixelFormat(hDeviceContext, &descriptor);
    if (::SetPixelFormat(hDeviceContext, format, &descriptor))
    {
        HGLRC tempContext = ::wglCreateContext(hDeviceContext);
        if (tempContext != nullptr)
        {
            ::wglMakeCurrent(hDeviceContext, tempContext);

            if (GL::Initialize()) // Initialize OpenGL extension.
            {
                // Determine the current OpenGL version numbers.
                GL::glGetIntegerv(GL_MAJOR_VERSION, &mMajorVersion);
                GL::glGetIntegerv(GL_MINOR_VERSION, &mMinorVersion);
                contextCreated = true;
            }

            wglMakeCurrent(hDeviceContext, nullptr);
            wglDeleteContext(tempContext); // Discard temporary context.
        }
    }

    ::ReleaseDC(hWndTemporary, hDeviceContext); // Done with device context.
    ::DestroyWindow(hWndTemporary);
    return contextCreated;
}

bool GraphicsContext::SelectBestPixelFormat(HDC hDeviceContext) const
{
    int hardwareLevel = 0; // Start from the best hardware specs.

    while (true)
    {
        int deviceAttributes[100] = { 0 };
        if (!GetDeviceAttributes(hardwareLevel++, &deviceAttributes[0]))
            break; // No more device attributes to try out.

        int pixelFormat = 0;
        unsigned int formatCount = 0;
        int formatFound = GL::wglChoosePixelFormatARB(hDeviceContext,
            deviceAttributes, nullptr, 1, &pixelFormat, &formatCount);

        if (formatFound == 0)
            continue;

        PIXELFORMATDESCRIPTOR descriptor = { 0 };
        ::DescribePixelFormat(hDeviceContext, pixelFormat,
            sizeof(PIXELFORMATDESCRIPTOR), &descriptor);

        if (::SetPixelFormat(hDeviceContext, pixelFormat, &descriptor))
            return true; // Pixel format successfully set!
    }

    return false;
}

bool GraphicsContext::GetDeviceAttributes(int hardwareLevel, int* pAttributes) const
{
    // Standard device feature set.
    pAttributes[0] = WGL_DRAW_TO_WINDOW_ARB;
    pAttributes[1] = GL_TRUE;
    pAttributes[2] = WGL_SUPPORT_OPENGL_ARB;
    pAttributes[3] = GL_TRUE;
    pAttributes[4] = WGL_DOUBLE_BUFFER_ARB;
    pAttributes[5] = GL_TRUE;
    pAttributes[6] = WGL_PIXEL_TYPE_ARB;
    pAttributes[7] = WGL_TYPE_RGBA_ARB;
    pAttributes[8] = WGL_COLOR_BITS_ARB;
    pAttributes[9] = 32;
    pAttributes[10] = WGL_DEPTH_BITS_ARB;
    pAttributes[11] = 24;
    pAttributes[12] = WGL_STENCIL_BITS_ARB;
    pAttributes[13] = 8;

    switch(hardwareLevel)
    {
    case 0: // Best in class hardware, 8x MSAA.
        pAttributes[14] = WGL_SAMPLE_BUFFERS_ARB;
        pAttributes[15] = GL_TRUE;
        pAttributes[16] = WGL_SAMPLES_ARB;
        pAttributes[17] = 8;
        return true;
    case 1: // Second grade hardware, try 4x MSAA.
        pAttributes[14] = WGL_SAMPLE_BUFFERS_ARB;
        pAttributes[15] = GL_TRUE;
        pAttributes[16] = WGL_SAMPLES_ARB;
        pAttributes[17] = 4;
        return true;
    case 2: // Third grade hardware...
        return true; // ... try standard feature set.
    }

    return false;
}
