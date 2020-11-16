using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace App1003
{
    public sealed partial class Fader : UserControl
    {
        private int channel;
        private string label;
        private Color color;
        private byte val = 0;
        private byte tmpVal = 0;
        private BytePair limit = new BytePair(0, 255);
        private byte limitBacklight = 0x26;

        public int Channel
        {
            get => channel;
            set
            {
                channel = value;
                channelText.Text = "Channel " + channel.ToString();
            }
        }

        public string Label
        {
            get => label;
            set
            {
                label = value;
                labelText.Text = label;
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                SolidColorBrush brush = new SolidColorBrush(color);
                channelText.Foreground = brush;
                labelText.Foreground = brush;
                slider.Foreground = brush;

                brush = new SolidColorBrush(Color.FromArgb(0xFF, (byte)(brush.Color.R / 3), (byte)(brush.Color.G / 3), (byte)(brush.Color.B / 3)));

                slider.Background = brush;
            }
        }

        public byte Max => limit.Greater;

        public byte Min => limit.Less;
        public BytePair Limit
        {
            get => limit;
            set => OnLimitChanged(value);
        }

        public double Value
        {
            get => val;
            set => OnValueChanged((byte)value);
        }

        public event LimitChangedHandler LimitChanged;

        public event ValueChangedHandler ValueChanged;

        private void OnLimitChanged(BytePair newLimit)
        {
            LimitChanged?.Invoke(this, new LimitChangedEventArgs(newLimit));
            limit = newLimit;
            nud.Limit = limit;
            DrawLimit();
        }

        private void OnValueChanged(byte newValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(newValue));

            val = newValue > limit.Greater ? limit.Greater : (newValue < limit.Less ? limit.Less : newValue);
            nud.Value = val;
        }

        public Fader()
        {
            this.InitializeComponent();

            flashButton.AddHandler(PointerPressedEvent, new PointerEventHandler(FlashButton_PointerPressed), true);
            flashButton.AddHandler(PointerReleasedEvent, new PointerEventHandler(FlashButton_PointerReleased), true);

            Value = limit.Less;
            DrawLimit();
        }

        public double StartFlash()
        {
            tmpVal = val;
            Value = limit.Greater;
            return 0;
        }

        public double StopFlash() => Value = tmpVal;

        private void Add(byte amount) => Value = (Value + amount > limit.Greater ? limit.Greater : (byte)(Value + amount));

        private void Sub(byte amount) => Value = (Value - amount < limit.Less ? limit.Less : (byte)(Value - amount));

        private void DrawLimit()
        {
            if (limit.Equals(new BytePair(0, 255)))
                limiter.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            else
            {
                limiter.Background = new SolidColorBrush(Color.FromArgb(limitBacklight, 0xFF, 0, 0));

                limiter.Children.Add(new Rectangle()
                {
                    Fill = new SolidColorBrush(Color.FromArgb(0xff, 0, limitBacklight, 0)),
                    Width = 24,
                    Height = limit.Difference / 2,
                    Translation = new Vector3(0, 127 - limit.Greater / 2, 0)
                });

                limiter.Children.Add(new Border()
                {
                    Width = 24,
                    Height = 128,
                    BorderThickness = new Thickness(2),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(0xff, limitBacklight, limitBacklight, 0))
                });
            } 
        }

        private void NumericBox_ValueChanged(object sender, ValueChangedEventArgs e) => slider.Value = e.Value;
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => Value = (byte)e.NewValue;
        private void OpenButton_Click(object sender, RoutedEventArgs e) => Value = limit.Greater;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Value = limit.Less;
        private void Slider_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(slider).Properties.MouseWheelDelta > 0)
                Add(3);
            else
                Sub(3);
        }
        private void FlashButton_PointerPressed(object sender, PointerRoutedEventArgs e) => StartFlash();
        private void FlashButton_PointerReleased(object sender, PointerRoutedEventArgs e) => StopFlash();
    }
}
