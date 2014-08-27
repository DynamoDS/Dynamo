
#ifndef _OPEN_INTERFACE_H_
#define _OPEN_INTERFACE_H_

#include "Utilities.h"
#include "Constants.h"
#include "../../../../extern/OpenGL/glcorearb.h"
#include "../../../../extern/OpenGL/glext.h"
#include "../../../../extern/OpenGL/wglext.h"
#include "../../../../extern/OpenGL/glm/glm/glm.hpp"
#include "../../../../extern/OpenGL/glm/glm/gtc/matrix_transform.hpp"
#include "../../../../extern/OpenGL/glm/glm/gtc/type_ptr.hpp"

#include "gl/gl.h" // Legacy OpenGL APIs to be included last.

namespace Dynamo { namespace Bloodstone { namespace OpenGL {

#define DEFGLPROC(t, n)     static t n;
#define INITGLPROC(t, n)    t GL::n = nullptr;
#define GETGLPROC(t, n)     {                                           \
                                n = ((t) wglGetProcAddress(#n));        \
                                message.append(#n);                     \
                                if (n == nullptr) {                     \
                                    OutputDebugStringA(#n);             \
                                    OutputDebugStringA(" [FAILED]\n");  \
                                    message.append(" [FAILED]\n");      \
                                } else {                                \
                                    message.append(" [SUCEEDED]\n");    \
                                }                                       \
                            }

#define GETLEGACYPROC(n)    {                                           \
                                if (n == nullptr)                       \
                                    n = ::n;                            \
                            }

    class GL
    {
    public:
        static bool Initialize()
        {
            // These values are to be populated by 'GETGLPROC' macro.
            std::string message;
            OutputDebugString(L"\nBegin OpenGL Initialization...\n");

            // Legacy OpenGL APIs.
            GETGLPROC(PFNGLBINDTEXTUREPROC,                 glBindTexture);
            GETGLPROC(PFNGLCLEARPROC,                       glClear);
            GETGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);
            GETGLPROC(PFNGLDISABLEPROC,                     glDisable);
            GETGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
            GETGLPROC(PFNGLENABLEPROC,                      glEnable);
            GETGLPROC(PFNGLGENTEXTURESPROC,                 glGenTextures);
            GETGLPROC(PFNGLGETINTEGERVPROC,                 glGetIntegerv);
            GETGLPROC(PFNGLGETSTRINGPROC,                   glGetString);
            GETGLPROC(PFNGLPOINTSIZEPROC,                   glPointSize);
            GETGLPROC(PFNGLPOLYGONMODEPROC,                 glPolygonMode);
            GETGLPROC(PFNGLTEXIMAGE2DPROC,                  glTexImage2D);
            GETGLPROC(PFNGLTEXPARAMETERFPROC,               glTexParameterf);
            GETGLPROC(PFNGLTEXPARAMETERIPROC,               glTexParameteri);
            GETGLPROC(PFNGLVIEWPORTPROC,                    glViewport);

            // Modern OpenGL APIs.
            GETGLPROC(PFNGLACTIVETEXTUREPROC,               glActiveTexture);
            GETGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
            GETGLPROC(PFNGLBINDATTRIBLOCATIONPROC,          glBindAttribLocation);
            GETGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
            GETGLPROC(PFNGLBINDVERTEXARRAYPROC,             glBindVertexArray);
            GETGLPROC(PFNGLBLENDEQUATIONSEPARATEPROC,       glBlendEquationSeparate);
            GETGLPROC(PFNGLBLENDFUNCSEPARATEPROC,           glBlendFuncSeparate);
            GETGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
            GETGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
            GETGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
            GETGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
            GETGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
            GETGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
            GETGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
            GETGLPROC(PFNGLDELETEVERTEXARRAYSPROC,          glDeleteVertexArrays);
            GETGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
            GETGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
            GETGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
            GETGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
            GETGLPROC(PFNGLGENVERTEXARRAYSPROC,             glGenVertexArrays);
            GETGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
            GETGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
            GETGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
            GETGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
            GETGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
            GETGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
            GETGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
            GETGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
            GETGLPROC(PFNGLUNIFORM1FPROC,                   glUniform1f);
            GETGLPROC(PFNGLUNIFORM1IPROC,                   glUniform1i);
            GETGLPROC(PFNGLUNIFORM2FPROC,                   glUniform2f);
            GETGLPROC(PFNGLUNIFORM2IPROC,                   glUniform2i);
            GETGLPROC(PFNGLUNIFORM3FPROC,                   glUniform3f);
            GETGLPROC(PFNGLUNIFORM3IPROC,                   glUniform3i);
            GETGLPROC(PFNGLUNIFORM4FPROC,                   glUniform4f);
            GETGLPROC(PFNGLUNIFORM4IPROC,                   glUniform4i);
            GETGLPROC(PFNGLUNIFORMMATRIX4FVPROC,            glUniformMatrix4fv);
            GETGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
            GETGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
            GETGLPROC(PFNWGLCHOOSEPIXELFORMATARBPROC,       wglChoosePixelFormatARB);
            GETGLPROC(PFNWGLCREATECONTEXTATTRIBSARBPROC,    wglCreateContextAttribsARB);

            GETLEGACYPROC(glBindTexture);
            GETLEGACYPROC(glClear);
            GETLEGACYPROC(glClearColor);
            GETLEGACYPROC(glDisable);
            GETLEGACYPROC(glDrawArrays);
            GETLEGACYPROC(glEnable);
            GETLEGACYPROC(glGenTextures);
            GETLEGACYPROC(glGetIntegerv);
            GETLEGACYPROC(glGetString);
            GETLEGACYPROC(glPointSize);
            GETLEGACYPROC(glPolygonMode);
            GETLEGACYPROC(glTexImage2D);
            GETLEGACYPROC(glTexParameterf);
            GETLEGACYPROC(glTexParameteri);
            GETLEGACYPROC(glViewport);

            auto pMessageData = message.c_str();
            OutputDebugStringA(pMessageData);
            OutputDebugString(L"\nOpenGL Initialization Completed\n");

            // These two creation methods have to exist.
            return ((nullptr != wglChoosePixelFormatARB) && 
                    (nullptr != wglCreateContextAttribsARB));
        }

        // Legacy OpenGL APIs.
        DEFGLPROC(PFNGLBINDTEXTUREPROC,                 glBindTexture);
        DEFGLPROC(PFNGLCLEARPROC,                       glClear);
        DEFGLPROC(PFNGLCLEARCOLORPROC,                  glClearColor);
        DEFGLPROC(PFNGLDISABLEPROC,                     glDisable);
        DEFGLPROC(PFNGLDRAWARRAYSPROC,                  glDrawArrays);
        DEFGLPROC(PFNGLENABLEPROC,                      glEnable);
        DEFGLPROC(PFNGLGENTEXTURESPROC,                 glGenTextures);
        DEFGLPROC(PFNGLGETINTEGERVPROC,                 glGetIntegerv);
        DEFGLPROC(PFNGLGETSTRINGPROC,                   glGetString);
        DEFGLPROC(PFNGLPOINTSIZEPROC,                   glPointSize);
        DEFGLPROC(PFNGLPOLYGONMODEPROC,                 glPolygonMode);
        DEFGLPROC(PFNGLTEXIMAGE2DPROC,                  glTexImage2D);
        DEFGLPROC(PFNGLTEXPARAMETERFPROC,               glTexParameterf);
        DEFGLPROC(PFNGLTEXPARAMETERIPROC,               glTexParameteri);
        DEFGLPROC(PFNGLVIEWPORTPROC,                    glViewport);

        // Modern OpenGL APIs.
        DEFGLPROC(PFNGLACTIVETEXTUREPROC,               glActiveTexture);
        DEFGLPROC(PFNGLATTACHSHADERPROC,                glAttachShader);
        DEFGLPROC(PFNGLBINDATTRIBLOCATIONPROC,          glBindAttribLocation);
        DEFGLPROC(PFNGLBINDBUFFERPROC,                  glBindBuffer);
        DEFGLPROC(PFNGLBINDVERTEXARRAYPROC,             glBindVertexArray);
        DEFGLPROC(PFNGLBLENDEQUATIONSEPARATEPROC,       glBlendEquationSeparate);
        DEFGLPROC(PFNGLBLENDFUNCSEPARATEPROC,           glBlendFuncSeparate);
        DEFGLPROC(PFNGLBUFFERDATAPROC,                  glBufferData);
        DEFGLPROC(PFNGLCOMPILESHADERPROC,               glCompileShader);
        DEFGLPROC(PFNGLCREATEPROGRAMPROC,               glCreateProgram);
        DEFGLPROC(PFNGLCREATESHADERPROC,                glCreateShader);
        DEFGLPROC(PFNGLDELETEBUFFERSPROC,               glDeleteBuffers);
        DEFGLPROC(PFNGLDELETEPROGRAMPROC,               glDeleteProgram);
        DEFGLPROC(PFNGLDELETESHADERPROC,                glDeleteShader);
        DEFGLPROC(PFNGLDELETEVERTEXARRAYSPROC,          glDeleteVertexArrays);
        DEFGLPROC(PFNGLDETACHSHADERPROC,                glDetachShader);
        DEFGLPROC(PFNGLDISABLEVERTEXATTRIBARRAYPROC,    glDisableVertexAttribArray);
        DEFGLPROC(PFNGLENABLEVERTEXATTRIBARRAYPROC,     glEnableVertexAttribArray);
        DEFGLPROC(PFNGLGENBUFFERSPROC,                  glGenBuffers);
        DEFGLPROC(PFNGLGENVERTEXARRAYSPROC,             glGenVertexArrays);
        DEFGLPROC(PFNGLGETATTRIBLOCATIONPROC,           glGetAttribLocation);
        DEFGLPROC(PFNGLGETPROGRAMINFOLOGPROC,           glGetProgramInfoLog);
        DEFGLPROC(PFNGLGETPROGRAMIVPROC,                glGetProgramiv);
        DEFGLPROC(PFNGLGETSHADERINFOLOGPROC,            glGetShaderInfoLog);
        DEFGLPROC(PFNGLGETSHADERIVPROC,                 glGetShaderiv);
        DEFGLPROC(PFNGLGETUNIFORMLOCATIONPROC,          glGetUniformLocation);
        DEFGLPROC(PFNGLLINKPROGRAMPROC,                 glLinkProgram);
        DEFGLPROC(PFNGLSHADERSOURCEPROC,                glShaderSource);
        DEFGLPROC(PFNGLUNIFORM1FPROC,                   glUniform1f);
        DEFGLPROC(PFNGLUNIFORM1IPROC,                   glUniform1i);
        DEFGLPROC(PFNGLUNIFORM2FPROC,                   glUniform2f);
        DEFGLPROC(PFNGLUNIFORM2IPROC,                   glUniform2i);
        DEFGLPROC(PFNGLUNIFORM3FPROC,                   glUniform3f);
        DEFGLPROC(PFNGLUNIFORM3IPROC,                   glUniform3i);
        DEFGLPROC(PFNGLUNIFORM4FPROC,                   glUniform4f);
        DEFGLPROC(PFNGLUNIFORM4IPROC,                   glUniform4i);
        DEFGLPROC(PFNGLUNIFORMMATRIX4FVPROC,            glUniformMatrix4fv);
        DEFGLPROC(PFNGLUSEPROGRAMPROC,                  glUseProgram);
        DEFGLPROC(PFNGLVERTEXATTRIBPOINTERPROC,         glVertexAttribPointer);
        DEFGLPROC(PFNWGLCHOOSEPIXELFORMATARBPROC,       wglChoosePixelFormatARB);
        DEFGLPROC(PFNWGLCREATECONTEXTATTRIBSARBPROC,    wglCreateContextAttribsARB);
    };

    class Camera; // Forward declaration.

    class GraphicsContext : public Dynamo::Bloodstone::IGraphicsContext
    {
    public:
        GraphicsContext();

    protected:
        virtual bool InitializeCore(HWND hWndOwner);
        virtual void UninitializeCore(void);
        virtual ICamera* GetDefaultCameraCore(void) const;
        virtual void GetDisplayPixelSizeCore(int& width, int& height) const;
        virtual IVertexShader* CreateVertexShaderCore(
            const std::string& content) const;
        virtual IFragmentShader* CreateFragmentShaderCore(
            const std::string& content) const;
        virtual IShaderProgram* CreateShaderProgramCore(ShaderName shaderName) const;
        virtual IVertexBuffer* CreateVertexBufferCore(void) const;
        virtual IBillboardVertexBuffer* CreateBillboardVertexBufferCore(void) const;
        virtual void BeginRenderFrameCore(HDC deviceContext) const;
        virtual void ActivateShaderProgramCore(IShaderProgram* pShaderProgram) const;
        virtual void RenderVertexBufferCore(IVertexBuffer* pVertexBuffer) const;
        virtual bool EndRenderFrameCore(HDC deviceContext) const;
        virtual void EnableAlphaBlendCore(void) const;
        virtual void ClearDepthBufferCore(void) const;

    private:
        bool InitializeWithDummyContext(HWND hWndOwner);
        bool SelectBestPixelFormat(HDC hDeviceContext) const;
        bool GetDeviceAttributes(int hardwareLevel, int* pAttributes) const;

        int mMajorVersion;
        int mMinorVersion;
        HWND mRenderWindow;
        HGLRC mhRenderContext;
        Camera* mpDefaultCamera;
    };

    class TrackBall : public Dynamo::Bloodstone::ITrackBall
    {
    public:
        TrackBall(Camera* pCamera);

    protected:
        virtual void MousePressedCore(int screenX, int screenY, Mode mode);
        virtual void MouseMovedCore(int screenX, int screenY);
        virtual void MouseReleasedCore(int screenX, int screenY);

    private:

        glm::vec2 GetMouseOnScreen(int screenX, int screenY) const;
        glm::vec3 GetProjectionOnTrackball(int screenX, int screenY) const;
        void RotateCameraInternal(int screenX, int screenY);
        void ZoomCameraInternal(int screenX, int screenY);
        void PanCameraInternal(int screenX, int screenY);
        void UpdateCameraInternal(void);

    private:
        Camera* mpCamera;
        CameraConfiguration mConfiguration;
        ITrackBall::Mode mTrackBallMode;

        static const float ZoomSpeed;
        static const float PanSpeed;
        static const float DynamicDampingFactor;

        // Camera manipulation related data members.
        glm::vec3 mCameraUpVector;
        glm::vec3 mCameraPosition;
        glm::vec3 mTargetPosition;
        glm::vec2 mPanStart, mPanEnd;
        glm::vec2 mZoomStart, mZoomEnd;
        glm::vec3 mRotateStart, mRotateEnd;
    };

    class Camera : public Dynamo::Bloodstone::ICamera
    {
    public:
        Camera(GraphicsContext* pGraphicsContext);
        void GetMatrices(glm::mat4& model, glm::mat4& view, glm::mat4& proj) const;
        GraphicsContext* GetGraphicsContext(void) const;

    protected:
        virtual void ConfigureCore(const CameraConfiguration* pConfiguration);
        virtual void BeginConfigureCore(const CameraConfiguration* pConfiguration);
        virtual void GetConfigurationCore(CameraConfiguration* pConfiguration) const;
        virtual void ResizeViewportCore(int width, int height);
        virtual bool IsInTransitionCore(void) const;
        virtual void UpdateFrameCore(void);
        virtual Dynamo::Bloodstone::ITrackBall* GetTrackBallCore() const;

    private:
        void InitializeTransition(const CameraConfiguration* pConfiguration);
        void FinalizeCurrentTransition(void);
        void ConfigureInternal(const CameraConfiguration* pConfiguration);

        glm::mat4 mModelMatrix;
        glm::mat4 mViewMatrix;
        glm::mat4 mProjMatrix;
        TrackBall* mpTrackBall;
        Interpolator* mpInterpolator;
        CameraConfiguration mBeginConfigValue;
        CameraConfiguration mFinalConfigValue;
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

    class VertexShader : public CommonShaderBase, public Dynamo::Bloodstone::IVertexShader
    {
    public:
        VertexShader(const GraphicsContext* pGraphicsContext);

    protected:
        virtual GLuint CreateShaderIdCore(void) const;
    };

    class FragmentShader : public CommonShaderBase, public Dynamo::Bloodstone::IFragmentShader
    {
    public:
        FragmentShader(const GraphicsContext* pGraphicsContext);

    protected:
        virtual GLuint CreateShaderIdCore(void) const;
    };

    class ShaderProgram : public Dynamo::Bloodstone::IShaderProgram
    {
    public:
        ShaderProgram(VertexShader* pVertexShader, FragmentShader* pFragmentShader);
        ~ShaderProgram(void);
        void Activate(void) const;
        int GetAttributeLocation(const std::string& name) const;

    protected:
        virtual int GetShaderParameterIndexCore(const std::string& name) const;
        virtual void SetParameterCore(int index, const float* pValues, int count) const;
        virtual void BindTransformMatrixCore(TransMatrix transform, const std::string& name);
        virtual void ApplyTransformationCore(const ICamera* pCamera) const;

    private:
        GLuint mProgramId;
        GLint mModelMatrixUniform;
        GLint mViewMatrixUniform;
        GLint mProjMatrixUniform;
        GLint mNormMatrixUniform;
        VertexShader* mpVertexShader;
        FragmentShader* mpFragmentShader;
    };

    struct VertexData
    {
        float x, y, z;
        float nx, ny, nz;
        float r, g, b, a;

        VertexData() :
            x(0.0f), y(0.0f), z(0.0f),
            nx(1.0f), ny(1.0f), nz(1.0f),
            r(1.0f), g(1.0f), b(1.0f), a(1.0f)
        {
        }
    };

    class VertexBuffer : public Dynamo::Bloodstone::IVertexBuffer
    {
    public:
        VertexBuffer(void);
        ~VertexBuffer(void);
        void Render(void) const;

    protected:
        virtual PrimitiveType GetPrimitiveTypeCore() const;
        virtual void LoadDataCore(const GeometryData& geometries);
        virtual void GetBoundingBoxCore(BoundingBox* pBoundingBox) const;
        virtual void BindToShaderProgramCore(IShaderProgram* pShaderProgram);

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

    class BillboardVertexBuffer : public Dynamo::Bloodstone::IBillboardVertexBuffer
    {
    public:
        BillboardVertexBuffer(const IGraphicsContext* pGraphicsContext);
        ~BillboardVertexBuffer(void);

    protected:
        virtual void RenderCore(void) const;
        virtual void UpdateCore(const std::vector<BillboardVertex>& vertices);
        virtual void BindToShaderProgramCore(IShaderProgram* pShaderProgram);

    private:
        void EnsureVertexBufferCreation(void);

        int mVertexCount;
        GLuint mVertexArrayId;
        GLuint mVertexBufferId;
    };

} } }

#endif
