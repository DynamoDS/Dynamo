
#include "stdafx.h"
#include "Constants.h"
#include "Resources\resource.h"

using namespace Dynamo::Bloodstone;
using namespace Dynamo::Bloodstone::OpenGL;

Version Dynamo::Bloodstone::OpenGL::GetOpenGLVersion(int major, int minor)
{
    switch(major)
    {
    case 2:
        if (minor == 1)
            return Version::OpenGL21;
        break;

    case 3:
        if (minor == 0)
            return Version::OpenGL30;
        if (minor == 1)
            return Version::OpenGL31;
        if (minor == 2)
            return Version::OpenGL32;
        if (minor == 3)
            return Version::OpenGL33;
        break;

    case 4:
        if (minor == 0)
            return Version::OpenGL40;
        if (minor == 1)
            return Version::OpenGL41;
        if (minor == 2)
            return Version::OpenGL42;
        if (minor == 3)
            return Version::OpenGL43;
        if (minor == 4)
            return Version::OpenGL44;
        break;
    }

    throw new std::exception("Unexpected OpenGL version");
}

Version Dynamo::Bloodstone::OpenGL::GetShaderVersion(int major, int minor)
{
    return Dynamo::Bloodstone::OpenGL::GetOpenGLVersion(major, minor);
}

bool Dynamo::Bloodstone::OpenGL::GetResourceIdentifiers(GetResourceIdentifiersParam* pParam)
{
    unsigned int vsids[ShaderName::MaxShaderName][Version::MaxEntries] = 
    {
        // Phong shader.
        {
            IDR_SHADER_PHONG_21_VERT,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
        },

        // Billboard text shader.
        {
            IDR_SHADER_BILLBOARD_TEXT_21_VERT,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
        }
    };

    unsigned int fsids[ShaderName::MaxShaderName][Version::MaxEntries] = 
    {
        // Phong shader.
        {
            IDR_SHADER_PHONG_21_FRAG,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
        },

        // Billboard text shader.
        {
            IDR_SHADER_BILLBOARD_TEXT_21_FRAG,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
        }
    };

    const int shaderName = ((int) pParam->shaderName);
    int shaderVersion = ((int) pParam->openGlVersion);

    while (shaderVersion >= 0)
    {
        if (pParam->vertexShaderId == 0 && (vsids[shaderName][shaderVersion] != 0))
            pParam->vertexShaderId = vsids[shaderName][shaderVersion];

        if (pParam->fragmentShaderId == 0 && (fsids[shaderName][shaderVersion] != 0))
            pParam->fragmentShaderId = fsids[shaderName][shaderVersion];

        shaderVersion = shaderVersion - 1;
    }

    return (pParam->vertexShaderId != 0 && (pParam->fragmentShaderId != 0));
}
