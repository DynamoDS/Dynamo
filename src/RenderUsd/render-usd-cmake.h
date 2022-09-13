// render-usd-cmake.h : Include file for standard system include files,
// or project specific include files.

#pragma once

#include <iostream>

#ifdef RENDERUSD_EXPORTS
#define RENDERUSD_API __declspec(dllexport)
#else
#define RENDERUSD_API __declspec(dllimport)
#endif

#include "glfw3.h"

#include <pxr/usd/usd/stage.h>
#include <pxr/usd/usd/common.h>
#include <pxr/base/gf/camera.h>
#include <pxr/usdImaging/usdImagingGL/engine.h>

#include <pxr/usd/usd/prim.h>

PXR_NAMESPACE_USING_DIRECTIVE

// TODO: Reference additional headers your program requires here.

class Renderer
{
public:
    void init();
    void render();

private:
    GLFWwindow* mWindow;
    UsdStageRefPtr mStage;
    GfCamera mCamera;
    UsdImagingGLEngine mRenderer;
    UsdImagingGLRenderParams mParams;
};
