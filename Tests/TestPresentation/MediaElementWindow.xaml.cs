////////////////////////////////////////////////////////////////////////////////
//
//  MediaElementWindow.xaml.cs - This file is part of LibVLC.NET.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace TestPresentation
{

  //****************************************************************************
  public partial class MediaElementWindow
    : Window
  {
    //==========================================================================
    public MediaElementWindow()
    {
      InitializeComponent();
    }

    //==========================================================================
    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
      //MediaElement.Source = null;
    }

    //==========================================================================
    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.Play();
    }

    //==========================================================================
    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.Pause();
    }

    //==========================================================================
    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.Stop();
    }

    //==========================================================================
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog open_file_dialog = new OpenFileDialog();
      if(open_file_dialog.ShowDialog() == true)
        MediaElement.Source = new Uri(open_file_dialog.FileName);
    }

    //==========================================================================
    private void ClearCurrentVideoStreamButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.CurrentVideoStream = null;
    }

    //==========================================================================
    private void ClearCurrentAudioStreamButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.CurrentAudioStream = null;
    }

    //==========================================================================
    private void ClearCurrentSubtitleStreamButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.CurrentSubtitleStream = null;
    }

    //==========================================================================
    private void ClearSourceButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.Source = null;
    }

    //==========================================================================
    private void NextFrameButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.NextFrame();
    }

    //==========================================================================
    private void PreviousChapterButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.PreviousChapter();
    }

    //==========================================================================
    private void NextChapterButton_Click(object sender, RoutedEventArgs e)
    {
      MediaElement.NextChapter();
    }

  } // class MediaElementWindow

}
