
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
    CameraConfiguration camConfig;
    this->Configure(&camConfig); // Default configuration.
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

    this->mConfiguration = *pConfiguration;
}

void Camera::FitToBoundingBoxCore(const BoundingBox* pBoundingBox)
{
    // Get the bound box center and its radius.
    auto configuration = mConfiguration;
    float boxCenter[3], radius = 0.0f;
    pBoundingBox->Get(&boxCenter[0], radius);

    // Calculate the distance from eye to the center.
    auto halfFov = configuration.fieldOfView * 0.5f;
    auto halfFovRadian = halfFov * glm::pi<float>() / 180.0f;
    auto distance = std::fabsf(radius / std::sinf(halfFovRadian));

    // Obtain the inversed view vector.
    float vx, vy, vz;
    configuration.GetViewDirection(vx, vy, vz);
    glm::vec3 inversedViewDir(-vx, -vy, -vz);
    inversedViewDir = glm::normalize(inversedViewDir);

    // Compute the new eye point based on direction and center.
    glm::vec3 center(boxCenter[0], boxCenter[1], boxCenter[2]);
    glm::vec3 eye = (center + (inversedViewDir * distance));

    // Update the configuration and reconfigure the camera.
    configuration.SetEyePoint(eye.x, eye.y, eye.z);
    configuration.SetCenterPoint(center.x, center.y, center.z);
    this->Configure(&configuration);
}

Dynamorph::ITrackBall* Camera::GetTrackBallCore() const
{
    return (const_cast<Camera *>(this))->mpTrackBall;
}
