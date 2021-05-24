using System;

namespace RTDP
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

    public delegate void ReconectingEventHandler(object sender, ReconnectingEventArgs e);
    public class ReconnectingEventArgs : EventArgs
    {
        public bool Reconnecting;

        public ReconnectingEventArgs(bool reconnecting)
        {
            Reconnecting = reconnecting;
        }
    }

    public delegate void CursorChangedEventHandler(object sender, CursorChangedEventArgs e);
    public class CursorChangedEventArgs : EventArgs
    {
        public CursorShape CursorShape;

        public CursorChangedEventArgs(CursorShape cursorShape)
        {
            CursorShape = cursorShape;
        }
    }
}
