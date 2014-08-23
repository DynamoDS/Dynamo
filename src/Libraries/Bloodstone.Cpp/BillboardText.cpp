
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

TextBitmapGeneratorWin32::TextBitmapGeneratorWin32()
{
}

void TextBitmapGeneratorWin32::CacheCore(
    const FontSpecs& fontSpecs, const std::wstring& text)
{
}

TextBitmap* TextBitmapGeneratorWin32::GenerateBitmapCore() const
{
    return nullptr;
}

static ITextBitmapGenerator* CreateTextBitmapGenerator(void)
{
    return new TextBitmapGeneratorWin32();
}

#endif

BillboardText::BillboardText(TextId textId, const FontSpecs& fontSpecs) : 
    mTextId(textId),
    mFontSpecs(fontSpecs)
{
    mForegroundRgba0[0] = mForegroundRgba0[1] = mForegroundRgba0[2] = 1.0f;
    mForegroundRgba1[0] = mForegroundRgba1[1] = mForegroundRgba1[2] = 1.0f;
    mBackgroundRgba[0] = mBackgroundRgba[1] = mBackgroundRgba[2] = 0.0f;

    // Alpha for both foreground and background colors.
    mForegroundRgba0[3] = mForegroundRgba1[3] = mBackgroundRgba[3] = 1.0f;
}

void BillboardText::Update(const std::vector<FontCharId>& content)
{
    mTextContent = content;
}

// ================================================================================
// BillboardTextGroup
// ================================================================================

BillboardTextGroup::BillboardTextGroup(IGraphicsContext* pGraphicsContext) : 
    mpGraphicsContext(pGraphicsContext),
    mpVertexBuffer(nullptr),
    mpShaderProgram(nullptr),
    mpBitmapGenerator(nullptr)
{
    mpBitmapGenerator = CreateTextBitmapGenerator();
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
