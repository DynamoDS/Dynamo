
#ifdef BLOODSTONE_EXPORTS
#define BLOODSTONE_API __declspec(dllexport)
#else
#define BLOODSTONE_API __declspec(dllimport)
#endif

namespace Gen = System::Collections::Generic;
namespace Ds = Autodesk::DesignScript::Interfaces;

namespace Dynamo { namespace Bloodstone {

    class IGraphicsContext;
    class IShaderProgram;
    class IVertexBuffer;
    class GeometryData;
    class NodeSceneData;
    class BoundingBox;
    class BillboardTextGroup;
    ref class Scene;

    public enum class SelectMode { AddToExisting, RemoveFromExisting, ClearExisting };
    public enum class RenderMode { Shaded, Primitive };

    public ref class NodeColor
    {
        float fr, fg, fb, fa;
        unsigned char r, g, b, a;

    public:
        NodeColor(unsigned char r, unsigned char g, unsigned char b, unsigned char a)
        {
            this->r = r;
            this->g = g;
            this->b = b;
            this->a = a;

            float inv255 = 1.0f / 255.0f;
            this->fr = r * inv255;
            this->fg = g * inv255;
            this->fb = b * inv255;
            this->fa = a * inv255;
        }

        void Get(float* pRgbaColor)
        {
            pRgbaColor[0] = fr;
            pRgbaColor[1] = fg;
            pRgbaColor[2] = fb;
            pRgbaColor[3] = fa;
        }
    };

    public ref class VisualizerWnd
    {
    public:

        // Static class methods
        static System::IntPtr Create(System::IntPtr hwndParent, int width, int height);
        static void Destroy(void);
        static VisualizerWnd^ CurrentInstance(void);
        static LRESULT WndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

        // Public class methods.
        bool IsGraphicsContextCreated(void);
        void ShowWindow(bool show);
        void RequestFrameUpdate(void);
        HWND GetWindowHandle(void);
        Scene^ GetScene(void);
        IGraphicsContext* GetGraphicsContext(void);

    private:

        // Private class instance methods.
        VisualizerWnd();
        bool Initialize(HWND hWndParent, int width, int height);
        void Uninitialize(void);
        LRESULT ProcessMouseMessage(UINT msg, WPARAM wParam, LPARAM lParam);
        LRESULT ProcessMessage(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam);

        // Static class data member.
        static VisualizerWnd^ mVisualizer = nullptr;

        // Class instance data members.
        bool mGraphicsContextCreated;
        HWND mhWndVisualizer;
        Scene^ mpScene;
        IGraphicsContext* mpGraphicsContext;
    };

    typedef Gen::IEnumerable<System::String^> Strings;
    typedef Gen::IEnumerable<Gen::KeyValuePair<System::String^, NodeColor^>> NodeColors;
    typedef Gen::IEnumerable<Gen::KeyValuePair<System::String^, RenderMode>> RenderModes;
    typedef Gen::IEnumerable<Gen::KeyValuePair<System::String^, Ds::IRenderPackage^>> RenderPackages;

    public ref class Scene
    {
    public:
        Scene(VisualizerWnd^ visualizer);
        void Initialize(int width, int height);
        void Destroy(void);
        void RenderScene(void);
        void ClearAllGeometries(void);
        void GetBoundingBox(BoundingBox& boundingBox);
        void UpdateNodeGeometries(RenderPackages^ geometries);
        void RemoveNodeGeometries(Strings^ identifiers);
        void SelectNodes(Strings^ identifiers, SelectMode selectMode);
        void SetNodeColor(NodeColors^ nodeColors);
        void SetNodeRenderMode(RenderModes^ renderModes);

    private:
        void RenderGeometries(const std::vector<NodeSceneData *>& geometries);

    private:
        int mAlphaParamIndex;
        int mColorParamIndex;
        int mControlParamsIndex;
        IShaderProgram* mpPhongShader;
        BillboardTextGroup* mpBillboardTextGroup;

        VisualizerWnd^ mVisualizer;
        std::map<std::wstring, NodeSceneData*>* mpNodeSceneData;
    };
} }
