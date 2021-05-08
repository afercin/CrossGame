using System;
using System.Runtime.InteropServices;

namespace Cross_Game.DataManipulation
{
    [Flags]
    enum InputType
    {
        Mouse    = 0,
        Keyboard = 1,
        Hardware = 2
    }
    [Flags]
    enum KeyEventF
    {
        /// <summary> The key is being pressed. </summary>
        KeyDown = 0x0000,
        /// <summary> The scan code was preceded by a prefix byte having the value 0xE0 (224). </summary>
        ExtendedKey = 0x0001,
        /// <summary> The key is being released. </summary>
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008,
    }
    [Flags]
    enum MouseEventFlags
    {
        /// <summary> Movement occurred. </summary>
        Move = 0x0001,
        /// <summary> The left button is down. </summary>
        MouseLButtonDown = 0x0002,
        /// <summary> The left button is up. </summary>
        MouseLButtonUp = 0x0004,
        /// <summary> The right button is down. </summary>
        MouseRButtonDown = 000008,
        /// <summary> The right button is up. </summary>
        MouseRButtonUp = 0x0010,
        /// <summary> The middle button is down. </summary>
        MouseMButtonDown = 0x0020,
        /// <summary> The middle button is up. </summary>
        MouseMButtonUp = 0x0040,
        /// <summary>  An X button was pressed. </summary>
        MouseXButtonDown = 0x0080,
        /// <summary> An X button was released. </summary>
        MouseXButtonUp = 0x0100,
        /// <summary> The wheel has been moved, if the mouse has a wheel. The amount of movement is specified in dwData. </summary>
        Wheel = 0x0800,
        /// <summary> The wheel button is tilted. </summary>
        HWheel = 0x1000,
        /// <summary> The dx and dy parameters contain normalized absolute coordinates. If not set, those parameters contain relative data: the change in position since the last reported position. This flag can be set, or not set, regardless of what kind of mouse or mouse-like device, if any, is connected to the system. For further information about relative mouse motion, see the following Remarks section. </summary>
        Absolute = 0x8000
    }
    [Flags]
    enum MonitorOptions
    {
        MONITOR_DEFAULTTONULL    = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }
    /// <summary> Defines a rectangle by the coordinates of its upper-left and lower-right corners. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    /// <summary> Defines the x and y coordinates of a point. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x;
        public int y;
    }
    /// <summary> Contains global cursor information. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }
    /// <summary> This structure is used by the SendInput function to synthesize keystrokes, stylus and mouse motions, and button clicks. </summary>
    struct Input
    {
        public int type;
        public InputUnion u;
    }
    /// <summary> Input's struct union. </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public readonly HardwareInput hi;
    }
    /// <summary> Contains information about simulated mouse input. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
    /// <summary> Contains information about simulated keyboard input. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public readonly uint time;
        public IntPtr dwExtraInfo;
    }
    /// <summary> Contains information about a simulated input device message. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct HardwareInput
    {
        public readonly uint uMsg;
        public readonly ushort wParamL;
        public readonly ushort wParamH;
    }
    /// <summary> Contains information about a window's maximized size and position and its minimum and maximum tracking size. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    };
    /// <summary> Contains information about a display monitor. </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }
    /// <summary> Clase de apoyo para utiliar funciones nativas de c++. </summary>
    class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        /// <summary> Retrieves the position of the mouse cursor, in screen coordinates. </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);
        /// <summary> Moves the cursor to the specified screen coordinates. If the new coordinates are not within the screen rectangle set by the most recent ClipCursor function call, the system automatically adjusts the coordinates so that the cursor stays within the rectangle. </summary>
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);
        /// <summary> Retrieves information about a display monitor. </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        /// <summary> Retrieves a handle to the display monitor that contains a specified point. </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);
        /// <summary> This function synthesizes keystrokes, stylus and mouse motions, and button clicks. </summary>
        public static uint SendInput(Input[] Inputs) => SendInput((uint)Inputs.Length, Inputs, Marshal.SizeOf(typeof(Input)));
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
        /// <summary> Retrieves information about the global cursor. </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();
        /// <summary> Retrieves information about the global cursor. </summary>
        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);
        /// <summary> Retrieves a handle to the desktop window. The desktop window covers the entire screen. The desktop window is the area on top of which other windows are painted. </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        /// <summary> Retrieves a handle to the foreground window (the window with which the user is currently working). The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads. </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        /// <summary> Retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars. A window device context permits painting anywhere in a window, because the origin of the device context is the upper-left corner of the window instead of the client area. </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        /// <summary> Releases a device context (DC), freeing it for use by other applications. The effect of the ReleaseDC function depends on the type of DC. It frees only common and window DCs. It has no effect on class or private DCs. </summary>
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        /// <summary> Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen. </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        /// <summary> BitBtl dwRop attribute: Copies the source rectangle directly to the destination rectangle. </summary>
        public const int SRCCOPY = 0x00CC0020;
        /// <summary> Performs a bit-block transfer of the color data corresponding to a rectangle of pixels from the specified source device context into a destination device context. </summary>
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hObjectSource,
            int nXSrc, int nYSrc, int dwRop);
        /// <summary> Creates a memory device context (DC) compatible with the specified device. </summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        /// <summary>  Creates a bitmap compatible with the device that is associated with the specified device context. </summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
            int nHeight);
        /// <summary> Deletes the specified device context (DC). </summary>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        /// <summary> Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid. </summary>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        /// <summary> Selects an object into the specified device context (DC). The new object replaces the previous object of the same type. </summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }
}
