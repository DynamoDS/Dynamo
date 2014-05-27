
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

void Camera::ConfigureCore(const CameraConfiguration* pConfiguration)
{
    glm::vec3 eye(
        pConfiguration->eye[0],
        pConfiguration->eye[1],
        pConfiguration->eye[2]);

    glm::vec3 center(
        pConfiguration->center[0],
        pConfiguration->center[1],
        pConfiguration->center[2]);

    glm::vec3 up(
        pConfiguration->up[0],
        pConfiguration->up[1],
        pConfiguration->up[2]);

    this->mViewMatrix = glm::lookAt(eye, center, up);

    this->mProjMatrix = glm::perspective(
        pConfiguration->fieldOfView,
        pConfiguration->aspectRatio,
        pConfiguration->nearClippingPlane,
        pConfiguration->farClippingPlane);
}

Dynamorph::ITrackBall* Camera::GetTrackBallCore() const
{
    return (const_cast<Camera *>(this))->mpTrackBall;
}
