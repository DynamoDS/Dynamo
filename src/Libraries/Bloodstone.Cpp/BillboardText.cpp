
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

// ================================================================================
// ITextBitmapGenerator
// ================================================================================

ITextBitmapGenerator::ITextBitmapGenerator() : mCurrentFontId(1024)
{
}

FontId ITextBitmapGenerator::CacheFont(const FontSpecs& fontSpecs)
{
    auto iterator = mFontSpecs.begin();
    for (; iterator != mFontSpecs.end(); ++iterator)
    {
        if (fontSpecs == iterator->second)
            return iterator->first;
    }

    auto fontId = mCurrentFontId++;
    auto pair = std::pair<FontId, FontSpecs>(fontId, fontSpecs);
    mFontSpecs.insert(pair);
    return pair.first;
}

TextBitmap* ITextBitmapGenerator::GenerateBitmap() const
{
    return this->GenerateBitmapCore();
}

#ifdef _WIN32

// ================================================================================
// TextBitmapGeneratorWin32
// ================================================================================

TextBitmapGeneratorWin32::TextBitmapGeneratorWin32()
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

BillboardText::BillboardText(TextId textId, FontId fontId) : 
    mTextId(textId),
    mFontId(fontId)
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
    mCurrentTextId(1024),
    mpGraphicsContext(pGraphicsContext),
    mpVertexBuffer(nullptr),
    mpShaderProgram(nullptr),
    mpBitmapGenerator(nullptr)
{
    mpBitmapGenerator = CreateTextBitmapGenerator();
}

TextId BillboardTextGroup::Create(const FontSpecs& fontSpecs)
{
    auto textId = mCurrentTextId++;
    auto fontId = mpBitmapGenerator->CacheFont(fontSpecs);
    auto pBillboardText = new BillboardText(textId, fontId);
    auto pair = std::pair<TextId, BillboardText*>(textId, pBillboardText);
    return textId;
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
