// Dynamorph.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Dynamorph.h"
#include "NodeGeometries.h"
#include "OpenGL Files\OpenInterfaces.h"

#include <msclr/marshal_cppstd.h>

using namespace System;
using namespace Dynamorph;
using namespace System::Collections::Generic;
using namespace Autodesk::DesignScript::Interfaces;

// ================================================================================
// Static helper methods
// ================================================================================

static bool GetPointGeometries(IRenderPackage^ rp, PointGeometryData& data)
{
    if (rp == nullptr || (rp->PointVertices->Count <= 0))
        return false;

    auto pv = rp->PointVertices;
    auto count = rp->PointVertices->Count;

    for (int p = 0; p < count; p = p + 3)
    {
        data.PushVertex((float) pv[p + 0], (float) pv[p + 1], (float) pv[p + 2]);
        data.PushColor(1.0f, 1.0f, 1.0f, 1.0f);
    }

    return true;
}

static bool GetLineStripGeometries(IRenderPackage^ rp, LineStripGeometryData& data)
{
    if (rp == nullptr || (rp->LineStripVertices->Count <= 0))
        return false;

    auto lsv = rp->LineStripVertices;
    auto lsc = rp->LineStripVertexColors;
    auto count = rp->LineStripVertices->Count;

    float factor = 1.0f / 255.0f;
    for (int p = 0, c = 0; p < count; p = p + 3, c = c + 4)
    {
        data.PushVertex((float) lsv[p + 0], (float) lsv[p + 1], (float) lsv[p + 2]);

        data.PushColor(
            ((int)lsc[c + 0]) * factor,
            ((int)lsc[c + 1]) * factor,
            ((int)lsc[c + 2]) * factor,
            ((int)lsc[c + 3]) * factor);
    }

    auto lsvc = rp->LineStripVertexCounts;
    count = rp->LineStripVertexCounts->Count;
    for (int index = 0; index < count; ++index)
        data.PushSegmentVertexCount(lsvc[index]);

    return true;
}

static bool GetTriangleGeometries(IRenderPackage^ rp, TriangleGeometryData& data)
{
    if (rp == nullptr || (rp->TriangleVertices->Count <= 0))
        return false;

    auto tv = rp->TriangleVertices;
    auto count = rp->TriangleVertices->Count;

    for (int p = 0; p < count; p = p + 3)
    {
        data.PushVertex((float) tv[p + 0], (float) tv[p + 1], (float) tv[p + 2]);
        data.PushColor(1.0f, 1.0f, 1.0f, 1.0f);
    }

    // TODO(Ben): Make use of normals from IRenderPackage when it is available.
    auto floats = data.VertexCount() * 3;
    const float* pCoords = data.GetCoordinates(0);
    const float* pEndCoords = &(pCoords[floats]);

    for (const float* p = pCoords; p < pEndCoords; p = p + 9)
    {
        float u[3] = { p[3] - p[0], p[4] - p[1], p[5] - p[2] };
        float v[3] = { p[6] - p[0], p[7] - p[1], p[8] - p[2] };

        float n[3] =
        {
            u[1] * v[2] - u[2] * v[1],
            u[2] * v[0] - u[0] * v[2],
            u[0] * v[1] - u[1] * v[0]
        };

        // Push one normal per vertex.
        data.PushNormal(n[0], n[1], n[2]);
        data.PushNormal(n[0], n[1], n[2]);
        data.PushNormal(n[0], n[1], n[2]);
    }

    return true;
}

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

Visualizer^ Visualizer::CurrentInstance(void)
{
    return Visualizer::mVisualizer;
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

void Visualizer::UpdateNodeGeometries(Dictionary<Guid, IRenderPackage^>^ geometries)
{
    BoundingBox outerBoundingBox;

    for each(KeyValuePair<Guid, IRenderPackage^> geometry in geometries)
    {
        System::String^ nodeId = geometry.Key.ToString()->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        NodeGeometries* pNodeGeometries = nullptr;
        auto found = mpNodeGeometries->find(identifier);
        if (found != mpNodeGeometries->end())
        {
            pNodeGeometries = found->second;
            pNodeGeometries->ClearVertexBuffers();
        }
        else
        {
            pNodeGeometries = new NodeGeometries(identifier);
            mpNodeGeometries->insert(std::pair<std::wstring, NodeGeometries*>
                (identifier, pNodeGeometries));
        }

        auto pRenderPackage = geometry.Value;
        PointGeometryData pointData(pRenderPackage->PointVertices->Count / 3);
        if (GetPointGeometries(pRenderPackage, pointData))
        {
            auto pVertexBuffer = mpGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(pointData);
            pNodeGeometries->AppendVertexBuffer(pVertexBuffer);
        }

        LineStripGeometryData lineData(0);
        if (GetLineStripGeometries(pRenderPackage, lineData))
        {
            auto pVertexBuffer = mpGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(lineData);
            pNodeGeometries->AppendVertexBuffer(pVertexBuffer);
        }

        TriangleGeometryData triangleData(pRenderPackage->TriangleVertices->Count / 3);
        if (GetTriangleGeometries(pRenderPackage, triangleData))
        {
            auto pVertexBuffer = mpGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(triangleData);
            pNodeGeometries->AppendVertexBuffer(pVertexBuffer);
        }

        // Finally, determine the bounding box for these geometries.
        BoundingBox boundingBox;
        pNodeGeometries->GetBoundingBox(&boundingBox);
        outerBoundingBox.EvaluateBox(boundingBox);
    }

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    pCamera->FitToBoundingBox(&outerBoundingBox);

    ::InvalidateRect(this->mhWndVisualizer, nullptr, true); // Update window.
}

Visualizer::Visualizer() : 
    mpNodeGeometries(nullptr),
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
        "                                               \n"
        "in vec3 inPosition;                            \n"
        "in vec4 inColor;                               \n"
        "out vec4 vertColor;                            \n"
        "                                               \n"
        "uniform mat4 model;                            \n"
        "uniform mat4 view;                             \n"
        "uniform mat4 proj;                             \n"
        "                                               \n"
        "void main(void)                                \n"
        "{                                              \n"
        "    vec4 pos = vec4(inPosition, 1.0);          \n"
        "    gl_Position = proj * view * model * pos;   \n"
        "    vertColor = inColor;                       \n"
        "}                                              \n");

    std::string fs(
        "#version 150 core                          \n"
        "                                           \n"
        "in vec4 vertColor;                         \n"
        "out vec4 outColor;                         \n"
        "                                           \n"
        "void main(void)                            \n"
        "{                                          \n"
        "    outColor = vertColor;                  \n"
        "}                                          \n");

    // Create shaders and their program.
    auto pvs = mpGraphicsContext->CreateVertexShader(vs);
    auto pfs = mpGraphicsContext->CreateFragmentShader(fs);

    mpShaderProgram = mpGraphicsContext->CreateShaderProgram(pvs, pfs);
    mpShaderProgram->BindTransformMatrix(TransMatrix::Model, "model");
    mpShaderProgram->BindTransformMatrix(TransMatrix::View, "view");
    mpShaderProgram->BindTransformMatrix(TransMatrix::Projection, "proj");

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    {
        CameraConfiguration camConfig;
        camConfig.SetEyePoint(10.0f, 15.0f, 20.0f);
        camConfig.SetCenterPoint(0.0f, 0.0f, 0.0f);
        camConfig.SetUpVector(0.0f, 1.0f, 0.0f);
        camConfig.aspectRatio = ((float) width) / height;
        pCamera->Configure(&camConfig);
    }

    TriangleGeometryData triangleData(12);

#pragma region Sample Test Data (To Be Removed)

    triangleData.PushVertex(-0.5f, -0.5f, -0.5f);
    triangleData.PushVertex( 0.5f, -0.5f, -0.5f);
    triangleData.PushVertex( 0.5f,  0.5f, -0.5f);
    triangleData.PushVertex( 0.5f,  0.5f, -0.5f);
    triangleData.PushVertex(-0.5f,  0.5f, -0.5f);
    triangleData.PushVertex(-0.5f, -0.5f, -0.5f);

    triangleData.PushVertex(-0.5f, -0.5f,  0.5f);
    triangleData.PushVertex( 0.5f, -0.5f,  0.5f);
    triangleData.PushVertex( 0.5f,  0.5f,  0.5f);
    triangleData.PushVertex( 0.5f,  0.5f,  0.5f);
    triangleData.PushVertex(-0.5f,  0.5f,  0.5f);
    triangleData.PushVertex(-0.5f, -0.5f,  0.5f);

    triangleData.PushVertex(-0.5f,  0.5f,  0.5f);
    triangleData.PushVertex(-0.5f,  0.5f, -0.5f);
    triangleData.PushVertex(-0.5f, -0.5f, -0.5f);
    triangleData.PushVertex(-0.5f, -0.5f, -0.5f);
    triangleData.PushVertex(-0.5f, -0.5f,  0.5f);
    triangleData.PushVertex(-0.5f,  0.5f,  0.5f);

    triangleData.PushVertex( 0.5f,  0.5f,  0.5f);
    triangleData.PushVertex( 0.5f,  0.5f, -0.5f);
    triangleData.PushVertex( 0.5f, -0.5f, -0.5f);
    triangleData.PushVertex( 0.5f, -0.5f, -0.5f);
    triangleData.PushVertex( 0.5f, -0.5f,  0.5f);
    triangleData.PushVertex( 0.5f,  0.5f,  0.5f);

    triangleData.PushVertex(-0.5f, -0.5f, -0.5f);
    triangleData.PushVertex( 0.5f, -0.5f, -0.5f);
    triangleData.PushVertex( 0.5f, -0.5f,  0.5f);
    triangleData.PushVertex( 0.5f, -0.5f,  0.5f);
    triangleData.PushVertex(-0.5f, -0.5f,  0.5f);
    triangleData.PushVertex(-0.5f, -0.5f, -0.5f);

    triangleData.PushVertex(-0.5f,  0.5f, -0.5f);
    triangleData.PushVertex( 0.5f,  0.5f, -0.5f);
    triangleData.PushVertex( 0.5f,  0.5f,  0.5f);
    triangleData.PushVertex( 0.5f,  0.5f,  0.5f);
    triangleData.PushVertex(-0.5f,  0.5f,  0.5f);
    triangleData.PushVertex(-0.5f,  0.5f, -0.5f);

    triangleData.PushColor(1.0f, 0.5f, 0.75f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.75f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.75f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.75f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.75f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.75f, 1.0f);

    triangleData.PushColor(0.5f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(0.5f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(0.5f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(0.5f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(0.5f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(0.5f, 1.0f, 0.50f, 1.0f);

    triangleData.PushColor(0.0f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.0f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.0f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.0f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.0f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.0f, 0.5f, 1.00f, 1.0f);

    triangleData.PushColor(1.0f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(1.0f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(1.0f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(1.0f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(1.0f, 1.0f, 0.50f, 1.0f);
    triangleData.PushColor(1.0f, 1.0f, 0.50f, 1.0f);

    triangleData.PushColor(1.0f, 0.5f, 0.25f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.25f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.25f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.25f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.25f, 1.0f);
    triangleData.PushColor(1.0f, 0.5f, 0.25f, 1.0f);

    triangleData.PushColor(0.5f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.5f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.5f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.5f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.5f, 0.5f, 1.00f, 1.0f);
    triangleData.PushColor(0.5f, 0.5f, 1.00f, 1.0f);

#pragma endregion

    mpVertexBuffer = mpGraphicsContext->CreateVertexBuffer();
    mpVertexBuffer->LoadData(triangleData);

    BoundingBox boundingBox;
    mpVertexBuffer->GetBoundingBox(&boundingBox);
    pCamera->FitToBoundingBox(&boundingBox);

    // Create storage for storing nodes and their geometries.
    mpNodeGeometries = new std::map<std::wstring, NodeGeometries*>();
}

void Visualizer::Uninitialize(void)
{
    if (this->mpNodeGeometries != nullptr)
    {
        auto iterator = mpNodeGeometries->begin();
        for (; iterator != mpNodeGeometries->end(); ++iterator) {
            auto pNodeGeometries = iterator->second;
            delete pNodeGeometries;
        }

        this->mpNodeGeometries->clear();
        this->mpNodeGeometries = nullptr;
    }

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

            // Apply camera transformation.
            auto pCamera = mpGraphicsContext->GetDefaultCamera();
            mpShaderProgram->ApplyTransformation(pCamera);

            auto iterator = mpNodeGeometries->begin();
            for (; iterator != mpNodeGeometries->end(); ++iterator)
            {
                auto pNodeGeometries = iterator->second;
                pNodeGeometries->Render(mpGraphicsContext);
            }

            // mpGraphicsContext->RenderVertexBuffer(mpVertexBuffer);
            mpGraphicsContext->EndRenderFrame(deviceContext);
            EndPaint(hWnd, &ps);
            return 0L;
        }

    case WM_ERASEBKGND:
        return 0L; // Avoid erasing background to flickering during sizing.
    }

    return ::DefWindowProc(hWnd, msg, wParam, lParam);
}
