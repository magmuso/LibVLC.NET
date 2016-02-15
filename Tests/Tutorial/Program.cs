////////////////////////////////////////////////////////////////////////////////
//
//  Program.cs - This file is part of LibVLC.NET.
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
//  $LastChangedRevision$
//  $LastChangedDate$
//  $LastChangedBy$
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibVLC.NET;
using System.Threading;

namespace Tutorial
{
  
  //****************************************************************************
  class Program
  {

    //==========================================================================
    static void Main(string[] args)
    {
      // The original tutorial code can be found at
      // http://wiki.videolan.org/LibVLC_Tutorial#Sample_LibVLC_Code

      LibVLCLibrary library = LibVLCLibrary.Load(null);
      try
      {
        IntPtr inst, mp, m;

        /* Load the VLC engine */
        inst = library.libvlc_new();

        /* Create a new item */
        m = library.libvlc_media_new_location(inst, null);

        /* Create a media player playing environement */
        mp = library.libvlc_media_player_new_from_media(m);

        /* No need to keep the media now */
        library.libvlc_media_release(m);


        /* play the media_player */
        library.libvlc_media_player_play(mp);

        Thread.Sleep(10000); /* Let it play a bit */

        /* Stop playing */
        library.libvlc_media_player_stop(mp);

        /* Free the media_player */
        library.libvlc_media_player_release(mp);

        library.libvlc_release(inst);

       
      }
      finally 
      {
        LibVLCLibrary.Free(library);
      }

    }

  } // class Program
}
