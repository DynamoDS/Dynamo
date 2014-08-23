
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

    enum class FontFlags : unsigned short
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
    };

    struct TextData
    {
        std::vector<FontCharId> text; // Text content
        unsigned char foregroundRgba0[4]; // Top foreground color.
        unsigned char foregroundRgba1[4]; // Bottom foreground color.
        unsigned char backgroundRgba[4];  // Background shadow color.
        float worldPosition[4]; // 4th entry ignored by vertex shader.
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
        void Cache(const FontSpecs& fontSpecs, const std::wstring& text);
        TextBitmap* GenerateBitmap() const;

    protected:
        virtual void CacheCore(const FontSpecs& fontSpecs, const std::wstring& text) = 0;
        virtual TextBitmap* GenerateBitmapCore() const = 0;
    };

#ifdef _WIN32

    class TextBitmapGeneratorWin32 : ITextBitmapGenerator
    {
    protected:
        virtual void CacheCore(const FontSpecs& fontSpecs, const std::wstring& text);
        virtual TextBitmap* GenerateBitmapCore() const;
    };

#endif

    class BillboardTextGroup
    {
    public:
        BillboardTextGroup(IGraphicsContext* pGraphicsContext);
        TextId Create(const FontSpecs& fontSpecs);
        void Destroy(TextId textId);
        void Render(void) const;
        void Update(TextId textId,
            const std::wstring& text,
            const float* position);
        void UpdateColor(TextId textId,
            const float* foregroundRgba,
            const float* backgroundRgba);
        void UpdateColor(TextId textId,
            const float* foregroundRgba0,
            const float* foregroundRgba1,
            const float* backgroundRgba);

    private:
        IVertexBuffer* mpVertexBuffer;
        IShaderProgram* mpShaderProgram;
        ITextBitmapGenerator* mpBitmapGenerator;
        IGraphicsContext* mpGraphicsContext;
    };

} }

#endif
