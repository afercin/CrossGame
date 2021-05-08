using Cross_Game.DataManipulation;
using System;
using System.Windows.Input;

namespace Cross_Game.Connection
{
    public class DXKeyboardSimulator
    {
        private bool[] pressed;

        public DXKeyboardSimulator()
        {
            pressed = new bool[0xFF];
        }
        public void SendKey(int key, Petition petition)
        {
            try
            {
                DirectXKeyStrokes keycode = (DirectXKeyStrokes)Enum.Parse(typeof(DirectXKeyStrokes), ((Key)key).ToString());
                Console.WriteLine((petition == Petition.KeyboardKeyDown? "Pressed: " : "Released: ") + keycode.ToString());

                if (pressed[(int)DirectXKeyStrokes.LeftCtrl] && pressed[(int)DirectXKeyStrokes.RightAlt] && (Key)key == Key.System)
                    keycode = DirectXKeyStrokes.LeftCtrl;

                pressed[(int)keycode] = petition == Petition.KeyboardKeyDown;

                NativeMethods.SendInput(new Input[]
                {
                new Input
                {
                    type = (int) DataManipulation.InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = (ushort) keycode,
                            dwFlags = (uint) ((petition == Petition.KeyboardKeyUp ? KeyEventF.KeyUp : KeyEventF.KeyDown) | KeyEventF.Scancode | KeyEventF.Unicode),
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ReleaseAllKeys()
        {
            foreach(int key in Enum.GetValues(typeof(DirectXKeyStrokes)))
                if (pressed[key])
                    SendKey(key, Petition.KeyboardKeyUp);
        }

        private enum DirectXKeyStrokes
        {
            Escape = 0x01,
            D1 = 0x02,
            D2 = 0x03,
            D3 = 0x04,
            D4 = 0x05,
            D5 = 0x06,
            D6 = 0x07,
            D7 = 0x08,
            D8 = 0x09,
            D9 = 0x0A,
            D0 = 0x0B,
            OemOpenBrackets = 0x0C, // '
            Oem6 = 0x0D, // ¡
            Back = 0x0E,

            Tab = 0x0F,
            Q = 0x10,
            W = 0x11,
            E = 0x12,
            R = 0x13,
            T = 0x14,
            Y = 0x15,
            U = 0x16,
            I = 0x17,
            O = 0x18,
            P = 0x19,
            Oem1 = 0x1A,        // `
            OemPlus = 0x1B,
            Return = 0x1C,

            LeftCtrl = 0x1D,
            A = 0x1E,
            S = 0x1F,
            D = 0x20,
            F = 0x21,
            G = 0x22,
            H = 0x23,
            J = 0x24,
            K = 0x25,
            L = 0x26,
            Oem3 = 0x27,        // ñ
            OemQuotes = 0x28,   // ´
            Oem5 = 0x29,        // º

            LeftShift = 0x2A,
            OemQuestion = 0x2B, // ç
            Z = 0x2C,
            X = 0x2D,
            C = 0x2E,
            V = 0x2F,
            B = 0x30,
            N = 0x31,
            M = 0x32,
            OemComma = 0x33,
            OemPeriod = 0x34,
            OemMinus = 0x35,
            RightShift = 0x36,

            Multiply = 0x37,
            LeftAlt = 0x38,
            Space = 0x39,
            Capital = 0x3A,

            F1 = 0x3B,
            F2 = 0x3C,
            F3 = 0x3D,
            F4 = 0x3E,
            F5 = 0x3F,
            F6 = 0x40,
            F7 = 0x41,
            F8 = 0x42,
            F9 = 0x43,
            F10 = 0x44,
            NumLock = 0x45,
            Scroll = 0x46,

            NumPad7 = 0x47,
            NumPad8 = 0x48,
            NumPad9 = 0x49,
            Subtract = 0x4A,

            NumPad4 = 0x4B,
            NumPad5 = 0x4C,
            NumPad6 = 0x4D,
            Add = 0x4E,

            NumPad1 = 0x4F,
            NumPad2 = 0x50,
            NumPad3 = 0x51,
            NumPad0 = 0x52,
            Decimal = 0x53,

            Snapshot = 0x54,

            OemBackslash = 0x56,        // <
            F11 = 0x57,
            F12 = 0x58,

            Clear = 0x59,               // Posiblemente no sea eso pero realiza el mismo evento que NumPad5

            F13 = 0x64,
            F14 = 0x65,
            F15 = 0x66,

            KanaMode = 0x70,
            AbntC1 = 0x73,
            DIK_CONVERT = 0x79,
            DIK_NOCONVERT = 0x7B,
            OemBackTab = 0x7C,
            DIK_YEN = 0x7D,
            AbntC2 = 0x7E,

            /** Extended Keys **/
            DIK_NUMPADEQUALS = 0x8D,    // = on numeric keypad (NEC PC98)
            MediaPreviousTrack = 0x90, 
            DIK_AT = 0x91,              // (NEC PC98)
            DIK_COLON = 0x92,           // (NEC PC98) posiblemente OemSemicolon
            DIK_UNDERLINE = 0x93,       // (NEC PC98)
            KanjiMode = 0x94,           // (Japanese keyboard) 
            DIK_STOP = 0x95,            // (NEC PC98)
            DIK_AX = 0x96,              // (Japan AX)
            DIK_UNLABELED = 0x97,       // (J3100)
            MediaNextTrack = 0x99,
            DIK_NUMPADENTER = 0x9C,     // Enter on numeric keypad
            RightCtrl = 0x9D,
            VolumeMute = 0xA0,
            DIK_CALCULATOR = 0xA1,      // Calculator
            MediaPlayPause = 0xA2,
            MediaStop = 0xA4,
            VolumeDown = 0xAE,
            VolumeUp = 0xB0,
            BrowserHome = 0xB2,
            DIK_NUMPADCOMMA = 0xB3,     // , on numeric keypad (NEC PC98)

            Divide = 0xB5,
            DIK_SYSRQ = 0xB7,
            RightAlt = 0xB8,
            Pause = 0xC5,
            Home = 0xC7,
            Up = 0xC8,
            Prior = 0xC9,
            Left = 0xCB,
            Right = 0xCD,
            End = 0xCF,
            Down = 0xD0,
            Next = 0xD1,
            Insert = 0xD2,
            Delete = 0xD3,
            LWin = 0xDB,
            RWin = 0xDC,

            Apps = 0xDD,
            DIK_POWER = 0xDE,           // System Power
            Sleep = 0xDF,
            DIK_WAKE = 0xE3,            // System Wake
            BrowserSearch = 0xE5,
            BrowserFavorites = 0xE6,
            BrowserRefresh = 0xE7,
            BrowserStop = 0xE8,
            BrowserForward = 0xE9,
            BrowserBack = 0xEA,
            DIK_MYCOMPUTER = 0xEB,      // My Computer
            LaunchMail = 0xEC,
            SelectMedia = 0xED,

            PageUp = Prior,
            PageDown = Next,
            System = LeftAlt

            /** Mouse events?  
            DIK_LEFTMOUSEBUTTON = 0x100,
            DIK_RIGHTMOUSEBUTTON = 0x101,
            DIK_MIDDLEWHEELBUTTON = 0x102,
            DIK_MOUSEBUTTON3 = 0x103,
            DIK_MOUSEBUTTON4 = 0x104,
            DIK_MOUSEBUTTON5 = 0x105,
            DIK_MOUSEBUTTON6 = 0x106,
            DIK_MOUSEBUTTON7 = 0x107,
            DIK_MOUSEWHEELUP = 0x108,
            DIK_MOUSEWHEELDOWN = 0x109,
            **/
        }
    }
}
