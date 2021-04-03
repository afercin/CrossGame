using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cross_Game
{
    /// <summary>
    /// Lógica de interacción para NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private int step;
        private int maximum;
        private int minimum;

        public int Step
        {
            get => step;
            set => step = value != 0 ? value : 1;
        }

        public int Maximum
        {
            get => maximum;
            set
            {
                maximum = value;
                if (maximum < Value)
                    Value = maximum;
            }
        }

        public int Minimum
        {
            get => minimum;
            set
            {
                minimum = value > maximum ? maximum : value;
                if (Value < minimum)
                    Value = minimum;
            }
        }

        public int Value
        {
            get => (int)GetValue(ValuePropertyChanged);
            set => SetValue(ValuePropertyChanged, value < minimum ? minimum : value > maximum ? maximum : value);
        }

        public static readonly DependencyProperty ValuePropertyChanged = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));

        public NumericUpDown()
        {
            InitializeComponent();
            step = 1;
            maximum = int.MaxValue;
            minimum = 0;
        }

        private static bool IsTextAllowed(string text) => _regex.IsMatch(text);

        private void NumericBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = IsTextAllowed(e.Text);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "Decrement": Value -= step; break;
                case "Increment": Value += step; break;
            }
        }

        private void NumericBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NumericBox.Text == string.Empty || int.Parse(NumericBox.Text) < minimum)
            {
                NumericBox.Text = minimum.ToString();
                NumericBox.CaretIndex = NumericBox.Text.Length;
            }
            if (int.Parse(NumericBox.Text) > maximum)
            {
                NumericBox.Text = maximum.ToString();
                NumericBox.CaretIndex = NumericBox.Text.Length;
            }
        }
    }
}
