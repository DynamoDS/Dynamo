// Dynamorph.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Dynamorph.h"

using namespace System;
using namespace Dynamorph;

System::IntPtr Visualizer::Create(System::IntPtr hWndParent, int width, int height)
{
    auto hWndChild = CreateWindowEx(0, L"static", L"", WS_CHILD | WS_VISIBLE, 0, 0,
        width, height, ((HWND) hWndParent.ToPointer()), NULL, NULL, 0);

    return System::IntPtr(hWndChild);
}
