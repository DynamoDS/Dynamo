using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.Collections;

namespace ExperimentalMesh
{
    internal class MeshColor : IColor
    {
        public MeshColor(byte r, byte g, byte b, byte a)
        {
            AlphaValue = a;
            RedValue = r;
            GreenValue = g;
            BlueValue = b;
        }
        public byte AlphaValue { get; set; }
        public byte RedValue { get;set; }
        public byte GreenValue { get;set; }
        public byte BlueValue { get;set; }
    }

    public class TopologyMesh : IDisposable
    {
        static TopologyMesh()
        {
            var uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            string install_path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            string protogeometry_filepath = Path.Combine(install_path, "ProtoGeometry.dll");
            Assembly geometry_assembly = Assembly.LoadFrom(protogeometry_filepath);
            Type edge_type = geometry_assembly.GetType(@"Autodesk.DesignScript.Geometry.Edge");
            Type face_type = geometry_assembly.GetType(@"Autodesk.DesignScript.Geometry.Face");
            Type vertex_type = geometry_assembly.GetType(@"Autodesk.DesignScript.Geometry.Vertex");
            Type factory_type = geometry_assembly.GetType(@"Autodesk.DesignScript.Geometry.HostFactory");

            BindingFlags binding_flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            edge_constructor = edge_type.GetConstructor(binding_flags, null, new Type[] { typeof(IEdgeEntity) }, null);
            face_constructor = face_type.GetConstructor(binding_flags, null, new Type[] { typeof(IFaceEntity) }, null);
            vertex_constructor = vertex_type.GetConstructor(binding_flags, null, new Type[] { typeof(IVertexEntity) }, null);

            persistence_manager_property = factory_type.GetProperty("PersistenceManager", binding_flags);
        }

        public static TopologyMesh FromSubDMesh(SubDivisionMesh mesh)
        {
            return new TopologyMesh(mesh);
        }

        public static TopologyMesh ByVerticesFaceIndices(Point[] vertices, int[][] faceIndices)
        {
            using (SubDivisionMesh mesh = SubDivisionMesh.ByVerticesFaceIndices(vertices, faceIndices, 0))
            {
                return new TopologyMesh(mesh);
            }
        }

        protected TopologyMesh(SubDivisionMesh mesh, bool persist = true)
        {
            m_meshEntity = GeometryExtension.ToEntity(mesh) as ISubDMeshEntity;
            if (null != m_meshEntity)
            {
                ISurfaceEntity surface = m_meshEntity.ConvertToSurface(false);
                if (persist)
                {
                    IPersistenceManager persistence_manager = persistence_manager_property.GetValue(null, null) as IPersistenceManager;
                    if (null != surface && null != persistence_manager)
                        m_brep = persistence_manager.Persist(surface) as IBRepEntity;
                }
                else
                if (null != surface)
                    m_brep = surface as IBRepEntity;
            }
        }

        public Face[] Faces
        {
            get
            {
                if (null != m_brep && null != face_constructor)
                    return m_brep.GetFaces().ConvertAll((IFaceEntity host)=>(Face)(face_constructor.Invoke(new object[]{(object)host})));
                return null;
            }
        }

        public Edge[] Edges
        {
            get
            {
                if (null != m_brep && null != edge_constructor)
                    return m_brep.GetEdges().ConvertAll((IEdgeEntity host) => (Edge)(edge_constructor.Invoke(new object[] { (object)host })));
                return null;
            }
        }

        public Vertex[] Vertices
        {
            get
            {
                if (null != m_brep && null != vertex_constructor)
                    return m_brep.GetVertices().ConvertAll((IVertexEntity host) => (Vertex)(vertex_constructor.Invoke(new object[] { (object)host })));
                return null;
            }
        }

        public int NumFaces
        {
            get
            {
                return m_brep.GetFaceCount();
            }
        }

        public int NumEdges
        {
            get
            {
                return m_brep.GetEdgeCount();
            }
        }

        public int NumVertices
        {
            get
            {
                return m_brep.GetVertexCount();
            }
        }

        private bool visibilityForHighlight = false;
        public bool Highlight(bool visibility)
        {
            if (null == Display)
                return false;
            if (!Display.Visible && visibility)
            {
                visibilityForHighlight = true;
                Display.Visible = true;
            }
            if (Display.Visible && !visibility && visibilityForHighlight)
            {
                visibilityForHighlight = false;
                Display.Visible = false;
            }
            return Display.Highlight(visibility);
        }

        private IDisplayable Display
        {
            get
            {
                return m_brep as IDisplayable;
            }
        }

        public void SetColor(byte redVal, byte blueVal, byte greenVal)
        {
            MeshColor color = new MeshColor(redVal, greenVal, blueVal, 255);
            if (null != Display)
                Display.SetColor(color);
        }

        public virtual bool Visible
        {
            get
            {
                if (null != Display)
                    return Display.Visible;
                return true;
            }
            set
            {
                if (null != Display)
                    Display.SetVisibility(value);
            }
        }
        
        public void SetColor(Color color)
        {
            if (null != Display)
                SetColor(color.RedValue, color.BlueValue, color.GreenValue);
        }

        public void SetVisibility(bool visible)
        {
            this.Visible = visible;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != m_brep)
                {
                    if (Application.Instance.IsExecuting)
                        (m_brep as IPersistentObject).Erase();
                }
            }

            //if (Application.Instance.IsExecuting)
              //  Application.Instance.ToBeDisposed(m_brep as IDisposable);
        }

        private IBRepEntity m_brep;
        private ISubDMeshEntity m_meshEntity;

        private static ConstructorInfo face_constructor;
        private static ConstructorInfo edge_constructor;
        private static ConstructorInfo vertex_constructor;
        private static PropertyInfo persistence_manager_property;
    }
}
