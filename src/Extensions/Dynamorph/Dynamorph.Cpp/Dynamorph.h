
#ifdef DYNAMORPH_EXPORTS
#define DYNAMORPH_API __declspec(dllexport)
#else
#define DYNAMORPH_API __declspec(dllimport)
#endif

namespace Gen = System::Collections::Generic;
namespace Ds = Autodesk::DesignScript::Interfaces;

namespace Dynamorph
{
    class IGraphicsContext;
    class IShaderProgram;
    class IVertexBuffer;
    class GeometryData;
    class NodeGeometries;
    class BoundingBox;

    public ref class NodeDetails
    {
    public:
        NodeDetails(int depth, Ds::IRenderPackage^ renderPackage)
        {
            this->depth = depth;
            this->renderPackage = renderPackage;
            this->red = this->green = this->blue = 1.0;
        }

        void SetColor(double red, double green, double blue)
        {
            this->red   = ((red < 0.0 ? 0.0 : red) > 1.0 ? 1.0 : red);
            this->green = ((green < 0.0 ? 0.0 : green) > 1.0 ? 1.0 : green);
            this->blue  = ((blue < 0.0 ? 0.0 : blue) > 1.0 ? 1.0 : blue);
        }

        property int Depth
        {
            int get() { return this->depth; }
        }

        property Ds::IRenderPackage^ RenderPackage
        {
            Ds::IRenderPackage^ get() { return this->renderPackage; }
        }

        property double Red     { double get() { return this->red;   } }
        property double Green   { double get() { return this->green; } }
        property double Blue    { double get() { return this->blue;  } }

    private:
        int depth;
        double red, green, blue;
        Ds::IRenderPackage^ renderPackage;
    };

    typedef Gen::IEnumerable<Gen::KeyValuePair<System::String^, NodeDetails^>> NodeDetailsType;

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
        void UpdateNodeDetails(NodeDetailsType^ nodeDetails);
        void RemoveNodeGeometries(Gen::IEnumerable<System::String^>^ nodes);

    private:

        // Private class instance methods.
        Visualizer();
        void Initialize(HWND hWndParent, int width, int height);
        void Uninitialize(void);
        void UpdateNodeGeometries(NodeDetailsType^ nodeDetails);
        void AssociateToDepthValues(NodeDetailsType^ nodeDetails);
        void GetGeometriesAtDepth(int depth, std::vector<NodeGeometries *>& geometries);
        void GetBoundingBox(std::vector<NodeGeometries *>& geometries, BoundingBox& box);
        void RequestFrameUpdate(void);
        void RenderWithBlendingFactor(void);
        void RenderGeometries(const std::vector<NodeGeometries *>& geometries, float alpha);
        LRESULT ProcessMouseMessage(UINT msg, WPARAM wParam, LPARAM lParam);
        LRESULT ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

        // Static class data member.
        static Visualizer^ mVisualizer = nullptr;

        // Class instance data members.
        HWND mhWndVisualizer;
        IGraphicsContext* mpGraphicsContext;

        int mAlphaParamIndex;
        int mColorParamIndex;
        int mControlParamsIndex;
        float mBlendingFactor;
        IShaderProgram* mpShaderProgram;

        // Node data.
        std::vector<std::vector<std::wstring> *>* mpGeomsOnDepthLevel;
        std::map<std::wstring, NodeGeometries*>* mpNodeGeometries;
    };
}
