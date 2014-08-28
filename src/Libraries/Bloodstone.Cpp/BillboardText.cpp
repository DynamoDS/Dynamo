
#include "stdafx.h"
#include "BillboardText.h"

using namespace Dynamo::Bloodstone;

// ================================================================================
// GlyphBitmap
// ================================================================================

int GlyphBitmap::Width() const
{
    return 0;
}

int GlyphBitmap::Height() const
{
    return 0;
}

unsigned char* GlyphBitmap::Data() const
{
    return nullptr;
}

// ================================================================================
// TextBitmapGenerator
// ================================================================================

TextBitmapGenerator::TextBitmapGenerator() :
    mContentUpdated(false),
    mCurrentFontId(1024),
    mpGlyphBitmap(nullptr)
{
}

TextBitmapGenerator::~TextBitmapGenerator()
{
}

FontId TextBitmapGenerator::CacheFont(const FontSpecs& fontSpecs)
{
    auto iterator = mFontSpecs.begin();
    for (; iterator != mFontSpecs.end(); ++iterator)
    {
        if (fontSpecs == iterator->second)
            return iterator->first;
    }

    auto fontId = mCurrentFontId++;
    std::pair<FontId, FontSpecs> pair(fontId, fontSpecs);
    mFontSpecs.insert(pair);

    mContentUpdated = true;
    return pair.first;
}

void TextBitmapGenerator::CacheGlyphs(const std::vector<GlyphId>& glyphs)
{
    auto iterator = glyphs.begin();
    for (; iterator != glyphs.end(); ++iterator)
    {
        // If glyph is not currently cached, add to pending list.
        if (mCachedGlyphs.find(*iterator) != mCachedGlyphs.end())
            continue;

        mContentUpdated = true;
        mGlyphsToCache.push_back(*iterator);
    }
}

const GlyphBitmap* TextBitmapGenerator::GenerateBitmap()
{
    if (mContentUpdated == false)
        return mpGlyphBitmap;

    mContentUpdated = false;
    mpGlyphBitmap = this->GenerateBitmapCore();
    return mpGlyphBitmap;
}

#ifdef _WIN32

// ================================================================================
// TextBitmapGeneratorWin32
// ================================================================================

TextBitmapGeneratorWin32::TextBitmapGeneratorWin32()
{
}

GlyphBitmap* TextBitmapGeneratorWin32::GenerateBitmapCore() const
{
    return nullptr;
}

TextBitmapGenerator* CreateTextBitmapGenerator(void)
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

TextId BillboardText::GetTextId(void) const
{
    return this->mTextId;
}

FontId BillboardText::GetFontId(void) const
{
    return this->mFontId;
}

const std::vector<GlyphId>& BillboardText::GetGlyphs(void) const
{
    return this->mTextContent;
}

void BillboardText::Update(const std::wstring& content)
{
    mTextContent.clear(); // Clear the existing content first.

    auto iterator = content.begin();
    for (; iterator != content.end(); ++iterator) {
        wchar_t character = *iterator;
        mTextContent.push_back(MAKEGLYPHID(mFontId, character));
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

#ifdef BLOODSTONE_EXPORTS

// ================================================================================
// BillboardTextGroup
// ================================================================================

BillboardTextGroup::BillboardTextGroup(IGraphicsContext* pGraphicsContext) : 
    mScreenSizeParamIndex(-1),
    mRegenerationHints(RegenerationHints::None),
    mCurrentTextId(1024),
    mpGraphicsContext(pGraphicsContext),
    mpVertexBuffer(nullptr),
    mpBillboardShader(nullptr),
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

    if (mpBillboardShader != nullptr) {
        delete mpBillboardShader;
        mpBillboardShader = nullptr;
    }

    if (mpVertexBuffer != nullptr) {
        delete mpVertexBuffer;
        mpVertexBuffer = nullptr;
    }
}

TextId BillboardTextGroup::CreateText(const FontSpecs& fontSpecs)
{
    auto textId = mCurrentTextId++;
    auto fontId = mpBitmapGenerator->CacheFont(fontSpecs);
    auto pBillboardText = new BillboardText(textId, fontId);

    // Insert the newly created billboard text into the internal list.
    std::pair<TextId, BillboardText*> pair(textId, pBillboardText);
    mBillboardTexts.insert(pair);
    ADDFLAG(mRegenerationHints, RegenerationHints::TextureContent);
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
    if (mBillboardTexts.size() <- 0) // Nothing to render, forget it.
        return;

    if (nullptr == mpVertexBuffer) {
        auto pThis = const_cast<BillboardTextGroup *>(this);
        pThis->Initialize();
    }

    if (mRegenerationHints != RegenerationHints::None) {
        auto pThis = const_cast<BillboardTextGroup *>(this);
        pThis->RegenerateInternal();
    }

    mpGraphicsContext->ActivateShaderProgram(mpBillboardShader);

    auto pCamera = mpGraphicsContext->GetDefaultCamera();
    mpBillboardShader->ApplyTransformation(pCamera);

    int width = 0, height = 0;
    mpGraphicsContext->GetDisplayPixelSize(width, height);
    float screenSize[] = { ((float) width), ((float) height) };
    mpBillboardShader->SetParameter(mScreenSizeParamIndex, screenSize, 2);

    mpVertexBuffer->Render();
}

void BillboardTextGroup::UpdateText(TextId textId, const std::wstring& text)
{
    auto pBillboardText = GetBillboardText(textId);
    if (pBillboardText != nullptr) {
        pBillboardText->Update(text);
        mpBitmapGenerator->CacheGlyphs(pBillboardText->GetGlyphs());
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

void BillboardTextGroup::Initialize(void)
{
    if (nullptr == mpVertexBuffer)
        mpVertexBuffer = mpGraphicsContext->CreateBillboardVertexBuffer();

    if (nullptr == mpBillboardShader) {
        auto name = ShaderName::BillboardText;
        mpBillboardShader = mpGraphicsContext->CreateShaderProgram(name);
    }

    mpBillboardShader->BindTransformMatrix(TransMatrix::Model, "model");
    mpBillboardShader->BindTransformMatrix(TransMatrix::View, "view");
    mpBillboardShader->BindTransformMatrix(TransMatrix::Projection, "proj");
    mScreenSizeParamIndex = mpBillboardShader->GetShaderParameterIndex("screenSize");
    mpVertexBuffer->BindToShaderProgram(mpBillboardShader);
}

void BillboardTextGroup::RegenerateInternal(void)
{
    if (HASFLAG(mRegenerationHints, RegenerationHints::TextureContent))
        RegenerateTexture();
    if (HASFLAG(mRegenerationHints, RegenerationHints::VertexBufferLayout))
        RegenerateVertexBuffer();
    if (HASFLAG(mRegenerationHints, RegenerationHints::VertexBufferContent))
        UpdateVertexBuffer();

    mRegenerationHints = RegenerationHints::None; // Clear all.
}

void BillboardTextGroup::RegenerateTexture(void)
{
}

void BillboardTextGroup::RegenerateVertexBuffer(void)
{
}

void BillboardTextGroup::UpdateVertexBuffer(void)
{
    const float position[]  = { 0.0f, 0.0f, 0.0f };
    const float texCoords[] = { 0.0f, 1.0f, 1.0f, 0.0f };
    const float offset[]    = { 0.0f, 16.0f, 64.0f, 0.0f };
    const float rgba0[]     = { 0.0f, 1.0f, 0.0f, 1.0f };
    const float rgba1[]     = { 0.0f, 0.0f, 1.0f, 1.0f };

    std::vector<BillboardVertex> vertices;
    BillboardQuadInfo quadInfo(vertices);
    quadInfo.position3 = position;
    quadInfo.offset4 = offset;
    quadInfo.texCoords4 = texCoords;
    quadInfo.foregroundRgba0 = rgba0;
    quadInfo.foregroundRgba1 = rgba1;
    FillQuad(quadInfo);

    mpVertexBuffer->Update(vertices);
}

void BillboardTextGroup::FillQuad(const BillboardQuadInfo& quadInfo) const
{
    const float* pos    = quadInfo.position3;
    const float* rgba0  = quadInfo.foregroundRgba0;
    const float* rgba1  = quadInfo.foregroundRgba1;
    const float* tc     = quadInfo.texCoords4;
    const float* off    = quadInfo.offset4;

    // Left-top vertex.
    const float tc0[] = { tc[0], tc[1] };
    const float off0[] = { off[0], off[1] };
    BillboardVertex lt(pos, &tc0[0], &off0[0], rgba0);

    // Right-top vertex.
    const float tc1[] = { tc[2], tc[1] };
    const float off1[] = { off[2], off[1] };
    BillboardVertex rt(pos, &tc1[0], &off1[0], rgba0);

    // Left-bottom vertex.
    const float tc2[] = { tc[0], tc[3] };
    const float off2[] = { off[0], off[3] };
    BillboardVertex lb(pos, &tc2[0], &off2[0], rgba1);

    // Right-bottom vertex.
    const float tc3[] = { tc[2], tc[3] };
    const float off3[] = { off[2], off[3] };
    BillboardVertex rb(pos, &tc3[0], &off3[0], rgba1);

    quadInfo.vertices.push_back(lt); // First triangle.
    quadInfo.vertices.push_back(rt);
    quadInfo.vertices.push_back(lb);
    quadInfo.vertices.push_back(lb); // Second triangle.
    quadInfo.vertices.push_back(rt);
    quadInfo.vertices.push_back(rb);
}

#endif // BLOODSTONE_EXPORTS
