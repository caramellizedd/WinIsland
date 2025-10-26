using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Windows.Media.Control;

namespace WinIsland
{
    /// <summary>
    /// Interaction logic for MusPlayer.xaml
    /// </summary>
    public partial class MusPlayer : Page
    {
        private bool mediaSessionEmpty = true; // Check if mediaSession is empty or equal to NULL.

        private DispatcherTimer waitForMD = new DispatcherTimer();
        private DispatcherTimer Tick = new DispatcherTimer();

        private MainWindow mw = MainWindow.instance;
        public MusPlayer()
        {
            InitializeComponent();
            getMediaSession();
            Tick.Interval = new TimeSpan(0, 0, 0, 0, 1);
            Tick.Tick += (e, a) =>
            {
                if (mw.sessionManager != null)
                {
                    try
                    {
                        if (mw.sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                        {
                            playPause.Content = "\xE102";
                        }
                        else if (mw.sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                        {
                            playPause.Content = "\xE769";
                        }
                    }
                    catch (NullReferenceException nfe)
                    {
                        playPause.Content = "\xE102";
                    }
                }
            };
            Tick.Start();
        }
        private async void getMediaSession()
        {
            mw.sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            if(mw.sessionManager == null)
            {
                mediaSessionEmpty = true;
                waitForMD.Interval = new TimeSpan(0, 0, 1);
                waitForMD.Tick += new EventHandler(async delegate (Object o, EventArgs args)
                {
                    Console.WriteLine("MediaSession is NULL!\nAttempting to look for one...");
                    if(mediaSessionEmpty != null)
                    {
                        waitForMD.Stop();
                        return;
                    }
                    mw.sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    if (mw.sessionManager == null) return;
                    mw.sessionManager.SessionsChanged += SessionManager_SessionsChanged;
                    mw.sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged;
                    mw.sessionManager.GetCurrentSession().PlaybackInfoChanged += MainWindow_PlaybackInfoChanged;
                    mw.sessionManager.GetCurrentSession().MediaPropertiesChanged += MainWindow_MediaPropertiesChanged;
                    mw.sessionManager.GetCurrentSession().TimelinePropertiesChanged += MainWindow_TimelinePropertiesChanged;
                    // Setup sessionManager events
                    // TODO: Set events for sessionManager
                    mediaSessionEmpty = false;
                    var songInfo = await mw.sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
                    if (songInfo != null)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            songTitle.Content = songInfo.Title;
                            songArtist.Content = songInfo.Artist;
                            songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                            if (Helper.GetBitmap(songInfo.Thumbnail) != null) 
                                mw.renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                            toggleMediaControls(true);
                        });
                    }
                });
                return;
            }
            try
            {
                mw.sessionManager.SessionsChanged += SessionManager_SessionsChanged;
                mw.sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged;
                mw.sessionManager.GetCurrentSession().PlaybackInfoChanged += MainWindow_PlaybackInfoChanged;
                mw.sessionManager.GetCurrentSession().MediaPropertiesChanged += MainWindow_MediaPropertiesChanged;
                mw.sessionManager.GetCurrentSession().TimelinePropertiesChanged += MainWindow_TimelinePropertiesChanged;
                mediaSessionEmpty = false;
                var songInfo = await mw.sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
                if (songInfo == null) return;
                this.Dispatcher.Invoke(() =>
                {
                    if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                        mw.renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                    toggleMediaControls(true);
                });
            }
            catch (NullReferenceException nfe)
            {
                // it happens, dont ask how.
                toggleMediaControls(false);
                mediaSessionEmpty = true;
                getMediaSession();
            }
        }
        private async void playPauseAsync()
        {
            try
            {
                mw.mediaProperties = await mw.sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
                mw.sessionManager.GetCurrentSession().TryTogglePlayPauseAsync();
                Console.WriteLine("{0} - {1}", mw.mediaProperties?.Artist, mw.mediaProperties?.Title);
                Console.WriteLine($"Status: {mw.sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus}");
                if (mw.sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                {
                    playPause.Content = "\xE769";
                }
                else if (mw.sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    playPause.Content = "\xE102";
                }
            }
            catch (NullReferenceException nfe)
            {
                toggleMediaControls(false);
                mediaSessionEmpty = true;
                getMediaSession();
            }
        }
        private void toggleMediaControls(bool value, bool inAnotherThread = true)
        {
            if (inAnotherThread)
            {
                this.Dispatcher.Invoke(() =>
                {
                    beforeRewind.IsEnabled = value;
                    playPause.IsEnabled = value;
                    afterForward.IsEnabled = value;
                    mw.toggleMediaControls(value, inAnotherThread);
                });
                
            }
            else
            {
                beforeRewind.IsEnabled = value;
                playPause.IsEnabled = value;
                afterForward.IsEnabled = value;
                mw.toggleMediaControls(value, inAnotherThread);
            }
        }
        // GSMTC Events
        private async void SessionManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            try
            {
                mw.mediaProperties = await sender.GetCurrentSession().TryGetMediaPropertiesAsync();
                toggleMediaControls(true, true);
            }
            catch (NullReferenceException nfe)
            {
                toggleMediaControls(false);
                mediaSessionEmpty = true;
                getMediaSession();
            }
        }

        private async void SessionManager_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            try
            {
                mw.mediaProperties = await sender.GetCurrentSession().TryGetMediaPropertiesAsync();
                toggleMediaControls(true, true);
            }
            catch (NullReferenceException nfe)
            {
                toggleMediaControls(false);
                mediaSessionEmpty = true;
                getMediaSession();
            }
            catch
            {
                // COM Crash fuck off
            }
        }
        private async void MainWindow_TimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            Console.WriteLine("MainWindow_TimelinePropertiesChanged Event Called");
            var songInfo = await sender.TryGetMediaPropertiesAsync();
            if (songInfo == null) return;
            this.Dispatcher.Invoke(() =>
            {
                songTitle.Content = songInfo.Title;
                songArtist.Content = songInfo.Artist;
                songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                    mw.renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                toggleMediaControls(true);
            });
        }
        private async void MainWindow_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            Console.WriteLine("MainWindow_MediaPropertiesChanged Event Called");
            var songInfo = await sender.TryGetMediaPropertiesAsync();
            if (songInfo == null) return;
            this.Dispatcher.Invoke(() =>
            {
                songTitle.Content = songInfo.Title;
                songArtist.Content = songInfo.Artist;
                songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                    mw.renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                toggleMediaControls(true);
            });
        }

        private async void MainWindow_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            Console.WriteLine("MainWindow_PlaybackInfoChanged Event Called");
            try
            {
                var songInfo = await sender.TryGetMediaPropertiesAsync();
                if (songInfo == null) return;
                this.Dispatcher.Invoke(() =>
                {
                    songTitle.Content = songInfo.Title;
                    songArtist.Content = songInfo.Artist;
                    songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                    if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                        mw.renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                    toggleMediaControls(true);
                });
            }
            catch
            {
                this.Dispatcher.Invoke(() =>
                {
                    songTitle.Content = "No media playing.";
                    songArtist.Content = "WinIsland by Charamellized.";
                    songThumbnail.Source = null;
                    toggleMediaControls(false);
                });
            }

        }
        // Button Events
        private async void beforeRewind_Click(object sender, RoutedEventArgs e)
        {
            await mw.sessionManager.GetCurrentSession().TrySkipPreviousAsync();
        }

        private void playPause_Click(object sender, RoutedEventArgs e)
        {
            playPauseAsync();
        }
        private void afterForward_Click(object sender, RoutedEventArgs e)
        {
            //currentSession.ControlSession.TrySkipNextAsync();
            mw.sessionManager.GetCurrentSession().TrySkipNextAsync();
        }
    }
}
