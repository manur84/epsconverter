using System;
using System.Windows;

namespace EPSConverter.Dialogs
{
    public partial class BrightnessContrastDialog : Window
    {
        private bool _isUpdating = false;

        public double Brightness { get; private set; } = 0;
        public double Contrast { get; private set; } = 0;

        public BrightnessContrastDialog()
        {
            InitializeComponent();
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            BrightnessTextBox.Text = ((int)e.NewValue).ToString();
            UpdatePreview();
            _isUpdating = false;
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            ContrastTextBox.Text = ((int)e.NewValue).ToString();
            UpdatePreview();
            _isUpdating = false;
        }

        private void BrightnessTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (int.TryParse(BrightnessTextBox.Text, out int value))
            {
                _isUpdating = true;
                BrightnessSlider.Value = Math.Clamp(value, -100, 100);
                UpdatePreview();
                _isUpdating = false;
            }
        }

        private void ContrastTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (int.TryParse(ContrastTextBox.Text, out int value))
            {
                _isUpdating = true;
                ContrastSlider.Value = Math.Clamp(value, -100, 100);
                UpdatePreview();
                _isUpdating = false;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            BrightnessSlider.Value = 0;
            ContrastSlider.Value = 0;
        }

        private void UpdatePreview()
        {
            PreviewText.Text = $"Helligkeit: {(int)BrightnessSlider.Value}, Kontrast: {(int)ContrastSlider.Value}";
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Brightness = BrightnessSlider.Value;
            Contrast = ContrastSlider.Value;
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
