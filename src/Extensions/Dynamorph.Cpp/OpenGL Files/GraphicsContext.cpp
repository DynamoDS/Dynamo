
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph::OpenGL;

GraphicsContext::GraphicsContext() : 
    mRenderWindow(nullptr),
    mhRenderContext(nullptr)
{
}

void GraphicsContext::InitializeCore(HWND hWndOwner)
{
    if (mhRenderContext != nullptr) {
        auto message = L"'GraphicsContext::InitializeCore' called twice";
        throw gcnew InvalidOperationException(gcnew String(message));
    }

    // TODO(Ben): This process of determining the pixel format and creation of 
    // corresponding render context is the most basic one and will likely fail 
    // on some machines. This should be updated to use more robust context creation
    // instead. Refer to "Proper Context Creation" in the following web page:
    // 
    //      http://www.opengl.org/wiki/Creating_an_OpenGL_Context_(WGL)
    // 
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

    mhRenderContext = ::wglCreateContext(hDeviceContext);
    ::wglMakeCurrent(hDeviceContext, mhRenderContext);

    ::ReleaseDC(mRenderWindow, hDeviceContext); // Done with device context.
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
}
