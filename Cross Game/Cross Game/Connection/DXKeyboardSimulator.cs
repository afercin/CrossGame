using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Cross_Game.Connection
{
    public class DXKeyboardSimulator
    {
        public static void SendKey(int key, Petition petition)
        {
            int keycode = (int)Enum.Parse(typeof(DirectXKeyStrokes), ((Key)key).ToString());

            Input[] input =
            {
                new Input
                {
                    type = (int) InputType.Keyboard,
                    u = new InputUnion
                    {
                        ki = new KeyboardInput
                        {
                            wVk = 0,
                            wScan = (ushort) keycode,
                            dwFlags = (uint) ((petition == Petition.KeyboardKeyUp ? KeyEventF.KeyUp : KeyEventF.KeyDown) | KeyEventF.Scancode),
                            dwExtraInfo = GetMessageExtraInfo()
                        }
                    }
                }
            };

            SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(Input)));
        }

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008,
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public readonly HardwareInput hi;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public readonly uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public readonly uint uMsg;
            public readonly ushort wParamL;
            public readonly ushort wParamH;
        }

        public enum DirectXKeyStrokes
        {
            DIK_UNKNOW = 0x00,

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
            Oem1 = 0x1A, // `
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
            Oem3 = 0x27, // ñ
            OemQuotes = 0x28, // ´
            Oem5 = 0x29, // º

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

            DIK_UNKNOW2 = 0x55,

            OemBackslash = 0x56, // <
            F11 = 0x57,
            F12 = 0x58,

            Clear = 0x59, // Posiblemente no sea eso pero realiza el mismo evento que NumPad5

            DIK_UNKNOW3 = 0x5A,
            DIK_UNKNOW4 = 0x5B, // no funciona
            DIK_UNKNOW5 = 0x5C, // no funciona
            DIK_UNKNOW6 = 0x5D,
            DIK_UNKNOW7 = 0x5E,
            DIK_UNKNOW8 = 0x5F,
            DIK_UNKNOW9 = 0x60,
            DIK_UNKNOW10 = 0x61,
            DIK_UNKNOW11 = 0x62,
            DIK_UNKNOW12 = 0x63,

            F13 = 0x64,
            F14 = 0x65,
            F15 = 0x66,

            DIK_UNKNOW13 = 0x67,
            DIK_UNKNOW14 = 0x68,
            DIK_UNKNOW15 = 0x69,
            DIK_UNKNOW16 = 0x6A,
            DIK_UNKNOW17 = 0x6B,
            DIK_UNKNOW18 = 0x6C,
            DIK_UNKNOW19 = 0x6D,
            DIK_UNKNOW20 = 0x6E,
            DIK_UNKNOW21 = 0x6F,
            DIK_KANA = 0x70,
            DIK_UNKNOW22 = 0x71,
            DIK_UNKNOW23 = 0x72,
            DIK_ABNT_C1 = 0x73, // /? on Brazilian keyboard
            DIK_UNKNOW24 = 0x74,
            DIK_UNKNOW25 = 0x75,
            DIK_UNKNOW26 = 0x76,
            DIK_UNKNOW27 = 0x77,
            DIK_UNKNOW28 = 0x78,
            DIK_CONVERT = 0x79,
            DIK_UNKNOW29 = 0x7A,
            DIK_NOCONVERT = 0x7B,
            DIK_TAB = 0x7C,
            DIK_YEN = 0x7D,
            DIK_ABNT_C2 = 0x7E, //  Numpad . on Brazilian keyboard
            DIK_UNKNOW30 = 0x7F,

            Insert = NumPad0,
            Delete = Decimal,
            Home = NumPad7,
            End = NumPad1,
            PageUp = NumPad9,
            PageDown = NumPad3,

            Left = NumPad4,
            Right = NumPad6,
            Up = NumPad8,
            Down = NumPad2,

            Prior = PageUp,
            Next = PageDown,
            /* *
                        DIK_NUMPADEQUALS = 0x8D,    // = on numeric keypad (NEC PC98) //
                        DIK_PREVTRACK = 0x90,    // Previous Track (DIK_CIRCUMFLEX on Japanese keyboard) //
                        DIK_AT = 0x91,    //                     (NEC PC98) //
                        DIK_COLON = 0x92,    //                     (NEC PC98) //
                        DIK_UNDERLINE = 0x93, //                     (NEC PC98) //
                        DIK_KANJI = 0x94,   // (Japanese keyboard)            //
                        DIK_STOP = 0x95,    //                     (NEC PC98) //
                        DIK_AX = 0x96,    //                     (Japan AX) //
                        DIK_UNLABELED = 0x97,    //                        (J3100) //
                        DIK_NEXTTRACK = 0x99,    // Next Track //
                        DIK_NUMPADENTER = 0x9C,    // Enter on numeric keypad //
                        DIK_RCONTROL = 0x9D,
                        DIK_MUTE = 0xA0,    // Mute //
                        DIK_CALCULATOR = 0xA1,    // Calculator //
                        DIK_PLAYPAUSE = 0xA2,    // Play / Pause //
                        DIK_MEDIASTOP = 0xA4,    // Media Stop //
                        DIK_VOLUMEDOWN = 0xAE,    // Volume - //
                        DIK_VOLUMEUP = 0xB0,    // Volume + //
                        DIK_WEBHOME = 0xB2,    // Web home //
                        DIK_NUMPADCOMMA = 0xB3,    // , on numeric keypad (NEC PC98) //
                        DIK_DIVIDE = 0xB5,    // / on numeric keypad //
                        DIK_SYSRQ = 0xB7,
                        DIK_RMENU = 0xB8,    // right Alt //
                        DIK_PAUSE = 0xC5,    // Pause //
                        DIK_HOME = 0xC7,    // Home on arrow keypad //
                        DIK_UP = 0xC8,    // UpArrow on arrow keypad //
                        DIK_PRIOR = 0xC9,    // PgUp on arrow keypad //
                        DIK_LEFT = 0xCB,    // LeftArrow on arrow keypad //
                        DIK_RIGHT = 0xCD,    // RightArrow on arrow keypad //
                        DIK_END = 0xCF,    // End on arrow keypad //
                        DIK_DOWN = 0xD0,    // DownArrow on arrow keypad //
                        DIK_NEXT = 0xD1,    // PgDn on arrow keypad //
                        DIK_INSERT = 0xD2,    // Insert on arrow keypad //
                        DIK_DELETE = 0xD3,    // Delete on arrow keypad //
                        DIK_LWIN = 0xDB,    // Left Windows key //
                        DIK_RWIN = 0xDC,    // Right Windows key //
                        DIK_APPS = 0xDD,    // AppMenu key //
                        DIK_POWER = 0xDE,    // System Power //
                        DIK_SLEEP = 0xDF,    // System Sleep //
                        DIK_WAKE = 0xE3,    // System Wake //
                        DIK_WEBSEARCH = 0xE5,    // Web Search //
                        DIK_WEBFAVORITES = 0xE6,    // Web Favorites //
                        DIK_WEBREFRESH = 0xE7,    // Web Refresh //
                        DIK_WEBSTOP = 0xE8,    // Web Stop //
                        DIK_WEBFORWARD = 0xE9,    // Web Forward //
                        DIK_WEBBACK = 0xEA,    // Web Back //
                        DIK_MYCOMPUTER = 0xEB,    // My Computer //
                        DIK_MAIL = 0xEC,    // Mail //
                        DIK_MEDIASELECT = 0xED    // Media Select 
                        /* */
        }
    }
}
