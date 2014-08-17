
#ifndef _CONSTANTS_H_
#define _CONSTANTS_H_

namespace Dynamo { namespace Bloodstone { namespace OpenGL {

    enum class Version
    {
        OPENGL_2_0 = 0,     SHADER_110 = 0,     // April 2004
        OPENGL_2_1 = 1,     SHADER_120 = 1,     // September 2006
        OPENGL_3_0 = 2,     SHADER_130 = 2,     // August 2008
        OPENGL_3_1 = 3,     SHADER_140 = 3,     // March 2009
        OPENGL_3_2 = 4,     SHADER_150 = 4,     // August 2009
        OPENGL_3_3 = 5,     SHADER_330 = 5,     // February 2010
        OPENGL_4_0 = 6,     SHADER_400 = 6,     // March 2010
        OPENGL_4_1 = 7,     SHADER_410 = 7,     // July 2010
        OPENGL_4_2 = 8,     SHADER_420 = 8,     // August 2011
        OPENGL_4_3 = 9,     SHADER_430 = 9,     // August 2012
        OPENGL_4_4 = 10,    SHADER_440 = 10,    // July 2013
    };

} } }

#endif
