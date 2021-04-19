using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public WaitingWindow()
        {
            InitializeComponent();
        }

        private void Window_SourceInitialized(object sender, EventArgs e) => Header.SetWindowHandler(this);

        public void SetText(string text)
        {
            LogText.Text = text;
        }

        private void Header_MenuButtonClick(object sender, ClickEventArgs e)
        {
            Close();
        }
    }
}
