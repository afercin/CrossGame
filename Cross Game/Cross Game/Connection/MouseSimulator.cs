using Cross_Game.DataManipulation;
using System;

namespace Cross_Game.Connection
{
    class MouseSimulator
    {
        private bool[] pressed;

        public MouseSimulator()
        {
            pressed = new bool[3];
        }

        public void Move(int x, int y) => Win32API.SetCursorPos(x, y);

        public void Button(Petition petition)
        {
            switch (petition)
            {
                case Petition.MouseLButtonDown: pressed[0] = true; break;
                case Petition.MouseRButtonDown: pressed[1] = true; break;
                case Petition.MouseMButtonDown: pressed[2] = true; break;
                case Petition.MouseLButtonUp: pressed[0] = false; break;
                case Petition.MouseRButtonUp: pressed[1] = false; break;
                case Petition.MouseMButtonUp: pressed[2] = false; break;
            }
            MouseEvent((MouseEventFlags)Enum.Parse(typeof(MouseEventFlags), petition.ToString()));
        }

        public void Wheel(int delta) => MouseEvent(MouseEventFlags.Wheel, delta);

        private void MouseEvent(MouseEventFlags value, int delta = 0)
        {
            if (!Win32API.GetCursorPos(out POINT currentMousePosition))
                currentMousePosition = new POINT { x = 0, y = 0 };

            Win32API.mouse_event((int)value,
                                 currentMousePosition.x,
                                 currentMousePosition.y,
                                 delta,
                                 0);
        }

        public void ReleaseAllClicks()
        {
            if (pressed[0])
                MouseEvent(MouseEventFlags.MouseLButtonUp);
            if (pressed[1])
                MouseEvent(MouseEventFlags.MouseRButtonUp);
            if (pressed[2])
                MouseEvent(MouseEventFlags.MouseMButtonUp);
        }

    }
}