
#ifndef _DYNAMORPH_H_
#define _DYNAMORPH_H_

namespace Dynamorph
{
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

    protected:
        virtual void InitializeCore(HWND hWndOwner) = 0;
        virtual void UninitializeCore(void) = 0;
    };
}

#endif
