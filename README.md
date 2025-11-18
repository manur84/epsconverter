# EPS Converter & Editor

Eine umfassende Windows-Desktop-Anwendung zum Öffnen, Bearbeiten und Vektorisieren von EPS-Dateien (Encapsulated PostScript).

## 🎯 Features

### Hauptfunktionen
- ✅ **EPS-Dateien öffnen und anzeigen** - Importieren und Betrachten von EPS-Dokumenten
- ✅ **Bildvektorisierung** - Konvertierung von Rasterbildern (PNG, JPG, BMP, etc.) in Vektorgrafiken
- ✅ **EPS-Export** - Speichern von Projekten als EPS-Dateien
- ✅ **Bildexport** - Exportieren in gängige Bildformate (PNG, JPG, BMP)
- ✅ **Batch-Konvertierung** - Mehrere Bilder gleichzeitig verarbeiten
- ✅ **Anpassbare Einstellungen** - Feinabstimmung der Vektorisierungsparameter

### Vektorisierungs-Einstellungen
- **Schwellenwert** (0-255): Steuert die Schwarz/Weiß-Konvertierung
- **Glättung** (0-10): Reduziert Bildrauschen vor der Vektorisierung
- **Detail-Level** (1-20): Bestimmt die Genauigkeit der Konturerkennung
- **Farbmodus**: Aktiviert/Deaktiviert Farbvektorisierung

## 🚀 Installation

### Voraussetzungen
- Windows 10/11
- .NET 8.0 Runtime oder höher

### Von Quellcode bauen

1. **Repository klonen:**
   ```bash
   git clone https://github.com/manur84/epsconverter.git
   cd epsconverter
   ```

2. **Projekt bauen:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Anwendung starten:**
   ```bash
   dotnet run
   ```

### Release-Version erstellen
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Die ausführbare Datei finden Sie unter: `bin/Release/net8.0-windows/win-x64/publish/EPSConverter.exe`

## 📖 Verwendung

### EPS-Datei öffnen
1. Klicken Sie auf **"📁 EPS Öffnen"** oder wählen Sie **Datei → EPS öffnen...**
2. Wählen Sie eine `.eps` Datei aus
3. Die Datei wird im Hauptfenster angezeigt

### Bild vektorisieren
1. Klicken Sie auf **"🖼️ Bild Öffnen"** oder wählen Sie **Datei → Bild öffnen...**
2. Wählen Sie ein Bild (PNG, JPG, BMP, GIF, TIFF)
3. Passen Sie die Vektorisierungs-Einstellungen im rechten Panel an:
   - **Schwellenwert**: Höhere Werte = mehr Weiß
   - **Glättung**: Höhere Werte = glattere Kanten
   - **Detail-Level**: Höhere Werte = mehr Details
4. Klicken Sie auf **"🔄 Vektorisieren"**
5. Speichern Sie das Ergebnis mit **"💾 Speichern"**

### Batch-Konvertierung
1. Wählen Sie **Vektorisieren → Batch-Konvertierung...**
2. Wählen Sie mehrere Bilddateien aus
3. Wählen Sie einen Zielordner
4. Die Anwendung verarbeitet alle Dateien automatisch

### Tastenkombinationen
- **Strg+O**: EPS öffnen
- **Strg+S**: Speichern
- **Strg+E**: Exportieren
- **Strg+V**: Vektorisieren

## 🏗️ Projektstruktur

```
EPSConverter/
├── EPSConverter.csproj       # Projekt-Konfiguration
├── App.xaml                   # Anwendungs-Definition
├── App.xaml.cs               # Anwendungs-Logik
├── MainWindow.xaml           # Hauptfenster UI
├── MainWindow.xaml.cs        # Hauptfenster Logik
├── Models/
│   ├── EPSData.cs            # EPS-Daten-Modell
│   └── VectorizationSettings.cs  # Einstellungs-Modell
└── Services/
    ├── EPSService.cs         # EPS-Verarbeitung
    ├── ImageService.cs       # Bild-Verarbeitung
    └── VectorizationService.cs   # Vektorisierungs-Engine
```

## 🔧 Technologie-Stack

- **Framework**: .NET 8.0 / WPF (Windows Presentation Foundation)
- **UI**: XAML mit modernem Design
- **Bildverarbeitung**:
  - Emgu.CV (OpenCV Wrapper) - Konturerkennung und Bildanalyse
  - SkiaSharp - Rendering und Grafik-Operationen
- **Design Pattern**: MVVM (Model-View-ViewModel)

## 🎨 Features im Detail

### EPS-Verarbeitung
Die Anwendung kann EPS-Dateien einlesen und parst automatisch:
- BoundingBox-Informationen
- Dokumentendimensionen
- PostScript-Befehle

### Vektorisierungs-Algorithmus
1. **Vorverarbeitung**: Graustufenkonvertierung und optionale Glättung
2. **Schwellenwert-Anwendung**: Binäre Bildkonvertierung
3. **Konturerkennung**: Erkennung von Objektgrenzen mit OpenCV
4. **Kontur-Approximation**: Reduzierung der Punktanzahl basierend auf Detail-Level
5. **EPS-Generierung**: Konvertierung der Konturen in PostScript-Befehle
6. **Farbextraktion**: Optionale Farbinformation aus Originalbild

### Unterstützte Formate

**Import:**
- EPS (Encapsulated PostScript)
- PNG (Portable Network Graphics)
- JPG/JPEG (Joint Photographic Experts Group)
- BMP (Bitmap)
- GIF (Graphics Interchange Format)
- TIFF (Tagged Image File Format)

**Export:**
- EPS (Encapsulated PostScript)
- PNG
- JPG
- BMP

## 🐛 Bekannte Einschränkungen

- Komplexe EPS-Dateien mit erweiterten PostScript-Features werden möglicherweise nicht vollständig unterstützt
- Die Vorschau verwendet eine vereinfachte Darstellung (für vollständiges Rendering wird Ghostscript empfohlen)
- Sehr große Bilder können längere Verarbeitungszeiten haben

## 🔮 Geplante Features

- [ ] Erweiterte Bearbeitungswerkzeuge (Skalieren, Drehen, Zuschneiden)
- [ ] Undo/Redo-Funktionalität
- [ ] Ebenen-System
- [ ] Erweiterte Farbpaletten-Verwaltung
- [ ] Integration von Ghostscript für besseres EPS-Rendering
- [ ] SVG-Import/Export
- [ ] PDF-Export
- [ ] Vektortext-Extraktion aus Bildern (OCR)

## 📄 Lizenz

Dieses Projekt ist Open Source und steht unter der MIT-Lizenz.

## 👥 Mitwirken

Beiträge sind willkommen! Bitte öffnen Sie ein Issue oder einen Pull Request für:
- Bug-Fixes
- Neue Features
- Dokumentationsverbesserungen
- Performance-Optimierungen

## 📞 Support

Bei Fragen oder Problemen:
- Öffnen Sie ein [GitHub Issue](https://github.com/manur84/epsconverter/issues)
- Kontaktieren Sie den Entwickler

## 🙏 Danksagungen

- **Emgu.CV** - OpenCV Wrapper für .NET
- **SkiaSharp** - Cross-platform 2D Graphics Library
- **WPF** - Windows Presentation Foundation Team

---

**Hinweis**: Diese Anwendung wurde entwickelt, um eine benutzerfreundliche Lösung für EPS-Konvertierung und Bildvektorisierung unter Windows bereitzustellen.

**Version**: 1.0.0
**Letztes Update**: November 2024
