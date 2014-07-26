
#include "stdafx.h"
#include "Bloodstone.h"
#include "Utilities.h"
#include "NodeSceneData.h"
#include "Resources\resource.h"

#include <msclr/marshal_cppstd.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Dynamo::Bloodstone;
using namespace Autodesk::DesignScript::Interfaces;

LRESULT _stdcall LocalWndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    return VisualizerWnd::WndProc(hWnd, msg, wParam, lParam);
}

System::IntPtr VisualizerWnd::Create(System::IntPtr hWndParent, int width, int height)
{
    if (VisualizerWnd::mVisualizer == nullptr)
    {
        auto hParent = ((HWND) hWndParent.ToPointer());
        VisualizerWnd::mVisualizer = gcnew VisualizerWnd();
        if (VisualizerWnd::mVisualizer->Initialize(hParent, width, height))
            VisualizerWnd::mVisualizer->mGraphicsContextCreated = true;
    }

    return System::IntPtr(VisualizerWnd::mVisualizer->GetWindowHandle());
}

void VisualizerWnd::Destroy(void)
{
    if (VisualizerWnd::mVisualizer != nullptr) {
        VisualizerWnd::mVisualizer->Uninitialize();
        VisualizerWnd::mVisualizer = nullptr;
    }
}

VisualizerWnd^ VisualizerWnd::CurrentInstance(void)
{
    return VisualizerWnd::mVisualizer;
}

LRESULT VisualizerWnd::WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    if (VisualizerWnd::mVisualizer == nullptr)
        return ::DefWindowProc(hWnd, msg, wParam, lParam);

    return VisualizerWnd::mVisualizer->ProcessMessage(hWnd, msg, wParam, lParam);
}

bool VisualizerWnd::IsGraphicsContextCreated(void)
{
    return this->mGraphicsContextCreated;
}

void VisualizerWnd::ShowWindow(bool show)
{
    if (::IsWindow(this->mhWndVisualizer)) {
        auto cmd = show ? SW_SHOW : SW_HIDE;
        ::ShowWindow(this->mhWndVisualizer, cmd);
    }
}

void VisualizerWnd::RequestFrameUpdate(void)
{
    ::InvalidateRect(this->mhWndVisualizer, nullptr, true); // Update window.
}

HWND VisualizerWnd::GetWindowHandle(void)
{
    return this->mhWndVisualizer;
}

Scene^ VisualizerWnd::GetScene(void)
{
    return this->mpScene;
}

IGraphicsContext* VisualizerWnd::GetGraphicsContext(void)
{
    return this->mpGraphicsContext;
}

VisualizerWnd::VisualizerWnd() : 
    mGraphicsContextCreated(false),
    mhWndVisualizer(nullptr),
    mpScene(nullptr),
    mpGraphicsContext(nullptr)
{
}

bool VisualizerWnd::Initialize(HWND hWndParent, int width, int height)
{
    if (mhWndVisualizer != nullptr) {
        auto message = L"'VisualizerWnd::Initialize' cannot be called twice.";
        throw gcnew System::InvalidOperationException(gcnew String(message));
    }

    WNDCLASSEX windowClass = { 0 };

    // The reason we create our own window class is that we need CS_OWNDC style 
    // to be specified for the window we are creating (OpenGL creation needs it).
    // 
    windowClass.cbSize = sizeof(windowClass);
    windowClass.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC | CS_DBLCLKS;
    windowClass.lpfnWndProc = LocalWndProc;
    windowClass.hCursor = ::LoadCursor(nullptr, IDC_ARROW);
    windowClass.hbrBackground = ::CreateSolidBrush(RGB(100, 149, 237));
    windowClass.lpszClassName = L"VisualizerClass";
    RegisterClassEx(&windowClass);

    mhWndVisualizer = CreateWindowEx(0, windowClass.lpszClassName, nullptr,
        WS_CHILD, 0, 0, width, height, hWndParent, nullptr, nullptr, 0);

    // Initialize graphics context for rendering.
    auto contextType = IGraphicsContext::ContextType::OpenGL;
    mpGraphicsContext = IGraphicsContext::Create(contextType);
    if (mpGraphicsContext->Initialize(mhWndVisualizer) == false)
        return false;

    mpScene = gcnew Scene(this);
    mpScene->Initialize(width, height);
    return true;
}

void VisualizerWnd::Uninitialize(void)
{
    if (this->mpScene != nullptr) {
        this->mpScene->Destroy();
        delete this->mpScene;
        this->mpScene = nullptr;
    }

    if (this->mpGraphicsContext != nullptr) {
        this->mpGraphicsContext->Uninitialize();
        this->mpGraphicsContext = nullptr;
    }

    if (this->mhWndVisualizer != nullptr) {
        ::DestroyWindow(this->mhWndVisualizer);
        this->mhWndVisualizer = nullptr;
    }
}

LRESULT VisualizerWnd::ProcessMouseMessage(UINT msg, WPARAM wParam, LPARAM lParam)
{
    auto x = GET_X_LPARAM(lParam);
    auto y = GET_Y_LPARAM(lParam);

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    auto pTrackBall = pCamera->GetTrackBall();

    switch (msg)
    {
    case WM_LBUTTONDBLCLK:
        {
            BoundingBox boundingBox;
            mpScene->GetBoundingBox(boundingBox);

            CameraConfiguration configuration;
            pCamera->GetConfiguration(&configuration);
            configuration.FitToBoundingBox(boundingBox);
            pCamera->BeginConfigure(&configuration);
            break;
        }

    case WM_LBUTTONDOWN:
        SetCapture(this->mhWndVisualizer);
        pTrackBall->MousePressed(x, y, ITrackBall::Mode::Rotate);
        break;

    case WM_RBUTTONDOWN:
        SetCapture(this->mhWndVisualizer);
        pTrackBall->MousePressed(x, y, ITrackBall::Mode::Zoom);
        break;

    case WM_MBUTTONDOWN:
        SetCapture(this->mhWndVisualizer);
        pTrackBall->MousePressed(x, y, ITrackBall::Mode::Pan);
        break;

    case WM_LBUTTONUP:
    case WM_RBUTTONUP:
    case WM_MBUTTONUP:
        pTrackBall->MouseReleased(x, y);
        ::ReleaseCapture();
        break;

    case WM_MOUSEMOVE:
        if ((wParam & (MK_LBUTTON | MK_RBUTTON | MK_MBUTTON)) == 0)
            return 0L; // Mouse button isn't pressed.

        pTrackBall->MouseMoved(x, y);
        break;
    }

    RequestFrameUpdate(); // Update window.
    return 0L; // Message processed.
}

LRESULT VisualizerWnd::ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    if (this->mGraphicsContextCreated == false)
        return ::DefWindowProc(hWnd, msg, wParam, lParam);

    switch (msg)
    {
    case WM_PAINT:
        {
            PAINTSTRUCT ps;
            bool requestFrameUpdate = false;
            HDC deviceContext = BeginPaint(hWnd, &ps);
            {
                mpGraphicsContext->BeginRenderFrame(deviceContext);
                mpScene->RenderScene();
                if (mpGraphicsContext->EndRenderFrame(deviceContext))
                    requestFrameUpdate = true;
            }
            EndPaint(hWnd, &ps);

            // If further frame is required.
            if (requestFrameUpdate != false)
                this->RequestFrameUpdate();

            return 0L;
        }

    case WM_MOUSEMOVE:
    case WM_LBUTTONDOWN:
    case WM_LBUTTONUP:
    case WM_RBUTTONDOWN:
    case WM_RBUTTONUP:
    case WM_MBUTTONDOWN:
    case WM_MBUTTONUP:
    case WM_LBUTTONDBLCLK:
        return ProcessMouseMessage(msg, wParam, lParam);

    case WM_ERASEBKGND:
        return 0L; // Avoid erasing background to flickering during sizing.

    case WM_SIZE:
        {
            if (mpGraphicsContext != nullptr)
            {
                auto pCamera = mpGraphicsContext->GetDefaultCamera();
                if (pCamera != nullptr) {
                    pCamera->ResizeViewport(LOWORD(lParam), HIWORD(lParam));
                    return 0L; // Message processed.
                }
            }
            break;
        }
    }

    return ::DefWindowProc(hWnd, msg, wParam, lParam);
}
