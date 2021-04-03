using System.Windows;
using System.Windows.Controls;

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para TransmissionOptions.xaml
    /// </summary>
    public partial class TransmissionOptions : UserControl
    {
        public TransmissionOptions()
        {
            InitializeComponent();
        }

        public bool Accept()
        {
            bool restart = TcpPort.Value != ConnectionUtils.TcpPort || UdpPort.Value != ConnectionUtils.UdpPort;

            ConnectionUtils.TcpPort = TcpPort.Value;
            ConnectionUtils.UdpPort = UdpPort.Value;
            ConnectionUtils.Delay = 1000 / FPS.Value;

            return restart;
        }
    }
}
