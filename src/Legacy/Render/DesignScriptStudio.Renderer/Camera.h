
#pragma once

#include "Primitives.h"

namespace DesignScriptStudio { namespace Renderer {

    class Camera
    {
    public:
        Camera(int pixelWidth, int pixelHeight);
        ~Camera(void);
        void SetOrthographicMode(bool orthographic);
        void SetProjectionMatrix(void) const;
        void SetModelViewMatrices(void) const;
        void FitToBoundingBox(const float* pCorners);

        float NearClipPlane(void) const;
        float FarClipPlane(void) const;
        Vector EyePosition(void) const;
        Vector ViewDirection(void) const;

    private:
        bool mOrthographicMode;
        float mContentDiameter;
        float mFieldOfView, mViewAspect;
        float mNearClip, mFarClip;
        Vector mEye, mTarget, mUpVector;
    };

} }
