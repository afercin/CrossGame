﻿using System;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ToggleSwitch;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para Slider.xaml
    /// </summary>
    public partial class WaitSlider : UserControl
    {
        public bool Active { get => active; set => active = value; }

        private readonly Brush blue = new SolidColorBrush(Color.FromRgb(48, 149, 191));
        private readonly Brush black = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        private readonly Brush gray = new SolidColorBrush(Color.FromRgb(179, 179, 179));
        private readonly Brush darkgray = new SolidColorBrush(Color.FromRgb(230, 230, 230));
        private readonly Storyboard leftAnimation;
        private readonly Storyboard rightAnimation;

        private bool active;
        private ThreadStart activeAction;
        private ThreadStart deactiveAction;
        private Thread actionThread;


        public WaitSlider()
        {
            InitializeComponent();
            activeAction = null;
            deactiveAction = null;
            actionThread = null;

            var duration = new Duration(TimeSpan.FromSeconds(0.15));

            var toLeft = new ThicknessAnimation();
            var toRight = new ThicknessAnimation();
            leftAnimation = new Storyboard();
            rightAnimation = new Storyboard();
            
            leftAnimation.Duration = rightAnimation.Duration = toRight.Duration = toLeft.Duration = duration;
            toRight.From = toLeft.To = new Thickness(34, 4, 4, 4);
            toRight.To = toLeft.From = new Thickness(4);

            leftAnimation.Children.Add(toLeft);
            Storyboard.SetTarget(leftAnimation, slider);
            Storyboard.SetTargetProperty(leftAnimation, new PropertyPath(MarginProperty));

            rightAnimation.Children.Add(toRight);
            Storyboard.SetTarget(rightAnimation, slider);
            Storyboard.SetTargetProperty(rightAnimation, new PropertyPath(MarginProperty));
        }

        public void SetActions(ThreadStart active, ThreadStart deactive)
        {
            activeAction = active;
            deactiveAction = deactive;
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (deactiveAction == null || activeAction == null)
                throw new Exception();
            Progress.Visibility = Visibility.Visible;
            if (actionThread == null)
            {
                actionThread = new Thread(() => WaitAction(Active ? deactiveAction : activeAction));
                actionThread.IsBackground = true;
                actionThread.Start();

                if (Active)
                    rightAnimation.Begin();
                else
                    leftAnimation.Begin();
            }
        }

        private void WaitAction(ThreadStart action)
        {
            Thread t = new Thread(action);
            t.IsBackground = true;
            t.Start();
            t.Join();

            active = !active;

            Dispatcher.Invoke(() =>
            {
                BackgroundBorder.Background = active ? blue : black;
                slider.Fill = active ? darkgray : gray;
                Progress.Visibility = Visibility.Hidden;
            });
            actionThread = null;
        }
    }
}
