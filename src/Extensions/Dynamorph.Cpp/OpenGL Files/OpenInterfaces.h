
#ifndef _OPEN_INTERFACE_H_
#define _OPEN_INTERFACE_H_

#include "../Interfaces.h"
#include "../../../../extern/OpenGL/glcorearb.h"
#include "../../../../extern/OpenGL/glext.h"

namespace Dynamorph { namespace OpenGL {

#define DEFGLPROC(t, n)     static t n;
#define INITGLPROC(t, n)    t GL::n = nullptr;
#define GETGLPROC(t, n)     {                                           \
                                n = ((t) wglGetProcAddress(#n));        \
                                if (n == nullptr) {                     \
                                    OutputDebugStringA(#n);             \
                                    OutputDebugStringA(" [FAILED]\n");  \
                                }                                       \
                            }

    class GL
    {
    public:
        static void Initialize()
        {
            OutputDebugString(L"\nBegin OpenGL Initialization...\n");

            GETGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
            GETGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
            GETGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
            GETGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
            GETGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
            GETGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
            GETGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
            GETGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
            GETGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
            GETGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
            GETGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
            GETGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
            GETGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
            GETGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
            GETGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
            GETGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
            GETGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
            GETGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
            GETGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
            GETGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
            GETGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
            GETGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
            GETGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
            GETGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
            GETGLPROC(PFNGLCLEARPROC,                       glClear);
            GETGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);

            OutputDebugString(L"\nOpenGL Initialization Completed\n");
        }

        DEFGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
        DEFGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
        DEFGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
        DEFGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
        DEFGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
        DEFGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
        DEFGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
        DEFGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
        DEFGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
        DEFGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
        DEFGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
        DEFGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
        DEFGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
        DEFGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
        DEFGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
        DEFGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
        DEFGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
        DEFGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
        DEFGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
        DEFGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
        DEFGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
        DEFGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
        DEFGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
        DEFGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
        DEFGLPROC(PFNGLCLEARPROC,                       glClear);
        DEFGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);
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
        virtual IVertexBuffer* CreateVertexBufferCore(void) const;
        virtual void BeginRenderFrameCore(void) const;
        virtual void ActivateShaderProgramCore(IShaderProgram* pShaderProgram) const;
        virtual void RenderVertexBufferCore(IVertexBuffer* pVertexBuffer) const;
        virtual void EndRenderFrameCore(void) const;

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
        void Activate(void) const;

    private:
        GLuint mProgramId;
        VertexShader* mpVertexShader;
        FragmentShader* mpFragmentShader;
    };

    class VertexBuffer : public Dynamorph::IVertexBuffer
    {
    public:
        VertexBuffer();
        ~VertexBuffer();

    protected:
        virtual void LoadDataCore(const std::vector<float>& positions);
        virtual void LoadDataCore(const std::vector<float>& positions,
            const std::vector<float>& colors);

    private:
        void EnsureVertexBufferCreation(void);

        GLuint mVertexBufferId;
    };
} }

#endif
