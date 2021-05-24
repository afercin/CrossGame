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

    public delegate void GotClientCredentialsEventHandler(object sender, GotClientCredentialsEventArgs e);
    public class GotClientCredentialsEventArgs : EventArgs
    {
        public int UserPriority { get; set; }

        public string email;
        public string password;
        public string localIP;
        public string publicIP;
        public string mac;

        public GotClientCredentialsEventArgs(string[] clientInfo)
        {
            email = clientInfo[0];
            password = clientInfo[1];
            localIP = clientInfo[2];
            publicIP = clientInfo[3];
            mac = clientInfo[4];
        }
    }
}
