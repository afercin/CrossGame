using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Cross_Game.DataManipulation
{
    /// <summary>
    /// Clase para trabajar con capturas de pantallas como array de bytes.
    /// </summary>
    class Screen
    {
        /// <summary>
        /// Clase de apoyo que guarda información relativa a la ventana a ser capturada.
        /// </summary>
        internal static class Display
        {
            /// <summary>
            /// Handler a la ventana que acaba de ser capturada.
            /// </summary>
            public static IntPtr Handle { get; set; } = IntPtr.Zero;
            /// <summary>
            /// Ancho de la ventana que acaba de ser capturada.
            /// </summary>
            public static int Width { get; set; } = (int)SystemParameters.PrimaryScreenWidth;
            /// <summary>
            /// Alto de la ventana que acaba de ser capturada.
            /// </summary>
            public static int Height { get; set; } = (int)SystemParameters.PrimaryScreenHeight;
        }
        /// <summary>
        /// Realiza una captura de pantalla y devuelve la imagen en un array de bytes.
        /// </summary>
        public static byte[] CaptureScreen()
        {
            byte[] data;
            using (Bitmap b = CaptureDesktop())
            {
                using (Bitmap resizedImg = new Bitmap(1024, 576))
                {
                    using (Graphics g = Graphics.FromImage(resizedImg))
                        g.DrawImage(b, 0, 0, resizedImg.Width, resizedImg.Height);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resizedImg.Save(ms, ImageFormat.Jpeg);
                        data = ms.ToArray();
                    }
                }
            }
            return data;
        }
        /// <summary>
        /// Devuelve la imagen correspondiente al array de bytes pasado por parametro.
        /// </summary>
        public static BitmapImage BytesToScreenImage(byte[] imageBytes)
        {
            BitmapImage bitmap = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            return bitmap;
        }
        /// <summary>
        /// Detecta la ventana que se está ejecutando en primer plano y le realiza una captura de pantalla.
        /// </summary>
        private static Bitmap CaptureDesktop()
        {
            IntPtr currentView = Win32API.GetDesktopWindow();

            if (Display.Handle != currentView)
            {
                Display.Handle = currentView;
                RECT windowRect = new RECT();

                Win32API.GetWindowRect(Display.Handle, ref windowRect);
                Display.Width = windowRect.right - windowRect.left;
                Display.Height = windowRect.bottom - windowRect.top;
            }

            IntPtr hdcSrc = Win32API.GetWindowDC(Display.Handle),
                   hdcDest = Win32API.CreateCompatibleDC(hdcSrc),
                   hBitmap = Win32API.CreateCompatibleBitmap(hdcSrc, Display.Width, Display.Height),
                   hOld = Win32API.SelectObject(hdcDest, hBitmap);

            Win32API.BitBlt(hdcDest, 0, 0, Display.Width, Display.Height, hdcSrc, 0, 0, Win32API.SRCCOPY);
            Win32API.SelectObject(hdcDest, hOld);

            Win32API.DeleteDC(hdcDest);
            Win32API.ReleaseDC(Display.Handle, hdcSrc);

            Bitmap img = Image.FromHbitmap(hBitmap);
            Win32API.DeleteObject(hBitmap);

            return img;
        }
        /// <summary>
        /// Devuelve el handler del escritorio o de la ventana con la que se está trabajando si está en pantalla completa.
        /// </summary>
        private static IntPtr GetCurrentView()
        {
            RECT a = new RECT(),
                 b = new RECT();

            IntPtr foreground = Win32API.GetForegroundWindow(),
                   desktop = Win32API.GetDesktopWindow();

            Win32API.GetWindowRect(foreground, ref b);
            Win32API.GetWindowRect(desktop, ref a);

            return a.left == b.left && a.right == b.right &&
                   a.top == b.top && a.bottom == b.bottom ? foreground : desktop;
        }
    }
}
