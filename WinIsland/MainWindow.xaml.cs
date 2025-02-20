using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Storage.Streams;
using WindowsMediaController;
using static WindowsMediaController.MediaManager;
using Grid = System.Windows.Controls.Grid;

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
        double animDurationGlobal = 0.2D;
        string lastTitleText = "";

        private static readonly MediaManager mediaManager = new MediaManager();
        private static MediaSession? currentSession = null;

        public MainWindow()
        {
            InitializeComponent();
            StartMouseTracking();
            mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
            mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
            mediaManager.OnFocusedSessionChanged += MediaManager_OnFocusedSessionChanged;
            mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

            mediaManager.Start();
        }

        private void MediaManager_OnAnyMediaPropertyChanged(MediaSession mediaSession, Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var songInfo = mediaSession.ControlSession.TryGetMediaPropertiesAsync().GetAwaiter().GetResult();
                if (songInfo == null) return;
                songTitle.Content = songInfo.Title;
                songArtist.Content = songInfo.Artist;
                songThumbnail.Source = Helper.GetThumbnail(songInfo.Thumbnail);
                songThumbnailBG.Source = Helper.GetThumbnail(songInfo.Thumbnail);
            });
        }

        private void MediaManager_OnFocusedSessionChanged(MediaSession mediaSession)
        {
            
        }

        private void MediaManager_OnAnySessionClosed(MediaSession mediaSession)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                currentSession = null;
            });
        }

        private void MediaManager_OnAnySessionOpened(MediaSession mediaSession)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                currentSession = mediaSession;
            });
        }

        BlurEffect be = new BlurEffect { Radius = 0, RenderingBias = RenderingBias.Performance };
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tick = new DispatcherTimer();
            tick.Start();
            tick.Tick += Tick_Tick;
            EnableDwmTransitions();
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
        }

        private void Tick_Tick(object? sender, EventArgs e)
        {
            clock.Content = DateTime.Now.ToString("hh:mm tt");
        }

        private void Window_MouseEnter()
        {
            gridBG.Visibility = Visibility.Collapsed;
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
                    //MakeWindowClickThrough(true);
                    //StatusText.Text = "Mouse in target area!";
                    //StatusText.Foreground = Brushes.Green;
                }
                else if (!inRange && isInTargetArea)
                {
                    isInTargetArea = false;
                    Window_MouseLeave();
                    showing = false;
                    //StatusText.Text = "Move the mouse!";
                    //StatusText.Foreground = Brushes.Black;
                }
            }
        }
        
        #region Mouse Events P/Invoke
        // Windows API: Get mouse position in screen coordinates
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
        // Structure for window rectangle
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
        #region Window P/Invoke

        // Windows API functions to modify window styles
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int     X, int Y, int cx, int cy, uint uFlags);


        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        #endregion
        private void Window_Activated(object sender, EventArgs e)
        {
            
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            ignoreMouseEvent = false;
            AnimateWindowSize(451, 51, (int)firstPos);
            isExpanded = false;
            //Height = 51;
            //Width = 451;
        }
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!firstLaunch) {
                firstLaunch = true;
                return;
            }
            if (isExpanded)
            {
                gridBG.Visibility = Visibility.Collapsed;
                ignoreMouseEvent = false;
                AnimateWindowSize(451, 51, (int)firstPos);
                isExpanded = false;
                return;
            }
            if (ignoreMouseEvent) return;
            isExpanded = true;
            Topmost = false;
            Topmost = true;
            ignoreMouseEvent = true;
            gridBG.Visibility = Visibility.Visible;
            AnimateWindowSize(852, 350, (int)firstPos - 200);
            //Height = 250;
            //Width = 902;
        }
        private void EnableDwmTransitions()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int enableTransition = 1; // Enable animations

            // Apply the transition effect
            DwmSetWindowAttribute(hwnd, DWMWA_TRANSITIONS_FORCEDISABLED, ref enableTransition, sizeof(int));
        }
        private async void AnimateWindowSize(int width, int height, int left)
        {
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
                From = 10,
                To = 0,
                Duration = TimeSpan.FromSeconds(animDurationGlobal)
            };
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            
            var stopwatch = Stopwatch.StartNew();

            int duration = (int)(animDurationGlobal * 1000);
            int startWidth = (int)this.Width;
            int startHeight = (int)this.Height;
            int startLeft = (int)this.Left;
            int delay = 0; // Delay between steps (ms)

            mainContent.BeginAnimation(Grid.OpacityProperty, opacity);

            while (stopwatch.ElapsedMilliseconds < duration)
            {
                double t = (double)stopwatch.ElapsedMilliseconds / duration;
                double easeT = EaseInOutCubic(t); // Apply easing function

                int newWidth = (int)(startWidth + (width - startWidth) * easeT);
                int newHeight = (int)(startHeight + (height - startHeight) * easeT);
                int newLeft = (int)(startLeft + (left - startLeft) * easeT);

                SetWindowPos(hwnd, IntPtr.Zero, newLeft, 0, newWidth, newHeight, SWP_NOZORDER | SWP_NOACTIVATE);
                await Task.Delay(delay);
            }
            be.BeginAnimation(BlurEffect.RadiusProperty, blurAnim2);
            mainContent.BeginAnimation(Grid.OpacityProperty, opacity2);
        }
        private double EaseInOutCubic(double t)
        {
            t = Math.Clamp(t, 0, 1); // Ensure t is within 0 and 1
            return t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {

        }
    }
    internal static class Helper
    {
        internal static BitmapImage? GetThumbnail(IRandomAccessStreamReference Thumbnail, bool convertToPng = true)
        {
            if (Thumbnail == null)
                return null;

            var thumbnailStream = Thumbnail.OpenReadAsync().GetAwaiter().GetResult();
            byte[] thumbnailBytes = new byte[thumbnailStream.Size];
            using (DataReader reader = new DataReader(thumbnailStream))
            {
                reader.LoadAsync((uint)thumbnailStream.Size).GetAwaiter().GetResult();
                reader.ReadBytes(thumbnailBytes);
            }

            byte[] imageBytes = thumbnailBytes;

            if (convertToPng)
            {
                using var fileMemoryStream = new System.IO.MemoryStream(thumbnailBytes);
                Bitmap thumbnailBitmap = (Bitmap)Bitmap.FromStream(fileMemoryStream);

                if (!thumbnailBitmap.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                {
                    using var pngMemoryStream = new System.IO.MemoryStream();
                    thumbnailBitmap.Save(pngMemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    imageBytes = pngMemoryStream.ToArray();
                }
            }

            var image = new BitmapImage();
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }

            return image;
        }
    }
}