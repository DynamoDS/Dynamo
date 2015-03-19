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
    /// A more lightweight alternative to List of T.
    /// On clear, only resets the count and does not clear the references 
    ///   => this works because of the ObjectManager.
    /// Includes a stack functionality.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SimpleList<T>
    {
        T[] items;
        int capacity;
        
        public int Count;

        /// <summary>
        /// Get the i-th element.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get { return items[i]; }
            set { items[i] = value; }
        }

        /// <summary>
        /// Size matters.
        /// </summary>
        void EnsureCapacity()
        {
            if (capacity == 0)
            {
                capacity = 32;
                items = new T[32];
            }
            else
            {
                var newItems = new T[capacity * 2];
                Array.Copy(items, newItems, capacity);
                capacity = 2 * capacity;
                items = newItems;
            }
        }

        /// <summary>
        /// Adds a vertex to the buffer.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (Count + 1 > capacity) EnsureCapacity();
            items[Count++] = item;
        }

        /// <summary>
        /// Pushes the value to the back of the list.
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            if (Count + 1 > capacity) EnsureCapacity();
            items[Count++] = item;
        }

        /// <summary>
        /// Pops the last value from the list.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            return items[--Count];
        }

        /// <summary>
        /// Sets the Count to 0, otherwise does nothing.
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }
    }

    /// <summary>
    /// A fancy name for a list of integers.
    /// </summary>
    class IndexBuffer : SimpleList<int>
    {

    }
              
    /// <summary>
    /// A priority based linked list.
    /// </summary>
    sealed class FaceList
    {
        ConvexFaceInternal first, last;
        
        /// <summary>
        /// Get the first element.
        /// </summary>
        public ConvexFaceInternal First { get { return first; } }

        /// <summary>
        /// Adds the element to the beginning.
        /// </summary>
        /// <param name="face"></param>
        void AddFirst(ConvexFaceInternal face)
        {
            face.InList = true;
            this.first.Previous = face;
            face.Next = this.first;
            this.first = face;
        }

        /// <summary>
        /// Adds a face to the list.
        /// </summary>
        /// <param name="face"></param>
        public void Add(ConvexFaceInternal face)
        {
            if (face.InList)
            {
                if (this.first.VerticesBeyond.Count < face.VerticesBeyond.Count)
                {
                    Remove(face);
                    AddFirst(face);
                }
                return;
            }

            face.InList = true;

            if (first != null && first.VerticesBeyond.Count < face.VerticesBeyond.Count)
            {
                this.first.Previous = face;
                face.Next = this.first;
                this.first = face;
            }
            else
            {
                if (this.last != null)
                {
                    this.last.Next = face;
                }
                face.Previous = this.last;
                this.last = face;
                if (this.first == null)
                {
                    this.first = face;
                }
            }
        }

        /// <summary>
        /// Removes the element from the list.
        /// </summary>
        /// <param name="face"></param>
        public void Remove(ConvexFaceInternal face)
        {
            if (!face.InList) return;

            face.InList = false;

            if (face.Previous != null)
            {
                face.Previous.Next = face.Next;
            }
            else if (/*first == face*/ face.Previous == null)
            {
                this.first = face.Next;
            }

            if (face.Next != null)
            {
                face.Next.Previous = face.Previous;
            }
            else if (/*last == face*/ face.Next == null)
            {
                this.last = face.Previous;
            }

            face.Next = null;
            face.Previous = null;
        }
    }

    /// <summary>
    /// Connector list.
    /// </summary>
    sealed class ConnectorList
    {
        FaceConnector first, last;

        /// <summary>
        /// Get the first element.
        /// </summary>
        public FaceConnector First { get { return first; } }

        /// <summary>
        /// Adds the element to the beginning.
        /// </summary>
        /// <param name="connector"></param>
        void AddFirst(FaceConnector connector)
        {
            this.first.Previous = connector;
            connector.Next = this.first;
            this.first = connector;
        }

        /// <summary>
        /// Adds a face to the list.
        /// </summary>
        /// <param name="element"></param>
        public void Add(FaceConnector element)
        {
            if (this.last != null)
            {
                this.last.Next = element;
            }
            element.Previous = this.last;
            this.last = element;
            if (this.first == null)
            {
                this.first = element;
            }
        }

        /// <summary>
        /// Removes the element from the list.
        /// </summary>
        /// <param name="connector"></param>
        public void Remove(FaceConnector connector)
        {
            if (connector.Previous != null)
            {
                connector.Previous.Next = connector.Next;
            }
            else if (/*first == face*/ connector.Previous == null)
            {
                this.first = connector.Next;
            }

            if (connector.Next != null)
            {
                connector.Next.Previous = connector.Previous;
            }
            else if (/*last == face*/ connector.Next == null)
            {
                this.last = connector.Previous;
            }

            connector.Next = null;
            connector.Previous = null;
        }
    }
}
