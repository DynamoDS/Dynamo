
#ifndef _OPEN_INTERFACE_H_
#define _OPEN_INTERFACE_H_

#include "../Interfaces.h"

namespace Dynamorph { namespace OpenGL {

    class GraphicsContext : public Dynamorph::IGraphicsContext
    {
    protected:
        virtual void CreateCore(HWND hWndOwner);
    };

} }

#endif
