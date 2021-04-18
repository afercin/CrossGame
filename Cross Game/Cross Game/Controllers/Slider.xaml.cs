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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para Slider.xaml
    /// </summary>
    public partial class Slider : UserControl
    {
        private bool beginProcess;
        private bool clicked;

        public bool BeginProcess { get => beginProcess; set => beginProcess = value; }
        public bool Clicked { get => clicked; set => clicked = value; }

        public Slider()
        {
            InitializeComponent();
        }

        private void BackgroundBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void BackgroundBorder_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void BackgroundBorder_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}
