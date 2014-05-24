// Dynamorph.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Dynamorph.h"
#include "OpenGL Files\OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;

// ================================================================================
// IGraphicsContext
// ================================================================================

IGraphicsContext* IGraphicsContext::Create(IGraphicsContext::ContextType contextType)
{
    switch(contextType)
    {
    case IGraphicsContext::ContextType::OpenGL:
        return new Dynamorph::OpenGL::GraphicsContext();
    }

    auto message = L"Invalid value for 'IGraphicsContext::ContextType'";
    throw gcnew System::InvalidOperationException(gcnew String(message));
}

// ================================================================================
// Visualizer
// ================================================================================

System::IntPtr Visualizer::Create(System::IntPtr hWndParent, int width, int height)
{
    if (Visualizer::mVisualizer == nullptr)
    {
        auto hParent = ((HWND) hWndParent.ToPointer());
        Visualizer::mVisualizer = gcnew Visualizer();
        Visualizer::mVisualizer->Initialize(hParent, width, height);
    }

    return System::IntPtr(Visualizer::mVisualizer->GetWindowHandle());
}

void Visualizer::Destroy(void)
{
    if (Visualizer::mVisualizer != nullptr) {
        Visualizer::mVisualizer->Uninitialize();
        Visualizer::mVisualizer = nullptr;
    }
}

LRESULT Visualizer::WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    if (Visualizer::mVisualizer == nullptr)
        return ::DefWindowProc(hWnd, msg, wParam, lParam);

    return Visualizer::mVisualizer->ProcessMessage(hWnd, msg, wParam, lParam);
}

HWND Visualizer::GetWindowHandle(void)
{
    return this->mhWndVisualizer;
}

Visualizer::Visualizer() : 
    mhWndVisualizer(nullptr),
    mpShaderProgram(nullptr),
    mpVertexBuffer(nullptr),
    mpGraphicsContext(nullptr)
{
}

void Visualizer::Initialize(HWND hWndParent, int width, int height)
{
    if (mhWndVisualizer != nullptr) {
        auto message = L"'Visualizer::Initialize' cannot be called twice.";
        throw gcnew System::InvalidOperationException(gcnew String(message));
    }

    WNDCLASSEX windowClass = { 0 };

    // The reason we create our own window class is that we need CS_OWNDC style 
    // to be specified for the window we are creating (OpenGL creation needs it).
    // 
    windowClass.cbSize = sizeof(windowClass);
    windowClass.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
    windowClass.lpfnWndProc = ((WNDPROC) Visualizer::WndProc);
    windowClass.hCursor = ::LoadCursor(nullptr, IDC_ARROW);
    windowClass.hbrBackground = ::CreateSolidBrush(RGB(100, 149, 237));
    windowClass.lpszClassName = L"VisualizerClass";
    RegisterClassEx(&windowClass);

    mhWndVisualizer = CreateWindowEx(0, windowClass.lpszClassName, nullptr,
        WS_CHILD | WS_VISIBLE, 0, 0, width, height, hWndParent, nullptr, nullptr, 0);

    // Initialize graphics context for rendering.
    auto contextType = IGraphicsContext::ContextType::OpenGL;
    mpGraphicsContext = IGraphicsContext::Create(contextType);
    mpGraphicsContext->Initialize(mhWndVisualizer);

    std::string vs(
        "#version 150 core                              \n"
        "in vec3 in_position;                           \n"
        "void main(void)                                \n"
        "{                                              \n"
        "    gl_Position = vec4(in_position, 1.0);      \n"
        "}                                              \n");

    std::string fs(
        "#version 150 core                              \n"
        "out vec4 out_Color;                            \n"
        "void main(void)                                \n"
        "{                                              \n"
        "    out_Color = vec4(1.0, 0.0, 0.0, 1.0);      \n"
        "}                                              \n");

    // Create shaders and their program.
    auto pvs = mpGraphicsContext->CreateVertexShader(vs);
    auto pfs = mpGraphicsContext->CreateFragmentShader(fs);
    mpShaderProgram = mpGraphicsContext->CreateShaderProgram(pvs, pfs);

    std::vector<float> positions;
    float data[] = { 0.0f, 0.5f, 0.0f, 0.5, -0.5, 0.0f, -0.5f, -0.5f, 0.0f };
    for (int index = 0; index < _countof(data); ++index)
        positions.push_back(data[index]);

    mpVertexBuffer = mpGraphicsContext->CreateVertexBuffer();
    mpVertexBuffer->LoadData(positions);
}

void Visualizer::Uninitialize(void)
{
    if (this->mpVertexBuffer != nullptr) {
        delete this->mpVertexBuffer;
        mpVertexBuffer = nullptr;
    }

    if (this->mpShaderProgram != nullptr) {
        delete this->mpShaderProgram;
        this->mpShaderProgram = nullptr;
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

LRESULT Visualizer::ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_PAINT:
        {
            PAINTSTRUCT ps;
            HDC deviceContext = BeginPaint(hWnd, &ps); 
            mpGraphicsContext->BeginRenderFrame(deviceContext);
            mpGraphicsContext->ActivateShaderProgram(mpShaderProgram);
            mpGraphicsContext->RenderVertexBuffer(mpVertexBuffer);
            mpGraphicsContext->EndRenderFrame(deviceContext);
            EndPaint(hWnd, &ps);
            return 0L;
        }
    }

    return ::DefWindowProc(hWnd, msg, wParam, lParam);
}
