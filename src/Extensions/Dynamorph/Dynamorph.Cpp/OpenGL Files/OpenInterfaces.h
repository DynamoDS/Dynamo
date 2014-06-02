
#ifndef _OPEN_INTERFACE_H_
#define _OPEN_INTERFACE_H_

#include "../Interfaces.h"
#include "../../../../../extern/OpenGL/glcorearb.h"
#include "../../../../../extern/OpenGL/glext.h"
#include "../../../../../extern/OpenGL/wglext.h"
#include "../../../../../extern/OpenGL/glm/glm/glm.hpp"
#include "../../../../../extern/OpenGL/glm/glm/gtc/matrix_transform.hpp"
#include "../../../../../extern/OpenGL/glm/glm/gtc/type_ptr.hpp"

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

            GETGLPROC(PFNGLGETSTRINGPROC,                   glGetString);
            GETGLPROC(PFNGLGETINTEGERVPROC,                 glGetIntegerv);
            GETGLPROC(PFNGLENABLEPROC,                      glEnable);
            GETGLPROC(PFNGLDISABLEPROC,                     glDisable);
            GETGLPROC(PFNGLPOLYGONMODEPROC,                 glPolygonMode);
            GETGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
            GETGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
            GETGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
            GETGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
            GETGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
            GETGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
            GETGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
            GETGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
            GETGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
            GETGLPROC(PFNGLBINDATTRIBLOCATIONPROC,          glBindAttribLocation);
            GETGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
            GETGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
            GETGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
            GETGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
            GETGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
            GETGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
            GETGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
            GETGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
            GETGLPROC(PFNGLGENVERTEXARRAYSPROC,             glGenVertexArrays);
            GETGLPROC(PFNGLDELETEVERTEXARRAYSPROC,          glDeleteVertexArrays);
            GETGLPROC(PFNGLBINDVERTEXARRAYPROC,             glBindVertexArray);
            GETGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
            GETGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
            GETGLPROC(PFNGLUNIFORMMATRIX4FVPROC,            glUniformMatrix4fv);
            GETGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
            GETGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
            GETGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
            GETGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
            GETGLPROC(PFNGLVIEWPORTPROC,                    glViewport);
            GETGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
            GETGLPROC(PFNGLCLEARPROC,                       glClear);
            GETGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);

            OutputDebugString(L"\nOpenGL Initialization Completed\n");
        }

        DEFGLPROC(PFNGLGETSTRINGPROC,                   glGetString);
        DEFGLPROC(PFNGLGETINTEGERVPROC,                 glGetIntegerv);
        DEFGLPROC(PFNGLENABLEPROC,                      glEnable);
        DEFGLPROC(PFNGLDISABLEPROC,                     glDisable);
        DEFGLPROC(PFNGLPOLYGONMODEPROC,                 glPolygonMode);
        DEFGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
        DEFGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
        DEFGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
        DEFGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
        DEFGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
        DEFGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
        DEFGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
        DEFGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
        DEFGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
        DEFGLPROC(PFNGLBINDATTRIBLOCATIONPROC,          glBindAttribLocation);
        DEFGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
        DEFGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
        DEFGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
        DEFGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
        DEFGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
        DEFGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
        DEFGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
        DEFGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
        DEFGLPROC(PFNGLGENVERTEXARRAYSPROC,             glGenVertexArrays);
        DEFGLPROC(PFNGLDELETEVERTEXARRAYSPROC,          glDeleteVertexArrays);
        DEFGLPROC(PFNGLBINDVERTEXARRAYPROC,             glBindVertexArray);
        DEFGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
        DEFGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
        DEFGLPROC(PFNGLUNIFORMMATRIX4FVPROC,            glUniformMatrix4fv);
        DEFGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
        DEFGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
        DEFGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
        DEFGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
        DEFGLPROC(PFNGLVIEWPORTPROC,                    glViewport);
        DEFGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
        DEFGLPROC(PFNGLCLEARPROC,                       glClear);
        DEFGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);
    };

    class Camera; // Forward declaration.

    class GraphicsContext : public Dynamorph::IGraphicsContext
    {
    public:
        GraphicsContext();

    protected:
        virtual void InitializeCore(HWND hWndOwner);
        virtual void UninitializeCore(void);
        virtual ICamera* GetDefaultCameraCore(void) const;
        virtual IVertexShader* CreateVertexShaderCore(
            const std::string& content) const;
        virtual IFragmentShader* CreateFragmentShaderCore(
            const std::string& content) const;
        virtual IShaderProgram* CreateShaderProgramCore(
            IVertexShader* pVertexShader, IFragmentShader* pFragmentShader);
        virtual IVertexBuffer* CreateVertexBufferCore(void) const;
        virtual void BeginRenderFrameCore(HDC deviceContext) const;
        virtual void ActivateShaderProgramCore(IShaderProgram* pShaderProgram) const;
        virtual void RenderVertexBufferCore(IVertexBuffer* pVertexBuffer) const;
        virtual void EndRenderFrameCore(HDC deviceContext) const;

    private:
        HWND mRenderWindow;
        HGLRC mhRenderContext;
        Camera* mpDefaultCamera;
    };

    class TrackBall : public Dynamorph::ITrackBall
    {
    public:
        TrackBall(Camera* pCamera);

    protected:
        virtual void MousePressedCore(int screenX, int screenY);
        virtual void MouseMovedCore(int screenX, int screenY);
        virtual void MouseReleasedCore(int screenX, int screenY);

    private:
        Camera* mpCamera;
    };

    class Camera : public Dynamorph::ICamera
    {
    public:
        Camera(GraphicsContext* pGraphicsContext);
        void GetMatrices(glm::mat4& model, glm::mat4& view, glm::mat4& proj) const;
        GraphicsContext* GetGraphicsContext(void) const;

    protected:
        virtual void ConfigureCore(const CameraConfiguration* pConfiguration);
        virtual void FitToBoundingBoxCore(const BoundingBox* pBoundingBox);
        virtual Dynamorph::ITrackBall* GetTrackBallCore() const;

    private:
        glm::mat4 mModelMatrix;
        glm::mat4 mViewMatrix;
        glm::mat4 mProjMatrix;
        TrackBall* mpTrackBall;
        CameraConfiguration mConfiguration;
        GraphicsContext* mpGraphicsContext;
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

    protected:
        virtual void BindTransformMatrixCore(TransMatrix transform, const std::string& name);
        virtual void ApplyTransformationCore(const ICamera* pCamera) const;

    private:
        GLuint mProgramId;
        GLint mModelMatrixUniform;
        GLint mViewMatrixUniform;
        GLint mProjMatrixUniform;
        VertexShader* mpVertexShader;
        FragmentShader* mpFragmentShader;
    };

    struct VertexData
    {
        float x, y, z;
        // float nx, ny, nz;
        float r, g, b, a;

        VertexData() :
            x(0.0f), y(0.0f), z(0.0f),
            r(1.0f), g(1.0f), b(1.0f), a(1.0f)
        {
        }
    };

    class VertexBuffer : public Dynamorph::IVertexBuffer
    {
    public:
        VertexBuffer(void);
        ~VertexBuffer(void);
        void Render(void) const;

    protected:
        virtual void LoadDataCore(const GeometryData& geometries);
        virtual void GetBoundingBoxCore(BoundingBox* pBoundingBox);

    private:
        void EnsureVertexBufferCreation(void);
        void LoadDataInternal(const std::vector<VertexData>& vertices);

        int mVertexCount;
        std::vector<int> mSegmentVertexCount;

        GLuint mVertexArrayId;
        GLuint mVertexBufferId;
        BoundingBox mBoundingBox;
        PrimitiveType mPrimitiveType;
    };
} }

#endif
