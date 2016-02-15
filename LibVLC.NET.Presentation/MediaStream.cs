////////////////////////////////////////////////////////////////////////////////
//
//  Presentation/MediaStream.cs - This file is part of LibVLC.NET.
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
//  $LastChangedRevision: 4950 $
//  $LastChangedDate: 2011-04-05 21:48:05 +0200 (Tue, 05 Apr 2011) $
//  $LastChangedBy: unknown $
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace LibVLC.NET.Presentation
{

  //****************************************************************************
  /// <summary>
  ///   Base class for all stream classes like a <see cref="VideoStream"/>, 
  ///   an <see cref="AudioStream"/> or a <see cref="SubtitleStream"/>.
  /// </summary>
  public class MediaStream
  {

    //==========================================================================
    private readonly Track m_Track;

    //==========================================================================
    internal MediaStream(Track track)
    {
      if(track == null)
        throw new ArgumentNullException("track");
      m_Track = track;
    }

    //==========================================================================
    /// <summary>
    ///   Overrides <see cref="Object.ToString"/> and returns a string
    ///   representing the media stream.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return m_Track.ToString();
    }

    #region Properties

    #region Track

    //==========================================================================                
    /// <summary>
    ///   Gets the <c>LibVLC</c> track encapsulated by the media stream.
    /// </summary>
    protected internal Track Track
    {
      get
      {
        return m_Track;
      }
    }

    #endregion // Track

    #region Name

    //==========================================================================                
    /// <summary>
    ///   Gets the name of the track.
    /// </summary>
    public string Name
    {
      get
      {
        return m_Track.Title;
      }
    }

    #endregion // Name

    #region Codec

    //==========================================================================                
    /// <summary>
    ///   Gets the codec of the track.
    /// </summary>
    public string Codec
    {
      get
      {
        return m_Track.Codec;
      }
    }

    #endregion // Codec

    #region Language

    //==========================================================================                
    /// <summary>
    ///   Gets the language of the track; may be <c>null</c> if the 
    ///   language could not be determined.
    /// </summary>
    /// <seealso cref="Culture"/>
    public string Language
    {
      get
      {
        return m_Track.Language;
      }
    }

    #endregion // Language

    #region Culture

    //==========================================================================                
    /// <summary>
    ///   Gets the culture of the track; may be <c>null</c> if the 
    ///   Culture could not be determined.
    /// </summary>
    /// <seealso cref="Language"/>
    public CultureInfo Culture
    {
      get
      {
        return m_Track.Culture;
      }
    }

    #endregion // Culture

    #endregion // Properties


  } // class MediaStream

}
