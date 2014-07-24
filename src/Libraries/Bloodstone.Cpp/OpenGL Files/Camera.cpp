
#include "stdafx.h"
#include "OpenInterfaces.h"

using namespace System;
using namespace Dynamo::Bloodstone;
using namespace Dynamo::Bloodstone::OpenGL;

// ================================================================================
// TrackBall
// ================================================================================

const float TrackBall::ZoomSpeed = 1.2f;
const float TrackBall::PanSpeed = 0.3f;
const float TrackBall::DynamicDampingFactor = 0.2f;

TrackBall::TrackBall(Camera* pCamera) :
    mpCamera(pCamera),
    mTrackBallMode(Mode::None)
{
}

void TrackBall::MousePressedCore(int screenX, int screenY, Mode mode)
{
    if (mTrackBallMode != Mode::None)
        return;

    mpCamera->GetConfiguration(mConfiguration); // Get the updated configuration.
    mCameraUpVector.x = mConfiguration.upVector[0];
    mCameraUpVector.y = mConfiguration.upVector[1];
    mCameraUpVector.z = mConfiguration.upVector[2];
    mCameraPosition.x = mConfiguration.eyePoint[0];
    mCameraPosition.y = mConfiguration.eyePoint[1];
    mCameraPosition.z = mConfiguration.eyePoint[2];
    mTargetPosition.x = mConfiguration.center[0];
    mTargetPosition.y = mConfiguration.center[1];
    mTargetPosition.z = mConfiguration.center[2];

    mTrackBallMode = mode;
    switch (mTrackBallMode)
    {
    case Dynamo::Bloodstone::ITrackBall::Mode::Rotate:
        mRotateStart = mRotateEnd = GetProjectionOnTrackball(screenX, screenY);
        break;
    case Dynamo::Bloodstone::ITrackBall::Mode::Zoom:
        mZoomStart = mZoomEnd = GetMouseOnScreen(screenX, screenY);
        break;
    case Dynamo::Bloodstone::ITrackBall::Mode::Pan:
        mPanStart = mPanEnd = GetMouseOnScreen(screenX, screenY);
        break;
    }
}

void TrackBall::MouseMovedCore(int screenX, int screenY)
{
    if (this->mTrackBallMode == ITrackBall::Mode::None)
        return;

    switch (mTrackBallMode)
    {
    case Dynamo::Bloodstone::ITrackBall::Mode::Rotate:
        RotateCameraInternal(screenX, screenY);
        break;
    case Dynamo::Bloodstone::ITrackBall::Mode::Zoom:
        ZoomCameraInternal(screenX, screenY);
        break;
    case Dynamo::Bloodstone::ITrackBall::Mode::Pan:
        mPanEnd = GetMouseOnScreen(screenX, screenY);
        break;
    }
}

void TrackBall::MouseReleasedCore(int screenX, int screenY)
{
    this->mTrackBallMode = ITrackBall::Mode::None;
}

glm::vec2 TrackBall::GetMouseOnScreen(int screenX, int screenY) const
{
    float sw = ((float) mConfiguration.viewportWidth);
    float sh = ((float) mConfiguration.viewportHeight);
    return glm::vec2(screenX / sw, screenY / sh);
}

glm::vec3 TrackBall::GetProjectionOnTrackball(int screenX, int screenY) const
{
    float sw = mConfiguration.viewportWidth * 0.5f;
    float sh = mConfiguration.viewportHeight * 0.5f;

    glm::vec3 mouseOnBall((screenX - sw) / sw, (sh - screenY) / sh, 0.0f);

    const auto length = glm::length(mouseOnBall);
    if (length > 1.0f)
        mouseOnBall = glm::normalize(mouseOnBall);
    else
        mouseOnBall.z = std::sqrtf(1.0f - length * length);

    glm::vec3 eyeDirection(mCameraPosition - mTargetPosition);
        
    glm::vec3 vector = glm::normalize(mCameraUpVector) * mouseOnBall.y;
    glm::vec3 objectUp = glm::cross(mCameraUpVector, eyeDirection);
    objectUp = glm::normalize(objectUp) * mouseOnBall.x;
    eyeDirection = glm::normalize(eyeDirection) * mouseOnBall.z;

    vector = vector + objectUp;
    vector = vector + eyeDirection;
    return vector;
}

void TrackBall::RotateCameraInternal(int screenX, int screenY)
{
    mRotateEnd = GetProjectionOnTrackball(screenX, screenY);
    glm::fquat quaternion(glm::normalize(mRotateStart), glm::normalize(mRotateEnd));

    glm::vec3 eyeDirection(mCameraPosition - mTargetPosition);
    eyeDirection = eyeDirection * quaternion;
    mRotateEnd = mRotateEnd * quaternion;
    mRotateStart = mRotateEnd;

    // Move the camera based on the current eye direction.
    mCameraUpVector = mCameraUpVector * quaternion;
    mCameraPosition = mTargetPosition + eyeDirection;

    // Update configuration before reconfiguring the camera.
    mConfiguration.upVector[0] = mCameraUpVector.x;
    mConfiguration.upVector[1] = mCameraUpVector.y;
    mConfiguration.upVector[2] = mCameraUpVector.z;
    mConfiguration.eyePoint[0] = mCameraPosition.x;
    mConfiguration.eyePoint[1] = mCameraPosition.y;
    mConfiguration.eyePoint[2] = mCameraPosition.z;

    mpCamera->Configure(&mConfiguration); // Update camera.
}

void TrackBall::ZoomCameraInternal(int screenX, int screenY)
{
    mZoomEnd = GetMouseOnScreen(screenX, screenY);
    auto factor = 1.0f + ((mZoomEnd.y - mZoomStart.y) * TrackBall::ZoomSpeed);
    if (factor == 1.0f || factor <= 0.0f)
        return;

    glm::vec3 eyeDirection(mCameraPosition - mTargetPosition);
    eyeDirection = eyeDirection * factor;
    mZoomStart.y += (mZoomEnd.y - mZoomStart.y) * TrackBall::DynamicDampingFactor;

    mCameraPosition = mTargetPosition + eyeDirection;
    mConfiguration.eyePoint[0] = mCameraPosition.x;
    mConfiguration.eyePoint[1] = mCameraPosition.y;
    mConfiguration.eyePoint[2] = mCameraPosition.z;
    mpCamera->Configure(&mConfiguration); // Update camera.
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
    if (pBoundingBox->IsInitialized() == false)
        return;

    // Get the bound box center and its radius.
    auto configuration = mConfiguration;
    float boxCenter[3], radius = 0.0f;
    pBoundingBox->Get(&boxCenter[0], radius);

    if (radius <= 0.0f) // Bounding box is empty.
        return;

    // Calculate the distance from eye to the center.
    auto halfFov = configuration.fieldOfView * 0.5f;
    auto halfFovRadian = halfFov * glm::pi<float>() / 180.0f;
    auto distance = std::fabsf(radius / std::sinf(halfFovRadian));
    configuration.farClippingPlane = distance + radius;

    // Obtain the inversed view vector.
    float vx, vy, vz;
    configuration.GetViewDirection(vx, vy, vz);
    glm::vec3 inversedViewDir(-vx, -vy, -vz);
    inversedViewDir = glm::normalize(inversedViewDir);

    // Compute the new eye point based on direction and origin.
    glm::vec3 eye = inversedViewDir * distance;

    // Update the configuration and reconfigure the camera.
    configuration.SetEyePoint(eye.x, eye.y, eye.z);
    configuration.SetCenterPoint(boxCenter[0], boxCenter[1], boxCenter[2]);
    this->Configure(&configuration);
}

ITrackBall* Camera::GetTrackBallCore() const
{
    return (const_cast<Camera *>(this))->mpTrackBall;
}

void Camera::ConfigureInternal(const CameraConfiguration* pConfiguration)
{
    glm::vec3 eyePoint(
        pConfiguration->eyePoint[0],
        pConfiguration->eyePoint[1],
        pConfiguration->eyePoint[2]);

    glm::vec3 center(
        pConfiguration->center[0],
        pConfiguration->center[1],
        pConfiguration->center[2]);

    glm::vec3 upVector(
        pConfiguration->upVector[0],
        pConfiguration->upVector[1],
        pConfiguration->upVector[2]);

    // The view is always looking at the origin from "eyePoint".
    this->mViewMatrix = glm::lookAt(eyePoint, center, upVector);

    const float w = ((float) pConfiguration->viewportWidth);
    const float h = ((float) pConfiguration->viewportHeight);

    this->mProjMatrix = glm::perspective(
        pConfiguration->fieldOfView, w / h,
        pConfiguration->nearClippingPlane,
        pConfiguration->farClippingPlane);

    this->mConfiguration = *pConfiguration;
}
