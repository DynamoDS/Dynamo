
#include "stdafx.h"
#include "Utilities.h"

bool Utils::LoadShaderResource(unsigned int id, std::string& content)
{
    auto module = GetModuleHandle(L"Bloodstone.Cpp.dll");
    if (module != nullptr)
    {
        auto resourceInfo = FindResource(module, MAKEINTRESOURCE(id), L"SHADER");
        if (resourceInfo != nullptr)
        {
            auto loaded = LoadResource(module, resourceInfo);
            if (loaded != nullptr)
            {
                // Does not have to be unlocked.
                auto data = LockResource(loaded);
                content = ((const char *) data);
            }

            FreeResource(loaded);
            return true;
        }
    }

    return false;
}

Interpolator::Interpolator(float durationInSeconds) : 
    mCurrentTimeInSeconds(-2.0f),
    mDurationInSeconds(durationInSeconds),
    mInversedFrequency(0.0)
{
    LARGE_INTEGER frequency;
    QueryPerformanceFrequency(&frequency);
    mInversedFrequency = 1.0 / frequency.QuadPart;
}

bool Interpolator::Update(float& fraction)
{
    if (mCurrentTimeInSeconds < -1.0f)
    {
        fraction = 0.0f;
        mCurrentTimeInSeconds = 0.0f;
        QueryPerformanceCounter(&mStartTime);
        return true;
    }

    mCurrentTimeInSeconds = GetSecondsSinceStarted();
    if (mCurrentTimeInSeconds >= mDurationInSeconds) {
        fraction = 1.0f;
        return false;
    }

    fraction = mCurrentTimeInSeconds / mDurationInSeconds;
    return true;
}

float Interpolator::GetSecondsSinceStarted(void) const
{
    LARGE_INTEGER currentTime;
    QueryPerformanceCounter(&currentTime);

    LONGLONG difference = currentTime.QuadPart - mStartTime.QuadPart;
    return ((float)(difference * mInversedFrequency));
}
