using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Graphics
{
    public class NodeData
    {
    }

    public class Scene
    {
        #region Private Class Data Members

        // There can only be one scene in a session.
        private static Scene currentScene = null;

        // Instance class data members.
        private Visualizer visualizer = null;
        private Dictionary<string, NodeData> nodeData = null;

        #endregion

        #region Public Class Operational Methods

        public static Scene CreateScene()
        {
            if (currentScene == null)
                currentScene = new Scene();

            return currentScene;
        }

        public static void DestroyScene()
        {
            if (currentScene != null)
                currentScene.Destroy();

            currentScene = null;
        }

        #endregion

        #region Private Class Helper Methods

        private Scene()
        {
            this.nodeData = new Dictionary<string, NodeData>();
            this.visualizer = new Visualizer();
        }

        private void Destroy()
        {
            this.visualizer.Destroy();
            this.visualizer = null;
        }

        #endregion
    }
}
