using Cross_Game.DataManipulation;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cross_Game.Connection
{
    class MouseSimulator
    {
        private static Dictionary<Petition, MouseEventFlags> mouseMapper = new Dictionary<Petition, MouseEventFlags>()
        {
            { Petition.MouseLButtonDown, MouseEventFlags.LeftDown },
            { Petition.MouseRButtonDown, MouseEventFlags.RightDown },
            { Petition.MouseMButtonDown, MouseEventFlags.MiddleDown },
            { Petition.MouseLButtonUp, MouseEventFlags.LeftUp },
            { Petition.MouseRButtonUp, MouseEventFlags.RightUp },
            { Petition.MouseMButtonUp, MouseEventFlags.MiddleUp }
        };

        public static void Move(int x, int y) => Win32API.SetCursorPos(x, y);

        public static void Button(Petition petition) => MouseEvent(mouseMapper[petition]);

        public static void Wheel(int delta) => MouseEvent(MouseEventFlags.Wheel, delta);

        public static void MouseEvent(MouseEventFlags value, int delta = 0)
        {
            if (!Win32API.GetCursorPos(out POINT currentMousePosition))
                currentMousePosition = new POINT { x = 0, y = 0 };

            mouse_event
                   ((int)value,
                    currentMousePosition.x,
                    currentMousePosition.y,
                    delta,
                    0)
                   ;
        }
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    }
}