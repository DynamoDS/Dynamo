#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2011 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using System.Collections.Generic;

namespace Nuclex.Game.Packing {

  /// <summary>Insufficient space left in packing area to contain a given object</summary>
  /// <remarks>
  ///   An exception being sent to you from deep space. Erm, no, wait, it's an exception
  ///   that occurs when a packing algorithm runs out of space and is unable to fit
  ///   the object you tried to pack into the remaining packing area.
  /// </remarks>
#if !NO_SERIALIZATION
  [Serializable]
#endif
  public class OutOfSpaceException : Exception {

    /// <summary>Initializes the exception</summary>
    public OutOfSpaceException() { }

    /// <summary>Initializes the exception with an error message</summary>
    /// <param name="message">Error message describing the cause of the exception</param>
    public OutOfSpaceException(string message) : base(message) { }

    /// <summary>Initializes the exception as a followup exception</summary>
    /// <param name="message">Error message describing the cause of the exception</param>
    /// <param name="inner">Preceding exception that has caused this exception</param>
    public OutOfSpaceException(string message, Exception inner) : base(message, inner) { }

#if !NO_SERIALIZATION

    /// <summary>Initializes the exception from its serialized state</summary>
    /// <param name="info">Contains the serialized fields of the exception</param>
    /// <param name="context">Additional environmental informations</param>
    protected OutOfSpaceException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context
    ) :
      base(info, context) { }

#endif

  }

} // namespace Nuclex.Game.Packing
