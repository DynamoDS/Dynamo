// Bloodstone.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Bloodstone.h"
#include "Utilities.h"
#include "NodeGeometries.h"
#include "OpenGL Files\OpenInterfaces.h"
#include "Resources\resource.h"

#include <msclr/marshal_cppstd.h>

using namespace System;
using namespace Dynamo::Bloodstone;
using namespace System::Collections::Generic;
using namespace Autodesk::DesignScript::Interfaces;
using namespace DynamoUtilities;

// ================================================================================
// Static helper methods
// ================================================================================

static float Normalize(unsigned char value)
{
    const float inverse = 1.0f / 255.0f;
    return ((float)(value * inverse));
}

static bool GetPointGeometries(IRenderPackage^ rp1, PointGeometryData& data)
{
    auto rp = dynamic_cast<IRenderPackage2 ^>(rp1);
    if (rp == nullptr || (rp->PointVertices->Count <= 0))
        return false;

    auto pv = rp->PointVertices;
    auto pc = rp->PointVertexColors;
    auto count = rp->PointVertices->Count;

    const float inv255 = 1.0f / 255.0f;

    for (int p = 0, c = 0; p < count; p = p + 3, c = c + 4)
    {
        data.PushVertex((float) pv[p + 0], (float) pv[p + 1], (float) pv[p + 2]);

        data.PushColor(Normalize(pc[c + 0]), Normalize(pc[c + 1]),
            Normalize(pc[c + 2]), Normalize(pc[c + 3]));
    }

    return true;
}

static bool GetLineStripGeometries(IRenderPackage^ rp1, LineStripGeometryData& data)
{
    auto rp = dynamic_cast<IRenderPackage2 ^>(rp1);
    if (rp == nullptr || (rp->LineStripVertices->Count <= 0))
        return false;

    auto lsv = rp->LineStripVertices;
    auto lsc = rp->LineStripVertexColors;
    auto count = rp->LineStripVertices->Count;

    for (int p = 0, c = 0; p < count; p = p + 3, c = c + 4)
    {
        data.PushVertex((float) lsv[p + 0], (float) lsv[p + 1], (float) lsv[p + 2]);

        data.PushColor(Normalize(lsc[c + 0]), Normalize(lsc[c + 1]),
            Normalize(lsc[c + 2]), Normalize(lsc[c + 3]));
    }

    auto lsvc = rp->LineStripVertexCounts;
    count = rp->LineStripVertexCounts->Count;
    for (int index = 0; index < count; ++index)
        data.PushSegmentVertexCount(lsvc[index]);

    return true;
}

static bool GetTriangleGeometries(IRenderPackage^ rp1, TriangleGeometryData& data)
{
    auto rp = dynamic_cast<IRenderPackage2 ^>(rp1);
    if (rp == nullptr || (rp->TriangleVertices->Count <= 0))
        return false;

    auto tv = rp->TriangleVertices;
    auto tn = rp->TriangleNormals;
    auto tc = rp->TriangleVertexColors;
    auto count = rp->TriangleVertices->Count;

    for (int p = 0, c = 0; p < count; p = p + 3, c = c + 4)
    {
        data.PushVertex((float) tv[p + 0], (float) tv[p + 1], (float) tv[p + 2]);
        data.PushNormal((float) tn[p + 0], (float) tn[p + 1], (float) tn[p + 2]);

        data.PushColor(Normalize(tc[c + 0]), Normalize(tc[c + 1]),
            Normalize(tc[c + 2]), Normalize(tc[c + 3]));
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
        return new Dynamo::Bloodstone::OpenGL::GraphicsContext();
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

void Visualizer::ShowWindow(bool show)
{
    if (::IsWindow(this->mhWndVisualizer)) {
        auto cmd = show ? SW_SHOW : SW_HIDE;
        ::ShowWindow(this->mhWndVisualizer, cmd);
    }
}

void Visualizer::BlendGeometryLevels(float blendingFactor)
{
    this->mBlendingFactor = blendingFactor;
    RequestFrameUpdate(); // Update window.
}

void Visualizer::UpdateNodeDetails(NodeDetailsType^ nodeDetails)
{
    UpdateNodeGeometries(nodeDetails);
    AssociateToDepthValues(nodeDetails);
    RequestFrameUpdate(); // Update window.
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
        delete pNodeGeometries; // Release node geometries and its resources.

        auto outer = mpGeomsOnDepthLevel->begin();
        for (; outer != mpGeomsOnDepthLevel->end(); ++outer)
        {
            auto pInnerVector = *outer;
            auto inner = pInnerVector->begin();
            for (; inner != pInnerVector->end(); ++inner)
            {
                if (identifier == *inner) {
                    pInnerVector->erase(inner);
                    break;
                }
            }
        }
    }
}

Visualizer::Visualizer() : 
    mAlphaParamIndex(-1),
    mColorParamIndex(-1),
    mControlParamsIndex(-1),
    mBlendingFactor(0.0f),
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

    std::string vs, fs;
    Utils::LoadShaderResource(IDR_SHADER_PHONG_VERT, vs);
    Utils::LoadShaderResource(IDR_SHADER_PHONG_FRAG, fs);

    // Create shaders and their program.
    auto pvs = mpGraphicsContext->CreateVertexShader(vs);
    auto pfs = mpGraphicsContext->CreateFragmentShader(fs);

    mpShaderProgram = mpGraphicsContext->CreateShaderProgram(pvs, pfs);
    mpShaderProgram->BindTransformMatrix(TransMatrix::Model, "model");
    mpShaderProgram->BindTransformMatrix(TransMatrix::View, "view");
    mpShaderProgram->BindTransformMatrix(TransMatrix::Projection, "proj");
    mpShaderProgram->BindTransformMatrix(TransMatrix::Normal, "normalMatrix");
    mAlphaParamIndex = mpShaderProgram->GetShaderParameterIndex("alpha");
    mColorParamIndex = mpShaderProgram->GetShaderParameterIndex("colorOverride");
    mControlParamsIndex = mpShaderProgram->GetShaderParameterIndex("controlParams");

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    {
        CameraConfiguration camConfig;
        camConfig.viewportWidth = width;
        camConfig.viewportHeight = height;
        pCamera->Configure(&camConfig);
    }

    // Create storage for storing nodes and their geometries.
    mpNodeGeometries = new std::map<std::wstring, NodeGeometries*>();
    mpGeomsOnDepthLevel = new std::vector<std::vector<std::wstring> *>();
}

void Visualizer::Uninitialize(void)
{
    if (this->mpGeomsOnDepthLevel != nullptr)
    {
        auto iterator = mpGeomsOnDepthLevel->begin();
        for (; iterator != mpGeomsOnDepthLevel->end(); ++iterator)
        {
            auto pInnerVector = *iterator;
            delete pInnerVector;
        }

        delete mpGeomsOnDepthLevel;
        mpGeomsOnDepthLevel = nullptr;
    }

    if (this->mpNodeGeometries != nullptr)
    {
        auto iterator = mpNodeGeometries->begin();
        for (; iterator != mpNodeGeometries->end(); ++iterator) {
            auto pNodeGeometries = iterator->second;
            delete pNodeGeometries;
        }

        delete this->mpNodeGeometries;
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

void Visualizer::UpdateNodeGeometries(NodeDetailsType^ nodeDetails)
{
    BoundingBox outerBoundingBox;

    for each (KeyValuePair<System::String^, NodeDetails^>^ detail in nodeDetails)
    {
        auto pRenderPackage = detail->Value->RenderPackage;
        if (pRenderPackage == nullptr)
            continue;

        System::String^ nodeId = detail->Key->ToLower();
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

        // Set the color of this node.
        float r = ((float)detail->Value->Red);
        float g = ((float)detail->Value->Green);
        float b = ((float)detail->Value->Blue);
        pNodeGeometries->SetColor(r, g, b, 1.0f);

        // Finally, determine the bounding box for these geometries.
        BoundingBox boundingBox;
        pNodeGeometries->GetBoundingBox(&boundingBox);
        outerBoundingBox.EvaluateBox(boundingBox);
    }

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    pCamera->FitToBoundingBox(&outerBoundingBox);
}

void Visualizer::AssociateToDepthValues(NodeDetailsType^ nodeDetails)
{
    mpGeomsOnDepthLevel->clear();

    int maxDepth = -1; // Determine the maximum number of levels required.
    for each (KeyValuePair<System::String^, NodeDetails^>^ detail in nodeDetails)
    {
        if (detail->Value->Depth > maxDepth)
            maxDepth = detail->Value->Depth;
    }

    if (maxDepth < 0) // There seems to be no depth values.
        return;

    // Pre-allocate the desired number of depth levels.
    for (int index = 0; index <= maxDepth; ++index)
        mpGeomsOnDepthLevel->push_back(new std::vector<std::wstring>());

    for each (KeyValuePair<System::String^, NodeDetails^>^ detail in nodeDetails)
    {
        System::String^ nodeId = detail->Key->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);
        (mpGeomsOnDepthLevel->at(detail->Value->Depth))->push_back(identifier);
    }
}

void Visualizer::GetGeometriesAtDepth(int depth, std::vector<NodeGeometries *>& geometries)
{
    auto pInnerList = mpGeomsOnDepthLevel->at(depth);
    auto iterator = pInnerList->begin();
    for (; iterator != pInnerList->end(); ++iterator)
    {
        auto found = mpNodeGeometries->find(*iterator);
        if (found != mpNodeGeometries->end())
            geometries.push_back(found->second);
    }
}

void Visualizer::GetBoundingBox(std::vector<NodeGeometries *>& geometries, BoundingBox& box)
{
    auto iterator = geometries.begin();
    for (; iterator != geometries.end(); ++iterator) {
        BoundingBox boundingBox;
        (*iterator)->GetBoundingBox(&boundingBox);
        box.EvaluateBox(boundingBox);
    }
}

void Visualizer::RequestFrameUpdate(void)
{
    ::InvalidateRect(this->mhWndVisualizer, nullptr, true); // Update window.
}

void Visualizer::RenderWithBlendingFactor(void)
{
    int maxIndex = ((int) mpGeomsOnDepthLevel->size()) - 1;
    if (maxIndex < 0) // If there is nothing to be rendered.
        return;

    int lower = ((int)std::floorf(this->mBlendingFactor));
    int upper = ((int)std::ceilf(this->mBlendingFactor));

    lower = ((lower < 0) ? 0 : lower);
    upper = ((upper > maxIndex) ? maxIndex : upper);

    std::vector<NodeGeometries *> lowerGeoms, upperGeoms;
    GetGeometriesAtDepth(lower, lowerGeoms);
    if (lower != upper)
        GetGeometriesAtDepth(upper, upperGeoms);

    if (lowerGeoms.size() <= 0 && (upperGeoms.size() <= 0))
        return; // No geometry to render with these settings.

    float integralPart = 0.0f;
    float alpha = std::modf(mBlendingFactor, &integralPart);

    BoundingBox lowerBox, upperBox;
    GetBoundingBox(lowerGeoms, lowerBox);
    GetBoundingBox(upperGeoms, upperBox);
    lowerBox.Interpolate(upperBox, alpha);

    mpGraphicsContext->EnableAlphaBlend();
    mpGraphicsContext->ActivateShaderProgram(mpShaderProgram);

    // Fit the camera to the bounding box, and apply transformation.
    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    pCamera->FitToBoundingBox(&lowerBox);
    mpShaderProgram->ApplyTransformation(pCamera);

    // Render lower level node geometries.
    RenderGeometries(lowerGeoms, 1.0f - alpha);
    if (upperGeoms.size() > 0) { // At 0.0 or 1.0 ends.
        mpGraphicsContext->ClearDepthBuffer();
        RenderGeometries(upperGeoms, alpha);
    }
}

void Visualizer::RenderGeometries(
    const std::vector<NodeGeometries *>& geometries, float alpha)
{
    mpShaderProgram->SetParameter(mAlphaParamIndex, &alpha, 1);

    float rgbaColor[4] = { 0 }, controlParams[4] = { 0 };
    auto iterator = geometries.begin();
    for (; iterator != geometries.end(); ++iterator)
    {
        auto pNodeGeometries = *iterator;
        pNodeGeometries->GetColor(&rgbaColor[0]);
        mpShaderProgram->SetParameter(mColorParamIndex, &rgbaColor[0], 4);

        // Draw primitives of lower dimensionality first (e.g. points and lines).
        controlParams[0] = 1.0f;
        mpShaderProgram->SetParameter(mControlParamsIndex, &controlParams[0], 4);
        pNodeGeometries->Render(mpGraphicsContext, Dimensionality::Low);

        // Draw primitives of higher dimensionality later (e.g. points and lines).
        controlParams[0] = 3.0f;
        mpShaderProgram->SetParameter(mControlParamsIndex, &controlParams[0], 4);
        pNodeGeometries->Render(mpGraphicsContext, Dimensionality::High);
    }
}

LRESULT Visualizer::ProcessMouseMessage(UINT msg, WPARAM wParam, LPARAM lParam)
{
    auto x = GET_X_LPARAM(lParam);
    auto y = GET_Y_LPARAM(lParam);

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    auto pTrackBall = pCamera->GetTrackBall();

    switch (msg)
    {
    case WM_LBUTTONDOWN:
        SetCapture(this->mhWndVisualizer);
        pTrackBall->MousePressed(x, y);
        break;

    case WM_LBUTTONUP:
        pTrackBall->MouseReleased(x, y);
        ::ReleaseCapture();
        break;

    case WM_MOUSEMOVE:
        if ((wParam & MK_LBUTTON) == 0)
            return 0L; // Mouse button isn't pressed.

        pTrackBall->MouseMoved(x, y);
        break;
    }

    RequestFrameUpdate(); // Update window.
    return 0L; // Message processed.
}

LRESULT Visualizer::ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_PAINT:
        {
            PAINTSTRUCT ps;
            HDC deviceContext = BeginPaint(hWnd, &ps);
            {
                mpGraphicsContext->BeginRenderFrame(deviceContext);
                RenderWithBlendingFactor();
                mpGraphicsContext->EndRenderFrame(deviceContext);
            }
            EndPaint(hWnd, &ps);
            return 0L;
        }

    case WM_LBUTTONDOWN:
    case WM_LBUTTONUP:
    case WM_MOUSEMOVE:
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
