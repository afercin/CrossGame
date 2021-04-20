using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public delegate void ActiveChangedEventHandler(object sender, ActiveChangedEventArgs e);
    public class ActiveChangedEventArgs : EventArgs
    {
        public bool IsActived { get; set; }
        public ActiveChangedEventArgs(bool isActived) : base() => IsActived = isActived;
    }
}
