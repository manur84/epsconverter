using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using EPSConverter.Services;
using EPSConverter.Models;
using EPSConverter.Dialogs;
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
        private readonly ImageTransformService _transformService;
        private readonly UndoRedoManager _undoRedoManager;

        private string? _currentFilePath;
        private readonly List<string> _loadedFiles = new();
        private ImageState _currentImageState = new();

        // Zoom and Pan
        private double _zoomLevel = 1.0;
        private Point? _lastMousePos;
        private bool _isPanning = false;

        public MainWindow()
        {
            InitializeComponent();
            _epsService = new EPSService();
            _vectorizationService = new VectorizationService();
            _imageService = new ImageService();
            _transformService = new ImageTransformService();
            _undoRedoManager = new UndoRedoManager();

            InitializeSliders();
            InitializeKeyBindings();
            InitializeUndoRedo();
            CheckGhostscript();
        }

        private void InitializeUndoRedo()
        {
            _undoRedoManager.StateChanged += (s, e) =>
            {
                UndoMenuItem.IsEnabled = _undoRedoManager.CanUndo;
                RedoMenuItem.IsEnabled = _undoRedoManager.CanRedo;
            };

            UndoMenuItem.IsEnabled = false;
            RedoMenuItem.IsEnabled = false;
        }

        private void InitializeKeyBindings()
        {
            // Undo: Ctrl+Z
            var undoGesture = new KeyGesture(Key.Z, ModifierKeys.Control);
            var undoBinding = new KeyBinding(new RelayCommand(Undo_Click), undoGesture);
            InputBindings.Add(undoBinding);

            // Redo: Ctrl+Y
            var redoGesture = new KeyGesture(Key.Y, ModifierKeys.Control);
            var redoBinding = new KeyBinding(new RelayCommand(Redo_Click), redoGesture);
            InputBindings.Add(redoBinding);

            // Scale: Ctrl+T
            var scaleGesture = new KeyGesture(Key.T, ModifierKeys.Control);
            var scaleBinding = new KeyBinding(new RelayCommand(Scale_Click), scaleGesture);
            InputBindings.Add(scaleBinding);

            // Rotate: Ctrl+R
            var rotateGesture = new KeyGesture(Key.R, ModifierKeys.Control);
            var rotateBinding = new KeyBinding(new RelayCommand(Rotate_Click), rotateGesture);
            InputBindings.Add(rotateBinding);
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

        private async void Scale_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewImage.Source == null)
            {
                MessageBox.Show("Bitte öffnen Sie zuerst eine Datei.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new ScaleDialog();
            if (dialog.ShowDialog() == true)
            {
                await ApplyScaleTransform(dialog.ScaleX, dialog.ScaleY);
            }
        }

        private async Task ApplyScaleTransform(double scaleX, double scaleY)
        {
            if (PreviewImage.Source is not BitmapSource source) return;

            try
            {
                ShowLoading(true);
                StatusText.Text = "Skaliere Bild...";

                var oldImage = source;
                var newImage = await _transformService.ScaleImageAsync(source, scaleX, scaleY);

                var command = new TransformCommand(
                    execute: () => PreviewImage.Source = newImage,
                    undo: () => PreviewImage.Source = oldImage
                );

                _undoRedoManager.ExecuteCommand(command);
                StatusText.Text = $"Bild skaliert: {scaleX * 100:F0}% × {scaleY * 100:F0}%";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Skalieren: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async void Rotate_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewImage.Source == null)
            {
                MessageBox.Show("Bitte öffnen Sie zuerst eine Datei.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new RotateDialog();
            if (dialog.ShowDialog() == true)
            {
                await ApplyRotationTransform(dialog.Angle);
            }
        }

        private async Task ApplyRotationTransform(double angle)
        {
            if (PreviewImage.Source is not BitmapSource source) return;

            try
            {
                ShowLoading(true);
                StatusText.Text = "Drehe Bild...";

                var oldImage = source;
                var newImage = await _transformService.RotateImageSkiaAsync(source, angle);

                var command = new TransformCommand(
                    execute: () => PreviewImage.Source = newImage,
                    undo: () => PreviewImage.Source = oldImage
                );

                _undoRedoManager.ExecuteCommand(command);
                StatusText.Text = $"Bild gedreht: {angle:F0}°";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Drehen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            _undoRedoManager.Undo();
            StatusText.Text = "Rückgängig";
        }

        private void Undo_Click(object? parameter)
        {
            if (parameter is RoutedEventArgs args)
                Undo_Click(this, args);
            else
                _undoRedoManager.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            _undoRedoManager.Redo();
            StatusText.Text = "Wiederholen";
        }

        private void Redo_Click(object? parameter)
        {
            if (parameter is RoutedEventArgs args)
                Redo_Click(this, args);
            else
                _undoRedoManager.Redo();
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

        // Zoom and Pan Functions
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = Math.Min(_zoomLevel * 1.2, 10.0);
            ApplyZoom();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = Math.Max(_zoomLevel / 1.2, 0.1);
            ApplyZoom();
        }

        private void ZoomFit_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewImage.Source == null) return;

            var viewportWidth = ImageScrollViewer.ViewportWidth;
            var viewportHeight = ImageScrollViewer.ViewportHeight;
            var imageWidth = PreviewImage.Source.Width;
            var imageHeight = PreviewImage.Source.Height;

            var scaleX = viewportWidth / imageWidth;
            var scaleY = viewportHeight / imageHeight;
            _zoomLevel = Math.Min(scaleX, scaleY) * 0.95;

            ApplyZoom();
        }

        private void ZoomActual_Click(object sender, RoutedEventArgs e)
        {
            _zoomLevel = 1.0;
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            ScaleTransform.ScaleX = _zoomLevel;
            ScaleTransform.ScaleY = _zoomLevel;
            ZoomText.Text = $"{_zoomLevel * 100:F0}%";
        }

        private void ImageContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Delta > 0)
                    ZoomIn_Click(sender, new RoutedEventArgs());
                else
                    ZoomOut_Click(sender, new RoutedEventArgs());

                e.Handled = true;
            }
        }

        private void ImageContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _lastMousePos = e.GetPosition(ImageScrollViewer);
            _isPanning = true;
            ImageContainer.Cursor = Cursors.SizeAll;
            ImageContainer.CaptureMouse();
        }

        private void ImageContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            ImageContainer.Cursor = Cursors.Hand;
            ImageContainer.ReleaseMouseCapture();
        }

        private void ImageContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning && _lastMousePos.HasValue)
            {
                var currentPos = e.GetPosition(ImageScrollViewer);
                var delta = currentPos - _lastMousePos.Value;

                ImageScrollViewer.ScrollToHorizontalOffset(ImageScrollViewer.HorizontalOffset - delta.X);
                ImageScrollViewer.ScrollToVerticalOffset(ImageScrollViewer.VerticalOffset - delta.Y);

                _lastMousePos = currentPos;
            }
        }

        // Flip Functions
        private async void FlipHorizontal_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFlip(true, false);
        }

        private async void FlipVertical_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFlip(false, true);
        }

        private async Task ApplyFlip(bool horizontal, bool vertical)
        {
            if (PreviewImage.Source is not BitmapSource source) return;

            try
            {
                ShowLoading(true);
                StatusText.Text = "Spiegle Bild...";

                var oldImage = source;
                var newImage = await _transformService.FlipImageAsync(source, horizontal, vertical);

                var command = new TransformCommand(
                    execute: () => PreviewImage.Source = newImage,
                    undo: () => PreviewImage.Source = oldImage
                );

                _undoRedoManager.ExecuteCommand(command);
                StatusText.Text = horizontal ? "Horizontal gespiegelt" : "Vertikal gespiegelt";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Spiegeln: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // Filter Functions
        private async void FilterGrayscale_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFilter("grayscale");
        }

        private async void FilterSepia_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFilter("sepia");
        }

        private async void FilterInvert_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFilter("invert");
        }

        private async void FilterBlur_Click(object sender, RoutedEventArgs e)
        {
            await ApplyFilter("blur");
        }

        private async Task ApplyFilter(string filterType)
        {
            if (PreviewImage.Source is not BitmapSource source) return;

            try
            {
                ShowLoading(true);
                StatusText.Text = $"Wende {filterType}-Filter an...";

                var oldImage = source;
                var newImage = await _transformService.ApplyFilterAsync(source, filterType);

                var command = new TransformCommand(
                    execute: () => PreviewImage.Source = newImage,
                    undo: () => PreviewImage.Source = oldImage
                );

                _undoRedoManager.ExecuteCommand(command);
                StatusText.Text = $"Filter angewendet: {filterType}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Anwenden des Filters: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        // Brightness/Contrast Dialog
        private async void BrightnessContrast_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewImage.Source == null)
            {
                MessageBox.Show("Bitte öffnen Sie zuerst eine Datei.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new BrightnessContrastDialog();
            if (dialog.ShowDialog() == true)
            {
                await ApplyBrightnessContrast(dialog.Brightness, dialog.Contrast);
            }
        }

        private async Task ApplyBrightnessContrast(double brightness, double contrast)
        {
            if (PreviewImage.Source is not BitmapSource source) return;

            try
            {
                ShowLoading(true);
                StatusText.Text = "Passe Helligkeit/Kontrast an...";

                var oldImage = source;
                var newImage = await _transformService.AdjustBrightnessContrastAsync(source, brightness, contrast);

                var command = new TransformCommand(
                    execute: () => PreviewImage.Source = newImage,
                    undo: () => PreviewImage.Source = oldImage
                );

                _undoRedoManager.ExecuteCommand(command);
                StatusText.Text = $"Helligkeit/Kontrast angepasst";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei Helligkeit/Kontrast: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }
    }
}
