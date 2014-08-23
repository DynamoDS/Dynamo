
#ifndef _BILLBOARD_TEXT_H_
#define _BILLBOARD_TEXT_H_

#include "Interfaces.h"
#include <string>
#include <vector>

#define MAKEFONTCHARID(fid, c)   (((fid & 0x0000ffff) << 16) | (c & 0x0000ffff))

namespace Dynamo { namespace Bloodstone {

    typedef unsigned int    TextId;
    typedef unsigned short  FontId;
    typedef unsigned int    FontCharId;

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
        std::vector<FontCharId> mCachedCharacters;
        std::map<FontId, FontSpecs> mFontSpecs;
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
        std::vector<FontCharId> mTextContent;
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

        TextId mCurrentTextId;
        std::map<TextId, BillboardText*> mBillboardTexts;

        IVertexBuffer* mpVertexBuffer;
        IShaderProgram* mpShaderProgram;
        ITextBitmapGenerator* mpBitmapGenerator;
        IGraphicsContext* mpGraphicsContext;
    };

} }

#endif
