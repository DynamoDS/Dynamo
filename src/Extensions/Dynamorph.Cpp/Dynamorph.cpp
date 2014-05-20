// Dynamorph.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Dynamorph.h"

using namespace System;
using namespace Dynamorph;

System::IntPtr Visualizer::Create(System::IntPtr hWndParent, int width, int height)
{
    if (Visualizer::mVisualizer == nullptr)
    {
        auto hParent = ((HWND) hWndParent.ToPointer());
        Visualizer::mVisualizer = gcnew Visualizer();
        Visualizer::mVisualizer->Initialize(hParent, width, height);
    }

    return System::IntPtr(Visualizer::mVisualizer->GetWindowHandle());
}

void Visualizer::Destroy(void)
{
    if (Visualizer::mVisualizer != nullptr) {
        Visualizer::mVisualizer->Uninitialize();
        Visualizer::mVisualizer = nullptr;
    }
}

HWND Visualizer::GetWindowHandle(void)
{
    return this->mhWndVisualizer;
}

Visualizer::Visualizer() : 
    mhWndVisualizer(nullptr)
{
}

void Visualizer::Initialize(HWND hWndParent, int width, int height)
{
    if (mhWndVisualizer != nullptr) {
        auto message = L"'Visualizer::Initialize' cannot be called twice.";
        throw gcnew System::InvalidOperationException(gcnew String(message));
    }

    mhWndVisualizer = CreateWindowEx(0, L"static", L"", WS_CHILD | WS_VISIBLE,
        0, 0, width, height, hWndParent, NULL, NULL, 0);
}

void Visualizer::Uninitialize(void)
{
    if (this->mhWndVisualizer != nullptr) {
        ::DestroyWindow(this->mhWndVisualizer);
        this->mhWndVisualizer = nullptr;
    }
}
