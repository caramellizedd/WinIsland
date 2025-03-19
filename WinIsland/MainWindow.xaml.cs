using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows;
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
        // TODO: Toggleable Settings - PLEASE move this to a settings class. (note to self)
        public bool blurEverywhere = false;

        public MainWindow()
        {
            InitializeComponent();
            StartMouseTracking();
            getMediaSession();
            toggleMediaControls(false);
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
            LinearGradientBrush gradientBrush = new LinearGradientBrush(CalculateAverageColor(bmp), Color.FromArgb(0, 0, 0, 0), new Point(0.0, 1), new Point(0.5, 1));
            LinearGradientBrush gradientBrush2 = new LinearGradientBrush(Color.FromArgb(0, 0, 0, 0), CalculateAverageColor(bmp), new Point(0.5, 1), new Point(1, 1));
            gridBG.Background = gradientBrush;
            gridBG2.Background = gradientBrush2;
            bg.Source = Helper.ConvertToImageSource(bmp);
            windowBorder.BorderBrush = new SolidColorBrush(CalculateAverageColor(bmp));
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
            AnimateWindowSize(341, 51, (int)firstPos, true, 1);
            isExpanded = false;
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
                gridBG.Visibility = Visibility.Collapsed;
                gridBG2.Visibility = Visibility.Collapsed;
                thumbnailGlow.Visibility = Visibility.Collapsed;
                ignoreMouseEvent = false;
                AnimateWindowSize(341, 51, (int)firstPos, true, 1);
                isExpanded = false;
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
            DoubleAnimation opacity = new DoubleAnimation
            {
                From = 1,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0)
            };
            DoubleAnimation opacity2 = new DoubleAnimation
            {
                From = 0.0,
                To = 1,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            DoubleAnimation blurAnim2 = new DoubleAnimation
            {
                From = 20,
                To = 0,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            var stopwatch = Stopwatch.StartNew();

            int duration = 300;
            int startWidth = (int)this.Width;
            int startHeight = (int)this.Height;
            int startLeft = (int)this.Left;
            int delay = 0; // Delay between steps (ms)

            islandContent.BeginAnimation(Grid.OpacityProperty, opacity);
            gradientBG.BeginAnimation(Grid.OpacityProperty, opacity);

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
            if(blurEverywhere)
                be.BeginAnimation(BlurEffect.RadiusProperty, blurAnim2);
            islandContent.BeginAnimation(Grid.OpacityProperty, opacity2);
            gradientBG.BeginAnimation(Grid.OpacityProperty, opacity2);
            isAnimating = false;
        }
        
        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {

        }
        private Color CalculateAverageColor(Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int red = 0;
            int green = 0;
            int blue = 0;
            int minDiversion = 15; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            int dropped = 0; // keep track of dropped pixels
            long[] totals = new long[] { 0, 0, 0 };
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            BitmapData srcData = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];
                        if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                        {
                            totals[2] += red;
                            totals[1] += green;
                            totals[0] += blue;
                        }
                        else
                        {
                            dropped++;
                        }
                    }
                }
            }

            int count = width * height - dropped;
            int avgR, avgB, avgG;
            if (totals[2] != 0)
                avgR = (int)(totals[2] / count);
            else
                avgR = 255;
            if (totals[1] != 0)
                avgG = (int)(totals[1] / count);
            else
                avgG = 255;
            if (totals[0] != 0)
                avgB = (int)(totals[0] / count);
            else
                avgB = 255;
            bm.UnlockBits(srcData);
            return Color.FromRgb(Convert.ToByte(avgR), Convert.ToByte(avgG), Convert.ToByte(avgB));
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
    }
}