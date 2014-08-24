
#ifndef _BILLBOARD_TEXT_H_
#define _BILLBOARD_TEXT_H_

#include "Interfaces.h"
#include <string>
#include <vector>

#define MAKEGLYPHID(fid, c) (((fid & 0x0000ffff) << 16) | (c & 0x0000ffff))
#define ADDFLAG(c, n)       ((RegenerationHints)(c | n))
#define HASFLAG(c, f)       ((c & f) != RegenerationHints::None)

namespace Dynamo { namespace Bloodstone {

    typedef unsigned int    TextId;
    typedef unsigned short  FontId;
    typedef unsigned int    GlyphId;

    enum FontFlags : unsigned short
    {
        Thin      = 0x0001,
        Bold      = 0x0002,
        Italic    = 0x0004,
        Underline = 0x0008,
        StrikeOut = 0x0010
    };

    struct FontSpecs
    {
        std::wstring face;
        int height;
        FontFlags flags;

        inline bool operator==(const FontSpecs& other) const
        {
            if (height != other.height || (flags != other.flags))
                return false;

            return this->face == other.face;
        }
    };

    struct GlyphMetrics
    {
        int innerWidth;
        int innerHeight;
        int outerWidth;
        int outerHeight;
        float texCoords[4];
        float advance;
    };

    class TextBitmap
    {
    public:
        int Width() const;
        int Height() const;
        unsigned char* Data() const;

    private:
        int mPixelWidth, mPixelHeight;
        unsigned char* mpBitmapData;
    };

    class ITextBitmapGenerator
    {
    public:
        ITextBitmapGenerator();
        virtual ~ITextBitmapGenerator();

        FontId CacheFont(const FontSpecs& fontSpecs);
        TextBitmap* GenerateBitmap() const;

    protected:
        virtual TextBitmap* GenerateBitmapCore() const = 0;

    protected:
        FontId mCurrentFontId;
        std::map<FontId, FontSpecs> mFontSpecs;
        std::map<GlyphId, GlyphMetrics> mCachedGlyphs;
    };

#ifdef _WIN32

    class TextBitmapGeneratorWin32 : public ITextBitmapGenerator
    {
    public:
        TextBitmapGeneratorWin32();

    protected:
        virtual TextBitmap* GenerateBitmapCore() const;
    };

#endif

    enum RegenerationHints
    {
        None                = 0x00000000,
        VertexBufferContent = 0x00000001,
        VertexBufferLayout  = 0x00000002 | VertexBufferContent,
        TextureContent      = 0x00000004 | VertexBufferLayout,

        All = TextureContent | VertexBufferLayout | VertexBufferContent
    };

    class BillboardText
    {
    public:
        BillboardText(TextId textId, FontId fontId);
        void Update(const std::wstring& content);
        void Update(const float* position);
        void UpdateForeground0(const float* rgba);
        void UpdateForeground1(const float* rgba);
        void UpdateBackground(const float* rgba);

    private:
        TextId mTextId;
        FontId mFontId;
        std::vector<GlyphId> mTextContent;
        float mForegroundRgba0[4]; // Top foreground color.
        float mForegroundRgba1[4]; // Bottom foreground color.
        float mBackgroundRgba[4];  // Background shadow color.
        float mWorldPosition[4]; // 4th entry ignored by vertex shader.
    };

    class BillboardTextGroup
    {
    public:
        BillboardTextGroup(IGraphicsContext* pGraphicsContext);
        ~BillboardTextGroup();

        TextId Create(const FontSpecs& fontSpecs);
        void Destroy(TextId textId);
        void Render(void) const;
        void UpdateText(TextId textId,
            const std::wstring& text);
        void UpdatePosition(TextId textId,
            const float* position);
        void UpdateColor(TextId textId,
            const float* foregroundRgba,
            const float* backgroundRgba);
        void UpdateColor(TextId textId,
            const float* foregroundRgba0,
            const float* foregroundRgba1,
            const float* backgroundRgba);

    private:

        BillboardText* GetBillboardText(TextId textId) const;
        void RegenerateInternal(void);
        void RegenerateTexture(void);
        void RegenerateVertexBuffer(void);
        void UpdateVertexBuffer(void);

        TextId mCurrentTextId;
        std::map<TextId, BillboardText*> mBillboardTexts;

        RegenerationHints mRegenerationHints;
        IVertexBuffer* mpVertexBuffer;
        IShaderProgram* mpShaderProgram;
        ITextBitmapGenerator* mpBitmapGenerator;
        IGraphicsContext* mpGraphicsContext;
    };

} }

#endif
