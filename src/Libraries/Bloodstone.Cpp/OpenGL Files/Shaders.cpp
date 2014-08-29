
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamo::Bloodstone;
using namespace Dynamo::Bloodstone::OpenGL;

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

    GLint result = GL_FALSE;
    GL::glGetShaderiv(mShaderId, GL_COMPILE_STATUS, &result);

    char buffer[2048] = { 0 };
    GL::glGetShaderInfoLog(mShaderId, sizeof(buffer), nullptr, buffer);
    return result == GL_TRUE;
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
    mProgramId(0),
    mModelMatrixUniform(0),
    mViewMatrixUniform(0),
    mProjMatrixUniform(0),
    mNormMatrixUniform(0),
    mpVertexShader(pVertexShader),
    mpFragmentShader(pFragmentShader)
{
    mProgramId = GL::glCreateProgram();
    GL::glAttachShader(mProgramId, mpVertexShader->GetShaderId());
    GL::glAttachShader(mProgramId, mpFragmentShader->GetShaderId());
    GL::glLinkProgram(mProgramId);

    GLint result = GL_FALSE;
    GL::glGetProgramiv(mProgramId, GL_LINK_STATUS, &result);

    char buffer[2048] = { 0 };
    GL::glGetProgramInfoLog(mProgramId, sizeof(buffer), nullptr, buffer);
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

int ShaderProgram::GetAttributeLocation(const std::string& name) const
{
    return GL::glGetAttribLocation(mProgramId, name.c_str());
}

int ShaderProgram::GetShaderParameterIndexCore(const std::string& name) const
{
    return GL::glGetUniformLocation(mProgramId, name.c_str());
}

void ShaderProgram::SetParameterCore(int index, const float* pv, int count) const
{
    switch (count)
    {
    case 1: GL::glUniform1f(index, pv[0]); break;
    case 2: GL::glUniform2f(index, pv[0], pv[1]); break;
    case 3: GL::glUniform3f(index, pv[0], pv[1], pv[2]); break;
    case 4: GL::glUniform4f(index, pv[0], pv[1], pv[2], pv[3]); break;

    default:
        throw new std::exception("Invalid 'count' value in 'ShaderProgram::SetParameter'");
    }
}

void ShaderProgram::BindTransformMatrixCore(TransMatrix transform, const std::string& name)
{
    GLint index = GL::glGetUniformLocation(mProgramId, name.c_str());

    switch (transform)
    {
    case Dynamo::Bloodstone::TransMatrix::Model:
        mModelMatrixUniform = index;
        break;
    case Dynamo::Bloodstone::TransMatrix::View:
        mViewMatrixUniform = index;
        break;
    case Dynamo::Bloodstone::TransMatrix::Projection:
        mProjMatrixUniform = index;
        break;
    case Dynamo::Bloodstone::TransMatrix::Normal:
        mNormMatrixUniform = index;
        break;
    }
}

void ShaderProgram::ApplyTransformationCore(const ICamera* pCamera) const
{
    auto pCameraInternal = dynamic_cast<const Camera *>(pCamera);
    if (pCameraInternal == nullptr)
        return;

    glm::mat4 model, view, proj;
    pCameraInternal->GetMatrices(model, view, proj);

    GL::glUniformMatrix4fv(mModelMatrixUniform, 1, GL_FALSE, glm::value_ptr(model));
    GL::glUniformMatrix4fv(mViewMatrixUniform, 1, GL_FALSE, glm::value_ptr(view));
    GL::glUniformMatrix4fv(mProjMatrixUniform, 1, GL_FALSE, glm::value_ptr(proj));

    glm::mat4 modelView(view * model);
    glm::mat4 normal = glm::transpose(glm::inverse(modelView));
    GL::glUniformMatrix4fv(mNormMatrixUniform, 1, GL_FALSE, glm::value_ptr(normal));
}
