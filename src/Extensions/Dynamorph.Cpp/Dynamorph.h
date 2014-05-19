
#ifdef DYNAMORPH_EXPORTS
#define DYNAMORPH_API __declspec(dllexport)
#else
#define DYNAMORPH_API __declspec(dllimport)
#endif

namespace Dynamorph
{
    DYNAMORPH_API public ref class Visualizer
    {
    public:

        static System::IntPtr Create(System::IntPtr hwndParent, int width, int height);
    };
}
