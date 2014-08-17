
#include "Interfaces.h"

#ifndef _CONSTANTS_H_
#define _CONSTANTS_H_

namespace Dynamo { namespace Bloodstone { namespace OpenGL {

    enum class Version
    {
        OpenGL21 = 0,     Shader120 = 0,     // September 2006
        OpenGL30 = 1,     Shader130 = 1,     // August 2008
        OpenGL31 = 2,     Shader140 = 2,     // March 2009
        OpenGL32 = 3,     Shader150 = 3,     // August 2009
        OpenGL33 = 4,     Shader330 = 4,     // February 2010
        OpenGL40 = 5,     Shader400 = 5,     // March 2010
        OpenGL41 = 6,     Shader410 = 6,     // July 2010
        OpenGL42 = 7,     Shader420 = 7,     // August 2011
        OpenGL43 = 8,     Shader430 = 8,     // August 2012
        OpenGL44 = 9,     Shader440 = 9,     // July 2013

        MaxEntries // Always the last entry.
    };

    struct GetResourceIdentifiersParam
    {
        GetResourceIdentifiersParam() : 
            vertexShaderId(0),
            fragmentShaderId(0)
        {
        }

        Dynamo::Bloodstone::OpenGL::Version openGlVersion;
        Dynamo::Bloodstone::ShaderName shaderName;
        unsigned int vertexShaderId;
        unsigned int fragmentShaderId;
    };

    Version GetOpenGLVersion(int major, int minor);
    Version GetShaderVersion(int major, int minor);
    bool GetResourceIdentifiers(GetResourceIdentifiersParam* pParam);

} } }

#endif
