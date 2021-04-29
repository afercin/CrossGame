using Cross_Game.DataManipulation;
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
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));

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

                Win32API.GetCursorPos(out POINT lMousePosition);

                parentWindow.Left = lMousePosition.x - targetHorizontal;
                parentWindow.Top = lMousePosition.y - targetVertical;

                parentWindow.DragMove();
            }
        }
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
            Win32API.GetCursorPos(out POINT lMousePosition);

            IntPtr lPrimaryScreen = Win32API.MonitorFromPoint(new POINT { x = 0, y = 0 }, MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            MONITORINFO lPrimaryScreenInfo = new MONITORINFO();
            if (Win32API.GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
            {
                return;
            }

            IntPtr lCurrentScreen = Win32API.MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

            MINMAXINFO lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            if (lPrimaryScreen.Equals(lCurrentScreen) == true)
            {
                lMmi.ptMaxPosition.x = lPrimaryScreenInfo.rcWork.left;
                lMmi.ptMaxPosition.y = lPrimaryScreenInfo.rcWork.top;
                lMmi.ptMaxSize.x = lPrimaryScreenInfo.rcWork.right - lPrimaryScreenInfo.rcWork.left;
                lMmi.ptMaxSize.y = lPrimaryScreenInfo.rcWork.bottom - lPrimaryScreenInfo.rcWork.top;
            }
            else
            {
                lMmi.ptMaxPosition.x = lPrimaryScreenInfo.rcMonitor.left;
                lMmi.ptMaxPosition.y = lPrimaryScreenInfo.rcMonitor.top;
                lMmi.ptMaxSize.x = lPrimaryScreenInfo.rcMonitor.right - lPrimaryScreenInfo.rcMonitor.left;
                lMmi.ptMaxSize.y = lPrimaryScreenInfo.rcMonitor.bottom - lPrimaryScreenInfo.rcMonitor.top;
            }

            Marshal.StructureToPtr(lMmi, lParam, true);
        }
    }
}
