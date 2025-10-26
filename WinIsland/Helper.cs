using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage.Streams;
using static WinIsland.PInvoke;
using Color = System.Windows.Media.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace WinIsland
{
    public class Helper
    {
        public static bool isWindows11()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var buildNumberString = registryKey.GetValue("CurrentBuildNumber").ToString();
            if (buildNumberString == null) return false;
            int buildNumber = Int32.Parse(buildNumberString);
            return buildNumber > 22000 ? true : false;
        }
        public static double GetDpiScale(Window handle)
        {
            var hwnd = new WindowInteropHelper(handle).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11; // X axis DPI scale
            }
            return 1.0; // Default scale
        }
        public static void ExtendFrameToClientArea(Window window)
        {
            IntPtr mainWindowPtr = new WindowInteropHelper(window).Handle;
            HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);

            MARGINS margins = new MARGINS();
            margins.Left = -1;
            margins.Right = -1;
            margins.Top = -1;
            margins.Bottom = -1;

            ExtendFrame(mainWindowPtr, margins);
        }
        // NOTE: useAcrylic only affects Windows 10 or 11 21H1
        public static void EnableBlur(Window window, bool useAcrylic = true)
        {
            Helper.ExtendFrameToClientArea(window);
            SetWindowAttribute(
                new WindowInteropHelper(window).Handle,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                1);
            // TODO: Fix Acrylic Blur for Windows 10.

            if (isWindows11())
            {
                SetWindowAttribute(
                    new WindowInteropHelper(window).Handle,
                    DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                    DWM_SYSTEMBACKDROP_TYPE.DWMSBT_TABBEDWINDOW);
                return;
            }
            // Acrylic doesn't work. It's stuck on Aero for now. (Win10 Only)
            HwndSource mainWindowSrc = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0xCC, 0, 0, 0);
            window.Background = new SolidColorBrush(Color.FromArgb(0x99, 0, 0, 0));
            var windowHelper = new WindowInteropHelper(window);

            PInvoke.AccentPolicy accent = new PInvoke.AccentPolicy
            {
                AccentState = PInvoke.AccentState.ACCENT_ENABLE_BLURBEHIND
            };
            accent.GradientColor = (0x99000000);
            int accentStructSize = Marshal.SizeOf<PInvoke.AccentPolicy>(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr<PInvoke.AccentPolicy>(accent, accentPtr, false);
            PInvoke.WindowCompositionAttributeData data = default(PInvoke.WindowCompositionAttributeData);
            data.Attribute = PInvoke.WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;
            PInvoke.SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }
        public static void DisableBlur(Window window)
        {
            // TODO: Add Support for Windows 11 Modern Acrylic or Mica
            // Render as transparent background for now
            if (isWindows11()) return;
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_INVALID_STATE;
            accent.GradientColor = (0 << 24) | (0x990000 & 0xFFFFFF);

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        // Color format: ABGR (DO NOT SPECIFY ALPHA VALUE)
        public static void setBorderColor(Window window, System.Windows.Media.Color rgb, int hexColor = 0x000000FF, Border w10Border = null)
        {
            if (isWindows11())
            {
                IntPtr hWnd = new WindowInteropHelper(Window.GetWindow(window)).EnsureHandle();
                int color = hexColor;
                DwmSetWindowAttribute(hWnd, PInvoke.DWMWINDOWATTRIBUTE.DWMWA_BORDER_COLOR, ref color, Marshal.SizeOf(color));
            }
            else
            {
                if (w10Border == null) return;
                if (rgb == null) return;
                w10Border.BorderBrush = new SolidColorBrush(rgb);
            }
        }

        public static int ConvertToABGR(int r, int g, int b)
        {
            string rstr = r.ToString("X");
            string gstr = g.ToString("X");
            string bstr = b.ToString("X");
            string abgr = "0x00" + bstr + gstr + rstr;
            return Convert.ToInt32(abgr, 16);
        }
        public static System.Windows.Media.Color CalculateAverageColor(Bitmap bm)
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
            return System.Windows.Media.Color.FromRgb(Convert.ToByte(avgR), Convert.ToByte(avgG), Convert.ToByte(avgB));
        }
        public static BitmapImage? GetThumbnail(IRandomAccessStreamReference Thumbnail, bool convertToPng = true)
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
        public static Bitmap? GetBitmap(IRandomAccessStreamReference Thumbnail)
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

            using var fileMemoryStream = new System.IO.MemoryStream(thumbnailBytes);

            return (Bitmap)Bitmap.FromStream(fileMemoryStream);
        }
        public static BitmapImage ConvertToImageSource(Bitmap src)
        {
            // Fix for crash on certain images.
            // Clone into a new 32bpp ARGB bitmap (safe for encoding)
            using (var safeBitmap = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(safeBitmap))
                {
                    g.DrawImage(src, 0, 0, src.Width, src.Height);
                }

                using (var memory = new MemoryStream())
                {
                    safeBitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memory;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
        }

    }
}
