
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamorph;
using namespace Dynamorph::OpenGL;

// ================================================================================
// TrackBall
// ================================================================================

TrackBall::TrackBall(Camera* pCamera) : mpCamera(pCamera),
    mTrackBallActivated(false),
    mPrevX(0), mPrevY(0), mCurrX(0), mCurrY(0)
{
}

void TrackBall::MousePressedCore(int screenX, int screenY)
{
    mCurrX = mPrevX = screenX;
    mCurrY = mPrevY = screenY;
    mTrackBallActivated = true;
    mpCamera->GetConfiguration(mConfiguration); // Get the updated configuration.
}

void TrackBall::MouseMovedCore(int screenX, int screenY)
{
    if (this->mTrackBallActivated == false)
        return;

    mCurrX = screenX;
    mCurrY = screenY;
    if (mCurrX == mPrevX && (mCurrY == mPrevY))
        return; // No movement.

    // Get the current transformation matrices from camera.
    glm::mat4 model, view, projection;
    mpCamera->GetMatrices(model, view, projection);

    glm::vec3 va = GetVector(mPrevX, mPrevY);
    glm::vec3 vb = GetVector(mCurrX, mCurrY);
    mPrevX = mCurrX;
    mPrevY = mCurrY;

    float angle = acos(std::min(1.0f, glm::dot(va, vb)));
    glm::vec3 axisInCameraCoords = glm::cross(va, vb);

    glm::mat3 cameraToObject = glm::inverse(glm::mat3(view) * glm::mat3(model));
    glm::vec3 axisInObjectCoords = cameraToObject * axisInCameraCoords;
    auto finalModel = glm::rotate(model, glm::degrees(angle), axisInObjectCoords);
    mpCamera->SetModelTransformation(finalModel);
}

void TrackBall::MouseReleasedCore(int screenX, int screenY)
{
    this->mTrackBallActivated = false;
}

glm::vec3 TrackBall::GetVector(int x, int y) const
{
    float sw = ((float) mConfiguration.viewportWidth);
    float sh = ((float) mConfiguration.viewportHeight);

    glm::vec3 vector = glm::vec3(
        ((2.0f * x) / sw) - 1.0,
        ((2.0f * y) / sh) - 1.0, 0);

    vector.y = -vector.y;
    float squared = vector.x * vector.x + vector.y * vector.y;
    if (squared <= 1.0f)
        vector.z = sqrt(1.0f - squared);  // Pythagore
    else
        vector = glm::normalize(vector);  // nearest point

    return vector;
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
    mpTrackBall = new TrackBall(this);
}

void Camera::GetConfiguration(CameraConfiguration& configuration) const
{
    configuration = this->mConfiguration;
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

void Camera::SetModelTransformation(glm::mat4& model)
{
    this->mModelMatrix = model;
}

void Camera::ConfigureCore(const CameraConfiguration* pConfiguration)
{
    ConfigureInternal(pConfiguration);
}

void Camera::ResizeViewportCore(int width, int height)
{
    CameraConfiguration configuration;
    this->GetConfiguration(configuration);

    configuration.viewportWidth = width;
    configuration.viewportHeight = height;
    this->ConfigureInternal(&configuration);
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

void Camera::ConfigureInternal(const CameraConfiguration* pConfiguration)
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

    const float w = ((float) pConfiguration->viewportWidth);
    const float h = ((float) pConfiguration->viewportHeight);

    this->mProjMatrix = glm::perspective(
        pConfiguration->fieldOfView, w / h,
        pConfiguration->nearClippingPlane,
        pConfiguration->farClippingPlane);

    this->mConfiguration = *pConfiguration;
}
