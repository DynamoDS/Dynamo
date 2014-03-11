
#include "stdafx.h"
#include "Internal.h"

using namespace DesignScriptStudio::Renderer;

std::string OpenGL::mStatusString = "";
const wchar_t* OpenGL::kInitializationMutex = L"OpenGLInitializationMutex";

bool OpenGL::Initialize(HDC hFrameBufferDC)
{
    if (NULL != OpenGL::MakeContextCurrent)
        return true; // Already initialized.

    mStatusString = "\nInitializing OpenGL...\n"; // Reset status messages.
    PFNWGLGETEXTENSIONSSTRINGARBPROC pfnwglGetExtensionsStringARB = 
        ((PFNWGLGETEXTENSIONSSTRINGARBPROC) wglGetProcAddress("wglGetExtensionsStringARB"));

    if (NULL == pfnwglGetExtensionsStringARB) {
        AppendStatus("'wglGetProcAddress(wglGetExtensionsStringARB)' failed!");
        return false;
    }

    const char* pExtensions = pfnwglGetExtensionsStringARB(hFrameBufferDC);
    if (ValidateExtensionStrings(pExtensions) == false)
        return false;

#if _DEBUG
    OutputDebugStringA("Extensions from 'glGetString(GL_EXTENSIONS)':\n");
    PrintExtensionStrings((const char*) glGetString(GL_EXTENSIONS));
    OutputDebugStringA("Extensions from 'wglGetExtensionsStringARB(HDC)':\n");
    PrintExtensionStrings(pExtensions);
#endif

    MakeContextCurrent      = (PFNWGLMAKECONTEXTCURRENTARBPROC)  wglGetProcAddress("wglMakeContextCurrentARB");
    ChoosePixelFormat       = (PFNWGLCHOOSEPIXELFORMATARBPROC)   wglGetProcAddress("wglChoosePixelFormatARB");
    CreatePbuffer           = (PFNWGLCREATEPBUFFERARBPROC)       wglGetProcAddress("wglCreatePbufferARB");
    DestroyPbuffer          = (PFNWGLDESTROYPBUFFERARBPROC)      wglGetProcAddress("wglDestroyPbufferARB");
    GetPbufferDC            = (PFNWGLGETPBUFFERDCARBPROC)        wglGetProcAddress("wglGetPbufferDCARB");
    ReleasePbufferDC        = (PFNWGLRELEASEPBUFFERDCARBPROC)    wglGetProcAddress("wglReleasePbufferDCARB");
    QueryPbuffer            = (PFNWGLQUERYPBUFFERARBPROC)        wglGetProcAddress("wglQueryPbufferARB");
    BindTexImage            = (PFNWGLBINDTEXIMAGEARBPROC)        wglGetProcAddress("wglBindTexImageARB");
    ReleaseTexImage         = (PFNWGLRELEASETEXIMAGEARBPROC)     wglGetProcAddress("wglReleaseTexImageARB");
    SetPbufferAttrib        = (PFNWGLSETPBUFFERATTRIBARBPROC)    wglGetProcAddress("wglSetPbufferAttribARB");

    GenBuffers              = (PFNGLGENBUFFERSPROC)              wglGetProcAddress("glGenBuffers");
    DeleteBuffers           = (PFNGLDELETEBUFFERSPROC)           wglGetProcAddress("glDeleteBuffers");
    IsBuffer                = (PFNGLISBUFFERPROC)                wglGetProcAddress("glIsBuffer");
    BindBuffer              = (PFNGLBINDBUFFERPROC)              wglGetProcAddress("glBindBuffer");
    BufferData              = (PFNGLBUFFERDATAPROC)              wglGetProcAddress("glBufferData");
    BufferSubData           = (PFNGLBUFFERSUBDATAPROC)           wglGetProcAddress("glBufferSubData");
    GenRenderbuffers        = (PFNGLGENRENDERBUFFERSPROC)        wglGetProcAddress("glGenRenderbuffers");
    BindRenderbuffer        = (PFNGLBINDRENDERBUFFERPROC)        wglGetProcAddress("glBindRenderbuffer");
    RenderbufferStorage     = (PFNGLRENDERBUFFERSTORAGEPROC)     wglGetProcAddress("glRenderbufferStorage");
    GenFramebuffers         = (PFNGLGENFRAMEBUFFERSPROC)         wglGetProcAddress("glGenFramebuffers");
    BindFramebuffer         = (PFNGLBINDFRAMEBUFFERPROC)         wglGetProcAddress("glBindFramebuffer");
    FramebufferRenderbuffer = (PFNGLFRAMEBUFFERRENDERBUFFERPROC) wglGetProcAddress("glFramebufferRenderbuffer");
    DeleteFramebuffers      = (PFNGLDELETEFRAMEBUFFERSPROC)      wglGetProcAddress("glDeleteFramebuffers");
    DeleteRenderbuffers     = (PFNGLDELETERENDERBUFFERSPROC)     wglGetProcAddress("glDeleteRenderbuffers");
    CheckFramebufferStatus  = (PFNGLCHECKFRAMEBUFFERSTATUSPROC)  wglGetProcAddress("glCheckFramebufferStatus");

    ENSURE_FUNCTION_POINTER_VALID(MakeContextCurrent);
    ENSURE_FUNCTION_POINTER_VALID(ChoosePixelFormat);
    ENSURE_FUNCTION_POINTER_VALID(CreatePbuffer);
    ENSURE_FUNCTION_POINTER_VALID(DestroyPbuffer);
    ENSURE_FUNCTION_POINTER_VALID(GetPbufferDC);
    ENSURE_FUNCTION_POINTER_VALID(ReleasePbufferDC);
    ENSURE_FUNCTION_POINTER_VALID(QueryPbuffer);
    ENSURE_FUNCTION_POINTER_VALID(BindTexImage);
    ENSURE_FUNCTION_POINTER_VALID(ReleaseTexImage);
    ENSURE_FUNCTION_POINTER_VALID(SetPbufferAttrib);

    ENSURE_FUNCTION_POINTER_VALID(GenBuffers);
    ENSURE_FUNCTION_POINTER_VALID(DeleteBuffers);
    ENSURE_FUNCTION_POINTER_VALID(IsBuffer);
    ENSURE_FUNCTION_POINTER_VALID(BindBuffer);
    ENSURE_FUNCTION_POINTER_VALID(BufferData);
    ENSURE_FUNCTION_POINTER_VALID(BufferSubData);
    ENSURE_FUNCTION_POINTER_VALID(GenRenderbuffers);
    ENSURE_FUNCTION_POINTER_VALID(BindRenderbuffer);
    ENSURE_FUNCTION_POINTER_VALID(RenderbufferStorage);
    ENSURE_FUNCTION_POINTER_VALID(GenFramebuffers);
    ENSURE_FUNCTION_POINTER_VALID(BindFramebuffer);
    ENSURE_FUNCTION_POINTER_VALID(FramebufferRenderbuffer);
    ENSURE_FUNCTION_POINTER_VALID(DeleteFramebuffers);
    ENSURE_FUNCTION_POINTER_VALID(DeleteRenderbuffers);
    ENSURE_FUNCTION_POINTER_VALID(CheckFramebufferStatus);

    AppendStatus("OpenGL initialization successful.");
    return true;
}

bool OpenGL::IsInitialized(void)
{
    return (NULL != OpenGL::MakeContextCurrent);
}

const void* OpenGL::GetErrorString(void)
{
    return (mStatusString.c_str());
}

bool OpenGL::ConstructFrameBuffer(HDC hFrameBufferDC, HGLRC* phFrameBufferRC, std::string& error)
{
    if (NULL == hFrameBufferDC || (NULL == phFrameBufferRC))
        return false;

    error.clear();
    *phFrameBufferRC = NULL; // Reset output argument.

    // Automatically find the default pixel format.
    PIXELFORMATDESCRIPTOR descriptor;
    ZeroMemory(&descriptor, sizeof(PIXELFORMATDESCRIPTOR));
    descriptor.nSize = sizeof(PIXELFORMATDESCRIPTOR);
    descriptor.nVersion = 1;
    descriptor.iPixelType = PFD_TYPE_RGBA;
    descriptor.cColorBits = 32;
    descriptor.cDepthBits = 24;
    descriptor.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
    descriptor.iLayerType = PFD_MAIN_PLANE;

    int pixelFormat = ::ChoosePixelFormat(hFrameBufferDC, &descriptor);
    if (0 == pixelFormat) {
        error.assign("'ChoosePixelFormat' failed!");
        return false;
    }

    if (0 == ::SetPixelFormat(hFrameBufferDC, pixelFormat, &descriptor)) {
        error.assign("'SetPixelFormat' failed!");
        return false;
    }

    HGLRC hFrameBufferRC = wglCreateContext(hFrameBufferDC);
    if (NULL == hFrameBufferRC)
        return false;

    *phFrameBufferRC = hFrameBufferRC; // Return to the caller.
    return (wglMakeCurrent(hFrameBufferDC, hFrameBufferRC) != FALSE);
}

void OpenGL::PrintExtensionStrings(const char* pExtensions)
{
#if _DEBUG

    if (NULL == pExtensions || (pExtensions[0] == 0x00))
        return;

    size_t characters = strlen(pExtensions) + 1;
    char* pBuffer = new char[characters];
    strcpy_s(pBuffer, characters, pExtensions);

    char* nextToken = NULL;
    char* pToken = strtok_s(pBuffer, " ", &nextToken);

    while (NULL != pToken)
    {
        pToken = strtok_s(NULL, " ", &nextToken);
        if (NULL != pToken)
            OutputDebugStringA(pToken);
    }

    delete [] pBuffer;

#endif
}

bool OpenGL::ValidateExtensionStrings(const char* pExtensions)
{
    if (NULL == pExtensions || (pExtensions[0] == 0x00))
        return false;

    // The following specification detailed the new Framebuffer Object (FBO) in OpenGL 3.0 
    // specs (2008). Search for "ARB_pbuffer" and find the benefits of the new 
    // "ARB_framebuffer_object" has over "ARB_pbuffer" and "ARB_render_texture" approach.
    // 
    // http://www.opengl.org/registry/specs/ARB/framebuffer_object.txt
    // 
    char* extensions[] = 
    {
        "WGL_ARB_pbuffer",
        "WGL_ARB_pixel_format",
        "WGL_ARB_make_current_read",
        "WGL_ARB_render_texture",
        "GL_ARB_framebuffer_object",
        "GL_EXT_framebuffer_object",
    };

    for (int index = 0; index < _countof(extensions); ++index)
    {
        if (strstr(pExtensions, extensions[index]) == NULL)
        {
            AppendStatus("Warning: Extension not found:");
            AppendStatus(extensions[index]);
        }
    }

    return true;
}

void OpenGL::AppendStatus(const char* pMessage)
{
    if (NULL != pMessage) {
        mStatusString += pMessage;
        mStatusString += "\n";
    }
}

// Windows specific OpenGL APIs
PFNWGLMAKECONTEXTCURRENTARBPROC  OpenGL::MakeContextCurrent         = NULL;
PFNWGLCHOOSEPIXELFORMATARBPROC   OpenGL::ChoosePixelFormat          = NULL;
PFNWGLCREATEPBUFFERARBPROC       OpenGL::CreatePbuffer              = NULL;
PFNWGLDESTROYPBUFFERARBPROC      OpenGL::DestroyPbuffer             = NULL;
PFNWGLGETPBUFFERDCARBPROC        OpenGL::GetPbufferDC               = NULL;
PFNWGLRELEASEPBUFFERDCARBPROC    OpenGL::ReleasePbufferDC           = NULL;
PFNWGLQUERYPBUFFERARBPROC        OpenGL::QueryPbuffer               = NULL;
PFNWGLBINDTEXIMAGEARBPROC        OpenGL::BindTexImage               = NULL;
PFNWGLRELEASETEXIMAGEARBPROC     OpenGL::ReleaseTexImage            = NULL;
PFNWGLSETPBUFFERATTRIBARBPROC    OpenGL::SetPbufferAttrib           = NULL;

// General OpenGL APIs
PFNGLGENBUFFERSPROC              OpenGL::GenBuffers                 = NULL;
PFNGLDELETEBUFFERSPROC           OpenGL::DeleteBuffers              = NULL;
PFNGLISBUFFERPROC                OpenGL::IsBuffer                   = NULL;
PFNGLBINDBUFFERPROC              OpenGL::BindBuffer                 = NULL;
PFNGLBUFFERDATAPROC              OpenGL::BufferData                 = NULL;
PFNGLBUFFERSUBDATAPROC           OpenGL::BufferSubData              = NULL;
PFNGLGENRENDERBUFFERSPROC        OpenGL::GenRenderbuffers           = NULL;
PFNGLBINDRENDERBUFFERPROC        OpenGL::BindRenderbuffer           = NULL;
PFNGLRENDERBUFFERSTORAGEPROC     OpenGL::RenderbufferStorage        = NULL;
PFNGLGENFRAMEBUFFERSPROC         OpenGL::GenFramebuffers            = NULL;
PFNGLBINDFRAMEBUFFERPROC         OpenGL::BindFramebuffer            = NULL;
PFNGLFRAMEBUFFERRENDERBUFFERPROC OpenGL::FramebufferRenderbuffer    = NULL;
PFNGLDELETEFRAMEBUFFERSPROC      OpenGL::DeleteFramebuffers         = NULL;
PFNGLDELETERENDERBUFFERSPROC     OpenGL::DeleteRenderbuffers        = NULL;
PFNGLCHECKFRAMEBUFFERSTATUSPROC  OpenGL::CheckFramebufferStatus     = NULL;
