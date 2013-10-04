using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media.Media3D;

namespace Dynamo
{

    /// <summary>
    /// RenderDescriptions provides the final geometry to be bound by the visualization.
    /// Collections are of the type ThreadSafeList which allows for the RenderDescription to be
    /// written to and read from asynchronously.
    /// </summary>
    public class RenderDescription
    {
        /// <summary>
        /// A collection of Point objects to be used for rendering points as selected.
        /// </summary>
        public ThreadSafeList<Point3D> SelectedPoints{ get ; internal set;}
 
        /// <summary>
        /// A collection of Point objects to be used for rendering lines as selected.
        /// </summary>
        public ThreadSafeList<Point3D> SelectedLines { get; internal set; }

        /// <summary>
        /// A collection of mesh objects to be used for rendering as selected.
        /// </summary>
        public ThreadSafeList<MeshGeometry3D> SelectedMeshes { get; internal set; }
 
        /// <summary>
        /// A collection of Point objects used to render points
        /// </summary>
        public ThreadSafeList<Point3D> Points { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render lines
        /// </summary>
        public ThreadSafeList<Point3D> Lines { get; internal set; }

        /// <summary>
        /// A collection of mesh objects to be used for rendering
        /// </summary>
        public ThreadSafeList<MeshGeometry3D> Meshes { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the x axes of transforms
        /// </summary>
        public ThreadSafeList<Point3D> XAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the y axes of transforms
        /// </summary>
        public ThreadSafeList<Point3D> YAxisPoints { get; internal set; }

        /// <summary>
        /// A collection of Point objects used to render the z axes of transforms
        /// </summary>
        public ThreadSafeList<Point3D> ZAxisPoints { get; internal set; }

        public RenderDescription()
        {
            Points = new ThreadSafeList<Point3D>();
            Lines = new ThreadSafeList<Point3D>();
            Meshes = new ThreadSafeList<MeshGeometry3D>();
            XAxisPoints = new ThreadSafeList<Point3D>();
            YAxisPoints = new ThreadSafeList<Point3D>();
            ZAxisPoints = new ThreadSafeList<Point3D>();

            SelectedPoints = new ThreadSafeList<Point3D>();
            SelectedLines = new ThreadSafeList<Point3D>();
            SelectedMeshes = new ThreadSafeList<MeshGeometry3D>();
        }

        public void Clear()
        {
            Points.Clear();
            Lines.Clear();
            Meshes.Clear();
            XAxisPoints.Clear();
            YAxisPoints.Clear();
            ZAxisPoints.Clear();
            SelectedPoints.Clear();
            SelectedLines.Clear();
            SelectedMeshes.Clear();
        }
    }

    /// <summary>
    /// A thread-safe IEnumerator implementation.
    /// See: http://www.codeproject.com/KB/cs/safe_enumerable.aspx
    /// </summary>
    public class SafeEnumerator<T> : IEnumerator<T>
    {
        // this is the (thread-unsafe)
        // enumerator of the underlying collection
        private readonly IEnumerator<T> _inner;

        // this is the object we shall lock on.
        private readonly object _lock;

        public SafeEnumerator(IEnumerator<T> inner, object @lock)
        {
            _inner = inner;
            _lock = @lock;

            // entering lock in constructor
            Monitor.Enter(_lock);
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            // .. and exiting lock on Dispose()
            // This will be called when foreach loop finishes
            Monitor.Exit(_lock);
        }

        /// <remarks>
        /// we just delegate actual implementation
        /// to the inner enumerator, that actually iterates
        /// over some collection
        /// </remarks>
        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }
    }

    /// <summary>
    /// A thread-safe IList implementation using the custom SafeEnumerator class
    /// See: http://www.codeproject.com/KB/cs/safe_enumerable.aspx
    /// See: http://theburningmonk.com/2010/03/thread-safe-enumeration-in-csharp/
    /// </summary>
    public class ThreadSafeList<T> : IList<T>
    {
        // the (thread-unsafe) collection that actually stores everything
        private readonly List<T> _inner;

        // this is the object we shall lock on.
        private readonly object _lock = new object();

        public ThreadSafeList()
        {
            _inner = new List<T>();
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _inner.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public T this[int index]
        {
            get
            {
                lock (_lock)
                {
                    return _inner[index];
                }
            }
            set
            {
                lock (_lock)
                {
                    _inner[index] = value;
                }
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (_lock)
            {
                // instead of returning an usafe enumerator,
                // we wrap it into our thread-safe class
                return new SafeEnumerator<T>(_inner.GetEnumerator(), _lock);
            }
        }

        /// <remarks>
        /// To be actually thread-safe, our collection must be locked on all other operations
        /// </remarks>
        public void Add(T item)
        {
            lock (_lock)
            {
                _inner.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    _inner.Add(item);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _inner.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_lock)
            {
                return _inner.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lock)
            {
                _inner.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item)
        {
            lock (_lock)
            {
                return _inner.Remove(item);
            }
        }

        public IEnumerator GetEnumerator()
        {
            lock (_lock)
            {
                return new SafeEnumerator<T>(_inner.GetEnumerator(), _lock);
            }
        }

        public int IndexOf(T item)
        {
            lock (_lock)
            {
                return _inner.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
            {
                _inner.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _inner.RemoveAt(index);
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_lock)
            {
                return new ReadOnlyCollection<T>(this);
            }
        }

        public void ForEach(Action<T> action)
        {
            if (action == null)
                return;

            lock (_lock)
            {
                foreach (var item in _inner)
                {
                    action(item);
                }
            }
        }

        public bool Exists(Predicate<T> match)
        {
            if (match == null)
                return false;

            lock (_lock)
            {
                foreach (var item in _inner)
                {
                    if (match(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
