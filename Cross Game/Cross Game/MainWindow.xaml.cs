using Cross_Game.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OptionButton currentOption;

        public string UserName
        {
            get => DBConnection.User_NickName.Split('#')[0];
        }
        public string UserNumber
        {
            get => DBConnection.User_NickName.Split('#')[1];
        }

        public MainWindow()
        {
            InitializeComponent();
            currentOption = Ordenadores;
            Ordenadores.Active = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Hide();

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        

        private void OptionButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OptionButton pressedOption = sender as OptionButton;
            if (currentOption != pressedOption)
            {
                currentOption.Active = false;
                ChangeMenuVisibility(currentOption.Name, Visibility.Hidden);
                ChangeMenuVisibility(pressedOption.Name, Visibility.Visible);
                currentOption = pressedOption;
            }
        }

        private void ChangeMenuVisibility(string name, Visibility visibility)
        {
            switch (currentOption.Name)
            {
                case "Ordenadores": break;
                case "Amigos": break;
                case "Transmisión": break;
            }
        }

        #region Resize and move window

        private bool mRestoreIfMove = false;

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            IntPtr mWindowHandle = (new WindowInteropHelper(this)).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(User32.WindowProc));
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                switch (WindowState)
                {
                    case WindowState.Normal: WindowState = WindowState.Maximized; break;
                    case WindowState.Maximized: WindowState = WindowState.Normal; break;
                }
            else if (WindowState == WindowState.Maximized)
                mRestoreIfMove = true;
            else
                DragMove();
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

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                User32.GetCursorPos(out User32.POINT lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                DragMove();
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

        #endregion
    }
}
