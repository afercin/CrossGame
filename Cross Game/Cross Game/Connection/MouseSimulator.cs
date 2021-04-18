using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cross_Game.Connection
{
    class MouseSimulator
    {
        private static Dictionary<Petition, MouseOperations.MouseEventFlags> mouseMapper = new Dictionary<Petition, MouseOperations.MouseEventFlags>()
        {
            { Petition.MouseLButtonDown, MouseOperations.MouseEventFlags.LeftDown },
            { Petition.MouseRButtonDown, MouseOperations.MouseEventFlags.RightDown },
            { Petition.MouseMButtonDown, MouseOperations.MouseEventFlags.MiddleDown },
            { Petition.MouseLButtonUp, MouseOperations.MouseEventFlags.LeftUp },
            { Petition.MouseRButtonUp, MouseOperations.MouseEventFlags.RightUp },
            { Petition.MouseMButtonUp, MouseOperations.MouseEventFlags.MiddleUp }
        };

        public static void Move(int x, int y) => MouseOperations.SetCursorPosition(x, y);

        public static void Button(Petition petition) => MouseOperations.MouseEvent(mouseMapper[petition]);

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