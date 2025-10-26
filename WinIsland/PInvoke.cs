using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinIsland
{
    public class PInvoke
    {
        #region Mouse PInvoke
        // Windows API: Get mouse position in screen coordinates
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        // Structure for window rectangle
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion
        #region Window P/Invoke

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;
        public enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
            DWMWA_BORDER_COLOR = 34,
            DWMWA_SYSTEMBACKDROP_TYPE = 38
        }
        public enum DWMNCRENDERINGPOLICY
        {
            DWMNCRP_USEWINDOWSTYLE,
            DWMNCRP_DISABLED,
            DWMNCRP_ENABLED,
            DWMNCRP_LAST
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }
        [Flags]
        public enum DWM_SYSTEMBACKDROP_TYPE : int
        {
            DWMSBT_AUTO = 0,
            DWMSBT_NONE = 1,
            DWMSBT_MAINWINDOW = 2, // Mica
            DWMSBT_TRANSIENTWINDOW = 3, // Acrylic
            DWMSBT_TABBEDWINDOW = 4 // Tabbed
        }
        // WHY IS THERE A TON OF THESE HEEELPPPP
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;
        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref DWM_SYSTEMBACKDROP_TYPE pvAttribute, int cbAttribute);
        public static int ExtendFrame(IntPtr hwnd, MARGINS margins)
            => DwmExtendFrameIntoClientArea(hwnd, ref margins);
        public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter)
            => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
        public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, DWM_SYSTEMBACKDROP_TYPE parameter)
            => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);
        [DllImport("dwmapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref DWMNCRENDERINGPOLICY pvAttribute,
        int cbAttribute);
        [DllImport("dwmapi.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, DWMNCRENDERINGPOLICY pvAttribute,
        uint cbAttribute);


        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        #endregion
        #region Scaling P/Invoke
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int value);

        private const int DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

        public static void EnableDpiAwareness()
        {
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
        }
        #endregion
    }
}
