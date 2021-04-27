using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Cross_Game.DataManipulation
{
    class Screen
    {
        internal static class Display
        {
            public static IntPtr Handle { get; set; } = IntPtr.Zero;
            public static int Width { get; set; } = (int)SystemParameters.PrimaryScreenWidth;
            public static int Height { get; set; } = (int)SystemParameters.PrimaryScreenHeight;
        }

        public static byte[] CaptureScreen()
        {
            byte[] data;
            using (Bitmap b = CaptureWindow(WIN32_API.GetDesktopWindow()))
            {
                using (Bitmap resizedImg = new Bitmap(1280, 720))
                {
                    using (Graphics g = Graphics.FromImage(resizedImg))
                        g.DrawImage(b, 0, 0, 1280, 720);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resizedImg.Save(ms, ImageFormat.Jpeg);
                        data = ms.ToArray();
                    }
                }
            }
            return data;
        }

        public static BitmapImage BytesToScreenImage(byte[] data)
        {
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(data))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            return bitmap;
        }
        private static Bitmap CaptureWindow(IntPtr handle)
        {
            if (Display.Handle != handle)
            {
                Display.Handle = handle;
                WIN32_API.RECT windowRect = new WIN32_API.RECT();

                WIN32_API.GetWindowRect(Display.Handle, ref windowRect);
                Display.Width = windowRect.right - windowRect.left;
                Display.Height = windowRect.bottom - windowRect.top;
            }

            IntPtr hdcSrc = WIN32_API.GetWindowDC(Display.Handle);

            IntPtr hdcDest = WIN32_API.CreateCompatibleDC(hdcSrc),
                   hBitmap = WIN32_API.CreateCompatibleBitmap(hdcSrc, Display.Width, Display.Height),
                   hOld = WIN32_API.SelectObject(hdcDest, hBitmap);

            WIN32_API.BitBlt(hdcDest, 0, 0, Display.Width, Display.Height, hdcSrc, 0, 0, WIN32_API.SRCCOPY);
            WIN32_API.SelectObject(hdcDest, hOld);

            WIN32_API.DeleteDC(hdcDest);
            WIN32_API.ReleaseDC(Display.Handle, hdcSrc);

            Bitmap img = Image.FromHbitmap(hBitmap);
            WIN32_API.DeleteObject(hBitmap);

            return img;
        }

        private class WIN32_API
        {
            #region User_32

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            #endregion

            #region GDI_32

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

            #endregion
        }
    }
}
