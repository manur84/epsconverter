# EPS Converter & Editor

Eine umfassende Windows-Desktop-Anwendung zum Öffnen, Bearbeiten und Vektorisieren von EPS-Dateien (Encapsulated PostScript).

## 🎯 Features

### Hauptfunktionen
- ✅ **EPS-Dateien öffnen und anzeigen** - Professionelles Rendering mit Ghostscript/ImageMagick
- ✅ **Bildvektorisierung** - Konvertierung von Rasterbildern in Vektorgrafiken
- ✅ **Bildbearbeitung** - Skalieren, Drehen, Spiegeln, Filter, Helligkeit/Kontrast
- ✅ **Zoom & Pan** - Mausrad-Zoom (Strg+Scroll) und Drag-to-Pan
- ✅ **Undo/Redo** - Unbegrenzte Rückgängig/Wiederholen-Funktion (Strg+Z/Strg+Y)
- ✅ **Filter** - Graustufen, Sepia, Invertieren, Weichzeichnen
- ✅ **EPS/Bild-Export** - Speichern in EPS, PNG, JPG, BMP
- ✅ **Batch-Konvertierung** - Mehrere Bilder gleichzeitig verarbeiten
- ✅ **PostScript-Parser** - Automatische Metadaten-Extraktion

### Bearbeitungs-Funktionen
- **Skalieren** (Strg+T): Größe ändern mit Seitenverhältnis-Option
- **Drehen** (Strg+R): Freie Rotation oder 90°/180° Schnelldrehung
- **Spiegeln**: Horizontal oder vertikal
- **Helligkeit/Kontrast**: Feinabstimmung der Bildqualität
- **Filter**: Graustufen, Sepia, Invertieren, Weichzeichnen

### Vektorisierungs-Einstellungen
- **Schwellenwert** (0-255): Steuert die Schwarz/Weiß-Konvertierung
- **Glättung** (0-10): Reduziert Bildrauschen vor der Vektorisierung
- **Detail-Level** (1-20): Bestimmt die Genauigkeit der Konturerkennung
- **Farbmodus**: Aktiviert/Deaktiviert Farbvektorisierung

## 🚀 Installation

### Voraussetzungen
- Windows 10/11
- .NET 8.0 Runtime oder höher
- **Empfohlen**: Ghostscript (für optimale EPS-Darstellung)

### Ghostscript Installation (Optional aber empfohlen)

Für die beste EPS-Rendering-Qualität sollten Sie Ghostscript installieren:

1. Besuchen Sie https://www.ghostscript.com/download/gsdnld.html
2. Laden Sie "Ghostscript 10.x for Windows (64 bit)" herunter
3. Führen Sie das Installationsprogramm aus
4. Starten Sie die EPS Converter Anwendung neu

**Hinweis**: Die Anwendung funktioniert auch ohne Ghostscript, verwendet dann aber ImageMagick als Fallback oder zeigt eine Platzhalter-Vorschau an.

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
3. Die Datei wird im Hauptfenster gerendert und angezeigt
4. Im Eigenschaften-Panel sehen Sie Dateiinformationen und Metadaten
5. Die Statusleiste zeigt Anzahl der Pfade, Textelemente und verwendeten Farben

**Rendering-Hierarchie:**
- **Beste Qualität**: Ghostscript (wenn installiert)
- **Gute Qualität**: ImageMagick (integrierter Fallback)
- **Basis-Vorschau**: Platzhalter mit Metadaten (wenn weder Ghostscript noch ImageMagick verfügbar)

### Bild vektorisieren
1. Klicken Sie auf **"🖼️ Bild Öffnen"** oder wählen Sie **Datei → Bild öffnen...**
2. Wählen Sie ein Bild (PNG, JPG, BMP, GIF, TIFF)
3. Passen Sie die Vektorisierungs-Einstellungen im rechten Panel an:
   - **Schwellenwert**: Höhere Werte = mehr Weiß
   - **Glättung**: Höhere Werte = glattere Kanten
   - **Detail-Level**: Höhere Werte = mehr Details
4. Klicken Sie auf **"🔄 Vektorisieren"**
5. Speichern Sie das Ergebnis mit **"💾 Speichern"**

### Bild bearbeiten
1. Öffnen Sie ein Bild oder EPS
2. Verwenden Sie die Bearbeitungs-Funktionen:
   - **Skalieren** (Strg+T): Größe ändern mit einstellbarem Seitenverhältnis
   - **Drehen** (Strg+R): Freie Rotation oder Schnelldrehung (90°, 180°)
   - **Spiegeln**: Horizontal oder vertikal
   - **Filter**: Graustufen, Sepia, Invertieren, Weichzeichnen
   - **Helligkeit/Kontrast**: Feinabstimmung der Bildqualität
3. Alle Änderungen können mit **Rückgängig** (Strg+Z) wiederhergestellt werden

### Zoom & Navigation
- **Zoom In**: 🔍+ Button oder Strg+Mausrad nach oben
- **Zoom Out**: 🔍- Button oder Strg+Mausrad nach unten
- **Zoom anpassen**: ⬜ Button passt Bild an Fenster an
- **100% Zoom**: 1:1 Button
- **Pan/Verschieben**: Linke Maustaste gedrückt halten und ziehen

### Batch-Konvertierung
1. Wählen Sie **Vektorisieren → Batch-Konvertierung...**
2. Wählen Sie mehrere Bilddateien aus
3. Wählen Sie einen Zielordner
4. Die Anwendung verarbeitet alle Dateien automatisch

### Tastenkombinationen
- **Strg+Z**: Rückgängig
- **Strg+Y**: Wiederholen
- **Strg+T**: Skalieren
- **Strg+R**: Drehen
- **Strg+Mausrad**: Zoom

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
    ├── EPSService.cs         # EPS-Verarbeitung mit Ghostscript/ImageMagick
    ├── ImageService.cs       # Bild-Verarbeitung
    ├── PostScriptParser.cs   # PostScript/EPS Parser
    └── VectorizationService.cs   # Vektorisierungs-Engine
```

## 🔧 Technologie-Stack

- **Framework**: .NET 8.0 / WPF (Windows Presentation Foundation)
- **UI**: XAML mit modernem Design
- **EPS-Rendering**:
  - Ghostscript.NET - Professionelles PostScript/EPS Rendering
  - Magick.NET (ImageMagick) - Fallback-Rendering
  - SkiaSharp - Grafik-Operationen und Platzhalter-Rendering
- **Bildverarbeitung**:
  - Emgu.CV (OpenCV Wrapper) - Konturerkennung und Bildanalyse
  - SkiaSharp - 2D-Grafik-Rendering
- **PostScript-Verarbeitung**:
  - Eigener PostScript-Parser für Metadaten-Extraktion
- **Design Pattern**: MVVM (Model-View-ViewModel)

## 🎨 Features im Detail

### EPS-Verarbeitung
Die Anwendung kann EPS-Dateien einlesen und verarbeitet diese in mehreren Schritten:

**1. Datei-Parsing:**
- BoundingBox und HiResBoundingBox Extraktion
- Dokumentendimensionen (Breite × Höhe in Punkten)
- Metadaten (Titel, Creator, Erstellungsdatum)
- PostScript-Befehle und Pfade
- Textelemente und verwendete Farben

**2. Rendering:**
- **Ghostscript**: Konvertiert PostScript in hochqualitative Rasterbilder
- **ImageMagick**: Alternative Rendering-Engine als Fallback
- **Platzhalter-Modus**: Zeigt Metadaten wenn kein Renderer verfügbar

**3. Analyse:**
- Automatische Extraktion von Vektorpfaden
- Erkennung von Textelementen
- Farbpaletten-Analyse (RGB, CMYK, Graustufen)

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
- Sehr große Bilder können längere Verarbeitungszeiten haben
- Einige spezielle PostScript-Operatoren werden vom Parser nicht erkannt

## 🔧 Troubleshooting

### Ghostscript-Probleme

**Problem: "Ghostscript nicht gefunden"**
- **Lösung**: Installieren Sie Ghostscript von https://www.ghostscript.com/download/gsdnld.html
- Stellen Sie sicher, dass Sie die 64-bit Version installieren
- Nach der Installation: Anwendung neu starten

**Problem: EPS wird nicht korrekt angezeigt**
- **Lösung 1**: Überprüfen Sie, ob Ghostscript installiert ist
- **Lösung 2**: Wenn Ghostscript installiert ist, versuchen Sie ImageMagick als Fallback
- **Lösung 3**: Die Platzhalter-Ansicht zeigt zumindest Metadaten und Dimensionen

### Rendering-Probleme

**Problem: Vorschau zeigt nur Platzhalter**
- Die Anwendung verwendet drei Rendering-Methoden in dieser Reihenfolge:
  1. Ghostscript (beste Qualität)
  2. ImageMagick (gute Qualität, integriert)
  3. Platzhalter (Metadaten-Anzeige)
- Installieren Sie Ghostscript für beste Ergebnisse

**Problem: Vektorisierung schlägt fehl**
- Überprüfen Sie, ob das Bild beschädigt ist
- Versuchen Sie verschiedene Schwellenwert-Einstellungen
- Reduzieren Sie das Detail-Level für komplexe Bilder

### Performance-Probleme

**Problem: Langsame Verarbeitung großer Bilder**
- Reduzieren Sie die DPI-Einstellung (z.B. von 600 auf 300)
- Verwenden Sie niedrigere Detail-Level-Einstellungen
- Schließen Sie andere Anwendungen während der Verarbeitung

## 🔮 Geplante Features

- [x] Erweiterte Bearbeitungswerkzeuge (Skalieren, Drehen, Spiegeln) ✅
- [x] Undo/Redo-Funktionalität ✅
- [x] Zoom- und Pan-Funktionen im Canvas ✅
- [x] Integration von Ghostscript für besseres EPS-Rendering ✅
- [x] PostScript-Parser für Metadaten-Extraktion ✅
- [x] Bildfilter (Graustufen, Sepia, Invertieren, Weichzeichnen) ✅
- [x] Helligkeit/Kontrast-Anpassung ✅
- [ ] Ebenen-System
- [ ] Erweiterte Farbpaletten-Verwaltung
- [ ] Zuschneiden-Werkzeug
- [ ] SVG-Import/Export
- [ ] PDF-Export
- [ ] Vektortext-Extraktion aus Bildern (OCR)
- [ ] Direktes Bearbeiten von Vektorpfaden
- [ ] Verlaufseditor

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

- **Ghostscript** - PostScript und PDF Interpreter
- **Ghostscript.NET** - .NET Wrapper für Ghostscript
- **ImageMagick / Magick.NET** - Bildverarbeitungs-Bibliothek
- **Emgu.CV** - OpenCV Wrapper für .NET
- **SkiaSharp** - Cross-platform 2D Graphics Library
- **WPF** - Windows Presentation Foundation Team

---

**Hinweis**: Diese Anwendung wurde entwickelt, um eine benutzerfreundliche Lösung für EPS-Konvertierung und Bildvektorisierung unter Windows bereitzustellen.

## 📋 Changelog

### Version 2.0.0 (Aktuell)
- ✅ **Vollständige Bildbearbeitung**: Skalieren, Drehen, Spiegeln
- ✅ **Undo/Redo-System**: Unbegrenzte Rückgängig/Wiederholen-Funktionalität
- ✅ **Zoom & Pan**: Mausrad-Zoom mit Strg+Scroll und Drag-to-Pan
- ✅ **Bildfilter**: Graustufen, Sepia, Invertieren, Weichzeichnen
- ✅ **Helligkeit/Kontrast**: Professionelle Bildanpassung
- ✅ **Tastenkombinationen**: Strg+Z, Strg+Y, Strg+T, Strg+R
- ✅ **Interaktive Dialoge**: Skalieren, Drehen, Helligkeit/Kontrast mit Live-Vorschau
- ✅ **Zoom-Toolbar**: Schnellzugriff auf Zoom-Funktionen
- ✅ **Transform-Service**: Professionelle Bildtransformationen mit SkiaSharp

### Version 1.1.0
- ✅ Ghostscript-Integration für professionelles EPS-Rendering
- ✅ PostScript-Parser für Metadaten-Extraktion
- ✅ Multi-Level Rendering-System (Ghostscript → ImageMagick → Fallback)
- ✅ Erweiterte EPS-Informationsanzeige (Pfade, Texte, Farben)
- ✅ Automatische Ghostscript-Erkennung beim Start
- ✅ Verbesserte Fehlerbehandlung und Fallback-Mechanismen

### Version 1.0.0
- Initiale Version mit grundlegenden Features
- EPS-Dateien öffnen und anzeigen
- Bildvektorisierung
- Batch-Konvertierung

**Version**: 2.0.0
**Letztes Update**: November 2024
