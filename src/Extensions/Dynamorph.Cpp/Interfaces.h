
#ifndef _DYNAMORPH_H_
#define _DYNAMORPH_H_

namespace Dynamorph
{
    class ITrackBall
    {
    public:
        virtual ~ITrackBall()
        {
        }

        void MousePressed(int screenX, int screenY)
        {
            this->MousePressedCore(screenX, screenY);
        }

        void MouseMoved(int screenX, int screenY)
        {
            this->MouseMovedCore(screenX, screenY);
        }

        void MouseReleased(int screenX, int screenY)
        {
            this->MouseReleasedCore(screenX, screenY);
        }

    protected:
        virtual void MousePressedCore(int screenX, int screenY) = 0;
        virtual void MouseMovedCore(int screenX, int screenY) = 0;
        virtual void MouseReleasedCore(int screenX, int screenY) = 0;
    };

    class ICamera
    {
    public:
        virtual ~ICamera()
        {
        }

        void SetView(const float* pEye, const float* pCenter, const float* pUp)
        {
            this->SetViewCore(pEye, pCenter, pUp);
        }

        ITrackBall* GetTrackBall() const
        {
            return this->GetTrackBallCore();
        }

    protected:
        virtual void SetViewCore(const float* pEye, const float* pCenter, const float* pUp) = 0;
        virtual ITrackBall* GetTrackBallCore() const = 0;
    };

    class IVertexShader
    {
    public:
        virtual ~IVertexShader()
        {
        }
    };

    class IFragmentShader
    {
    public:
        virtual ~IFragmentShader()
        {
        }
    };

    enum class TransMatrix
    {
        Model, View, Projection
    };

    class IShaderProgram
    {
    public:
    public:
        virtual ~IShaderProgram()
        {
        }

        void BindTransformMatrix(TransMatrix transform, const std::string& name)
        {
            this->BindTransformMatrixCore(transform, name);
        }

        void ApplyTransformation(const ICamera* pCamera) const
        {
        }

    protected:
        virtual void BindTransformMatrixCore(TransMatrix transform, const std::string& name) = 0;
        virtual void ApplyTransformationCore(const ICamera* pCamera) const = 0;
    };

    class IVertexBuffer
    {
    public:
        virtual ~IVertexBuffer()
        {
        }

        void LoadData(const std::vector<float>& positions)
        {
            this->LoadDataCore(positions);
        }

        void LoadData(const std::vector<float>& positions,
            const std::vector<float>& rgbaColors)
        {
            this->LoadDataCore(positions, rgbaColors);
        }

    protected:
        virtual void LoadDataCore(const std::vector<float>& positions) = 0;
        virtual void LoadDataCore(const std::vector<float>& positions,
            const std::vector<float>& rgbaColors) = 0;
    };

    class IGraphicsContext
    {
    public:
        enum class ContextType
        {
            OpenGL
        };

    public:
        static IGraphicsContext* Create(IGraphicsContext::ContextType contextType);

    public:
        virtual ~IGraphicsContext()
        {
        }

        void Initialize(HWND hWndOwner)
        {
            this->InitializeCore(hWndOwner);
        }

        void Uninitialize(void)
        {
            this->UninitializeCore();
        }

        ICamera* GetDefaultCamera() const
        {
            return this->GetDefaultCameraCore();
        }

        IVertexShader* CreateVertexShader(const std::string& content) const
        {
            return this->CreateVertexShaderCore(content);
        }

        IFragmentShader* CreateFragmentShader(const std::string& content) const
        {
            return this->CreateFragmentShaderCore(content);
        }

        IShaderProgram* CreateShaderProgram(
            IVertexShader* pVertexShader,
            IFragmentShader* pFragmentShader)
        {
            return this->CreateShaderProgramCore(pVertexShader, pFragmentShader);
        }

        IVertexBuffer* CreateVertexBuffer(void) const
        {
            return this->CreateVertexBufferCore();
        }

        void BeginRenderFrame(HDC deviceContext) const
        {
            this->BeginRenderFrameCore(deviceContext);
        }

        void ActivateShaderProgram(IShaderProgram* pShaderProgram) const
        {
            this->ActivateShaderProgramCore(pShaderProgram);
        }

        void RenderVertexBuffer(IVertexBuffer* pVertexBuffer) const
        {
            this->RenderVertexBufferCore(pVertexBuffer);
        }

        void EndRenderFrame(HDC deviceContext) const
        {
            this->EndRenderFrameCore(deviceContext);
        }

    protected:
        virtual void InitializeCore(HWND hWndOwner) = 0;
        virtual void UninitializeCore(void) = 0;
        virtual ICamera* GetDefaultCameraCore(void) const = 0;

        virtual IVertexShader* CreateVertexShaderCore(
            const std::string& content) const = 0;
        virtual IFragmentShader* CreateFragmentShaderCore(
            const std::string& content) const = 0;
        virtual IShaderProgram* CreateShaderProgramCore(
            IVertexShader* pVertexShader, IFragmentShader* pFragmentShader) = 0;

        virtual IVertexBuffer* CreateVertexBufferCore(void) const = 0;
        virtual void BeginRenderFrameCore(HDC deviceContext) const = 0;
        virtual void ActivateShaderProgramCore(IShaderProgram* pShaderProgram) const = 0;
        virtual void RenderVertexBufferCore(IVertexBuffer* pVertexBuffer) const = 0;
        virtual void EndRenderFrameCore(HDC deviceContext) const = 0;
    };
}

#endif
