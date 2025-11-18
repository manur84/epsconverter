# Build-Anleitung für EPS Converter

Diese Anleitung zeigt Ihnen, wie Sie das EPS Converter Projekt kompilieren und ausführen.

## Voraussetzungen

### Software-Anforderungen
- **.NET 8.0 SDK** (oder höher)
  - Download: https://dotnet.microsoft.com/download
- **Windows 10/11** (WPF ist Windows-spezifisch)
- Optional: **Visual Studio 2022** oder **Visual Studio Code**

### Überprüfen Sie Ihre .NET Installation
```bash
dotnet --version
```
Sollte `8.0.x` oder höher anzeigen.

## Build-Optionen

### Option 1: Kommandozeilen-Build

#### 1. Dependencies installieren
```bash
cd /path/to/epsconverter
dotnet restore
```

#### 2. Debug-Build erstellen
```bash
dotnet build
```

#### 3. Release-Build erstellen
```bash
dotnet build -c Release
```

#### 4. Anwendung ausführen
```bash
# Debug-Version
dotnet run

# oder Release-Version
dotnet run -c Release
```

### Option 2: Standalone Executable erstellen

Für eine verteilbare .exe-Datei:

```bash
# Für 64-bit Windows
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Für 32-bit Windows
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true
```

Die ausführbare Datei finden Sie unter:
```
bin/Release/net8.0-windows/win-x64/publish/EPSConverter.exe
```

### Option 3: Mit Visual Studio

1. Öffnen Sie `EPSConverter.csproj` in Visual Studio 2022
2. Wählen Sie die Build-Konfiguration (Debug/Release)
3. Drücken Sie `F5` zum Debuggen oder `Ctrl+Shift+B` zum Bauen
4. Die Anwendung startet automatisch

## Projekt-Struktur

```
EPSConverter/
├── EPSConverter.csproj        # NuGet-Pakete und Projekt-Konfiguration
├── App.xaml / App.xaml.cs     # Anwendungs-Einstiegspunkt
├── MainWindow.xaml / .cs      # Hauptfenster
├── Models/                     # Datenmodelle
│   ├── EPSData.cs
│   └── VectorizationSettings.cs
└── Services/                   # Business-Logik
    ├── EPSService.cs
    ├── ImageService.cs
    └── VectorizationService.cs
```

## NuGet-Pakete

Das Projekt verwendet folgende externe Bibliotheken:

- **SkiaSharp** (2.88.7) - Grafik-Rendering
- **SkiaSharp.Views.WPF** (2.88.7) - WPF-Integration
- **Emgu.CV** (4.9.0) - OpenCV Wrapper für Bildverarbeitung
- **Emgu.CV.runtime.windows** (4.9.0) - Windows-spezifische OpenCV Runtime
- **CommunityToolkit.Mvvm** (8.2.2) - MVVM-Helper

Diese werden automatisch mit `dotnet restore` heruntergeladen.

## Troubleshooting

### Problem: "The specified framework 'Microsoft.NETCore.App' version '8.0.x' was not found"
**Lösung**: Installieren Sie das .NET 8.0 SDK von https://dotnet.microsoft.com/download

### Problem: NuGet-Pakete können nicht wiederhergestellt werden
**Lösung**:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Problem: Emgu.CV-Fehler beim Ausführen
**Lösung**: Stellen Sie sicher, dass die OpenCV-Runtime-Dateien korrekt kopiert wurden:
```bash
dotnet clean
dotnet restore
dotnet build
```

### Problem: WPF-Anwendung startet nicht
**Lösung**: Überprüfen Sie, ob Sie auf Windows sind. WPF läuft nur auf Windows.

## Build-Konfigurationen

### Debug-Modus
- Optimierungen deaktiviert
- Debug-Symbole enthalten
- Einfacheres Debugging
```bash
dotnet build -c Debug
```

### Release-Modus
- Optimierungen aktiviert
- Kleinere Dateigröße
- Bessere Performance
```bash
dotnet build -c Release
```

## Erweiterte Optionen

### Spezifische Target-Frameworks
```bash
dotnet build --framework net8.0-windows
```

### Mit detaillierter Ausgabe
```bash
dotnet build -v detailed
```

### Paralleles Build deaktivieren
```bash
dotnet build /m:1
```

## Installation und Verteilung

### Installer erstellen (optional)

Für professionelle Distribution können Sie Tools wie:
- **Inno Setup** - https://jrsoftware.org/isinfo.php
- **WiX Toolset** - https://wixtoolset.org/
- **ClickOnce** - Eingebaut in Visual Studio

verwenden, um einen Windows Installer zu erstellen.

### Portable Version

Die mit `dotnet publish --self-contained true` erstellte .exe ist bereits portabel und benötigt keine .NET Runtime-Installation auf dem Zielsystem.

## Performance-Tipps

1. **Release-Build verwenden** für bessere Performance
2. **ReadyToRun aktivieren** für schnelleren Start:
   ```xml
   <PublishReadyToRun>true</PublishReadyToRun>
   ```
3. **Trimming aktivieren** für kleinere Dateigröße:
   ```xml
   <PublishTrimmed>true</PublishTrimmed>
   ```

## Continuous Integration

### GitHub Actions Beispiel
```yaml
name: Build

on: [push]

jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build -c Release
    - name: Test
      run: dotnet test
```

## Lizenz

MIT License - siehe LICENSE Datei

---

Bei Problemen oder Fragen, öffnen Sie bitte ein Issue auf GitHub.
