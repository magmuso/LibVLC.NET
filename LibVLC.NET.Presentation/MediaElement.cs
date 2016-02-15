////////////////////////////////////////////////////////////////////////////////
//
//  MediaElement.cs - This file is part of LibVLC.NET.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LibVLC.NET.Presentation
{

  //****************************************************************************
  /// <summary>
  ///   Represents a media like a video or audio element.
  /// </summary>
  public class MediaElement
    : FrameworkElement
  {

    //==========================================================================
    private static readonly ConcurrentDictionary<MediaPlayer, WeakReference<MediaElement>> m_MediaElements = new ConcurrentDictionary<MediaPlayer, WeakReference<MediaElement>>();

    //==========================================================================
    private static MediaElement GetMediaElement(MediaPlayer mediaPlayer)
    {
      if(mediaPlayer == null)
        return null;

      WeakReference<MediaElement> reference;
      if(!m_MediaElements.TryGetValue(mediaPlayer, out reference))
        return null;

      MediaElement media_element = null;
      if(reference != null)
        if(!reference.TryGetTarget(out media_element))
          media_element = null;

      if(media_element == null)
      {
        m_MediaElements.TryRemove(mediaPlayer, out reference);
        mediaPlayer.Dispose();
      }

      return media_element;
    }

    //==========================================================================
    private static void MediaPlayer_Event(object sender, MediaPlayerEventArgs e)
    {
      MediaPlayer media_player = sender as MediaPlayer;
      MediaElement media_element = GetMediaElement(media_player);
      if(media_element == null)
        return;

      Action action = null;

      switch(e.Event)
      {
        case MediaPlayerEvent.TimeChanged:
          action = media_element.MediaPlayer_TimeChanged;
          break;

        case MediaPlayerEvent.MediaChanged:
          action = media_element.MediaPlayer_MediaChanged;
          break;

        case MediaPlayerEvent.Opening:
          action = media_element.MediaPlayer_Opening;
          break;

        case MediaPlayerEvent.Playing:
          action = media_element.MediaPlayer_Playing;
          break;

        case MediaPlayerEvent.Paused:
          action = media_element.MediaPlayer_Paused;
          break;

        case MediaPlayerEvent.Stopped:
          action = media_element.MediaPlayer_Stopped;
          break;

        case MediaPlayerEvent.EndReached:
          action = media_element.MediaPlayer_EndReached;
          break;

        case MediaPlayerEvent.EncounteredError:
          action = media_element.MediaPlayer_EncounteredError;
          break;
      }

      if(action != null)
        media_element.Dispatcher.BeginInvoke((Action)delegate
        {
          if(media_element.MediaPlayer == media_player)
            action();
        });
    }

    //==========================================================================
    private static void MediaPlayer_VideoFormat(object sender, EventArgs e)
    {
      MediaPlayer media_player = sender as MediaPlayer;
      MediaElement media_element = GetMediaElement(media_player);
      if(media_element != null)
        media_element.Dispatcher.BeginInvoke((Action)delegate
        {
          if(media_element.MediaPlayer == media_player)
            media_element.MediaPlayer_VideoFormat();
        });
    }

    //==========================================================================
    private static void MediaPlayer_Display(object sender, EventArgs e)
    {
      MediaPlayer media_player = sender as MediaPlayer;
      MediaElement media_element = GetMediaElement(media_player);
      if(media_element != null)
        media_element.Dispatcher.BeginInvoke((Action)delegate
        {
          if(media_element.MediaPlayer == media_player)
            media_element.MediaPlayer_VideoDisplay();
        });
    }

    //==========================================================================
    private static void MediaPlayer_VideoCleanup(object sender, EventArgs e)
    {
      MediaPlayer media_player = sender as MediaPlayer;
      MediaElement media_element = GetMediaElement(media_player);
      if(media_element != null)
        media_element.Dispatcher.BeginInvoke((Action)delegate
        {
          if(media_element.MediaPlayer == media_player)
            media_element.MediaPlayer_VideoCleanup();
        });
    }

    #region MediaPlayer Callback Handling

    //==========================================================================
    private void MediaPlayer_MediaChanged()
    {
      ActualSource = MediaPlayer.Location;
    }

    //==========================================================================
    private void MediaPlayer_TimeChanged()
    {
      if(!IsOpen)
      {

        if(Length == null)
          Length = MediaPlayer.Length;
        if(VideoStreams == null)
        {
          VideoStreams = MediaPlayer.VideoTracks.Select((t) => new VideoStream(t)).ToArray();
          int track_index = MediaPlayer.VideoTrackIndex;
          CurrentVideoStream = ((track_index < 0) || (track_index >= VideoStreams.Length)) ? null : VideoStreams[track_index];
        }
        if(AudioStreams == null)
        {
          AudioStreams = MediaPlayer.AudioTracks.Select((t) => new AudioStream(t)).ToArray();
          int track_index = MediaPlayer.AudioTrackIndex;
          CurrentAudioStream = ((track_index < 0) || (track_index >= AudioStreams.Length)) ? null : AudioStreams[track_index];
        }
        if(SubtitleStreams == null)
        {
          SubtitleStreams = MediaPlayer.SubtitleTracks.Select((t) => new SubtitleStream(t)).ToArray();
          int track_index = MediaPlayer.SubtitleTrackIndex;
          CurrentSubtitleStream = ((track_index < 0) || (track_index >= SubtitleStreams.Length)) ? null : SubtitleStreams[track_index];
        }
        if(ChapterCount == null)
          ChapterCount = MediaPlayer.ChapterCount;

        IsOpen = true;
        RaiseOpened();
      }

      Position = null;
      CurrentChapter = null;
      RaisePositionChanged();
    }

    //==========================================================================
    private void MediaPlayer_Opening()
    {
      Position = null;

      State = MediaElementState.Opening;
      RaiseOpening();
    }

    //==========================================================================
    private void MediaPlayer_EncounteredError()
    {
      if(MediaPlayer.SubitemCount > 0)
      {
        // Do nothing! 
        // Wait for EndReached which will load and play the first subitem...
      }
      else
      {
        MediaPlayer.Stop();

        State = MediaElementState.EncounteredError;
        RaiseEncounteredError();
      }
    }

    //==========================================================================
    private void MediaPlayer_EndReached()
    {
      if(MediaPlayer.SubitemCount > 0)
      {
        MediaPlayer.LoadSubitem(0);
        MediaPlayer.Play();
      }
      else
      {
        MediaPlayer.Stop();

        State = MediaElementState.EndReached;
        RaiseEndReached();
      }
    }

    //==========================================================================
    private void MediaPlayer_Playing()
    {
      State = MediaElementState.Playing;
      RaisePlaying();
    }

    //==========================================================================
    private void MediaPlayer_Stopped()
    {                            
      if((State != MediaElementState.EncounteredError) && (State != MediaElementState.EndReached))
        State = MediaElementState.Stopped;
      IsOpen = false;
      Length = null;
      Position = null;
      VideoStreams = null;
      AudioStreams = null;
      SubtitleStreams = null;
      ChapterCount = null;
      CurrentChapter = null;
      RaiseStopped();
    }


    //==========================================================================
    private void MediaPlayer_Paused()
    {
      State = MediaElementState.Paused;
      RaisePaused();
    }

    //==========================================================================
    private void MediaPlayer_VideoFormat()
    {
      VideoBuffer video_buffer = MediaPlayer.VideoBuffer;
      if(video_buffer != null)
      {
        VideoBufferBitmap = new WriteableBitmap((int)video_buffer.Width, (int)video_buffer.Height, 96, 96, PixelFormats.Bgra32, null);
        FPS = MediaPlayer.FPS;
      }
      else
        VideoBufferBitmap = null;
    }

    //==========================================================================
    private void MediaPlayer_VideoDisplay()
    {
      if(VideoBufferBitmap == null)
        return;

      VideoBuffer video_buffer = MediaPlayer.VideoBuffer;
      if(video_buffer == null)
        return;

      if((VideoBufferBitmap.PixelWidth != video_buffer.Width) || (VideoBufferBitmap.PixelHeight != video_buffer.Height))
        return;

      VideoBufferBitmap.WritePixels(new Int32Rect(0, 0, (int)video_buffer.Width, (int)video_buffer.Height), video_buffer.FrameBuffer, (int)video_buffer.Stride, 0);

      if(State == MediaElementState.Paused)
      {
        Position = null;
        CurrentChapter = null;
      }

      ++m_FrameCounter;
      DateTime now = DateTime.Now;
      double seconds = (now - m_SecondStart).TotalSeconds;
      if(seconds >= 1.0)
      {
        ActualFPS = m_FrameCounter / seconds;
        m_FrameCounter = 0;
        m_SecondStart = now;
      }
    }

    //==========================================================================
    private void MediaPlayer_VideoCleanup()
    {
      VideoBufferBitmap = null;
      ActualFPS = null;
      FPS = null;
      m_FrameCounter = 0;
      m_SecondStart = DateTime.Now;
    }

    #endregion // MediaPlayer Callback Handling

    //==========================================================================
    /// <summary>
    ///   Initializes a new <see cref="MediaElement"/> instance.
    /// </summary>
    public MediaElement()
    {
      // ...
    }

    #region Layout and rendering

    //==========================================================================
    /// <summary>
    ///   Overrides <see cref="FrameworkElement.MeasureOverride"/> and
    ///   calculates the size to display a loaded video.
    /// </summary>
    /// <param name="availableSize">
    ///   The available size.
    /// </param>
    /// <returns>
    ///   The needed size.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
      if(VideoBufferBitmap == null)
        return new Size(0, 0);

      if(Stretch == System.Windows.Media.Stretch.None)
        return new Size(VideoBufferBitmap.Width, VideoBufferBitmap.Height);

      if(Double.IsPositiveInfinity(availableSize.Width) && Double.IsPositiveInfinity(availableSize.Height))
        return new Size(VideoBufferBitmap.Width, VideoBufferBitmap.Height);

      double video_aspect = VideoBufferBitmap.Width / VideoBufferBitmap.Height;

      if(Double.IsPositiveInfinity(availableSize.Width))
        return new Size(video_aspect * availableSize.Height, availableSize.Height);

      if(Double.IsPositiveInfinity(availableSize.Height))
        return new Size(availableSize.Width, availableSize.Width / video_aspect);

      if((Stretch == System.Windows.Media.Stretch.Fill) || (Stretch == System.Windows.Media.Stretch.UniformToFill))
        return availableSize;

      double available_aspect = availableSize.Width / availableSize.Height;

      if(video_aspect < available_aspect)
        return new Size(video_aspect * availableSize.Height, availableSize.Height);

      if(video_aspect > available_aspect)
        return new Size(availableSize.Width, availableSize.Width / video_aspect);

      return availableSize;
    }

    //==========================================================================
    /// <summary>
    ///   Overrides <see cref="UIElement.OnRender"/> and renders the current
    ///   video.
    /// </summary>
    /// <param name="drawingContext">
    ///   The drawing context to use for rendering.
    /// </param>
    protected override void OnRender(DrawingContext drawingContext)
    {
      if(VideoBufferBitmap != null)
      {
        Size size = RenderSize;
        double aspect = size.Width / size.Height;
        double video_aspect = VideoBufferBitmap.Width / VideoBufferBitmap.Height;

        switch(Stretch)
        {
          case Stretch.None:
            size = new Size(VideoBufferBitmap.Width, VideoBufferBitmap.Height);
            break;

          case Stretch.Fill:
            size = RenderSize;
            break;

          case Stretch.Uniform:
            if(video_aspect > aspect)
              size = new Size(size.Width, size.Width / video_aspect);
            else if(video_aspect < aspect)
              size = new Size(size.Height * video_aspect, size.Height);
            break;

          case Stretch.UniformToFill:
            if(video_aspect < aspect)
              size = new Size(size.Width, size.Width / video_aspect);
            else if(video_aspect > aspect)
              size = new Size(size.Height * video_aspect, size.Height);
            break;
        }

        drawingContext.PushClip(new RectangleGeometry(new Rect(RenderSize)));
        drawingContext.DrawImage(VideoBufferBitmap, new Rect(
          (RenderSize.Width - size.Width) / 2,
          (RenderSize.Height - size.Height) / 2, size.Width, size.Height));
        drawingContext.Pop();
      }

      base.OnRender(drawingContext);
    }

    #endregion // Layout and rendering

    #region Playback Control

    //==========================================================================
    /// <summary>
    ///   Starts or resumes playback.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   Will be thrown if no media has been loaded.
    /// </exception>
    public void Play()
    {
      if(MediaPlayer != null)
        MediaPlayer.Play();
    }

    //==========================================================================
    /// <summary>
    ///   Pauses or resumes playback.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   Will be thrown if no media has been loaded.
    /// </exception>
    public void Pause()
    {
      if(MediaPlayer != null)
        MediaPlayer.Pause();
    }

    //==========================================================================
    /// <summary>
    ///   Stops playback.
    /// </summary>
    public void Stop()
    {
      if(MediaPlayer != null)
        MediaPlayer.Stop();
    }

    //==========================================================================
    public void NextFrame()
    {
      if(MediaPlayer != null)
      {
        MediaPlayer.NextFrame();
        //Position = null;
      }
    }

    //==========================================================================
    public void PreviousChapter()
    {
      if(MediaPlayer != null)
        MediaPlayer.PreviousChapter();
    }

    //==========================================================================
    public void NextChapter()
    {
      if(MediaPlayer != null)
        MediaPlayer.NextChapter();
    }

    #endregion // Playback Control

    #region Properties

    #region VideoBufferBitmap

    //==========================================================================                
    private WriteableBitmap VideoBufferBitmap
    {
      get
      {
        return (WriteableBitmap)GetValue(VideoBufferBitmapProperty);
      }

      set
      {
        SetValue(VideoBufferBitmapPropertyKey, value);
      }
    }

    //==========================================================================
    private static readonly DependencyPropertyKey VideoBufferBitmapPropertyKey =
        DependencyProperty.RegisterReadOnly("VideoBufferBitmap", typeof(WriteableBitmap), typeof(MediaElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    //==========================================================================
    private static readonly DependencyProperty VideoBufferBitmapProperty = VideoBufferBitmapPropertyKey.DependencyProperty;


    #endregion // VideoBufferBitmap

    #region IsOpen

    //==========================================================================                
    /// <summary>
    ///   Gets the value of IsOpen of the MediaElement.
    /// </summary>
    public bool IsOpen
    {
      get
      {
        return (bool)GetValue(IsOpenProperty);
      }

      private set
      {
        SetValue(IsOpenPropertyKey, value);
      }
    }

    //==========================================================================
    private static readonly DependencyPropertyKey IsOpenPropertyKey =
        DependencyProperty.RegisterReadOnly("IsOpen", typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="IsOpen"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsOpenProperty = IsOpenPropertyKey.DependencyProperty;

    #endregion // IsOpen

    #region MediaPlayer

    //==========================================================================                
    private MediaPlayer MediaPlayer
    {
      get
      {
        return (MediaPlayer)GetValue(MediaPlayerProperty);
      }

      set
      {
        SetValue(MediaPlayerPropertyKey, value);
      }
    }

    //==========================================================================
    private void OnMediaPlayerChanged(MediaPlayer oldValue, MediaPlayer newValue)
    {
      if(oldValue != null)
      {
        WeakReference<MediaElement> reference;
        m_MediaElements.TryRemove(oldValue, out reference);

        oldValue.VideoCleanup -= MediaPlayer_VideoCleanup;
        oldValue.Display -= MediaPlayer_Display;
        oldValue.VideoFormat -= MediaPlayer_VideoFormat;
        oldValue.Event -= MediaPlayer_Event;
        oldValue.Dispose();
      }

      if(newValue != null)
      {
        m_MediaElements.TryAdd(newValue, new WeakReference<MediaElement>(this));

        newValue.Event += MediaPlayer_Event;
        newValue.VideoFormat += MediaPlayer_VideoFormat;
        newValue.Display += MediaPlayer_Display;
        newValue.VideoCleanup += MediaPlayer_VideoCleanup;

        newValue.Volume = (int)Math.Round(Volume * 100);
        newValue.Location = Source;

        State = MediaElementState.Stopped;
      }
      else
        State = MediaElementState.Empty;

      IsOpen = false;
      VideoBufferBitmap = null;
      ActualFPS = null;
      FPS = null;
      Length = null;
      Position = null;
      VideoStreams = null;
      CurrentVideoStream = null;
      AudioStreams = null;
      CurrentAudioStream = null;
      SubtitleStreams = null;
      CurrentSubtitleStream = null;
      ChapterCount = null;
      CurrentChapter = null;
    }

    //==========================================================================
    private static void OnMediaPlayerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnMediaPlayerChanged((MediaPlayer)e.OldValue, (MediaPlayer)e.NewValue);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey MediaPlayerPropertyKey =
        DependencyProperty.RegisterReadOnly("MediaPlayer", typeof(MediaPlayer), typeof(MediaElement), new FrameworkPropertyMetadata(default(MediaPlayer), OnMediaPlayerChanged));

    //==========================================================================
    private static readonly DependencyProperty MediaPlayerProperty = MediaPlayerPropertyKey.DependencyProperty;

    #endregion // MediaPlayer

    #region ActualFPS

    //==========================================================================                
    private DateTime m_SecondStart = DateTime.Now;
    private int m_FrameCounter = 0;

    //==========================================================================                
    /// <summary>
    ///   Gets the actual number of video frames rendered during the most
    ///   recent second.
    /// </summary>
    public double? ActualFPS
    {
      get
      {
        return (double?)GetValue(ActualFPSProperty);
      }

      private set
      {
        SetValue(ActualFPSPropertyKey, value);
      }
    }

    //==========================================================================
    private static readonly DependencyPropertyKey ActualFPSPropertyKey =
        DependencyProperty.RegisterReadOnly("ActualFPS", typeof(double?), typeof(MediaElement), new FrameworkPropertyMetadata(null));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="ActualFPS"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActualFPSProperty = ActualFPSPropertyKey.DependencyProperty;

    #endregion // ActualFPS

    #region FPS

    //==========================================================================                
    /// <summary>
    ///   Gets the number of video frames to display in a second.
    /// </summary>
    public double? FPS
    {
      get
      {
        return (double?)GetValue(FPSProperty);
      }

      private set
      {
        SetValue(FPSPropertyKey, value);
      }
    }

    //==========================================================================
    private static readonly DependencyPropertyKey FPSPropertyKey =
        DependencyProperty.RegisterReadOnly("FPS", typeof(double?), typeof(MediaElement), new FrameworkPropertyMetadata(null));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="FPS"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty FPSProperty = FPSPropertyKey.DependencyProperty;

    #endregion // FPS

    #region Source

    //==========================================================================                
    /// <summary>
    ///   Gets or sets the source of the media loaded in the media element.
    /// </summary>
    public Uri Source
    {
      get
      {
        return (Uri)GetValue(SourceProperty);
      }

      set
      {
        SetValue(SourceProperty, value);
      }
    }

    //==========================================================================
    private void OnSourceChanged(Uri oldSource, Uri newSource)
    {
      MediaPlayer = newSource == null ? null : new MediaPlayer(m_Library, null);
      ActualSource = null;
    }

    //==========================================================================
    private Uri CoerceSource(Uri value)
    {
      if((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
        return null;
      
      return value;
    }

    //==========================================================================
    private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnSourceChanged((Uri)e.OldValue, (Uri)e.NewValue);
    }

    //==========================================================================
    private static object CoerceSource(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceSource(value as Uri);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Source"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(MediaElement), new FrameworkPropertyMetadata(default(Uri), OnSourceChanged, CoerceSource));

    #endregion // Source

    #region ActualSource

    //==========================================================================                
    /// <summary>
    ///   Gets the actual source of the current media.
    /// </summary>
    public Uri ActualSource
    {
      get
      {
        return (Uri)GetValue(ActualSourceProperty);
      }

      private set
      {
        SetValue(ActualSourcePropertyKey, value);
      }
    }

    //==========================================================================
    private static readonly DependencyPropertyKey ActualSourcePropertyKey =
        DependencyProperty.RegisterReadOnly("ActualSource", typeof(Uri), typeof(MediaElement), new FrameworkPropertyMetadata(null));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="ActualSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActualSourceProperty = ActualSourcePropertyKey.DependencyProperty;

    #endregion // ActualSource

    #region Length

    //==========================================================================                
    /// <summary>
    ///   Gets the length of the playing media.
    /// </summary>
    public TimeSpan? Length
    {
      get
      {
        return (TimeSpan?)GetValue(LengthProperty);
      }

      private set
      {
        SetValue(LengthPropertyKey, value);
      }
    }

    //==========================================================================
    private void OnLengthChanged(TimeSpan? oldValue, TimeSpan? newValue)
    {
      CoerceValue(PositionProperty);
    }

    //==========================================================================
    private static void OnLengthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnLengthChanged((TimeSpan?)e.OldValue, (TimeSpan?)e.NewValue);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey LengthPropertyKey =
        DependencyProperty.RegisterReadOnly("Length", typeof(TimeSpan?), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnLengthChanged));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="Length"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty LengthProperty = LengthPropertyKey.DependencyProperty;

    #endregion // Length

    #region Position

    //==========================================================================                
    /// <summary>
    ///   Gets the current position of the playing media.
    /// </summary>
    public TimeSpan? Position
    {
      get
      {
        return (TimeSpan?)GetValue(PositionProperty);
      }

      set
      {
        SetValue(PositionProperty, value);
      }
    }

    //==========================================================================
    private void OnPositionChanged(TimeSpan? oldValue, TimeSpan? newValue)
    {
      if(MediaPlayer != null)
        if(newValue != null)
          if(newValue.Value != MediaPlayer.Time)
            MediaPlayer.Time = newValue.Value;
    }

    //==========================================================================
    private TimeSpan? CoercePosition(TimeSpan? value)
    {
      if(MediaPlayer == null)
        return null;

      if(Length == null)
        return null;

      if(value == null)
        return MediaPlayer.Time;

      if(value < TimeSpan.Zero)
        value = TimeSpan.Zero;
      if(value > Length)
        value = Length;

      return value;
    }

    //==========================================================================
    private static void OnPositionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnPositionChanged((TimeSpan?)e.OldValue, (TimeSpan?)e.NewValue);
    }

    //==========================================================================
    private static object CoercePosition(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoercePosition((TimeSpan?)value);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Position"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register("Position", typeof(TimeSpan?), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnPositionChanged, CoercePosition));

    #endregion // Position

    #region Volume

    //==========================================================================                
    /// <summary>
    ///   Gets or sets the volume.
    /// </summary>
    public double Volume
    {
      get
      {
        return (double)GetValue(VolumeProperty);
      }

      set
      {
        SetValue(VolumeProperty, value);
      }
    }

    //==========================================================================
    private void OnVolumeChanged(double oldValue, double newValue)
    {
      if(MediaPlayer != null)
        MediaPlayer.Volume = (int)Math.Round(Volume * 100);
    }

    //==========================================================================
    private double CoerceVolume(double value)
    {
      value = Math.Round(value, 2);

      if(value < 0.0)
        value = 0.0;
      if(value > 2.0)
        value = 2.0;

      return value;
    }


    //==========================================================================
    private static void OnVolumeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnVolumeChanged((double)e.OldValue, (double)e.NewValue);
    }

    //==========================================================================
    private static object CoerceVolume(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceVolume((double)value);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Volume"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VolumeProperty =
        DependencyProperty.Register("Volume", typeof(double), typeof(MediaElement), new FrameworkPropertyMetadata(1.0, OnVolumeChanged, CoerceVolume));

    #endregion // Volume

    #region Stretch

    //==========================================================================                
    /// <summary>
    ///   Gets or sets how fit a video into the available space.
    /// </summary>
    public Stretch Stretch
    {
      get
      {
        return (Stretch)GetValue(StretchProperty);
      }

      set
      {
        SetValue(StretchProperty, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Stretch"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(MediaElement), new FrameworkPropertyMetadata(Stretch.UniformToFill, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion // Stretch

    #region State

    //==========================================================================                
    /// <summary>
    ///   Gets the current state of the media element.
    /// </summary>
    public MediaElementState State
    {
      get
      {
        return (MediaElementState)GetValue(StateProperty);
      }

      private set
      {
        SetValue(StatePropertyKey, value);
      }
    }

    //==========================================================================
    private void OnStateChanged(MediaElementState oldValue, MediaElementState newValue)
    {
      // ...
    }

    //==========================================================================
    private static void OnStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnStateChanged((MediaElementState)e.OldValue, (MediaElementState)e.NewValue);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey StatePropertyKey =
        DependencyProperty.RegisterReadOnly("State", typeof(MediaElementState), typeof(MediaElement), new FrameworkPropertyMetadata(MediaElementState.Empty, OnStateChanged));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="State"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

    #endregion // State

    #region VideoStreams

    //==========================================================================                
    /// <summary>
    ///   Gets a collection of all video streams of the playing media.
    /// </summary>
    public VideoStream[] VideoStreams
    {
      get
      {
        return (VideoStream[])GetValue(VideoStreamsProperty);
      }

      private set
      {
        SetValue(VideoStreamsPropertyKey, value);
      }
    }

    //==========================================================================
    private void OnVideoStreamsChanged(VideoStream[] oldValue, VideoStream[] newValue)
    {
      CoerceValue(CurrentVideoStreamProperty);
    }

    //==========================================================================
    private static void OnVideoStreamsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnVideoStreamsChanged((VideoStream[])e.OldValue, (VideoStream[])e.NewValue);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey VideoStreamsPropertyKey =
        DependencyProperty.RegisterReadOnly("VideoStreams", typeof(VideoStream[]), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnVideoStreamsChanged));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="VideoStreams"/> dependency 
    ///   property.
    /// </summary>
    public static readonly DependencyProperty VideoStreamsProperty = VideoStreamsPropertyKey.DependencyProperty;

    #endregion // VideoStreams

    #region CurrentVideoStream

    //==========================================================================                
    /// <summary>
    ///   Gets the current video stream.
    /// </summary>
    public VideoStream CurrentVideoStream
    {
      get
      {
        return (VideoStream)GetValue(CurrentVideoStreamProperty);
      }

      set
      {
        SetValue(CurrentVideoStreamProperty, value);
      }
    }

    //==========================================================================
    private void OnCurrentVideoStreamChanged(VideoStream oldValue, VideoStream newValue)
    {
      if(MediaPlayer != null)
      {
        int index = -1;

        if(newValue != null)
          index = newValue.Track.Index;

        MediaPlayer.VideoTrackIndex = index;
      }
    }

    //==========================================================================
    private VideoStream CoerceCurrentVideoStream(VideoStream value)
    {
      if(VideoStreams == null)
        return null;

      if(!VideoStreams.Contains(value))
        return null;

      return value;
    }


    //==========================================================================
    private static void OnCurrentVideoStreamChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnCurrentVideoStreamChanged((VideoStream)e.OldValue, (VideoStream)e.NewValue);
    }

    //==========================================================================
    private static object CoerceCurrentVideoStream(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceCurrentVideoStream((VideoStream)value);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="CurrentVideoStream"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentVideoStreamProperty =
        DependencyProperty.Register("CurrentVideoStream", typeof(VideoStream), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnCurrentVideoStreamChanged, CoerceCurrentVideoStream));

    #endregion // CurrentVideoStream

    #region AudioStreams

    //==========================================================================                
    /// <summary>
    ///   Gets a collection of all audio streams of the playing media.
    /// </summary>
    public AudioStream[] AudioStreams
    {
      get
      {
        return (AudioStream[])GetValue(AudioStreamsProperty);
      }

      private set
      {
        SetValue(AudioStreamsPropertyKey, value);
      }
    }

    //==========================================================================
    private void OnAudioStreamsChanged(AudioStream[] oldValue, AudioStream[] newValue)
    {
    }

    //==========================================================================
    private static void OnAudioStreamsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnAudioStreamsChanged((AudioStream[])e.OldValue, (AudioStream[])e.NewValue);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey AudioStreamsPropertyKey =
        DependencyProperty.RegisterReadOnly("AudioStreams", typeof(AudioStream[]), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnAudioStreamsChanged));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="AudioStreams"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AudioStreamsProperty = AudioStreamsPropertyKey.DependencyProperty;

    #endregion // AudioStreams

    #region CurrentAudioStream

    //==========================================================================                
    /// <summary>
    ///   Gets or sets the value of <see cref="CurrentAudioStream"/> of the 
    ///   <see cref="MediaElement"/>.
    /// </summary>
    public AudioStream CurrentAudioStream
    {
      get
      {
        return (AudioStream)GetValue(CurrentAudioStreamProperty);
      }

      set
      {
        SetValue(CurrentAudioStreamProperty, value);
      }
    }

    //==========================================================================
    private void OnCurrentAudioStreamChanged(AudioStream oldValue, AudioStream newValue)
    {
      if(MediaPlayer != null)
      {
        int index = -1;

        if(newValue != null)
          index = newValue.Track.Index;

        MediaPlayer.AudioTrackIndex = index;
      }
    }

    //==========================================================================
    private AudioStream CoerceCurrentAudioStream(AudioStream value)
    {
      if(AudioStreams == null)
        return null;

      if(!AudioStreams.Contains(value))
        return null;

      return value;
    }


    //==========================================================================
    private static void OnCurrentAudioStreamChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnCurrentAudioStreamChanged((AudioStream)e.OldValue, (AudioStream)e.NewValue);
    }

    //==========================================================================
    private static object CoerceCurrentAudioStream(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceCurrentAudioStream((AudioStream)value);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="CurrentAudioStream"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentAudioStreamProperty =
        DependencyProperty.Register("CurrentAudioStream", typeof(AudioStream), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnCurrentAudioStreamChanged, CoerceCurrentAudioStream));

    #endregion // CurrentAudioStream

    #region SubtitleStreams

    //==========================================================================                
    public SubtitleStream[] SubtitleStreams
    {
      get
      {
        return (SubtitleStream[])GetValue(SubtitleStreamsProperty);
      }

      private set
      {
        SetValue(SubtitleStreamsPropertyKey, value);
      }
    }

    //==========================================================================
    private void OnSubtitleStreamsChanged(SubtitleStream[] oldValue, SubtitleStream[] newValue)
    {
    }

    //==========================================================================
    private static void OnSubtitleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnSubtitleStreamsChanged((SubtitleStream[])e.OldValue, (SubtitleStream[])e.NewValue);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey SubtitleStreamsPropertyKey =
        DependencyProperty.RegisterReadOnly("SubtitleStreams", typeof(SubtitleStream[]), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnSubtitleChanged));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="SubtitleStreams"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SubtitleStreamsProperty = SubtitleStreamsPropertyKey.DependencyProperty;

    #endregion // SubtitleStreams

    #region CurrentSubtitleStream

    //==========================================================================                
    public SubtitleStream CurrentSubtitleStream
    {
      get
      {
        return (SubtitleStream)GetValue(CurrentSubtitleStreamProperty);
      }

      set
      {
        SetValue(CurrentSubtitleStreamProperty, value);
      }
    }

    //==========================================================================
    private void OnCurrentSubtitleStreamChanged(SubtitleStream oldValue, SubtitleStream newValue)
    {
      if(MediaPlayer != null)
      {
        int index = -1;

        if(newValue != null)
          index = newValue.Track.Index;

        MediaPlayer.SubtitleTrackIndex = index;
      }
    }

    //==========================================================================
    private SubtitleStream CoerceCurrentSubtitleStream(SubtitleStream value)
    {
      if(SubtitleStreams == null)
        return null;

      if(!SubtitleStreams.Contains(value))
        return null;

      return value;
    }


    //==========================================================================
    private static void OnCurrentSubtitleStreamChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnCurrentSubtitleStreamChanged((SubtitleStream)e.OldValue, (SubtitleStream)e.NewValue);
    }

    //==========================================================================
    private static object CoerceCurrentSubtitleStream(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceCurrentSubtitleStream((SubtitleStream)value);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="CurrentSubtitleStream"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentSubtitleStreamProperty =
        DependencyProperty.Register("CurrentSubtitleStream", typeof(SubtitleStream), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnCurrentSubtitleStreamChanged, CoerceCurrentSubtitleStream));

    #endregion // CurrentSubtitleStream

    #region ChapterCount

    //==========================================================================                
    /// <summary>
    ///   Gets or sets the value of <see cref="ChapterCount"/> of the 
    ///   <see cref="MediaElement"/>.
    /// </summary>
    public int? ChapterCount
    {
      get
      {
        return (int?)GetValue(ChapterCountProperty);
      }

      private set
      {
        SetValue(ChapterCountPropertyKey, value);
      }
    }

    //==========================================================================
    private void OnChapterCountChanged(int? oldValue, int? newValue)
    {
      CoerceValue(CurrentChapterProperty);
    }

    //==========================================================================
    private int? CoerceChapterCount(int? value)
    {
      return value;
    }


    //==========================================================================
    private static void OnChapterCountChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnChapterCountChanged((int?)e.OldValue, (int?)e.NewValue);
    }

    //==========================================================================
    private static object CoerceChapterCount(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceChapterCount((int?)value);
    }

    //==========================================================================
    private static readonly DependencyPropertyKey ChapterCountPropertyKey =
        DependencyProperty.RegisterReadOnly("ChapterCount", typeof(int?), typeof(MediaElement), new PropertyMetadata(default(int?), OnChapterCountChanged, CoerceChapterCount));

    //==========================================================================
    /// <summary>
    ///   Identifies the readonly <see cref="ChapterCount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ChapterCountProperty = ChapterCountPropertyKey.DependencyProperty;

    #endregion // ChapterCount

    #region CurrentChapter

    //==========================================================================                
    /// <summary>
    ///   Gets the current CurrentChapter of the playing media.
    /// </summary>
    public int? CurrentChapter
    {
      get
      {
        return (int?)GetValue(CurrentChapterProperty);
      }

      set
      {
        SetValue(CurrentChapterProperty, value);
      }
    }

    //==========================================================================
    private void OnCurrentChapterChanged(int? oldValue, int? newValue)
    {
      if(MediaPlayer != null)
        if(newValue != null)
          if(newValue.Value != MediaPlayer.Chapter)
            MediaPlayer.Chapter = newValue.Value;
    }

    //==========================================================================
    private int? CoerceCurrentChapter(int? value)
    {
      if(MediaPlayer == null)
        return null;

      if(ChapterCount == null)
        return null;

      if(value == null)
        return MediaPlayer.Chapter;

      if(value < 0)
        value = 0;
      if(value >= ChapterCount.Value)
        value = ChapterCount.Value - 1;

      return value;
    }

    //==========================================================================
    private static void OnCurrentChapterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      (sender as MediaElement).OnCurrentChapterChanged((int?)e.OldValue, (int?)e.NewValue);
    }

    //==========================================================================
    private static object CoerceCurrentChapter(DependencyObject sender, object value)
    {
      return (sender as MediaElement).CoerceCurrentChapter((int?)value);
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="CurrentChapter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CurrentChapterProperty =
        DependencyProperty.Register("CurrentChapter", typeof(int?), typeof(MediaElement), new FrameworkPropertyMetadata(null, OnCurrentChapterChanged, CoerceCurrentChapter));

    #endregion // CurrentChapter

    #endregion // Properties

    #region Events

    #region Opening

    //==========================================================================
    private void RaiseOpening()
    {
      OnOpening(new RoutedEventArgs(OpeningEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when the media is opened and raises 
    ///   <see cref="Opening"/>.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnOpening(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be raised when the media is opened.
    /// </summary>
    public event RoutedEventHandler Opening
    {
      add
      {
        AddHandler(OpeningEvent, value);
      }

      remove
      {
        RemoveHandler(OpeningEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Opening"/> routed event.
    /// </summary>
    public static readonly RoutedEvent OpeningEvent = EventManager.RegisterRoutedEvent("Opening", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // Opening

    #region Opened

    //==========================================================================
    private void RaiseOpened()
    {
      OnOpened(new RoutedEventArgs(OpenedEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when the media has been opened.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnOpened(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be raised when the media has been opened.
    /// </summary>
    public event RoutedEventHandler Opened
    {
      add
      {
        AddHandler(OpenedEvent, value);
      }

      remove
      {
        RemoveHandler(OpenedEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Opened"/> routed event.
    /// </summary>
    public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // Opened

    #region Playing

    //==========================================================================
    private void RaisePlaying()
    {
      OnPlaying(new RoutedEventArgs(PlayingEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when playback has been started or resumed and raises
    ///   <see cref="Playing"/>.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnPlaying(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be raised when playback has been started or resumed.
    /// </summary>
    public event RoutedEventHandler Playing
    {
      add
      {
        AddHandler(PlayingEvent, value);
      }

      remove
      {
        RemoveHandler(PlayingEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Playing"/> routed event.
    /// </summary>
    public static readonly RoutedEvent PlayingEvent = EventManager.RegisterRoutedEvent("Playing", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // Playing

    #region Paused

    //==========================================================================
    private void RaisePaused()
    {
      OnPaused(new RoutedEventArgs(PausedEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when playback has paused and raises
    ///   <see cref="Paused"/>.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnPaused(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be raised when playback has been paused.
    /// </summary>
    public event RoutedEventHandler Paused
    {
      add
      {
        AddHandler(PausedEvent, value);
      }

      remove
      {
        RemoveHandler(PausedEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Paused"/> routed event.
    /// </summary>
    public static readonly RoutedEvent PausedEvent = EventManager.RegisterRoutedEvent("Paused", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // Paused

    #region Stopped

    //==========================================================================
    private void RaiseStopped()
    {
      OnStopped(new RoutedEventArgs(StoppedEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when playback has been stopped and raises
    ///   <see cref="Stopped"/>.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnStopped(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be raised when playback has been stopped.
    /// </summary>
    public event RoutedEventHandler Stopped
    {
      add
      {
        AddHandler(StoppedEvent, value);
      }

      remove
      {
        RemoveHandler(StoppedEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="Stopped"/> routed event.
    /// </summary>
    public static readonly RoutedEvent StoppedEvent = EventManager.RegisterRoutedEvent("Stopped", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // Stopped

    #region EndReached

    //==========================================================================
    private void RaiseEndReached()
    {
      OnEndReached(new RoutedEventArgs(EndReachedEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when the end of the media has been reached and raises
    ///   <see cref="EndReached"/>.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnEndReached(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be raised when the end of the media has been reached.
    /// </summary>
    public event RoutedEventHandler EndReached
    {
      add
      {
        AddHandler(EndReachedEvent, value);
      }

      remove
      {
        RemoveHandler(EndReachedEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="EndReached"/> routed event.
    /// </summary>
    public static readonly RoutedEvent EndReachedEvent = EventManager.RegisterRoutedEvent("EndReached", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // EndReached

    #region EncounteredError

    //==========================================================================
    private void RaiseEncounteredError()
    {
      OnEncounteredError(new RoutedEventArgs(EncounteredErrorEvent));
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when there has been an error encountered while 
    ///   opening or playing the media and raises 
    ///   <see cref="EncounteredError"/>.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="RoutedEventArgs"/> instance providing further 
    ///   information.
    /// </param>
    protected virtual void OnEncounteredError(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    /// <summary>
    ///   Will be invoked when there has been an error encountered while 
    ///   opening or playing the media.
    /// </summary>
    public event RoutedEventHandler EncounteredError
    {
      add
      {
        AddHandler(EncounteredErrorEvent, value);
      }

      remove
      {
        RemoveHandler(EncounteredErrorEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="EncounteredError"/> routed event.
    /// </summary>
    public static readonly RoutedEvent EncounteredErrorEvent = EventManager.RegisterRoutedEvent("EncounteredError", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // EncounteredError

    #region PositionChanged

    //==========================================================================
    private void RaisePositionChanged()
    {
      OnPositionChanged(new RoutedEventArgs(PositionChangedEvent));
    }

    //==========================================================================
    protected virtual void OnPositionChanged(RoutedEventArgs e)
    {
      RaiseEvent(e);
    }

    //==========================================================================
    public event RoutedEventHandler PositionChanged
    {
      add
      {
        AddHandler(PositionChangedEvent, value);
      }

      remove
      {
        RemoveHandler(PositionChangedEvent, value);
      }
    }

    //==========================================================================
    /// <summary>
    ///   Identifies the <see cref="PositionChanged"/> routed event.
    /// </summary>
    public static readonly RoutedEvent PositionChangedEvent = EventManager.RegisterRoutedEvent("PositionChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MediaElement));

    #endregion // PositionChanged
    
    #endregion // Events

    //==========================================================================
    private static readonly LibVLCLibrary m_Library;

    //==========================================================================
    static MediaElement()
    {
      if(!(bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
        m_Library = LibVLCLibrary.Load(null);
    }

  } // class MediaElement

}
