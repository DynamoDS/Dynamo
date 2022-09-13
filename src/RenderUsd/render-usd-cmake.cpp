// render-usd-cmake.cpp : Defines the entry point for the application.
//


#include "render-usd-cmake.h"


using namespace std;

void Renderer::init()
{
    if (!glfwInit())
        throw std::exception();

#if (BOOST_OS_WINDOWS)
#else
    glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
#if (BOOST_OS_MACOS)
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 2);
#else
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 5);
#endif
    glfwWindowHint(GLFW_SAMPLES, 16);
#endif

    // create a window with glfw
    glfwWindowHint(GLFW_VISIBLE, GLFW_FALSE);
    mWindow = glfwCreateWindow(640, 480, "Hidden OpenGL window", NULL, NULL);

    if (!mWindow)
    {
        glfwTerminate();
        throw std::exception();
    }

    glfwMakeContextCurrent(mWindow);

    //load glad calls for OpenGL version 3.3
    /*if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        glfwTerminate();
        throw std::exception();
    }*/

    mStage = UsdStage::CreateInMemory();

    auto sphere = pxr::UsdGeomSphere::Define(mStage, SdfPath("/SomeSphere"));
}

void Renderer::render()
{
    auto frustum = mCamera.GetFrustum();

    mRenderer.SetCameraState(
        frustum.ComputeViewMatrix(),
        frustum.ComputeProjectionMatrix());

    mRenderer.Render(mStage->GetDefaultPrim(), mParams);

    // Clear OpenGL errors. Because UsdImagingGL::TestIntersection prints them.
    while (glGetError() != GL_NO_ERROR) {}
}
