
#include "stdafx.h"
#include "Bloodstone.h"
#include "Utilities.h"
#include "NodeSceneData.h"
#include "BillboardText.h"
#include "Resources\resource.h"

#include <msclr/marshal_cppstd.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Dynamo::Bloodstone;
using namespace Autodesk::DesignScript::Interfaces;

extern bool GetPointGeometries(IRenderPackage^ rp, PointGeometryData& data);
extern bool GetLineStripGeometries(IRenderPackage^ rp, LineStripGeometryData& data);
extern bool GetTriangleGeometries(IRenderPackage^ rp, TriangleGeometryData& data);

Scene::Scene(VisualizerWnd^ visualizer) : 
    mAlphaParamIndex(-1),
    mColorParamIndex(-1),
    mControlParamsIndex(-1),
    mpPhongShader(nullptr),
    mpBillboardTextGroup(nullptr),
    mpNodeSceneData(nullptr),
    mVisualizer(visualizer)
{
    // Create storage for storing nodes and their geometries.
    mpNodeSceneData = new std::map<std::wstring, NodeSceneData*>();
}

void Scene::Initialize(int width, int height)
{
    auto pGraphicsContext = mVisualizer->GetGraphicsContext();
    mpPhongShader = pGraphicsContext->CreateShaderProgram(ShaderName::Phong);
    mpPhongShader->BindTransformMatrix(TransMatrix::Model, "model");
    mpPhongShader->BindTransformMatrix(TransMatrix::View, "view");
    mpPhongShader->BindTransformMatrix(TransMatrix::Projection, "proj");
    mpPhongShader->BindTransformMatrix(TransMatrix::Normal, "normalMatrix");
    mAlphaParamIndex = mpPhongShader->GetShaderParameterIndex("alpha");
    mColorParamIndex = mpPhongShader->GetShaderParameterIndex("colorOverride");
    mControlParamsIndex = mpPhongShader->GetShaderParameterIndex("controlParams");

    auto pCamera = pGraphicsContext->GetDefaultCamera();
    {
        CameraConfiguration camConfig;
        camConfig.viewportWidth = width;
        camConfig.viewportHeight = height;
        pCamera->Configure(&camConfig);
    }

    mpBillboardTextGroup = new BillboardTextGroup(pGraphicsContext);

#if 1 // Temporary demo code section.

    auto flags = FontFlags::Bold | FontFlags::Underline;
    FontSpecs fontSpecs(L"Consolas");
    fontSpecs.height = 12;
    fontSpecs.flags = ((FontFlags) flags);

    float worldPosition[] = { 10.0f, 10.0f, 10.0f };
    auto textId = mpBillboardTextGroup->CreateText(fontSpecs);
    mpBillboardTextGroup->UpdateText(textId, L"Hello World");
    mpBillboardTextGroup->UpdatePosition(textId, worldPosition);

#endif
}

void Scene::Destroy(void)
{
    if (this->mpBillboardTextGroup != nullptr)
    {
        delete this->mpBillboardTextGroup;
        this->mpBillboardTextGroup = nullptr;
    }

    if (this->mpNodeSceneData != nullptr)
    {
        ClearAllGeometries();
        delete this->mpNodeSceneData;
        this->mpNodeSceneData = nullptr;
    }

    if (this->mpPhongShader != nullptr) {
        delete this->mpPhongShader;
        this->mpPhongShader = nullptr;
    }
}

void Scene::RenderScene(void)
{
    std::vector<NodeSceneData *> geometries;

    BoundingBox boundingBox;
    auto iterator = mpNodeSceneData->begin();
    for (; iterator != mpNodeSceneData->end(); ++iterator) {
        BoundingBox innerBox;
        iterator->second->GetBoundingBox(&innerBox);
        boundingBox.EvaluateBox(innerBox);
        geometries.push_back(iterator->second);
    }

    if (geometries.size() > 0)
    {
        auto pGraphicsContext = mVisualizer->GetGraphicsContext();
        pGraphicsContext->EnableAlphaBlend();
        pGraphicsContext->ActivateShaderProgram(mpPhongShader);

        // Fit the camera to the bounding box, and apply transformation.
        auto pCamera = pGraphicsContext->GetDefaultCamera();
        mpPhongShader->ApplyTransformation(pCamera);

        RenderGeometries(geometries);
    }

    mpBillboardTextGroup->Render(); // Render billboard text.
}

void Scene::ClearAllGeometries(void)
{
    auto iterator = mpNodeSceneData->begin();
    for (; iterator != mpNodeSceneData->end(); ++iterator) {
        auto pNodeSceneData = iterator->second;
        delete pNodeSceneData;
    }

    mpNodeSceneData->clear();
    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::GetBoundingBox(BoundingBox& boundingBox)
{
    if (mpNodeSceneData == nullptr) {
        boundingBox.Reset(0.0f, 0.0f, 0.0f);
        return;
    }

    auto iterator = mpNodeSceneData->begin();
    for (; iterator != mpNodeSceneData->end(); ++iterator) {
        BoundingBox innerBox;
        iterator->second->GetBoundingBox(&innerBox);
        boundingBox.EvaluateBox(innerBox);
    }
}

void Scene::UpdateNodeGeometries(RenderPackages^ geometries)
{
    BoundingBox outerBoundingBox;
    auto pGraphicsContext = mVisualizer->GetGraphicsContext();

    for each (KeyValuePair<System::String^, Ds::IRenderPackage^>^ geometry in geometries)
    {
        auto pRenderPackage = geometry->Value;
        if (pRenderPackage == nullptr)
            continue;

        System::String^ nodeId = geometry->Key->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        NodeSceneData* pNodeSceneData = nullptr;
        auto found = mpNodeSceneData->find(identifier);
        if (found != mpNodeSceneData->end())
        {
            pNodeSceneData = found->second;
            pNodeSceneData->ClearVertexBuffers();
        }
        else
        {
            pNodeSceneData = new NodeSceneData(identifier);
            mpNodeSceneData->insert(std::pair<std::wstring, NodeSceneData*>
                (identifier, pNodeSceneData));
        }

        PointGeometryData pointData(pRenderPackage->PointVertices->Count / 3);
        if (GetPointGeometries(pRenderPackage, pointData))
        {
            auto pVertexBuffer = pGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(pointData);
            pVertexBuffer->BindToShaderProgram(mpPhongShader);
            pNodeSceneData->AppendVertexBuffer(pVertexBuffer);
        }

        LineStripGeometryData lineData(0);
        if (GetLineStripGeometries(pRenderPackage, lineData))
        {
            auto pVertexBuffer = pGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(lineData);
            pVertexBuffer->BindToShaderProgram(mpPhongShader);
            pNodeSceneData->AppendVertexBuffer(pVertexBuffer);
        }

        TriangleGeometryData triangleData(pRenderPackage->TriangleVertices->Count / 3);
        if (GetTriangleGeometries(pRenderPackage, triangleData))
        {
            auto pVertexBuffer = pGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(triangleData);
            pVertexBuffer->BindToShaderProgram(mpPhongShader);
            pNodeSceneData->AppendVertexBuffer(pVertexBuffer);
        }

        // Finally, determine the bounding box for these geometries.
        BoundingBox boundingBox;
        pNodeSceneData->GetBoundingBox(&boundingBox);
        outerBoundingBox.EvaluateBox(boundingBox);
    }

    CameraConfiguration configuration;
    auto pCamera = pGraphicsContext->GetDefaultCamera();
    pCamera->GetConfiguration(&configuration);
    configuration.FitToBoundingBox(outerBoundingBox);
    pCamera->BeginConfigure(&configuration);

    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::RemoveNodeGeometries(Strings^ identifiers)
{
    for each (System::String^ identifier in identifiers)
    {
        System::String^ nodeId = identifier->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        auto found = mpNodeSceneData->find(identifier);
        if (found == mpNodeSceneData->end())
            continue; // The node does not have any associated geometries.

        // Release the node geometry ownership from map.
        NodeSceneData* pNodeSceneData = found->second;
        mpNodeSceneData->erase(found);
        delete pNodeSceneData; // Release node geometries and its resources.
    }

    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::SelectNodes(Strings^ identifiers, SelectMode selectMode)
{
    if (selectMode == SelectMode::ClearExisting) {
        auto iterator = mpNodeSceneData->begin();
        for (; iterator != mpNodeSceneData->end(); ++iterator) {
            auto pNodeSceneData = iterator->second;
            pNodeSceneData->SetSelected(false); // Clear selection.
        }
    }

    for each (System::String^ identifier in identifiers)
    {
        System::String^ nodeId = identifier->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        auto found = mpNodeSceneData->find(identifier);
        if (found == mpNodeSceneData->end())
            continue; // The node does not have any associated geometries.

        NodeSceneData* pNodeSceneData = found->second;
        if (selectMode == SelectMode::RemoveFromExisting)
            pNodeSceneData->SetSelected(false);
        else
            pNodeSceneData->SetSelected(true);
    }

    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::SetNodeColor(NodeColors^ nodeColors)
{
    for each (KeyValuePair<System::String^, NodeColor^>^ nodeColor in nodeColors)
    {
        float color[4] = { 0 };
        nodeColor->Value->Get(color);

        System::String^ nodeId = nodeColor->Key->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        auto found = mpNodeSceneData->find(identifier);
        if (found == mpNodeSceneData->end())
            continue; // The node does not have any associated geometries.

        NodeSceneData* pNodeSceneData = found->second;
        pNodeSceneData->SetColor(color[0], color[1], color[2], color[3]);
    }

    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::SetNodeRenderMode(RenderModes^ renderModes)
{
    for each (KeyValuePair<System::String^, RenderMode>^ renderMode in renderModes)
    {
        System::String^ nodeId = renderMode->Key->ToLower();
        std::wstring identifier = msclr::interop::marshal_as<std::wstring>(nodeId);

        auto found = mpNodeSceneData->find(identifier);
        if (found == mpNodeSceneData->end())
            continue; // The node does not have any associated geometries.

        NodeSceneData* pNodeSceneData = found->second;
        pNodeSceneData->SetRenderMode(renderMode->Value);
    }

    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::RenderGeometries(const std::vector<NodeSceneData *>& geometries)
{
    float alpha = 1.0f;
    auto pGraphicsContext = mVisualizer->GetGraphicsContext();
    mpPhongShader->SetParameter(mAlphaParamIndex, &alpha, 1);

    auto iterator = geometries.begin();
    for (; iterator != geometries.end(); ++iterator)
    {
        float rgbaColor[4] = { 0 };
        float controlParams[4] = { 0 };

        auto pNodeSceneData = *iterator;
        pNodeSceneData->GetColor(&rgbaColor[0]);

        // Use the node color if one is specified.
        if (rgbaColor[3] > 0.01f)
            controlParams[1] = 1.0f; // Override color.

        if (pNodeSceneData->GetSelected()) {
            rgbaColor[0] = 154.0f / 255.0f;
            rgbaColor[1] = 206.0f / 255.0f;
            rgbaColor[2] = 235.0f / 255.0f;
            controlParams[1] = 1.0f; // Override color.
        }

        mpPhongShader->SetParameter(mColorParamIndex, &rgbaColor[0], 4);

        // Draw primitives of lower dimensionality first (e.g. points and lines).
        controlParams[0] = 1.0f;
        mpPhongShader->SetParameter(mControlParamsIndex, &controlParams[0], 4);
        pNodeSceneData->Render(pGraphicsContext, Dimensionality::Low);

        if (pNodeSceneData->GetRenderMode() == RenderMode::Shaded)
            controlParams[0] = 3.0f;

        // Draw primitives of higher dimensionality later (e.g. triangles).
        mpPhongShader->SetParameter(mControlParamsIndex, &controlParams[0], 4);
        pNodeSceneData->Render(pGraphicsContext, Dimensionality::High);
    }
}
