////////////////////////////////////////////////////////////////////////////////
//
//  MediaElementState.cs - This file is part of LibVLC.NET.
//
//    Copyright (C) 2011 Boris Richter <himself@boris-richter.net>
//
//  ==========================================================================
//  
//  LibVLC.NET is free software; you can redistribute it and/or modify it 
//  under the terms of the GNU Lesser General Public License as published by 
//  the Free Software Foundation; either version 2.1 of the License, or (at 
//  your option) any later version.
//    
//  LibVLC.NET is distributed in the hope that it will be useful, but WITHOUT 
//  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
//  FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public 
//  License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License 
//  along with LibVLC.NET; if not, see http://www.gnu.org/licenses/.
//
//  ==========================================================================
// 
//  $LastChangedRevision: 11087 $
//  $LastChangedDate: 2011-09-06 22:22:48 +0200 (Tue, 06 Sep 2011) $
//  $LastChangedBy: unknown $
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibVLC.NET.Presentation
{
  
  //****************************************************************************
  /// <summary>
  ///   Represents the state of a <see cref="MediaElement"/>.
  /// </summary>
  public enum MediaElementState
  {

    //==========================================================================
    /// <summary>
    ///   No media has been loaded.
    /// </summary>
    Empty,

    //==========================================================================
    /// <summary>
    ///   The media element is currently opening a media.
    /// </summary>
    Opening,

    //==========================================================================
    /// <summary>
    ///   The media element is currently playing a media.
    /// </summary>
    Playing,

    //==========================================================================
    /// <summary>
    ///   Playback is currently paused.
    /// </summary>
    Paused,

    //==========================================================================
    /// <summary>
    ///   The end of the media has been reached.
    /// </summary>
    EndReached,

    //==========================================================================
    /// <summary>
    ///   The media has been stopped.
    /// </summary>
    Stopped,

    //==========================================================================
    /// <summary>
    ///   There has been an error opening or playing the media.
    /// </summary>
    EncounteredError,

  } // enum MediaElementState

}
