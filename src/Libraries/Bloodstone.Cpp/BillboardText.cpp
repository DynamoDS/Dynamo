
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

ITextBitmapGenerator::~ITextBitmapGenerator()
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

void BillboardText::Update(const std::wstring& content)
{
    mTextContent.clear(); // Clear the existing content first.

    auto iterator = content.begin();
    for (; iterator != content.end(); ++iterator) {
        wchar_t character = *iterator;
        mTextContent.push_back(MAKEFONTCHARID(mFontId, character));
    }
}

void BillboardText::Update(const float* position)
{
    for (int i = 0; i < 4; i++)
        mWorldPosition[i] = position[i];
}

void BillboardText::UpdateForeground0(const float* rgba)
{
    for (int i = 0; i < 4; i++)
        mForegroundRgba0[i] = rgba[i];
}

void BillboardText::UpdateForeground1(const float* rgba)
{
    for (int i = 0; i < 4; i++)
        mForegroundRgba1[i] = rgba[i];
}

void BillboardText::UpdateBackground(const float* rgba)
{
    for (int i = 0; i < 4; i++)
        mBackgroundRgba[i] = rgba[i];
}

// ================================================================================
// BillboardTextGroup
// ================================================================================

BillboardTextGroup::BillboardTextGroup(IGraphicsContext* pGraphicsContext) : 
    mRegenerationHints(RegenerationHints::None),
    mCurrentTextId(1024),
    mpGraphicsContext(pGraphicsContext),
    mpVertexBuffer(nullptr),
    mpShaderProgram(nullptr),
    mpBitmapGenerator(nullptr)
{
    mpBitmapGenerator = CreateTextBitmapGenerator();
}

BillboardTextGroup::~BillboardTextGroup()
{
    auto iterator = mBillboardTexts.begin();
    for (; iterator != mBillboardTexts.end(); ++iterator)
        delete ((BillboardText *)(iterator->second));

    mBillboardTexts.clear();
}

TextId BillboardTextGroup::Create(const FontSpecs& fontSpecs)
{
    auto textId = mCurrentTextId++;
    auto fontId = mpBitmapGenerator->CacheFont(fontSpecs);
    auto pBillboardText = new BillboardText(textId, fontId);

    // Insert the newly created billboard text into the internal list.
    auto pair = std::pair<TextId, BillboardText*>(textId, pBillboardText);
    mBillboardTexts.insert(pair);


    return textId;
}

void BillboardTextGroup::Destroy(TextId textId)
{
    auto iterator = mBillboardTexts.find(textId);
    if (iterator == mBillboardTexts.end())
        return; // Text not found.

    mBillboardTexts.erase(iterator);
    delete ((BillboardText *)(iterator->second));
    ADDFLAG(mRegenerationHints, RegenerationHints::VertexBufferLayout);
}

void BillboardTextGroup::Render(void) const
{
    if (mRegenerationHints != RegenerationHints::None) {
        auto pThis = const_cast<BillboardTextGroup *>(this);
        pThis->RegenerateInternal();
    }
}

void BillboardTextGroup::UpdateText(TextId textId, const std::wstring& text)
{
    auto pBillboardText = GetBillboardText(textId);
    if (pBillboardText != nullptr) {
        pBillboardText->Update(text);
        ADDFLAG(mRegenerationHints, RegenerationHints::All);
    }
}

void BillboardTextGroup::UpdatePosition(TextId textId, const float* position)
{
    auto pBillboardText = GetBillboardText(textId);
    if (pBillboardText != nullptr) {
        pBillboardText->Update(position);
        ADDFLAG(mRegenerationHints, RegenerationHints::VertexBufferContent);
    }
}

void BillboardTextGroup::UpdateColor(TextId textId,
    const float* foregroundRgba,
    const float* backgroundRgba)
{
    auto pBillboardText = GetBillboardText(textId);
    if (pBillboardText == nullptr)
        return;

    pBillboardText->UpdateForeground0(foregroundRgba);
    pBillboardText->UpdateForeground1(foregroundRgba);
    pBillboardText->UpdateBackground(backgroundRgba);
    ADDFLAG(mRegenerationHints, RegenerationHints::VertexBufferContent);
}

void BillboardTextGroup::UpdateColor(TextId textId,
    const float* foregroundRgba0,
    const float* foregroundRgba1,
    const float* backgroundRgba)
{
    auto pBillboardText = GetBillboardText(textId);
    if (pBillboardText == nullptr)
        return;

    pBillboardText->UpdateForeground0(foregroundRgba0);
    pBillboardText->UpdateForeground1(foregroundRgba1);
    pBillboardText->UpdateBackground(backgroundRgba);
    ADDFLAG(mRegenerationHints, RegenerationHints::VertexBufferContent);
}

BillboardText* BillboardTextGroup::GetBillboardText(TextId textId) const
{
    auto iterator = mBillboardTexts.find(textId);
    return ((iterator == mBillboardTexts.end()) ? nullptr : iterator->second);
}

void BillboardTextGroup::RegenerateInternal(void)
{
    if (HASFLAG(mRegenerationHints, RegenerationHints::TextureContent)) {
    }

    if (HASFLAG(mRegenerationHints, RegenerationHints::VertexBufferLayout)) {
    }

    if (HASFLAG(mRegenerationHints, RegenerationHints::VertexBufferContent)) {
    }

    mRegenerationHints = RegenerationHints::None; // Clear all.
}
