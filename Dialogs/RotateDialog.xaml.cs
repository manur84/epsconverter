using System;
using System.Windows;

namespace EPSConverter.Dialogs
{
    public partial class RotateDialog : Window
    {
        private bool _isUpdating = false;

        public double Angle { get; private set; } = 0.0;

        public RotateDialog()
        {
            InitializeComponent();
        }

        private void AngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            AngleTextBox.Text = ((int)e.NewValue).ToString();
            UpdatePreview(e.NewValue);
            _isUpdating = false;
        }

        private void AngleTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (double.TryParse(AngleTextBox.Text, out double value))
            {
                _isUpdating = true;
                AngleSlider.Value = Math.Clamp(value, -180, 180);
                UpdatePreview(value);
                _isUpdating = false;
            }
        }

        private void Rotate90Left_Click(object sender, RoutedEventArgs e)
        {
            AngleSlider.Value = -90;
        }

        private void Rotate90Right_Click(object sender, RoutedEventArgs e)
        {
            AngleSlider.Value = 90;
        }

        private void Rotate180_Click(object sender, RoutedEventArgs e)
        {
            AngleSlider.Value = 180;
        }

        private void UpdatePreview(double angle)
        {
            PreviewText.Text = $"{angle:F0}°";
            PreviewRotation.Angle = angle;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Angle = AngleSlider.Value;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
