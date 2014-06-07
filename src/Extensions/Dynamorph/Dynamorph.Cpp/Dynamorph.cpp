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
    auto tn = rp->TriangleNormals;
    auto count = rp->TriangleVertices->Count;

    for (int p = 0; p < count; p = p + 3)
    {
        data.PushVertex((float) tv[p + 0], (float) tv[p + 1], (float) tv[p + 2]);
        data.PushNormal((float) tn[p + 0], (float) tn[p + 1], (float) tn[p + 2]);
        data.PushColor(1.0f, 1.0f, 1.0f, 1.0f);
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

void Visualizer::RemoveNodeGeometries(IEnumerable<System::String^>^ identifiers)
{
    for each (System::String^ identifier in identifiers)
    {
        System::String^ nodeId = identifier->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        auto found = mpNodeGeometries->find(identifier);
        if (found == mpNodeGeometries->end())
            continue; // The node does not have any associated geometries.

        // Release the node geometry ownership from map.
        NodeGeometries* pNodeGeometries = found->second;
        mpNodeGeometries->erase(found);

        auto outer = mpGeomsOnDepthLevel->begin();
        for (; outer != mpGeomsOnDepthLevel->end(); ++outer)
        {
            auto inner = outer->begin();
            for (; inner != outer->end(); ++inner)
            {
                if (pNodeGeometries == *inner) {
                    outer->erase(inner);
                    break;
                }
            }
        }

        delete pNodeGeometries; // Release node geometries and its resources.
    }
}

Visualizer::Visualizer() : 
    mpNodeGeometries(nullptr),
    mpGeomsOnDepthLevel(nullptr),
    mhWndVisualizer(nullptr),
    mpShaderProgram(nullptr),
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
        "#version 330                                                   \n"
        "                                                               \n"
        "layout (location = 0) in vec3 inPosition;                      \n"
        "layout (location = 1) in vec3 inNormal;                        \n"
        "layout (location = 2) in vec4 inColor;                         \n"
        "                                                               \n"
        "out vec3 vertNormal;                                           \n"
        "out vec3 vertPosition;                                         \n"
        "out vec4 vertColor;                                            \n"
        "                                                               \n"
        "uniform mat4 model;                                            \n"
        "uniform mat4 view;                                             \n"
        "uniform mat4 proj;                                             \n"
        "                                                               \n"
        "void main(void)                                                \n"
        "{                                                              \n"
        "    vec4 modelPos = vec4(inPosition, 1.0);                     \n"
        "    gl_Position = proj * view * model * modelPos;              \n"
        "                                                               \n"
        "    // Compute varying parameters                              \n"
        "    vec4 viewPos = view * model * modelPos;                    \n"
        "    vertPosition = vec3(viewPos) / viewPos.w;                  \n"
        "    vertColor = inColor;                                       \n"
        "    vertNormal = normalize(inNormal);                          \n"
        "}                                                              \n");

    std::string fs(
        "#version 330                                                   \n"
        "                                                               \n"
        "in vec3 vertNormal;                                            \n"
        "in vec3 vertPosition;                                          \n"
        "in vec4 vertColor;                                             \n"
        "                                                               \n"
        "out vec4 outColor;                                             \n"
        "                                                               \n"
        "const vec3 lightPosition = vec3(1.0, 1.0, 1.0);                \n"
        "const vec3 ambientColor  = vec3(0.3, 0.0, 0.0);                \n"
        "const vec3 diffuseColor  = vec3(0.5, 0.0, 0.0);                \n"
        "const vec3 specularColor = vec3(1.0, 1.0, 1.0);                \n"
        "                                                               \n"
        "void main(void)                                                \n"
        "{                                                              \n"
        "    vec3 normal = normalize(vertNormal);                       \n"
        "    vec3 lightDir = normalize(lightPosition - vertPosition);   \n"
        "    vec3 reflectDir = reflect(-lightDir, normal);              \n"
        "    vec3 viewDir = normalize(-vertPosition);                   \n"
        "                                                               \n"
        "    float lambertian = max(dot(lightDir, normal), 0.0);        \n"
        "    float specular = 0.0;                                      \n"
        "                                                               \n"
        "    if(lambertian > 0.0) {                                     \n"
        "       float specAngle = max(dot(reflectDir, viewDir), 0.0);   \n"
        "       specular = pow(specAngle, 4.0);                         \n"
        "    }                                                          \n"
        "                                                               \n"
        "    vec3 a = ambientColor * vertColor.rgb;                     \n"
        "    outColor = vec4(a +                                        \n"
        "        lambertian * diffuseColor +                            \n"
        "        specular * specularColor, 1.0);                        \n"
        "}                                                              \n");

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

    // Create storage for storing nodes and their geometries.
    mpNodeGeometries = new std::map<std::wstring, NodeGeometries*>();
    mpGeomsOnDepthLevel = new std::vector<std::vector<NodeGeometries*>>();
}

void Visualizer::Uninitialize(void)
{
    // The ultimate ownership of NodeGeometries is in mpNodeGeometries map, 
    // so here we simply clear the mpGeomsOnDepthLevel vector, and delete the 
    // vector itself.
    // 
    if (this->mpGeomsOnDepthLevel != nullptr) {
        mpGeomsOnDepthLevel->clear();
        mpGeomsOnDepthLevel = nullptr;
    }

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

            mpGraphicsContext->EndRenderFrame(deviceContext);
            EndPaint(hWnd, &ps);
            return 0L;
        }

    case WM_ERASEBKGND:
        return 0L; // Avoid erasing background to flickering during sizing.
    }

    return ::DefWindowProc(hWnd, msg, wParam, lParam);
}
