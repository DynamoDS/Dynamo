
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
        void Create(HWND hWndOwner) { this->CreateCore(hWndOwner); }

    protected:
        virtual void CreateCore(HWND hWndOwner) = 0;
    };
}

#endif
