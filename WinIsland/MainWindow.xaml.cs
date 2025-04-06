﻿using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Media.Control;
using static WinIsland.PInvoke;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Grid = System.Windows.Controls.Grid;
using Point = System.Windows.Point;
using Label = System.Windows.Controls.Label;
using Button = System.Windows.Controls.Button;

namespace WinIsland
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double firstPos = 0;
        bool firstLaunch = false;
        bool showing = false;
        bool isInTargetArea = false;
        bool isAnimating = false;
        bool ignoreMouseEvent = false;
        bool isExpanded = false;
        bool expandAnimFinished = false;
        private DispatcherTimer mouseCheckTimer;
        private DispatcherTimer tick;
        private DispatcherTimer waitForMedia;
        double animDurationGlobal = 0.2D;
        bool mediaSessionIsNull = false;
        // Long ass name, like what the fuck? who made this shit, couldn't you just name it like GSMTC?
        private GlobalSystemMediaTransportControlsSessionManager? sessionManager;
        private GlobalSystemMediaTransportControlsSessionMediaProperties? mediaProperties;
        Settings settings = new Settings();

        public MainWindow()
        {
            InitializeComponent();
            StartMouseTracking();
            getMediaSession();
            toggleMediaControls(false);
            settingsButton.IsEnabled = false;
            settingsButton.Opacity = 0;
        }

        private async void getMediaSession()
        {
            sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            if (sessionManager == null) {
                mediaSessionIsNull = true;
                waitForMedia.Interval = new TimeSpan(0,0,1);
                waitForMedia.Tick += new EventHandler(async delegate (Object o, EventArgs a){
                    Console.WriteLine("MediaSession is null!\nAttempting to look for one...");
                    if(mediaSessionIsNull != null)
                    {
                        waitForMedia.Stop();
                        return;
                    }
                    sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                    if (sessionManager == null) return;
                    sessionManager.SessionsChanged += SessionManager_SessionsChanged;
                    sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged;
                    sessionManager.GetCurrentSession().PlaybackInfoChanged += MainWindow_PlaybackInfoChanged;
                    sessionManager.GetCurrentSession().MediaPropertiesChanged += MainWindow_MediaPropertiesChanged;
                    sessionManager.GetCurrentSession().TimelinePropertiesChanged += MainWindow_TimelinePropertiesChanged;
                    mediaSessionIsNull = false;
                    var songInfo = await sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
                    if (songInfo == null) return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        songTitle.Content = songInfo.Title;
                        songArtist.Content = songInfo.Artist;
                        songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                        if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                            renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                        toggleMediaControls(true);
                    });
                });
                return;
            }
            try
            {
                sessionManager.SessionsChanged += SessionManager_SessionsChanged;
                sessionManager.CurrentSessionChanged += SessionManager_CurrentSessionChanged;
                sessionManager.GetCurrentSession().PlaybackInfoChanged += MainWindow_PlaybackInfoChanged;
                sessionManager.GetCurrentSession().MediaPropertiesChanged += MainWindow_MediaPropertiesChanged;
                sessionManager.GetCurrentSession().TimelinePropertiesChanged += MainWindow_TimelinePropertiesChanged;
                mediaSessionIsNull = false;
                var songInfo = await sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
                if (songInfo == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    songTitle.Content = songInfo.Title;
                    songArtist.Content = songInfo.Artist;
                    songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                    if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                        renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                    toggleMediaControls(true);
                });
            }
            catch(NullReferenceException nfe)
            {
                // it happens, dont ask how.
                toggleMediaControls(false);
                mediaSessionIsNull = true;
                getMediaSession();
            }
            
        }

        private async void MainWindow_TimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            Console.WriteLine("MainWindow_TimelinePropertiesChanged Event Called");
            var songInfo = await sender.TryGetMediaPropertiesAsync();
            if (songInfo == null) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                songTitle.Content = songInfo.Title;
                songArtist.Content = songInfo.Artist;
                songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                    renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                toggleMediaControls(true);
            });
        }
        private async void MainWindow_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            Console.WriteLine("MainWindow_MediaPropertiesChanged Event Called");
            var songInfo = await sender.TryGetMediaPropertiesAsync();
            if (songInfo == null) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                songTitle.Content = songInfo.Title;
                songArtist.Content = songInfo.Artist;
                songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                    renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    songTitle.Content = songInfo.Title;
                    songArtist.Content = songInfo.Artist;
                    songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                    if (Helper.GetBitmap(songInfo.Thumbnail) != null)
                        renderGradient(Helper.GetBitmap(songInfo.Thumbnail));
                    toggleMediaControls(true);
                });
            }
            catch
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    songTitle.Content = "No media playing.";
                    songArtist.Content = "WinIsland by Charamellized.";
                    songThumbnail.Source = null;
                    toggleMediaControls(false);
                });
            }
            
        }

        private async void SessionManager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            try
            {
                mediaProperties = await sender.GetCurrentSession().TryGetMediaPropertiesAsync();
                toggleMediaControls(true, true);
            }
            catch (NullReferenceException nfe)
            {
                toggleMediaControls(false);
                mediaSessionIsNull = true;
                getMediaSession();
            }
        }

        private async void SessionManager_SessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
        {
            try
            {
                mediaProperties = await sender.GetCurrentSession().TryGetMediaPropertiesAsync();
                toggleMediaControls(true, true);
            }
            catch(NullReferenceException nfe)
            {
                toggleMediaControls(false);
                mediaSessionIsNull = true;
                getMediaSession();
            }
        }
        private void renderGradient(Bitmap bmp)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush(Helper.CalculateAverageColor(bmp), Color.FromArgb(0, 0, 0, 0), new Point(0.0, 1), new Point(0.5, 1));
            LinearGradientBrush gradientBrush2 = new LinearGradientBrush(Color.FromArgb(0, 0, 0, 0), Helper.CalculateAverageColor(bmp), new Point(0.5, 1), new Point(1, 1));
            gridBG.Background = gradientBrush;
            gridBG2.Background = gradientBrush2;
            bg.Source = Helper.ConvertToImageSource(bmp);
            Color color = Helper.CalculateAverageColor(bmp);
            windowBorder.BorderBrush = new SolidColorBrush(color);
            settings.thumbnail = bmp;
            settings.borderColor = color;
            dropShadowEffect.Color = color;
        }

        BlurEffect be = new BlurEffect { Radius = 0, RenderingBias = RenderingBias.Performance };
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            tick = new DispatcherTimer();
            tick.Tick += Tick_Tick;
            tick.Start();
            firstPos = Left;
            //MakeWindowClickThrough(false);
            Topmost = true;
            Top = 0;
            ShowInTaskbar = false;
            mainContent.Effect = be;
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
            Window_MouseLeave();
            Focus();
        }

        private void Tick_Tick(object? sender, EventArgs e)
        {
            clock.Content = DateTime.Now.ToString("hh:mm tt");
            PowerStatus p = SystemInformation.PowerStatus;
            //clock.Content = batteryPercentage;
            if(p.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online)
                setBatteryPercentage((int)(p.BatteryLifePercent * 100), true);
            else
                setBatteryPercentage((int)(p.BatteryLifePercent * 100));

        }
        // TODO: Move this to Utils.cs with the function returning a string
        private void setBatteryPercentage(int percentage, bool isCharging = false)
        {
            int percentageL = percentage / 10;
            if (isCharging)
            {
                switch (percentageL)
                {
                    case 0:
                        battery.Content = "\xE85A";
                        break;
                    case 1:
                        battery.Content = "\xE85B";
                        break;
                    case 2:
                        battery.Content = "\xE85C";
                        break;
                    case 3:
                        battery.Content = "\xE85D";
                        break;
                    case 4:
                        battery.Content = "\xE85E";
                        break;
                    case 5:
                        battery.Content = "\xE85F";
                        break;
                    case 6:
                        battery.Content = "\xE860";
                        break;
                    case 7:
                        battery.Content = "\xE861";
                        break;
                    case 8:
                        battery.Content = "\xE862";
                        break;
                    case 9:
                        battery.Content = "\xE83E";
                        break;
                    case 10:
                        battery.Content = "\xEA93";
                        break;
                }
                return;
            }
            switch (percentageL)
            {
                case 0:
                    battery.Content = "\xE850";
                    break;
                case 1:
                    battery.Content = "\xE851";
                    break;
                case 2:
                    battery.Content = "\xE852";
                    break;
                case 3:
                    battery.Content = "\xE853";
                    break;
                case 4:
                    battery.Content = "\xE854";
                    break;
                case 5:
                    battery.Content = "\xE855";
                    break;
                case 6:
                    battery.Content = "\xE856";
                    break;
                case 7:
                    battery.Content = "\xE857";
                    break;
                case 8:
                    battery.Content = "\xE858";
                    break;
                case 9:
                    battery.Content = "\xE859";
                    break;
                case 10:
                    battery.Content = "\xE83F";
                    break;
            }
        }
        private void Window_MouseEnter()
        {
            //bg.Visibility = Visibility.Collapsed;
            gridBG.Visibility = Visibility.Collapsed;
            gridBG2.Visibility = Visibility.Collapsed;
            thumbnailGlow.Visibility = Visibility.Collapsed;
            double currentY = -40.0D;
            WindowTransform.BeginAnimation(TranslateTransform.YProperty, null);
            WindowTransform.Y = currentY;
            // Create an animation for the TranslateTransform
            DoubleAnimation moveUpAnimation = new DoubleAnimation
            {
                From = currentY,
                To = 0,
                Duration = TimeSpan.FromSeconds(animDurationGlobal),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation blurAnim = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            DoubleAnimation generic = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            moveUpAnimation.CurrentTimeInvalidated += (sender, e) =>
            {
                isAnimating = true;
            };

            moveUpAnimation.Completed += (sender, e) =>
            {
                isAnimating = false;
            };
            // Apply animation to the TranslateTransform
            WindowTransform.BeginAnimation(TranslateTransform.YProperty, moveUpAnimation);
            be.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);
            mainContent.BeginAnimation(Grid.OpacityProperty, generic);
        }

        private void Window_MouseLeave()
        {
            double currentY = 0.0D;
            WindowTransform.BeginAnimation(TranslateTransform.YProperty, null);
            WindowTransform.Y = currentY;

            // Create an animation for the TranslateTransform
            DoubleAnimation moveUpAnimation = new DoubleAnimation
            {
                From = currentY,
                To = -50,
                Duration = TimeSpan.FromSeconds(animDurationGlobal),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation blurAnim = new DoubleAnimation
            {
                From = 0,
                To = 20,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            DoubleAnimation generic = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            moveUpAnimation.CurrentTimeInvalidated += (sender, e) =>
            {
                isAnimating = true;
            };

            moveUpAnimation.Completed += (sender, e) =>
            {
                isAnimating = false;
            };

            // Apply animation to the TranslateTransform
            WindowTransform.BeginAnimation(TranslateTransform.YProperty, moveUpAnimation);
            be.BeginAnimation(BlurEffect.RadiusProperty, blurAnim);
            mainContent.BeginAnimation(Grid.OpacityProperty, generic);
        }

        private void StartMouseTracking()
        {
            mouseCheckTimer = new DispatcherTimer();
            mouseCheckTimer.Interval = TimeSpan.FromMilliseconds(100); // Check every 100ms
            mouseCheckTimer.Tick += CheckMousePosition;
            mouseCheckTimer.Start();
        }
        private void CheckMousePosition(object sender, EventArgs e)
        {
            if (ignoreMouseEvent) return;
            POINT mousePos;
            if (GetCursorPos(out mousePos))
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                RECT windowRect;
                if (!GetWindowRect(hwnd, out windowRect))
                    return;
                bool inRange = (mousePos.X >= windowRect.Left && mousePos.X <= windowRect.Right) &&
                          (showing ? (mousePos.Y >= 0 && mousePos.Y <= 41) : (mousePos.Y >= 0 && mousePos.Y <= 1));

                if (inRange && !isInTargetArea)
                {
                    isAnimating = true;
                    Thread.Sleep(210);
                    if (!inRange) return;
                    isInTargetArea = true;
                    Window_MouseEnter();
                    showing = true;
                }
                else if (!inRange && isInTargetArea)
                {
                    isInTargetArea = false;
                    Window_MouseLeave();
                    showing = false;
                }
            }
        }
        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            ignoreMouseEvent = false;
            //bg.Visibility = Visibility.Collapsed;
            gridBG.Visibility = Visibility.Collapsed;
            gridBG2.Visibility = Visibility.Collapsed;
            thumbnailGlow.Visibility = Visibility.Collapsed;
            isExpanded = false;
            AnimateWindowSize(341, 51, (int)firstPos, true, 1);
            //Height = 51;
            //Width = 451;
        }
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!firstLaunch)
            {
                firstLaunch = true;
                return;
            }
            if (isExpanded)
            {
                //bg.Visibility = Visibility.Collapsed;
                isExpanded = false;
                gridBG.Visibility = Visibility.Collapsed;
                gridBG2.Visibility = Visibility.Collapsed;
                thumbnailGlow.Visibility = Visibility.Collapsed;
                ignoreMouseEvent = false;
                AnimateWindowSize(341, 51, (int)firstPos, true, 1);
                return;
            }
            if (ignoreMouseEvent) return;
            isExpanded = true;
            Topmost = false;
            Topmost = true;
            ignoreMouseEvent = true;
            AnimateWindowSize(852, 350, (int)firstPos - 255);
            new Thread(() =>
            {
                while (isAnimating) ;
                this.Dispatcher.Invoke(() =>
                {
                    //bg.Visibility = Visibility.Visible;
                    gridBG.Visibility = Visibility.Visible;
                    gridBG2.Visibility = Visibility.Visible;
                    thumbnailGlow.Visibility = Visibility.Visible;
                });
            }).Start();
            //Height = 250;
            //Width = 902;
        }
        private async void AnimateWindowSize(int width, int height, int left, bool easeoutback = true, double bounceRadius = 2)
        {
            isAnimating = true;
            ThicknessAnimation ta;
            if (isExpanded)
            {
                ta = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 10, 0),
                    To = new Thickness(0, 0, 35, 0),
                    Duration = new TimeSpan(0, 0, 0, 0, 300),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };
                settingsButton.IsEnabled = true;
            }
            else if (!isExpanded)
            {
                ta = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 32, 0),
                    To = new Thickness(0, 0, 10, 0),
                    Duration = new TimeSpan(0, 0, 0, 0, 300),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };
                settingsButton.IsEnabled = false;
            }
            else
            {
                ta = new ThicknessAnimation
                {
                    From = new Thickness(0, 0, 32, 0),
                    To = new Thickness(0, 0, 10, 0),
                    Duration = new TimeSpan(0, 0, 0, 0, 300),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut }
                };
                settingsButton.IsEnabled = false;
            }
            DoubleAnimation blurIslandContentBegin = new DoubleAnimation
            {
                From = 0,
                To = 20,
                Duration = TimeSpan.FromSeconds(animDurationGlobal/2)
            };
            DoubleAnimation resetOpacity = new DoubleAnimation
                {
                    From = 1,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0)
                };
            DoubleAnimation resetOpacityDim = new DoubleAnimation
            {
                From = 1,
                To = 0.3,
                Duration = TimeSpan.FromSeconds(animDurationGlobal/2)
            };
            DoubleAnimation opacityShowAnim = new DoubleAnimation
            {
                From = 0.0,
                To = 1,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            DoubleAnimation opacityShowAnimDim = new DoubleAnimation
            {
                From = 0.3,
                To = 1,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            DoubleAnimation blurIslandContentAnim = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromSeconds(animDurationGlobal/2)
            };
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            var stopwatch = Stopwatch.StartNew();

            int duration = 300;
            int startWidth = (int)this.Width;
            int startHeight = (int)this.Height;
            int startLeft = (int)this.Left;
            int delay = 0; // Delay between steps (ms)

            islandContent.BeginAnimation(Grid.OpacityProperty, resetOpacity);
            gradientBG.BeginAnimation(Grid.OpacityProperty, resetOpacity);
            settingsButton.BeginAnimation(Button.OpacityProperty, resetOpacity);
            if (settings.blurEverywhere)
            {
                mainContent.BeginAnimation(Grid.OpacityProperty, resetOpacityDim);
                be.BeginAnimation(BlurEffect.RadiusProperty, blurIslandContentBegin);
            }
            if (!isExpanded)
                battery.BeginAnimation(Label.MarginProperty, ta);

            while (stopwatch.ElapsedMilliseconds < duration)
            {
                double t = (double)stopwatch.ElapsedMilliseconds / duration;
                double easeT = 0; // Apply easing function
                if (easeoutback)
                    easeT = Easing.EaseOutBack(t, bounceRadius);
                else
                {
                    easeT = Easing.EaseInOutCubic(t);
                }
                    

                int newWidth = (int)(startWidth + (width - startWidth) * easeT);
                int newHeight = (int)(startHeight + (height - startHeight) * easeT);
                int newLeft = (int)(startLeft + (left - startLeft) * easeT);

                SetWindowPos(hwnd, IntPtr.Zero, newLeft, 0, newWidth, newHeight, SWP_NOZORDER | SWP_NOACTIVATE);
                await Task.Delay(delay);
            }
            if (settings.blurEverywhere)
            {
                mainContent.BeginAnimation(Grid.OpacityProperty, opacityShowAnimDim);
                be.BeginAnimation(BlurEffect.RadiusProperty, blurIslandContentAnim);
            }
            if (isExpanded)
            {
                settingsButton.BeginAnimation(Button.OpacityProperty, opacityShowAnim);
                battery.BeginAnimation(Label.MarginProperty, ta);
            }
            islandContent.BeginAnimation(Grid.OpacityProperty, opacityShowAnim);
            gradientBG.BeginAnimation(Grid.OpacityProperty, opacityShowAnim);
            isAnimating = false;
        }
        
        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private async void beforeRewind_Click(object sender, RoutedEventArgs e)
        {
            await sessionManager.GetCurrentSession().TrySkipPreviousAsync();
        }

        private void playPause_Click(object sender, RoutedEventArgs e)
        {
            playPauseAsync();
        }

        private async void playPauseAsync()
        {
            try
            {
                mediaProperties = await sessionManager.GetCurrentSession().TryGetMediaPropertiesAsync();
                sessionManager.GetCurrentSession().TryTogglePlayPauseAsync();
                Console.WriteLine("{0} - {1}", mediaProperties?.Artist, mediaProperties?.Title);
                Console.WriteLine($"Status: {sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus}");
                if (sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
                {
                    playPause.Content = "\xE769";
                }
                else if (sessionManager.GetCurrentSession().GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                {
                    playPause.Content = "\xE102";
                }
            }catch(NullReferenceException nfe)
            {
                toggleMediaControls(false);
                mediaSessionIsNull = true;
                getMediaSession();
            }
        }

        private void afterForward_Click(object sender, RoutedEventArgs e)
        {
            //currentSession.ControlSession.TrySkipNextAsync();
            sessionManager.GetCurrentSession().TrySkipNextAsync();
        }
        public void toggleMediaControls(bool value, bool inAnotherThread = true)
        {
            if (inAnotherThread)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    beforeRewind.IsEnabled = value;
                    playPause.IsEnabled = value;
                    afterForward.IsEnabled = value;
                });
            }
            else
            {
                beforeRewind.IsEnabled = value;
                playPause.IsEnabled = value;
                afterForward.IsEnabled = value;
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            PopoutWindow pw = new PopoutWindow();
            pw.Show();
        }
    }
}