using System;

namespace Cross_Game
{
    public enum PressedButton
    {
        Close,
        Maximize,
        Minimize
    }

    public delegate void ClickEventHandler(object sender, ClickEventArgs e);
    public class ClickEventArgs : EventArgs
    {
        public PressedButton PressedButton { get; set; }
        public ClickEventArgs(PressedButton pressedButton) : base() => PressedButton = pressedButton;
    }
}
