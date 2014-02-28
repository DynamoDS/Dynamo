using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace DSGeometry
{
    enum ExecutionEvent
    {
        StartUp,
        OnBeginExecution,
        OnSuspendExecution,
        OnResumeExecution,
        OnEndExecution,
        ShutDown
    }

    public class DSGeometryApplication : IExtensionApplication
    {
        public void StartUp()
        {
            m_events.Add(ExecutionEvent.StartUp);
        }
        public void OnBeginExecution(IExecutionSession session)
        {
            m_events.Add(ExecutionEvent.OnBeginExecution);
        }
        public void OnSuspendExecution(IExecutionSession session)
        {
            m_events.Add(ExecutionEvent.OnSuspendExecution);
        }
        public void OnResumeExecution(IExecutionSession session)
        {
            m_events.Add(ExecutionEvent.OnResumeExecution);
        }
        public void OnEndExecution(IExecutionSession session)
        {
            m_events.Add(ExecutionEvent.OnEndExecution);
        }
        public void ShutDown()
        {
            m_events.Add(ExecutionEvent.ShutDown);
        }
        public static void Check()
        {
            int nCount = m_events.Count;
            if (nCount < 2)
            {
                throw new System.Exception("IExtensionApplication methods are not called correctly!");
            }
            else if (nCount == 2)
            {
                if ((m_events[m_events.Count - 1] != ExecutionEvent.OnBeginExecution) ||
                     (m_events[m_events.Count - 2] != ExecutionEvent.StartUp))
                {
                    throw new System.Exception("IExtensionApplication methods are not called correctly!");
                }
            }
            else if (nCount > 2)
            {
                if (m_events[m_events.Count - 2] != ExecutionEvent.OnEndExecution)
                {
                    throw new System.Exception("IExtensionApplication methods are not called correctly!");
                }
                else
                {
                    m_events.RemoveRange(2, nCount-2);
                }
            }
        }

        private static List<ExecutionEvent> m_events = new List<ExecutionEvent>();
    }

}
