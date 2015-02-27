
#include "stdafx.h"
#include "Internal.h"
#include "Camera.h"

using namespace DesignScriptStudio::Renderer;

Camera::Camera(int pixelWidth, int pixelHeight) : 
    mOrthographicMode(true),
    mContentDiameter(10.0f),
    mFieldOfView(45.0f),
    mNearClip(0.1f),
    mFarClip(100000.0f)
{
    mEye        = Vector(20.0f, 20.0f, 10.0f);
    mTarget     = Vector( 0.0f,  0.0f,  0.0f);
    mUpVector   = Vector( 0.0f,  0.0f,  1.0f);
    mViewAspect = ((float) pixelWidth) / pixelHeight;
}

Camera::~Camera(void)
{
}

void Camera::SetOrthographicMode(bool orthographic)
{
    mOrthographicMode = orthographic;
}

void Camera::SetProjectionMatrix(void) const
{
    glMatrixMode(GL_PROJECTION);
    glLoadIdentity();

    if (false != mOrthographicMode) {
        double value = mContentDiameter * 0.5f;
        glOrtho(-value, value, -value, value, mNearClip, mFarClip);
    }
    else
    {
        gluPerspective(mFieldOfView, mViewAspect, mNearClip, mFarClip);
    }
}

void Camera::SetModelViewMatrices(void) const
{
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    gluLookAt(mEye.x, mEye.y, mEye.z, mTarget.x, mTarget.y, 
        mTarget.z, mUpVector.x, mUpVector.y, mUpVector.z);
}

void Camera::FitToBoundingBox(const float* pCorners)
{
    // Backup the view direction first before changing the center point.
    Vector backward = Vector(0.0f, 0.0f, 0.0f) - Vector(20.0f, 20.0f, 10.0f);
    backward.Normalize(); // Ensure we won't move camera beyond what's needed.

    const float minX = pCorners[0];
    const float minY = pCorners[1];
    const float minZ = pCorners[2];
    const float maxX = pCorners[3];
    const float maxY = pCorners[4];
    const float maxZ = pCorners[5];

    // Calculate bounding sphere.
    const float dx = maxX - minX;
    const float dy = maxY - minY;
    const float dz = maxZ - minZ;

    // It is possible that we have only a single point, and 
    // therefore, the bounding box will be zero in size. That 
    // causes the camera to be at the same location as the 
    // point, causing nothing to be visible (clipped by the 
    // near plane.
    // 
    mContentDiameter = sqrtf((dx * dx) + (dy * dy) + (dz * dz));
    if (mContentDiameter <= 0.0f)
        mContentDiameter = 10.0f;

    float distance = ((mContentDiameter * 0.5f) / sinf(mFieldOfView * 0.5f)) * 1.2f;

    // Move the view target first.
    mTarget.x = (minX + (dx * 0.5f));
    mTarget.y = (minY + (dy * 0.5f));
    mTarget.z = (minZ + (dz * 0.5f));

    // Now adjust the eye point backward from the target.
    mEye = (mTarget + (backward * distance));
}

float Camera::NearClipPlane(void) const
{
    return this->mNearClip;
}

float Camera::FarClipPlane(void) const
{
    return this->mFarClip;
}

Vector Camera::EyePosition(void) const
{
    return this->mEye;
}

Vector Camera::ViewDirection(void) const
{
    Vector view = mTarget - mEye;
    view.Normalize();
    return view;
}
