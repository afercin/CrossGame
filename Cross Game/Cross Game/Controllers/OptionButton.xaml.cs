using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para OptionButton.xaml
    /// </summary>
    public partial class OptionButton : UserControl
    {
        public bool Active
        {
            get => (bool)GetValue(ActivePropertyChanged);
            set
            {
                SetValue(ActivePropertyChanged, value);
                BackColor.BeginAnimation(OpacityProperty, Active ? ActiveAnimation : InactiveAnimation);
            }
        }

        public PackIconMaterialKind IconType
        {
            get => (PackIconMaterialKind)GetValue(IconTypePropertyChanged);
            set => SetValue(IconTypePropertyChanged, value);
        }

        public static readonly DependencyProperty ActivePropertyChanged = DependencyProperty.Register("Active", typeof(bool), typeof(OptionButton), new PropertyMetadata(false));
        public static readonly DependencyProperty IconTypePropertyChanged = DependencyProperty.Register("IconType", typeof(PackIconMaterialKind), typeof(OptionButton), new PropertyMetadata(PackIconMaterialKind.Cog));

        private readonly DoubleAnimation ActiveAnimation;
        private readonly DoubleAnimation InactiveAnimation;
        private readonly DoubleAnimation OverAnimation;

        public OptionButton()
        {
            InitializeComponent();
            TimeSpan time = TimeSpan.FromSeconds(0.20);
            OverAnimation = new DoubleAnimation(0.25, time);
            InactiveAnimation = new DoubleAnimation(0, time);
            ActiveAnimation = new DoubleAnimation(0.75, time);
        }

        private void Option_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Active)
                BackColor.BeginAnimation(OpacityProperty, OverAnimation);
        }

        private void Option_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Active)
                BackColor.BeginAnimation(OpacityProperty, InactiveAnimation);
        }

        private void Option_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Active = true;
        }
    }
}
