using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Cross_Game.DataManipulation
{
    class Screen
    {
        internal static class Display
        {
            private static int width = (int)SystemParameters.PrimaryScreenWidth;
            private static int height = (int)SystemParameters.PrimaryScreenHeight;
            private static int top = 0;
            private static int left = 0;

            public static int Width { get => width; set => width = value; }
            public static int Height { get => height; set => height = value; }
            public static int Top { get => top; set => top = value; }
            public static int Left { get => left; set => left = value; }
        }

        public static byte[] CaptureScreen()
        {
            byte[] data;
            using (Bitmap b = new Bitmap(Display.Width, Display.Height))
            {
                using (Graphics g = Graphics.FromImage(b))
                    g.CopyFromScreen(Display.Left, Display.Top, 0, 0, b.Size);
                using (MemoryStream ms = new MemoryStream())
                {
                    b.Save(ms, ImageFormat.Jpeg);
                    data = ms.ToArray();
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
    }
}
