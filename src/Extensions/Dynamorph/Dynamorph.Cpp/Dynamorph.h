
#ifdef DYNAMORPH_EXPORTS
#define DYNAMORPH_API __declspec(dllexport)
#else
#define DYNAMORPH_API __declspec(dllimport)
#endif

namespace Gen = System::Collections::Generic;
namespace Ds = Autodesk::DesignScript::Interfaces;

typedef Gen::IEnumerable<Gen::KeyValuePair<System::String^, int>> NodeDepthsType;
typedef Gen::Dictionary<System::String^, Ds::IRenderPackage^> NodeGeomsType;

namespace Dynamorph
{
    class IGraphicsContext;
    class IShaderProgram;
    class IVertexBuffer;
    class GeometryData;
    class NodeGeometries;

    public ref class UpdateGeometryParam
    {
    public:
        UpdateGeometryParam(NodeDepthsType^ depths, NodeGeomsType^ geometries)
        {
            this->depths = depths;
            this->geometries = geometries;
        }

        property NodeDepthsType^ Depth
        {
            NodeDepthsType^ get() { return this->depths; }
        }

        property NodeGeomsType^ Geometries
        {
            NodeGeomsType^ get() { return this->geometries; }
        }

    private:
        NodeDepthsType^ depths;
        NodeGeomsType^ geometries;
    };

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
        void BlendGeometryLevels(float blendingFactor);
        void UpdateNodeGeometries(UpdateGeometryParam^ geometryParam);
        void RemoveNodeGeometries(Gen::IEnumerable<System::String^>^ nodes);

    private:

        // Private class instance methods.
        Visualizer();
        void Initialize(HWND hWndParent, int width, int height);
        void Uninitialize(void);
        void UpdateNodeGeometries(NodeGeomsType^ geometries);
        void AssociateToDepthValues(NodeDepthsType^ depths);
        void RenderWithBlendingFactor(void);
        void RenderGeometriesAtDepth(int depth, float alpha);
        LRESULT ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

        // Static class data member.
        static Visualizer^ mVisualizer = nullptr;

        // Class instance data members.
        float mBlendingFactor;
        HWND mhWndVisualizer;
        IGraphicsContext* mpGraphicsContext;
        IShaderProgram* mpShaderProgram;

        // Node data.
        std::vector<std::vector<std::wstring> *>* mpGeomsOnDepthLevel;
        std::map<std::wstring, NodeGeometries*>* mpNodeGeometries;
    };
}
