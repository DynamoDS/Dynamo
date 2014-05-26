
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;
using namespace Dynamorph::OpenGL;

// ================================================================================
// TrackBall
// ================================================================================

TrackBall::TrackBall(Camera* pCamera) : mpCamera(pCamera)
{
}

void TrackBall::MousePressedCore(int screenX, int screenY)
{
}

void TrackBall::MouseMovedCore(int screenX, int screenY)
{
}

void TrackBall::MouseReleasedCore(int screenX, int screenY)
{
}

// ================================================================================
// Camera
// ================================================================================

Camera::Camera(GraphicsContext* pGraphicsContext) :
    mpTrackBall(nullptr),
    mpGraphicsContext(pGraphicsContext)
{
    this->mModelMatrix = glm::mat4(1.0); // Identity
    this->mViewMatrix  = glm::mat4(1.0); // Identity
    this->mProjMatrix  = glm::mat4(1.0); // Identity
}

void Camera::GetMatrices(glm::mat4& model, glm::mat4& view, glm::mat4& proj) const
{
    model = this->mModelMatrix;
    view  = this->mViewMatrix;
    proj  = this->mProjMatrix;
}

GraphicsContext* Camera::GetGraphicsContext(void) const
{
    return this->mpGraphicsContext;
}

void Camera::SetViewCore(const float* pEye, const float* pCenter, const float* pUp)
{
    glm::vec3 eye(pEye[0], pEye[1], pEye[2]);
    glm::vec3 center(pCenter[0], pCenter[1], pCenter[2]);
    glm::vec3 up(pUp[0], pUp[1], pUp[2]);

    this->mViewMatrix = glm::lookAt(eye, center, up);
}

Dynamorph::ITrackBall* Camera::GetTrackBallCore() const
{
    return (const_cast<Camera *>(this))->mpTrackBall;
}
