using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cross_Game.Windows
{
    /// <summary>
    /// Lógica de interacción para Waiting_Window.xaml
    /// </summary>
    public partial class WaitingWindow : Window
    {
        public event EventHandler WaitStopped;
        public event EventHandler WaitEnd;

        private Thread waitThread;

        public WaitingWindow()
        {
            InitializeComponent();
            waitThread = null;
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        public delegate void Action();
        public void Wait(Action waitTask, string waitText)
        {
            WaitText.Text = waitText;
            waitThread = new Thread(()=>
            {
                Thread t = new Thread(new ThreadStart(waitTask));
                t.IsBackground = true;
                t.Start();
                t.Join();
                if (WaitEnd != null)
                    WaitEnd.Invoke(null, null);
                Dispatcher.Invoke(() => Close());
            });
            waitThread.IsBackground = true;
            waitThread.Start();
            ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StopWaiting();
        }

        private void Header_MenuButtonClick(object sender, ClickEventArgs e)
        {
            StopWaiting();
        }

        private void StopWaiting()
        {
            waitThread.Abort();
            if (WaitStopped != null)
                WaitStopped.Invoke(null, null);
            Close();
        }
    }
}
