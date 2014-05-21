
#ifndef _OPEN_INTERFACE_H_
#define _OPEN_INTERFACE_H_

#include "../Interfaces.h"
#include "../../../../extern/OpenGL/glext.h"

namespace Dynamorph { namespace OpenGL {

#define DEFGLPROC(t, n)   static t n;
#define GETGLPROC(t, n)   { n = ((t) wglGetProcAddress(#n)); }
#define INITGLPROC(t, n)  t GL::n = nullptr;

    class GL
    {
    public:
        static void Initialize()
        {
            GETGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
            GETGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
            GETGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
            GETGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
            GETGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
            GETGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
            GETGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
            GETGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
            GETGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
            GETGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
            GETGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
            GETGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
            GETGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
            GETGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
            GETGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
            GETGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
            GETGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
            GETGLPROC(PFNGLDRAWARRAYSEXTPROC,               glDrawArraysExt);
        }

        DEFGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
        DEFGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
        DEFGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
        DEFGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
        DEFGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
        DEFGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
        DEFGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
        DEFGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
        DEFGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
        DEFGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
        DEFGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
        DEFGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
        DEFGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
        DEFGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
        DEFGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
        DEFGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
        DEFGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
        DEFGLPROC(PFNGLDRAWARRAYSEXTPROC,               glDrawArraysExt);
    };

    class GraphicsContext : public Dynamorph::IGraphicsContext
    {
    public:
        GraphicsContext();

    protected:
        virtual void InitializeCore(HWND hWndOwner);
        virtual void UninitializeCore(void);
        virtual IVertexShader* CreateVertexShaderCore(
            const std::string& content) const;
        virtual IFragmentShader* CreateFragmentShaderCore(
            const std::string& content) const;
        virtual IShaderProgram* CreateShaderProgramCore(
            IVertexShader* pVertexShader, IFragmentShader* pFragmentShader);

    private:
        HWND mRenderWindow;
        HGLRC mhRenderContext;
    };

    class CommonShaderBase
    {
    public:
        CommonShaderBase(const GraphicsContext* pGraphicsContext);
        virtual ~CommonShaderBase(void);

        bool LoadFromContent(const std::string& content);
        GLuint GetShaderId(void) const;

    protected:
        virtual GLuint CreateShaderIdCore(void) const = 0;

    private:
        GLuint mShaderId;
        const GraphicsContext* mpGraphicsContext;
    };

    class VertexShader : public CommonShaderBase, public Dynamorph::IVertexShader
    {
    public:
        VertexShader(const GraphicsContext* pGraphicsContext);

    protected:
        virtual GLuint CreateShaderIdCore(void) const;
    };

    class FragmentShader : public CommonShaderBase, public Dynamorph::IFragmentShader
    {
    public:
        FragmentShader(const GraphicsContext* pGraphicsContext);

    protected:
        virtual GLuint CreateShaderIdCore(void) const;
    };

    class ShaderProgram : public Dynamorph::IShaderProgram
    {
    public:
        ShaderProgram(VertexShader* pVertexShader, FragmentShader* pFragmentShader);
        ~ShaderProgram(void);

    private:
        GLuint mProgramId;
        VertexShader* mpVertexShader;
        FragmentShader* mpFragmentShader;
    };
} }

#endif
