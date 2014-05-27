
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

    struct CameraConfiguration
    {
        CameraConfiguration()
        {
            memset(this, 0, sizeof(CameraConfiguration));
            eye[0] = eye[1] = eye[2] = 10.0f;
            center[0] = center[1] = center[2] = 0.0f;
            up[1] = 1.0f; // Default up-vector is Y-axis

            fieldOfView = 45.0f;
            aspectRatio = 3.0f / 2.0f;
            nearClippingPlane = 1.0f;
            farClippingPlane = 1000.0f;
        }

        void SetEyePoint(float x, float y, float z)
        {
            eye[0] = x;
            eye[1] = y;
            eye[2] = z;
        }

        void SetCenterPoint(float x, float y, float z)
        {
            center[0] = x;
            center[1] = y;
            center[2] = z;
        }

        void SetUpVector(float x, float y, float z)
        {
            up[0] = x;
            up[1] = y;
            up[2] = z;
        }

        // View matrix.
        float eye[3];
        float center[3];
        float up[3];

        // Projection matrix.
        float fieldOfView;
        float aspectRatio;
        float nearClippingPlane;
        float farClippingPlane;
    };

    class ICamera
    {
    public:
        virtual ~ICamera()
        {
        }

        void Configure(const CameraConfiguration* pConfiguration)
        {
            this->ConfigureCore(pConfiguration);
        }

        ITrackBall* GetTrackBall() const
        {
            return this->GetTrackBallCore();
        }

    protected:
        virtual void ConfigureCore(const CameraConfiguration* pConfiguration) = 0;
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
            this->ApplyTransformationCore(pCamera);
        }

    protected:
        virtual void BindTransformMatrixCore(TransMatrix transform, const std::string& name) = 0;
        virtual void ApplyTransformationCore(const ICamera* pCamera) const = 0;
    };

    class BoundingBox
    {
    public:
        BoundingBox()
        {
            memset(&mBox[0], 0, sizeof(mBox));
        }

        void Reset(float x, float y, float z)
        {
            mBox[0] = mBox[3] = x;
            mBox[1] = mBox[4] = y;
            mBox[2] = mBox[5] = z;
        }

        void EvaluatePoint(float x, float y, float z)
        {
            mBox[0] = ((x < mBox[0]) ? x : mBox[0]);
            mBox[1] = ((y < mBox[1]) ? y : mBox[1]);
            mBox[2] = ((z < mBox[2]) ? z : mBox[2]);

            mBox[4] = ((x > mBox[4]) ? x : mBox[4]);
            mBox[5] = ((y > mBox[5]) ? y : mBox[5]);
            mBox[6] = ((z > mBox[6]) ? z : mBox[6]);
        }

        void Get(float* pMin, float* pMax)
        {
            pMin[0] = this->mBox[0];
            pMin[1] = this->mBox[1];
            pMin[2] = this->mBox[2];

            pMax[0] = this->mBox[3];
            pMax[1] = this->mBox[4];
            pMax[2] = this->mBox[5];
        }

        void Get(float* pCenter, float& radius)
        {
            float dx = ((mBox[3] - mBox[0]) * 0.5f);
            float dy = ((mBox[4] - mBox[1]) * 0.5f);
            float dz = ((mBox[5] - mBox[2]) * 0.5f);

            pCenter[0] = mBox[0] + dx;
            pCenter[1] = mBox[1] + dy;
            pCenter[2] = mBox[2] + dz;

            radius = std::sqrtf((dx * dx) + (dy * dy) + (dz * dz));
        }

        BoundingBox& operator=(const BoundingBox& other)
        {
            this->mBox[0] = other.mBox[0];
            this->mBox[1] = other.mBox[1];
            this->mBox[2] = other.mBox[2];
            this->mBox[3] = other.mBox[3];
            this->mBox[4] = other.mBox[4];
            this->mBox[5] = other.mBox[5];
            return *this;
        }

    private:
        float mBox[6];
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

        void GetBoundingBox(BoundingBox* pBoundingBox)
        {
            GetBoundingBoxCore(pBoundingBox);
        }

    protected:
        virtual void LoadDataCore(const std::vector<float>& positions) = 0;
        virtual void LoadDataCore(const std::vector<float>& positions,
            const std::vector<float>& rgbaColors) = 0;

        virtual void GetBoundingBoxCore(BoundingBox* pBoundingBox) = 0;
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
