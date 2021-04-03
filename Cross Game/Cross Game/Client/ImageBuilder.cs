using System;
using System.Collections.Generic;

namespace Cross_Game.Client
{
    class ImageBuilder
    {
        private readonly Dictionary<int, byte[]> images;
        private readonly Dictionary<int, int> offsets;

        public ImageBuilder()
        {
            images = new Dictionary<int, byte[]>();
            offsets = new Dictionary<int, int>();
        }

        public byte[] Build(byte[] data, int recv)
        {
            try
            {
                if (recv == 5 && data[0] < 60) // Nueva imagen
                {
                    images[data[0]] = new byte[BitConverter.ToInt32(data, 1)];
                    offsets[data[0]] = 0;
                }
                else // Reconstruir imagen
                {
                    int img = data[0];

                    Array.Copy(data, 1, images[img], offsets[img], recv - 1);
                    offsets[img] += data.Length - 1;

                    if (offsets[img] >= images[img].Length)
                        return images[img];
                }
            }
            catch { }
            return null;
        }
    }
}
