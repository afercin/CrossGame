using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Cross_Game.Client
{
    class KeyboardHook : IDisposable
    {
        private static IntPtr hookId = IntPtr.Zero;
        private readonly InterceptKeys.LowLevelKeyboardProc callback;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                return HookCallbackInner(nCode, wParam, lParam);
            }
            catch
            {
                Console.WriteLine("There was some error somewhere...");
            }
            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private IntPtr HookCallbackInner(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)InterceptKeys.WM_KEYDOWN)
                    KeyPress(this, new KeyEventArgs(Marshal.ReadInt32(lParam), true));
                else if (wParam == (IntPtr)InterceptKeys.WM_KEYUP)
                    KeyPress(this, new KeyEventArgs(Marshal.ReadInt32(lParam), false));
            }
            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public event KeyEventHandler KeyPress;

        public KeyboardHook()
        {
            callback = HookCallback;
            hookId = InterceptKeys.SetHook(callback);
        }

        ~KeyboardHook()
        {
            Dispose();
        }

        public void Dispose()
        {
            InterceptKeys.UnhookWindowsHookEx(hookId);
        }
    }

    internal static class InterceptKeys
    {
        public delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        public static int WH_KEYBOARD_LL = 13;
        public static int WM_KEYDOWN = 0x0100;
        public static int WM_KEYUP = 0x0101;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public class KeyEventArgs : EventArgs
    {
        public int VKCode;
        public Key Key;
        public bool IsKeyDown;

        public KeyEventArgs(int VKCode, bool isKeyDown)
        {
            this.VKCode = VKCode;
            this.IsKeyDown = isKeyDown;
            this.Key = KeyInterop.KeyFromVirtualKey(VKCode);
        }
    }

    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

}
