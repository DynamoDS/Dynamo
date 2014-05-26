
#ifndef _DYNAMORPH_H_
#define _DYNAMORPH_H_

namespace Dynamorph
{
    class IVertexShader
    {
    public:
        virtual ~IVertexShader() { }
    };

    class IFragmentShader
    {
    public:
        virtual ~IFragmentShader() { }
    };

    class IShaderProgram
    {
    public:
        virtual ~IShaderProgram() { }
    };

    class IVertexBuffer
    {
    public:
        virtual ~IVertexBuffer() { }

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
        virtual ~IGraphicsContext() { }
        void Initialize(HWND hWndOwner) { this->InitializeCore(hWndOwner); }
        void Uninitialize(void) { this->UninitializeCore(); }

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
