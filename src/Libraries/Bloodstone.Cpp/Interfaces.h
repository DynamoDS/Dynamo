
#ifndef _BLOODSTONE_H_
#define _BLOODSTONE_H_

namespace Dynamo { namespace Bloodstone {

    class GeometryData
    {
    public:
        virtual ~GeometryData(void) { }

        void PushVertex(float x, float y, float z)
        {
            mCoordinates.push_back(x);
            mCoordinates.push_back(y);
            mCoordinates.push_back(z);
        }

        void PushColor(float r, float g, float b, float a)
        {
            mRgbaColors.push_back(r);
            mRgbaColors.push_back(g);
            mRgbaColors.push_back(b);
            mRgbaColors.push_back(a);
        }

        int VertexCount(void) const
        {
            return ((int) mCoordinates.size()) / 3;
        }

        const float* GetCoordinates(int vertex) const
        {
            return &mCoordinates[vertex * 3];
        }

        const float* GetRgbaColors(int vertex) const
        {
            return &mRgbaColors[vertex * 4];
        }

    protected:
        GeometryData(int vertexCount)
        {
            mCoordinates.reserve(vertexCount * 3);
            mRgbaColors.reserve(vertexCount * 4);
        }

        std::vector<float> mCoordinates;
        std::vector<float> mRgbaColors;
    };

    class PointGeometryData : public GeometryData
    {
    public:
        PointGeometryData(int pointCount) :
            GeometryData(pointCount)
        {
        }
    };

    class LineStripGeometryData : public GeometryData
    {
    public:
        LineStripGeometryData(int lineCount) : 
            GeometryData(lineCount + 1)
        {
        }

        void PushSegmentVertexCount(int segmentVertexCount)
        {
            mSegmentVertexCount.push_back(segmentVertexCount);
        }

        int GetSegmentCount(void) const
        {
            return ((int) mSegmentVertexCount.size());
        }

        const int* GetSegmentVertexCounts(void) const
        {
            return &mSegmentVertexCount[0];
        }

    private:
        std::vector<int> mSegmentVertexCount;
    };

    class TriangleGeometryData : public GeometryData
    {
    public:
        TriangleGeometryData(int triangleCount) : 
            GeometryData(triangleCount * 3)
        {
        }

        void PushNormal(float x, float y, float z)
        {
            mNormalCoords.push_back(x);
            mNormalCoords.push_back(y);
            mNormalCoords.push_back(z);
        }

        const float* GetNormalCoords(int vertex) const
        {
            return &mNormalCoords[vertex * 3];
        }

    private:
        std::vector<float> mNormalCoords;
    };

    class ITrackBall
    {
    public:
        enum class Mode { None, Rotate, Zoom, Pan };

    public:
        virtual ~ITrackBall()
        {
        }

        void MousePressed(int screenX, int screenY, Mode mode)
        {
            this->MousePressedCore(screenX, screenY, mode);
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
        virtual void MousePressedCore(int screenX, int screenY, Mode mode) = 0;
        virtual void MouseMovedCore(int screenX, int screenY) = 0;
        virtual void MouseReleasedCore(int screenX, int screenY) = 0;
    };

    struct CameraConfiguration
    {
        CameraConfiguration()
        {
            memset(this, 0, sizeof(CameraConfiguration));
            eyePoint[0] = eyePoint[1] = eyePoint[2] = 1.0f;
            center[0] = center[1] = center[2] = 0.0f;
            upVector[1] = 1.0f; // Default up-vector is Y-axis

            viewportWidth = 1280;
            viewportHeight = 720;
            fieldOfView = 45.0f;
            nearClippingPlane = 0.1f;
            farClippingPlane = 1000.0f;
        }

        void SetEyePoint(float x, float y, float z)
        {
            eyePoint[0] = x;
            eyePoint[1] = y;
            eyePoint[2] = z;
        }

        void SetCenterPoint(float x, float y, float z)
        {
            center[0] = x;
            center[1] = y;
            center[2] = z;
        }

        void SetUpVector(float x, float y, float z)
        {
            upVector[0] = x;
            upVector[1] = y;
            upVector[2] = z;
        }

        void GetViewDirection(float& x, float& y, float& z)
        {
            // Always viewing origin.
            x = 0.0f - eyePoint[0];
            y = 0.0f - eyePoint[1];
            z = 0.0f - eyePoint[2];
        }

        // View matrix.
        float eyePoint[3];
        float center[3];
        float upVector[3];

        // Projection matrix.
        int viewportWidth;
        int viewportHeight;
        float fieldOfView;
        float nearClippingPlane;
        float farClippingPlane;
    };

    class BoundingBox
    {
    public:
        BoundingBox() : mInitialized(false)
        {
            memset(&mBox[0], 0, sizeof(mBox));
        }

        bool IsInitialized(void) const
        {
            return this->mInitialized;
        }

        void Invalidate(void)
        {
            this->mInitialized = false;
        }

        void Reset(float x, float y, float z)
        {
            mBox[0] = mBox[3] = x;
            mBox[1] = mBox[4] = y;
            mBox[2] = mBox[5] = z;
            this->mInitialized = true;
        }

        void Interpolate(const BoundingBox& other, float factor)
        {
            if (this->mInitialized == false)
            {
                if (other.mInitialized == false)
                    throw new std::exception("'Interpolate' cannot be called now");

                this->EvaluateBox(other);
                return;
            }

            if (other.mInitialized != false)
            {
                // Only interpolate if other box is initialized.
                mBox[0] += ((other.mBox[0] - mBox[0]) * factor);
                mBox[1] += ((other.mBox[1] - mBox[1]) * factor);
                mBox[2] += ((other.mBox[2] - mBox[2]) * factor);
                mBox[3] += ((other.mBox[3] - mBox[3]) * factor);
                mBox[4] += ((other.mBox[4] - mBox[4]) * factor);
                mBox[5] += ((other.mBox[5] - mBox[5]) * factor);
            }
        }

        void EvaluateBox(const BoundingBox& other)
        {
            float min[3], max[3];
            other.Get(&min[0], &max[0]);
            EvaluatePoint(min[0], min[1], min[2]);
            EvaluatePoint(max[0], max[1], max[2]);
        }

        void EvaluatePoint(float x, float y, float z)
        {
            if (this->mInitialized == false) {
                Reset(x, y, z);
                return; 
            }

            mBox[0] = ((x < mBox[0]) ? x : mBox[0]);
            mBox[1] = ((y < mBox[1]) ? y : mBox[1]);
            mBox[2] = ((z < mBox[2]) ? z : mBox[2]);

            mBox[3] = ((x > mBox[3]) ? x : mBox[3]);
            mBox[4] = ((y > mBox[4]) ? y : mBox[4]);
            mBox[5] = ((z > mBox[5]) ? z : mBox[5]);
        }

        void Get(float* pMin, float* pMax) const
        {
            pMin[0] = this->mBox[0];
            pMin[1] = this->mBox[1];
            pMin[2] = this->mBox[2];

            pMax[0] = this->mBox[3];
            pMax[1] = this->mBox[4];
            pMax[2] = this->mBox[5];
        }

        void Get(float* pCenter, float& radius) const
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
        bool mInitialized;
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

        void ResizeViewport(int width, int height)
        {
            this->ResizeViewportCore(width, height);
        }

        void FitToBoundingBox(const BoundingBox* pBoundingBox)
        {
            this->FitToBoundingBoxCore(pBoundingBox);
        }

        ITrackBall* GetTrackBall() const
        {
            return this->GetTrackBallCore();
        }

    protected:
        virtual void ConfigureCore(const CameraConfiguration* pConfiguration) = 0;
        virtual void ResizeViewportCore(int width, int height) = 0;
        virtual void FitToBoundingBoxCore(const BoundingBox* pBoundingBox) = 0;
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
        Model, View, Projection, Normal
    };

    class IShaderProgram
    {
    public:
        virtual ~IShaderProgram()
        {
        }

        int GetShaderParameterIndex(const std::string& name) const
        {
            return this->GetShaderParameterIndexCore(name);
        }

        void SetParameter(int index, const float* pValues, int count) const
        {
            return this->SetParameterCore(index, pValues, count);
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
        virtual int GetShaderParameterIndexCore(const std::string& name) const = 0;
        virtual void SetParameterCore(int index, const float* pValues, int count) const = 0;
        virtual void BindTransformMatrixCore(TransMatrix transform, const std::string& name) = 0;
        virtual void ApplyTransformationCore(const ICamera* pCamera) const = 0;
    };

    class IVertexBuffer
    {
    public:
        enum class PrimitiveType
        {
            None, Point, LineStrip, Triangle
        };

    public:
        virtual ~IVertexBuffer()
        {
        }

        PrimitiveType GetPrimitiveType() const
        {
            return this->GetPrimitiveTypeCore();
        }

        void LoadData(const GeometryData& geometries)
        {
            this->LoadDataCore(geometries);
        }

        void GetBoundingBox(BoundingBox* pBoundingBox)
        {
            GetBoundingBoxCore(pBoundingBox);
        }

    protected:
        virtual PrimitiveType GetPrimitiveTypeCore() const = 0;
        virtual void LoadDataCore(const GeometryData& geometries) = 0;
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

        bool Initialize(HWND hWndOwner)
        {
            return this->InitializeCore(hWndOwner);
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

        void EnableAlphaBlend(void) const
        {
            this->EnableAlphaBlendCore();
        }

        void ClearDepthBuffer(void) const
        {
            this->ClearDepthBufferCore();
        }

    protected:
        virtual bool InitializeCore(HWND hWndOwner) = 0;
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
        virtual void EnableAlphaBlendCore(void) const = 0;
        virtual void ClearDepthBufferCore(void) const = 0;
    };
} }

#endif
