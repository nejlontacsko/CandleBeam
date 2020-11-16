using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace App1003
{
    public sealed partial class NumericBox : UserControl
    {
        private byte val = 0;
        private BytePair limit = new BytePair(0, 255);

        private bool scissors = false;
        public byte Value
        {
            get => val;
            set => OnValueChanged(value);
        }

        public BytePair Limit
        {
            get => limit;
            set => Value = Limit.Greater < Value ? Limit.Greater : (Limit.Less > Value ? Limit.Less : Value);
        }

        public byte Max => limit.Greater;

        public byte Min => limit.Less;

        public event ValueChangedHandler ValueChanged;

        private void OnValueChanged(byte newValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(newValue));

            if (scissors)
                scissors = false;
            else
            {
                val = newValue > limit.Greater ? limit.Greater : (newValue < limit.Less ? limit.Less : newValue);
                scissors = true;
                txtBox.Text = val.ToString();
            }
        }

        public NumericBox() => this.InitializeComponent();

        private void Add(int amount) =>
            Value = (Value + amount > limit.Greater ? limit.Greater : (byte)(Value + amount));

        private void Sub(int amount) =>
            Value = (Value - amount < limit.Less ? limit.Less : (byte)(Value - amount));

        private void addValue(object sender, RoutedEventArgs e) => Add(1);

        private void subValue(object sender, RoutedEventArgs e) => Sub(1);

        private void TxtBox_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(txtBox).Properties.MouseWheelDelta > 0)
                Add(3);
            else
                Sub(3);
        }

        private void TxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            byte result;

            if (byte.TryParse(((TextBox)sender).Text, out result))
                if (!scissors)
                    Value = result;
            else
                txtBox.Text = val.ToString();
        }
    }
}
