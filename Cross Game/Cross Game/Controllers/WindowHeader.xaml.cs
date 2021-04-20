using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para WindowHeader.xaml
    /// </summary>
    public partial class WindowHeader : UserControl
    {
        public bool MaximizeButton { get; set; }
        public bool MinimizeButton { get; set; }
        public string HeaderText
        {
            get => headerText;
            set
            {
                headerText = value;
                Caption.Text = value;
            }
        }

        public event ClickEventHandler MenuButtonClick;

        private string headerText;
        private bool mRestoreIfMove;
        private Window parentWindow;

        public WindowHeader()
        {
            InitializeComponent();
            mRestoreIfMove = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MenuButtonClick.Invoke(sender, new ClickEventArgs((PressedButton)Enum.Parse(typeof(PressedButton), ((Button)sender).Name)));
        }

        public void SetWindowHandler(Window parentWindow)
        {
            this.parentWindow = parentWindow;
            IntPtr mWindowHandle = (new WindowInteropHelper(parentWindow)).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(User32.WindowProc));

            if (!MaximizeButton)
                Maximize.Visibility = Visibility.Hidden;
            if (!MinimizeButton)
                Minimize.Visibility = Visibility.Hidden;
        }
        
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && MaximizeButton)
                switch (parentWindow.WindowState)
                {
                    case WindowState.Normal: parentWindow.WindowState = WindowState.Maximized; break;
                    case WindowState.Maximized: parentWindow.WindowState = WindowState.Normal; break;
                }
            else if (parentWindow.WindowState == WindowState.Maximized)
                mRestoreIfMove = true;
            else
                parentWindow.DragMove();
        }

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mRestoreIfMove = false;
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (mRestoreIfMove)
            {
                mRestoreIfMove = false;

                double percentHorizontal = e.GetPosition(this).X / parentWindow.ActualWidth;
                double targetHorizontal = parentWindow.RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / parentWindow.ActualHeight;
                double targetVertical = parentWindow.RestoreBounds.Height * percentVertical;

                parentWindow.WindowState = WindowState.Normal;

                User32.GetCursorPos(out User32.POINT lMousePosition);

                parentWindow.Left = lMousePosition.X - targetHorizontal;
                parentWindow.Top = lMousePosition.Y - targetVertical;

                parentWindow.DragMove();
            }
        }

        internal class User32
        {
            enum MonitorOptions : uint
            {
                MONITOR_DEFAULTTONULL = 0x00000000,
                MONITOR_DEFAULTTOPRIMARY = 0x00000001,
                MONITOR_DEFAULTTONEAREST = 0x00000002
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;

                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MINMAXINFO
            {
                public POINT ptReserved;
                public POINT ptMaxSize;
                public POINT ptMaxPosition;
                public POINT ptMinTrackSize;
                public POINT ptMaxTrackSize;
            };

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private class MONITORINFO
            {
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                public RECT rcMonitor = new RECT();
                public RECT rcWork = new RECT();
                public int dwFlags = 0;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left, Top, Right, Bottom;

                public RECT(int left, int top, int right, int bottom)
                {
                    Left = left;
                    Top = top;
                    Right = right;
                    Bottom = bottom;
                }
            }

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetCursorPos(out POINT lpPoint);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

            [DllImport("user32.dll")]
            private static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

            public static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                switch (msg)
                {
                    case 0x0024:
                        WmGetMinMaxInfo(hwnd, lParam);
                        break;
                }

                return IntPtr.Zero;
            }

            public static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
            {
                GetCursorPos(out POINT lMousePosition);

                IntPtr lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
                MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
                if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
                {
                    return;
                }

                IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

                MINMAXINFO lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                if (lPrimaryScreen.Equals(lCurrentScreen) == true)
                {
                    lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcWork.Left;
                    lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcWork.Top;
                    lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcWork.Right - lPrimaryScreenInfo.rcWork.Left;
                    lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcWork.Bottom - lPrimaryScreenInfo.rcWork.Top;
                }
                else
                {
                    lMmi.ptMaxPosition.X = lPrimaryScreenInfo.rcMonitor.Left;
                    lMmi.ptMaxPosition.Y = lPrimaryScreenInfo.rcMonitor.Top;
                    lMmi.ptMaxSize.X = lPrimaryScreenInfo.rcMonitor.Right - lPrimaryScreenInfo.rcMonitor.Left;
                    lMmi.ptMaxSize.Y = lPrimaryScreenInfo.rcMonitor.Bottom - lPrimaryScreenInfo.rcMonitor.Top;
                }

                Marshal.StructureToPtr(lMmi, lParam, true);
            }
        }
    }
}
