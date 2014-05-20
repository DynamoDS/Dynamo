
#ifdef DYNAMORPH_EXPORTS
#define DYNAMORPH_API __declspec(dllexport)
#else
#define DYNAMORPH_API __declspec(dllimport)
#endif

namespace Dynamorph
{
    class IGraphicsContext;

    DYNAMORPH_API public ref class Visualizer
    {
    public:

        // Static class methods
        static System::IntPtr Create(System::IntPtr hwndParent, int width, int height);
        static void Destroy(void);

        // Public class methods.
        HWND GetWindowHandle(void);

    private:

        // Private class instance methods.
        Visualizer();
        void Initialize(HWND hWndParent, int width, int height);
        void Uninitialize(void);

        // Static class data member.
        static Visualizer^ mVisualizer = nullptr;

        // Class instance data members.
        HWND mhWndVisualizer;
        IGraphicsContext* mpGraphicsContext;
    };
}
