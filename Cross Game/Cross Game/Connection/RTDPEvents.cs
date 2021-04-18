using System;

namespace Cross_Game.Connection
{
    public delegate void ReceivedBufferEventHandler(object sender, ReceivedBufferEventArgs e);
    public class ReceivedBufferEventArgs : EventArgs
    {
        public byte[] Buffer;
        public int BufferSize;

        public ReceivedBufferEventArgs(byte[] buffer, int bufferSize)
        {
            Buffer = buffer;
            BufferSize = bufferSize;
        }
    }

    public delegate void ImageBuiltEventHandler(object sender, ImageBuiltEventArgs e);
    public class ImageBuiltEventArgs : EventArgs
    {
        public byte[] Image;

        public ImageBuiltEventArgs(byte[] image)
        {
            Image = image;
        }
    }

    public delegate void CursorShangedEventHandler(object sender, CursorShangedEventArgs e);
    public class CursorShangedEventArgs : EventArgs
    {
        public CursorShape CursorShape;

        public CursorShangedEventArgs(CursorShape cursorShape)
        {
            CursorShape = cursorShape;
        }
    }
}
