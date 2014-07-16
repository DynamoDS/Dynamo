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
// Static helper methods (TODO: Move them into a utility class)
// ================================================================================

static float Normalize(unsigned char value)
{
    const float inverse = 1.0f / 255.0f;
    return ((float)(value * inverse));
}

bool GetPointGeometries(IRenderPackage^ rp, PointGeometryData& data)
{
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

bool GetLineStripGeometries(IRenderPackage^ rp, LineStripGeometryData& data)
{
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

bool GetTriangleGeometries(IRenderPackage^ rp, TriangleGeometryData& data)
{
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
