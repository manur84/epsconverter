using System;
using System.Windows;

namespace EPSConverter.Dialogs
{
    public partial class ScaleDialog : Window
    {
        private bool _isUpdating = false;

        public double ScaleX { get; private set; } = 1.0;
        public double ScaleY { get; private set; } = 1.0;

        public ScaleDialog()
        {
            InitializeComponent();
        }

        private void WidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            WidthTextBox.Text = ((int)e.NewValue).ToString();

            if (MaintainAspectCheckBox.IsChecked == true)
            {
                HeightSlider.Value = e.NewValue;
                HeightTextBox.Text = ((int)e.NewValue).ToString();
            }

            UpdatePreview();
            _isUpdating = false;
        }

        private void HeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;

            _isUpdating = true;
            HeightTextBox.Text = ((int)e.NewValue).ToString();

            if (MaintainAspectCheckBox.IsChecked == true)
            {
                WidthSlider.Value = e.NewValue;
                WidthTextBox.Text = ((int)e.NewValue).ToString();
            }

            UpdatePreview();
            _isUpdating = false;
        }

        private void WidthTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (int.TryParse(WidthTextBox.Text, out int value))
            {
                _isUpdating = true;
                WidthSlider.Value = Math.Clamp(value, 10, 500);

                if (MaintainAspectCheckBox.IsChecked == true)
                {
                    HeightSlider.Value = WidthSlider.Value;
                    HeightTextBox.Text = ((int)WidthSlider.Value).ToString();
                }

                UpdatePreview();
                _isUpdating = false;
            }
        }

        private void HeightTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            if (int.TryParse(HeightTextBox.Text, out int value))
            {
                _isUpdating = true;
                HeightSlider.Value = Math.Clamp(value, 10, 500);

                if (MaintainAspectCheckBox.IsChecked == true)
                {
                    WidthSlider.Value = HeightSlider.Value;
                    WidthTextBox.Text = ((int)HeightSlider.Value).ToString();
                }

                UpdatePreview();
                _isUpdating = false;
            }
        }

        private void MaintainAspect_Changed(object sender, RoutedEventArgs e)
        {
            if (MaintainAspectCheckBox.IsChecked == true && !_isUpdating)
            {
                _isUpdating = true;
                HeightSlider.Value = WidthSlider.Value;
                HeightTextBox.Text = WidthTextBox.Text;
                UpdatePreview();
                _isUpdating = false;
            }
        }

        private void UpdatePreview()
        {
            PreviewText.Text = $"{(int)WidthSlider.Value}% × {(int)HeightSlider.Value}%";
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ScaleX = WidthSlider.Value / 100.0;
            ScaleY = HeightSlider.Value / 100.0;
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
