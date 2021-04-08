﻿using System;
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
    /// Lógica de interacción para OptionButton.xaml
    /// </summary>
    public partial class OptionButton : UserControl
    {
        public bool Active
        {
            get => (bool)GetValue(ValuePropertyChanged);
            set
            {
                SetValue(ValuePropertyChanged, value);
                OverEffect.Opacity = value ? 1.0 : 0.0;
            }
        }

        public static readonly DependencyProperty ValuePropertyChanged = DependencyProperty.Register("Active", typeof(bool), typeof(OptionButton), new PropertyMetadata(false));

        public OptionButton()
        {
            InitializeComponent();
        }

        private void Option_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Active)
                OverEffect.Opacity = 0.5;
            BackColor.Opacity = 0.5;
        }

        private void Option_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Active)
                OverEffect.Opacity = 0.0;
            BackColor.Opacity = 0.0;
        }

        private void Option_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Active = true;
            BackColor.Opacity = 1.0;
        }

        private void Option_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BackColor.Opacity = 0.5;
        }
    }
}
