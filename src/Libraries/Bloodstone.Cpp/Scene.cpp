
#include "stdafx.h"
#include "Bloodstone.h"
#include "Utilities.h"
#include "NodeGeometries.h"
#include "Resources\resource.h"

#include <msclr/marshal_cppstd.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace Dynamo::Bloodstone;
using namespace Autodesk::DesignScript::Interfaces;

extern bool GetPointGeometries(IRenderPackage^ rp, PointGeometryData& data);
extern bool GetLineStripGeometries(IRenderPackage^ rp, LineStripGeometryData& data);
extern bool GetTriangleGeometries(IRenderPackage^ rp, TriangleGeometryData& data);

Scene::Scene(Visualizer^ visualizer) : 
    mAlphaParamIndex(-1),
    mColorParamIndex(-1),
    mControlParamsIndex(-1),
    mpShaderProgram(nullptr),
    mpNodeGeometries(nullptr),
    mVisualizer(nullptr)
{
    // Create storage for storing nodes and their geometries.
    mpNodeGeometries = new std::map<std::wstring, NodeGeometries*>();
}

void Scene::Initialize(int width, int height)
{
    std::string vs, fs;
    Utils::LoadShaderResource(IDR_SHADER_PHONG_VERT, vs);
    Utils::LoadShaderResource(IDR_SHADER_PHONG_FRAG, fs);

    // Create shaders and their program.
    auto pGraphicsContext = mVisualizer->GetGraphicsContext();
    auto pvs = pGraphicsContext->CreateVertexShader(vs);
    auto pfs = pGraphicsContext->CreateFragmentShader(fs);

    mpShaderProgram = pGraphicsContext->CreateShaderProgram(pvs, pfs);
    mpShaderProgram->BindTransformMatrix(TransMatrix::Model, "model");
    mpShaderProgram->BindTransformMatrix(TransMatrix::View, "view");
    mpShaderProgram->BindTransformMatrix(TransMatrix::Projection, "proj");
    mpShaderProgram->BindTransformMatrix(TransMatrix::Normal, "normalMatrix");
    mAlphaParamIndex = mpShaderProgram->GetShaderParameterIndex("alpha");
    mColorParamIndex = mpShaderProgram->GetShaderParameterIndex("colorOverride");
    mControlParamsIndex = mpShaderProgram->GetShaderParameterIndex("controlParams");

    auto pCamera = pGraphicsContext->GetDefaultCamera();
    {
        CameraConfiguration camConfig;
        camConfig.viewportWidth = width;
        camConfig.viewportHeight = height;
        pCamera->Configure(&camConfig);
    }
}

void Scene::Destroy(void)
{
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
}

void Scene::RenderScene(void)
{
    std::vector<NodeGeometries *> geometries;

    BoundingBox boundingBox;
    auto iterator = mpNodeGeometries->begin();
    for (; iterator != mpNodeGeometries->end(); ++iterator) {
        BoundingBox innerBox;
        iterator->second->GetBoundingBox(&innerBox);
        boundingBox.EvaluateBox(innerBox);
        geometries.push_back(iterator->second);
    }

    auto pGraphicsContext = mVisualizer->GetGraphicsContext();
    pGraphicsContext->EnableAlphaBlend();
    pGraphicsContext->ActivateShaderProgram(mpShaderProgram);

    // Fit the camera to the bounding box, and apply transformation.
    auto pCamera = pGraphicsContext->GetDefaultCamera();
    pCamera->FitToBoundingBox(&boundingBox);
    mpShaderProgram->ApplyTransformation(pCamera);

    RenderGeometries(geometries);
}

void Scene::UpdateNodeDetails(NodeDetailsType^ nodeDetails)
{
    UpdateNodeGeometries(nodeDetails);
    mVisualizer->RequestFrameUpdate(); // Update window.
}

void Scene::RemoveNodeGeometries(IEnumerable<System::String^>^ identifiers)
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
    }
}

void Scene::UpdateNodeGeometries(NodeDetailsType^ nodeDetails)
{
    BoundingBox outerBoundingBox;
    auto pGraphicsContext = mVisualizer->GetGraphicsContext();

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
            auto pVertexBuffer = pGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(pointData);
            pNodeGeometries->AppendVertexBuffer(pVertexBuffer);
        }

        LineStripGeometryData lineData(0);
        if (GetLineStripGeometries(pRenderPackage, lineData))
        {
            auto pVertexBuffer = pGraphicsContext->CreateVertexBuffer();
            pVertexBuffer->LoadData(lineData);
            pNodeGeometries->AppendVertexBuffer(pVertexBuffer);
        }

        TriangleGeometryData triangleData(pRenderPackage->TriangleVertices->Count / 3);
        if (GetTriangleGeometries(pRenderPackage, triangleData))
        {
            auto pVertexBuffer = pGraphicsContext->CreateVertexBuffer();
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

    auto pCamera = pGraphicsContext->GetDefaultCamera();
    pCamera->FitToBoundingBox(&outerBoundingBox);
}

void Scene::RenderGeometries(const std::vector<NodeGeometries *>& geometries)
{
    float alpha = 1.0f;
    auto pGraphicsContext = mVisualizer->GetGraphicsContext();
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
        pNodeGeometries->Render(pGraphicsContext, Dimensionality::Low);

        // Draw primitives of higher dimensionality later (e.g. points and lines).
        // controlParams[0] = 3.0f;
        // mpShaderProgram->SetParameter(mControlParamsIndex, &controlParams[0], 4);
        pNodeGeometries->Render(pGraphicsContext, Dimensionality::High);
    }
}
