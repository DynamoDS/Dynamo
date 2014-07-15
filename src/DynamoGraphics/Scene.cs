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
        private VisualizerHwndHost hwndHost = null;
        private Dictionary<string, NodeData> nodeData = null;

        #endregion

        #region Public Class Properties

        public VisualizerHwndHost HwndHost { get { return hwndHost; } }

        #endregion

        #region Public Class Operational Methods

        public static Scene CreateScene(double width, double height)
        {
            if (currentScene == null)
                currentScene = new Scene(width, height);

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

        private Scene(double width, double height)
        {
            this.nodeData = new Dictionary<string, NodeData>();
            this.hwndHost = new VisualizerHwndHost(width, height);
            this.hwndHost.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Destroy()
        {
            if (this.hwndHost != null)
            {
                this.hwndHost.Dispose();
                this.hwndHost = null;
            }
        }

        #endregion
    }
}
