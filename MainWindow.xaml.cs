using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using EPSConverter.Services;
using EPSConverter.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EPSConverter
{
    public partial class MainWindow : Window
    {
        private readonly EPSService _epsService;
        private readonly VectorizationService _vectorizationService;
        private readonly ImageService _imageService;
        private string? _currentFilePath;
        private readonly List<string> _loadedFiles = new();

        public MainWindow()
        {
            InitializeComponent();
            _epsService = new EPSService();
            _vectorizationService = new VectorizationService();
            _imageService = new ImageService();

            InitializeSliders();
            CheckGhostscript();
        }

        private void CheckGhostscript()
        {
            if (!_epsService.IsGhostscriptAvailable())
            {
                var result = MessageBox.Show(
                    "Ghostscript wurde nicht gefunden.\n\n" +
                    "Für die beste EPS-Darstellung wird Ghostscript empfohlen.\n" +
                    "Die Anwendung funktioniert auch ohne Ghostscript, verwendet dann aber ImageMagick oder eine Fallback-Anzeige.\n\n" +
                    "Möchten Sie mehr über die Installation von Ghostscript erfahren?",
                    "Ghostscript nicht gefunden",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show(
                        "Ghostscript kann von folgender Seite heruntergeladen werden:\n\n" +
                        "https://www.ghostscript.com/download/gsdnld.html\n\n" +
                        "Laden Sie die Windows 64-bit Version herunter und installieren Sie sie.\n" +
                        "Nach der Installation starten Sie die Anwendung neu.",
                        "Ghostscript Installation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        private void InitializeSliders()
        {
            ThresholdSlider.ValueChanged += (s, e) => ThresholdValueText.Text = ((int)e.NewValue).ToString();
            SmoothingSlider.ValueChanged += (s, e) => SmoothingValueText.Text = ((int)e.NewValue).ToString();
            DetailSlider.ValueChanged += (s, e) => DetailValueText.Text = ((int)e.NewValue).ToString();
        }

        private async void OpenEPS_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "EPS Files (*.eps)|*.eps|All Files (*.*)|*.*",
                Title = "EPS-Datei öffnen"
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadEPSFile(dialog.FileName);
            }
        }

        private async void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Bilddateien|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|Alle Dateien|*.*",
                Title = "Bilddatei öffnen",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    await LoadImageFile(fileName);
                }
            }
        }

        private async Task LoadEPSFile(string filePath)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = $"Lade EPS-Datei: {Path.GetFileName(filePath)}";

                var epsData = await _epsService.LoadEPSAsync(filePath);
                _currentFilePath = filePath;

                if (!_loadedFiles.Contains(filePath))
                {
                    _loadedFiles.Add(filePath);
                    FileListBox.Items.Add(Path.GetFileName(filePath));
                }

                // Render EPS to image for preview
                var bitmap = await _epsService.RenderToImageAsync(epsData, 300);
                PreviewImage.Source = bitmap;

                UpdateFileInfo(filePath, epsData);
                StatusText.Text = $"EPS geladen: {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der EPS-Datei: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Laden";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task LoadImageFile(string filePath)
        {
            try
            {
                ShowLoading(true);
                StatusText.Text = $"Lade Bild: {Path.GetFileName(filePath)}";

                var bitmap = await _imageService.LoadImageAsync(filePath);
                _currentFilePath = filePath;

                if (!_loadedFiles.Contains(filePath))
                {
                    _loadedFiles.Add(filePath);
                    FileListBox.Items.Add(Path.GetFileName(filePath));
                }

                PreviewImage.Source = bitmap;
                UpdateFileInfoFromImage(filePath, bitmap);
                StatusText.Text = $"Bild geladen: {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Bildes: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Fehler beim Laden";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void Vectorize_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("Bitte öffnen Sie zuerst eine Bilddatei.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                ShowLoading(true);
                StatusText.Text = "Vektorisierung läuft...";

                var settings = new VectorizationSettings
                {
                    Threshold = (int)ThresholdSlider.Value,
                    Smoothing = (int)SmoothingSlider.Value,
                    DetailLevel = (int)DetailSlider.Value,
                    ColorMode = ColorModeCheckBox.IsChecked ?? false
                };

                var epsContent = await _vectorizationService.VectorizeImageAsync(_currentFilePath, settings);

                // Preview the vectorized result
                var tempEpsPath = Path.Combine(Path.GetTempPath(), "temp_vectorized.eps");
                await File.WriteAllTextAsync(tempEpsPath, epsContent);

                var epsData = await _epsService.LoadEPSAsync(tempEpsPath);
                var bitmap = await _epsService.RenderToImageAsync(epsData, 300);
                PreviewImage.Source = bitmap;

                StatusText.Text = "Vektorisierung abgeschlossen";
                MessageBox.Show("Bild erfolgreich vektorisiert! Verwenden Sie 'Speichern' um die EPS-Datei zu sichern.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei der Vektorisierung: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Vektorisierung fehlgeschlagen";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void SaveEPS_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("Keine Datei zum Speichern geöffnet.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "EPS Files (*.eps)|*.eps",
                Title = "Als EPS speichern",
                FileName = Path.GetFileNameWithoutExtension(_currentFilePath) + ".eps"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ShowLoading(true);
                    StatusText.Text = "Speichere EPS...";

                    // If current file is an image, vectorize it first
                    if (!_currentFilePath.EndsWith(".eps", StringComparison.OrdinalIgnoreCase))
                    {
                        var settings = new VectorizationSettings
                        {
                            Threshold = (int)ThresholdSlider.Value,
                            Smoothing = (int)SmoothingSlider.Value,
                            DetailLevel = (int)DetailSlider.Value,
                            ColorMode = ColorModeCheckBox.IsChecked ?? false
                        };

                        var epsContent = await _vectorizationService.VectorizeImageAsync(_currentFilePath, settings);
                        await File.WriteAllTextAsync(dialog.FileName, epsContent);
                    }
                    else
                    {
                        File.Copy(_currentFilePath, dialog.FileName, true);
                    }

                    StatusText.Text = $"Gespeichert: {Path.GetFileName(dialog.FileName)}";
                    MessageBox.Show("EPS-Datei erfolgreich gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    StatusText.Text = "Speichern fehlgeschlagen";
                }
                finally
                {
                    ShowLoading(false);
                }
            }
        }

        private async void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewImage.Source == null)
            {
                MessageBox.Show("Kein Bild zum Exportieren verfügbar.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|BMP Image (*.bmp)|*.bmp",
                Title = "Als Bild exportieren"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _imageService.SaveImageAsync(PreviewImage.Source, dialog.FileName);
                    StatusText.Text = $"Exportiert: {Path.GetFileName(dialog.FileName)}";
                    MessageBox.Show("Bild erfolgreich exportiert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Exportieren: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void BatchConvert_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Bilddateien|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
                Title = "Bilder für Batch-Konvertierung auswählen",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                var folderDialog = new SaveFileDialog
                {
                    Title = "Zielordner auswählen (Dateiname wird ignoriert)",
                    FileName = "output"
                };

                if (folderDialog.ShowDialog() == true)
                {
                    var outputFolder = Path.GetDirectoryName(folderDialog.FileName) ?? "";
                    await PerformBatchConversion(dialog.FileNames, outputFolder);
                }
            }
        }

        private async Task PerformBatchConversion(string[] files, string outputFolder)
        {
            try
            {
                ShowLoading(true);
                int total = files.Length;
                int processed = 0;

                var settings = new VectorizationSettings
                {
                    Threshold = (int)ThresholdSlider.Value,
                    Smoothing = (int)SmoothingSlider.Value,
                    DetailLevel = (int)DetailSlider.Value,
                    ColorMode = ColorModeCheckBox.IsChecked ?? false
                };

                foreach (var file in files)
                {
                    processed++;
                    StatusText.Text = $"Verarbeite {processed}/{total}: {Path.GetFileName(file)}";

                    try
                    {
                        var epsContent = await _vectorizationService.VectorizeImageAsync(file, settings);
                        var outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(file) + ".eps");
                        await File.WriteAllTextAsync(outputPath, epsContent);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other files
                        Console.WriteLine($"Fehler bei {file}: {ex.Message}");
                    }
                }

                MessageBox.Show($"Batch-Konvertierung abgeschlossen!\n{processed} Dateien verarbeitet.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusText.Text = $"Batch-Konvertierung abgeschlossen ({processed} Dateien)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei der Batch-Konvertierung: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListBox.SelectedIndex >= 0 && FileListBox.SelectedIndex < _loadedFiles.Count)
            {
                var filePath = _loadedFiles[FileListBox.SelectedIndex];
                _ = Task.Run(async () =>
                {
                    if (filePath.EndsWith(".eps", StringComparison.OrdinalIgnoreCase))
                        await LoadEPSFile(filePath);
                    else
                        await LoadImageFile(filePath);
                });
            }
        }

        private void Scale_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Skalierungsfunktion in Entwicklung", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rotationsfunktion in Entwicklung", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Rückgängig-Funktion in Entwicklung", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Wiederholen-Funktion in Entwicklung", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Einstellungen aktualisiert";
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "EPS Converter & Editor v1.0\n\n" +
                "Eine Windows-Anwendung zum Öffnen, Bearbeiten und Vektorisieren von EPS-Dateien.\n\n" +
                "Features:\n" +
                "• EPS-Dateien öffnen und anzeigen\n" +
                "• Bilder in EPS-Vektoren konvertieren\n" +
                "• Batch-Konvertierung\n" +
                "• Einstellbare Vektorisierungsparameter\n\n" +
                "© 2024 EPS Converter",
                "Über EPS Converter",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ShowLoading(bool show)
        {
            LoadingPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateFileInfo(string filePath, EPSData epsData)
        {
            FileNameText.Text = $"Datei: {Path.GetFileName(filePath)}";
            FileSizeText.Text = $"Größe: {new FileInfo(filePath).Length / 1024} KB";
            DimensionsText.Text = $"Abmessungen: {epsData.Width:F1} × {epsData.Height:F1} pt";

            // Parse and display additional EPS information
            var parser = new PostScriptParser();
            try
            {
                var psDoc = parser.Parse(epsData.Content ?? "");
                var summary = parser.GetSummary(psDoc);

                // Update status with parsing info
                if (!string.IsNullOrEmpty(summary))
                {
                    StatusText.Text = $"EPS geladen: {psDoc.Paths.Count} Pfade, {psDoc.TextElements.Count} Texte, {psDoc.Colors.Count} Farben";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Parser error: {ex.Message}");
            }

            // Show Ghostscript availability warning
            if (!_epsService.IsGhostscriptAvailable())
            {
                StatusText.Text += " | ⚠️ Ghostscript nicht installiert - eingeschränkte Vorschau";
            }
        }

        private void UpdateFileInfoFromImage(string filePath, BitmapSource bitmap)
        {
            FileNameText.Text = $"Datei: {Path.GetFileName(filePath)}";
            FileSizeText.Text = $"Größe: {new FileInfo(filePath).Length / 1024} KB";
            DimensionsText.Text = $"Abmessungen: {bitmap.PixelWidth} x {bitmap.PixelHeight}";
        }
    }
}
