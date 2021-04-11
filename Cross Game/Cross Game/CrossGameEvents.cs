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

    public class ClickEventArgs : EventArgs
    {
        public PressedButton PressedButton;
        public ClickEventArgs(PressedButton pressedButton) : base() => PressedButton = pressedButton;
    }
}
