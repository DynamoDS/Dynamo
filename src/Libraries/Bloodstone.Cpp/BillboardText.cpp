
#include "stdafx.h"
#include "BillboardText.h"

using namespace Dynamo::Bloodstone;

// ================================================================================
// TextBitmap
// ================================================================================

int TextBitmap::Width() const
{
    return 0;
}

int TextBitmap::Height() const
{
    return 0;
}

unsigned char* TextBitmap::Data() const
{
    return nullptr;
}

#ifdef _WIN32

// ================================================================================
// TextBitmapGeneratorWin32
// ================================================================================

void TextBitmapGeneratorWin32::CacheCore(
    const FontSpecs& fontSpecs, const std::wstring& text)
{
}

TextBitmap* TextBitmapGeneratorWin32::GenerateBitmapCore() const
{
    return nullptr;
}

#endif

// ================================================================================
// BillboardTextGroup
// ================================================================================

BillboardTextGroup::BillboardTextGroup(IGraphicsContext* pGraphicsContext) : 
    mpGraphicsContext(pGraphicsContext),
    mpVertexBuffer(nullptr),
    mpShaderProgram(nullptr),
    mpBitmapGenerator(nullptr)
{
}

TextId BillboardTextGroup::Create(const FontSpecs& fontSpecs)
{
    // TODO: Initialize font colors to white on black.
    return 0;
}

void BillboardTextGroup::Destroy(TextId textId)
{
}

void BillboardTextGroup::Render(void) const
{
}
void BillboardTextGroup::Update(TextId textId,
                                const std::wstring& text,
                                const float* worldPosition)
{
}

void BillboardTextGroup::UpdateColor(TextId textId,
    const float* foregroundRgba,
    const float* backgroundRgba)
{
}

void BillboardTextGroup::UpdateColor(TextId textId,
    const float* foregroundRgba0,
    const float* foregroundRgba1,
    const float* backgroundRgba)
{
}
