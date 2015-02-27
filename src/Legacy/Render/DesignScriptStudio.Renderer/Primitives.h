
#pragma once

#include <math.h>

namespace DesignScriptStudio { namespace Renderer {

    class Vector
    {
    public:
        Vector() : x(0), y(0), z(0) { }
        Vector(float _x, float _y, float _z) : x(_x), y(_y), z(_z) { }

        Vector operator- () const
        {
            return Vector(-x, -y, -z);
        }

        Vector operator+ (const Vector& v) const    { return Vector(x + v.x, y + v.y, z + v.z);     }
        Vector operator- (const Vector& v) const    { return Vector(x - v.x, y - v.y, z - v.z);     }
        Vector operator* (float d) const            { return Vector(x * d, y * d, z * d);           }
        Vector operator* (const Vector& v) const    { return Cross(v);                              }

        float Length() const
        {
            return sqrtf(x * x + y * y + z * z);
        }

        void Normalize()
        {
            float invLength = 1.0f / Length(); 
            x *= invLength;
            y *= invLength;
            z *= invLength;
        }

        Vector Inverse(void) const
        {
            return Vector(-x, -y, -z);
        }

        Vector Cross(const Vector& v) const
        {
            float ox = (y * v.z) - (v.y * z);
            float oy = (z * v.x) - (v.z * x);
            float oz = (x * v.y) - (v.x * y);
            return Vector(ox, oy, oz);
        }

        float x;
        float y;
        float z;
    };

} }
