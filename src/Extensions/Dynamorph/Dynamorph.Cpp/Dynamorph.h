
#ifdef DYNAMORPH_EXPORTS
#define DYNAMORPH_API __declspec(dllexport)
#else
#define DYNAMORPH_API __declspec(dllimport)
#endif

namespace Dynamorph
{
    class IGraphicsContext;
    class IShaderProgram;
    class IVertexBuffer;
    class GeometryData;
    class NodeGeometries;

    public ref class Visualizer
    {
    public:

        // Static class methods
        static System::IntPtr Create(System::IntPtr hwndParent, int width, int height);
        static void Destroy(void);
        static Visualizer^ CurrentInstance(void);
        static LRESULT _stdcall WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

        // Public class methods.
        HWND GetWindowHandle(void);
        void UpdateNodeGeometries(System::Collections::Generic::Dictionary<System::Guid,
            Autodesk::DesignScript::Interfaces::IRenderPackage^>^ geometries);
        void RemoveNodeGeometries(System::Collections::Generic::IEnumerable<System::String^>^ nodes);

    private:

        // Private class instance methods.
        Visualizer();
        void Initialize(HWND hWndParent, int width, int height);
        void Uninitialize(void);
        LRESULT ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

        // Static class data member.
        static Visualizer^ mVisualizer = nullptr;

        // Class instance data members.
        HWND mhWndVisualizer;
        IGraphicsContext* mpGraphicsContext;
        IShaderProgram* mpShaderProgram;

        // Node data.
        std::vector<std::vector<NodeGeometries*>>* mpGeomsOnDepthLevel;
        std::map<std::wstring, NodeGeometries*>* mpNodeGeometries;
    };
}
