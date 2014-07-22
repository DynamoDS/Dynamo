
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
