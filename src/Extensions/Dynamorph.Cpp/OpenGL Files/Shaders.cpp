
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;
using namespace Dynamorph::OpenGL;

// ================================================================================
// CommonShaderBase
// ================================================================================

CommonShaderBase::CommonShaderBase(const GraphicsContext* pGraphicsContext) : 
    mpGraphicsContext(pGraphicsContext), mShaderId(0)
{
}

CommonShaderBase::~CommonShaderBase(void)
{
    if (mShaderId != 0) {
        GL::glDeleteShader(mShaderId);
        mShaderId = 0;
    }
}

bool CommonShaderBase::LoadFromContent(const std::string& content)
{
    mShaderId = this->CreateShaderIdCore();

    auto source = ((const GLchar *) content.c_str());
    GL::glShaderSource(mShaderId, 1, &source, nullptr);
    GL::glCompileShader(mShaderId);

    GLint result = GL_FALSE, infoLogLength = 0;
    GL::glGetShaderiv(mShaderId, GL_COMPILE_STATUS, &result);
    GL::glGetShaderiv(mShaderId, GL_INFO_LOG_LENGTH, &infoLogLength);

    std::vector<char> errorMessage(infoLogLength);
    GL::glGetShaderInfoLog(mShaderId, infoLogLength, nullptr, &errorMessage[0]);

    return true; // TODO(Ben): Make return value indicative.
}

GLuint CommonShaderBase::GetShaderId(void) const
{
    return this->mShaderId;
}

// ================================================================================
// VertexShader
// ================================================================================

VertexShader::VertexShader(const GraphicsContext* pGraphicsContext) : 
    CommonShaderBase(pGraphicsContext)
{
}

GLuint VertexShader::CreateShaderIdCore(void) const
{
    return GL::glCreateShader(GL_VERTEX_SHADER);
}

// ================================================================================
// FragmentShader
// ================================================================================

FragmentShader::FragmentShader(const GraphicsContext* pGraphicsContext) : 
    CommonShaderBase(pGraphicsContext)
{
}

GLuint FragmentShader::CreateShaderIdCore(void) const
{
    return GL::glCreateShader(GL_FRAGMENT_SHADER);
}

// ================================================================================
// ShaderProgram
// ================================================================================

ShaderProgram::ShaderProgram(
    VertexShader* pVertexShader, FragmentShader* pFragmentShader) : 
mProgramId(0), mpVertexShader(pVertexShader), mpFragmentShader(pFragmentShader)
{
    mProgramId = GL::glCreateProgram();
    GL::glAttachShader(mProgramId, mpVertexShader->GetShaderId());
    GL::glAttachShader(mProgramId, mpFragmentShader->GetShaderId());
    GL::glLinkProgram(mProgramId);

    GLint result = GL_FALSE, infoLogLength = 0;
    GL::glGetProgramiv(mProgramId, GL_LINK_STATUS, &result);
    GL::glGetProgramiv(mProgramId, GL_INFO_LOG_LENGTH, &infoLogLength);

    std::vector<char> errorMessage(max(infoLogLength, int(1)));
    GL::glGetProgramInfoLog(mProgramId, infoLogLength, nullptr, &errorMessage[0]);
}

ShaderProgram::~ShaderProgram(void)
{
    if (mpVertexShader != nullptr) {
        GL::glDetachShader(mProgramId, mpVertexShader->GetShaderId());
        delete mpVertexShader;
        mpVertexShader = nullptr;
    }

    if (mpFragmentShader != nullptr) {
        GL::glDetachShader(mProgramId, mpFragmentShader->GetShaderId());
        delete mpFragmentShader;
        mpFragmentShader = nullptr;
    }

    if (mProgramId != 0) {
        GL::glDeleteProgram(mProgramId);
        mProgramId = 0;
    }
}

void ShaderProgram::Activate(void) const
{
    GL::glUseProgram(mProgramId);
}
