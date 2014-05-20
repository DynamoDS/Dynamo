
#ifndef _OPEN_INTERFACE_H_
#define _OPEN_INTERFACE_H_

#include "../Interfaces.h"
#include "../../../../extern/OpenGL/glext.h"

namespace Dynamorph { namespace OpenGL {

    class GraphicsContext : public Dynamorph::IGraphicsContext
    {
    public:
        GraphicsContext();

    protected:
        virtual void InitializeCore(HWND hWndOwner);
        virtual void UninitializeCore(void);

    private:
        HWND mRenderWindow;
        HGLRC mhRenderContext;
    };

} }

#endif
