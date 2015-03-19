/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2014 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it 
 *  under the terms of  the GNU Lesser General Public License as published by 
 *  the Free Software Foundation; either version 2.1 of the License, or 
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, 
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser 
 *  General Public License for more details.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A helper class for object allocation/storage. 
    /// This helps the GC a lot as it prevents the creation of about 75% of 
    /// new face objects (in the case of ConvexFaceInternal). In the case of
    /// FaceConnectors and DefferedFaces, the difference is even higher (in most
    /// cases O(1) vs O(number of created faces)). 
    /// </summary>
    class ObjectManager
    {
        readonly int Dimension;

        ConvexHullInternal Hull;
        int FacePoolSize, FacePoolCapacity;
        ConvexFaceInternal[] FacePool;
        IndexBuffer FreeFaceIndices;
        FaceConnector ConnectorStack;
        SimpleList<IndexBuffer> EmptyBufferStack;
        SimpleList<DeferredFace> DeferredFaceStack;

        /// <summary>
        /// Return the face to the pool for later use.
        /// </summary>
        /// <param name="faceIndex"></param>
        public void DepositFace(int faceIndex)
        {
            var face = FacePool[faceIndex];
            var af = face.AdjacentFaces;
            for (int i = 0; i < af.Length; i++)
            {
                af[i] = -1;
            }
            FreeFaceIndices.Push(faceIndex);
        }

        /// <summary>
        /// Reallocate the face pool, including the AffectedFaceFlags
        /// </summary>
        void ReallocateFacePool()
        {
            var newPool = new ConvexFaceInternal[2 * FacePoolCapacity];
            var newTags = new bool[2 * FacePoolCapacity];
            Array.Copy(FacePool, newPool, FacePoolCapacity);
            Buffer.BlockCopy(Hull.AffectedFaceFlags, 0, newTags, 0, FacePoolCapacity * sizeof(bool));
            FacePoolCapacity = 2 * FacePoolCapacity;
            Hull.FacePool = newPool;
            this.FacePool = newPool;
            Hull.AffectedFaceFlags = newTags;
        }

        /// <summary>
        /// Create a new face and put it in the pool.
        /// </summary>
        /// <returns></returns>
        int CreateFace()
        {
            var index = FacePoolSize;
            var face = new ConvexFaceInternal(Dimension, index, GetVertexBuffer());
            FacePoolSize++;
            if (FacePoolSize > FacePoolCapacity) ReallocateFacePool();
            FacePool[index] = face;
            return index;
        }

        /// <summary>
        /// Return index of an unused face or creates a new one.
        /// </summary>
        /// <returns></returns>
        public int GetFace()
        {
            if (FreeFaceIndices.Count > 0) return FreeFaceIndices.Pop();
            return CreateFace();
        }

        /// <summary>
        /// Store a face connector in the "embedded" linked list.
        /// </summary>
        /// <param name="connector"></param>
        public void DepositConnector(FaceConnector connector)
        {
            if (ConnectorStack == null)
            {
                connector.Next = null;
                ConnectorStack = connector;
            }
            else
            {
                connector.Next = ConnectorStack;
                ConnectorStack = connector;
            }
        }

        /// <summary>
        /// Get an unused face connector. If none is available, create it.
        /// </summary>
        /// <returns></returns>
        public FaceConnector GetConnector()
        {
            if (ConnectorStack == null) return new FaceConnector(Dimension);

            var ret = ConnectorStack;
            ConnectorStack = ConnectorStack.Next;
            ret.Next = null;
            return ret;
        }

        /// <summary>
        /// Deposit the index buffer.
        /// </summary>
        /// <param name="buffer"></param>
        public void DepositVertexBuffer(IndexBuffer buffer)
        {
            buffer.Clear();
            EmptyBufferStack.Push(buffer);
        }

        /// <summary>
        /// Get a store index buffer or create a new instance.
        /// </summary>
        /// <returns></returns>
        public IndexBuffer GetVertexBuffer()
        {
            return EmptyBufferStack.Count != 0 ? EmptyBufferStack.Pop() : new IndexBuffer();
        }

        /// <summary>
        /// Deposit the deferred face.
        /// </summary>
        /// <param name="face"></param>
        public void DepositDeferredFace(DeferredFace face)
        {
            DeferredFaceStack.Push(face);
        }

        /// <summary>
        /// Get the deferred face.
        /// </summary>
        /// <returns></returns>
        public DeferredFace GetDeferredFace()
        {
            return DeferredFaceStack.Count != 0 ? DeferredFaceStack.Pop() : new DeferredFace();
        }

        /// <summary>
        /// Create the manager.
        /// </summary>
        /// <param name="hull"></param>
        public ObjectManager(ConvexHullInternal hull)
        {
            this.Dimension = hull.Dimension;
            this.Hull = hull;
            this.FacePool = hull.FacePool;
            this.FacePoolSize = 0;
            this.FacePoolCapacity = hull.FacePool.Length;
            this.FreeFaceIndices = new IndexBuffer();

            this.EmptyBufferStack = new SimpleList<IndexBuffer>();
            this.DeferredFaceStack = new SimpleList<DeferredFace>();
        }
    }
}
