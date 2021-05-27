using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cross_Game.Controllers
{
    /// <summary>
    /// Lógica de interacción para Slider.xaml
    /// </summary>
    public partial class WaitSlider : UserControl
    {
        public bool Active { get => active; set => active = value; }
        public bool Done { get; set; }

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

        public delegate void Action();

        public void SetActions(Action active, Action deactive)
        {
            activeAction = new ThreadStart(active);
            deactiveAction = new ThreadStart(deactive);
        }

        public void Slider_MouseDown(object sender, MouseButtonEventArgs e)
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
                BackgroundBorder.Background = active && Done ? CrossGameUtils.BlueBrush : CrossGameUtils.BlackBrush;
                slider.Fill = active ? CrossGameUtils.WhiteBrush : CrossGameUtils.LightGrayBrush;
                Progress.Visibility = Visibility.Hidden;
            });
            actionThread = null;
        }
    }
}
