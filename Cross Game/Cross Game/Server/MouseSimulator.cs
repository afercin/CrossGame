using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cross_Game.Server
{
    class MouseSimulator
    {
        private static Dictionary<Actions, MouseOperations.MouseEventFlags> mouseMapper = new Dictionary<Actions, MouseOperations.MouseEventFlags>()
        {
            { Actions.MouseLButtonDown, MouseOperations.MouseEventFlags.LeftDown },
            { Actions.MouseRButtonDown, MouseOperations.MouseEventFlags.RightDown },
            { Actions.MouseMButtonDown, MouseOperations.MouseEventFlags.MiddleDown },
            { Actions.MouseLButtonUp, MouseOperations.MouseEventFlags.LeftUp },
            { Actions.MouseRButtonUp, MouseOperations.MouseEventFlags.RightUp },
            { Actions.MouseMButtonUp, MouseOperations.MouseEventFlags.MiddleUp }
        };

        public static void Move(int x, int y) => MouseOperations.SetCursorPosition(x, y);

        public static void Button(Actions action) => MouseOperations.MouseEvent(mouseMapper[action]);

        public static void Wheel(int delta) => MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.Wheel, delta);

        private class MouseOperations
        {
            [Flags]
            public enum MouseEventFlags
            {
                LeftDown = 0x00000002,
                LeftUp = 0x00000004,
                MiddleDown = 0x00000020,
                MiddleUp = 0x00000040,
                Move = 0x00000001,
                Absolute = 0x00008000,
                RightDown = 0x00000008,
                RightUp = 0x00000010,
                Wheel = 0x00000800
            }

            [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetCursorPos(int x, int y);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetCursorPos(out MousePoint lpMousePoint);

            [DllImport("user32.dll")]
            private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

            public static void SetCursorPosition(int x, int y)
            {
                SetCursorPos(x, y);
            }

            public static MousePoint GetCursorPosition()
            {
                var gotPoint = GetCursorPos(out MousePoint currentMousePoint);
                if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
                return currentMousePoint;
            }

            public static void MouseEvent(MouseEventFlags value, int delta = 0)
            {
                MousePoint position = GetCursorPosition();

                mouse_event
                    ((int)value,
                     position.X,
                     position.Y,
                     delta,
                     0)
                    ;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct MousePoint
            {
                public int X;
                public int Y;

                public MousePoint(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }
        }
    }
}
