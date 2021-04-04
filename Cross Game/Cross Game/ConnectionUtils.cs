using System.IO;
using System.IO.Compression;

namespace Cross_Game
{
    /* 
     * Dimensiones: 0% --> 100% (simple precisión)
     * 
     *             8 bits                                  32 bits                                                32 bits
     * ╔════════════════════════════╦══════════════════════════════════════════════════════╦══════════════════════════════════════════════════════╗
     * ║           Action           ║                     xPosition                        ║                     yPosition                        ║ = 9 bytes
     * ╚════════════════════════════╩══════════════════════════════════════════════════════╩══════════════════════════════════════════════════════╝
     * 
     * 
     * Teclas de un teclado: 105 ==> 128 = 2^7 = 7 bits
     * 
     *             8 bits                       8 bits
     * ╔════════════════════════════╦═══════════════════════════╗
     * ║           Action           ║            keyP           ║ = 2 bytes
     * ╚════════════════════════════╩═══════════════════════════╝
     *
     *
     * Otros eventos:
     * 
     *             8 bits
     * ╔════════════════════════════╗
     * ║           Action           ║ = 1 byte
     * ╚════════════════════════════╝
     */

    public enum Actions
    {
        MouseMove           = 0x00,
        MouseLButtonUp      = 0x01,
        MouseLButtonDown    = 0x02,
        MouseRButtonUp      = 0x03,
        MouseRButtonDown    = 0x04,
        MouseMButtonUp      = 0x05,
        MouseMButtonDown    = 0x06,
        MouseWheel          = 0x07,
        KeyboardKeyUp       = 0x08,
        KeyboardKeyDown     = 0x09,
        CursorChanged       = 0x0A,

        EmulatorInfo        = 0x80,
        GameInfo            = 0x81,

        DecreaseDelay       = 0xFC,
        IncreaseDelay       = 0xFD,
        QuitGame            = 0xFE,
        EndConnetion        = 0xFF
    }
    
    public enum CursorShape
    {
        None        = 0x00,
        Arrow       = 0x03,
        IBeam       = 0x05,
        ArrowCD     = 0x07,
        Cross       = 0x15,
        SizeNWSE    = 0x0D,
        SizeNESW    = 0x0F,
        SizeWE      = 0x11,
        SizeNS      = 0x13,
        Wait        = 0x19,
        Hand        = 0x1F
    }

    class ConnectionUtils
    {
        public static readonly int MaxPacketSize = 65507;
        //public static int MaxPacketSize = 65527;

        public static int TcpPort;
        public static int UdpPort;
        public static int Delay;

        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        public static readonly string[] LogHeader =
        {
            ".%%..%%.%%%%%%.%%%%%..%%%%%%........%%...%%.%%%%%%.........%%%%...%%%%..........%%%%...%%%%...%%%%..%%%%%%.%%..%%.",
            ".%%..%%.%%.....%%..%%.%%............%%...%%.%%............%%.....%%..%%........%%..%%.%%.....%%..%%...%%...%%%.%%.",
            ".%%%%%%.%%%%...%%%%%..%%%%..........%%.%.%%.%%%%..........%%.%%%.%%..%%........%%%%%%.%%.%%%.%%%%%%...%%...%%.%%%.",
            ".%%..%%.%%.....%%..%%.%%............%%%%%%%.%%............%%..%%.%%..%%........%%..%%.%%..%%.%%..%%...%%...%%..%%.",
            ".%%..%%.%%%%%%.%%..%%.%%%%%%.........%%.%%..%%%%%%.........%%%%...%%%%.........%%..%%..%%%%..%%..%%.%%%%%%.%%..%%."
        };
        public static readonly string[] LogFooter =
        {
            "::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::",
            "··················································································································"
        };

        public static void WriteLog(string filename, string[] text)
        {
            File.AppendAllLines(filename, text);
        }
    }
}
