# TT_Website

Vereinsportal fuer den TSV 1883 Bogen Tischtennis.

Die Anwendung ist eine ASP.NET Core Blazor Web App mit Adminbereich, SQLite-Datenbank, bearbeitbaren Seiteninhalten, News, Galerie, Sponsoren, Dokumenten, Formularen und myTischtennis-Anbindung.

## Voraussetzungen fuer Entwicklung

- .NET 10 SDK
- Visual Studio Code oder Visual Studio
- Git optional

Fuer die fertige Windows-Publish-ZIP ist keine IDE notwendig.

## Projekt aus dem Quellcode starten

### Variante 1: Repository klonen

```bash
git clone <repo-url>
cd TT_Website
dotnet restore
dotnet build
dotnet run
```

### Variante 2: ZIP von GitHub herunterladen

1. ZIP entpacken.
2. Terminal im Projektordner oeffnen, also im Ordner mit `TT_Website.csproj`.
3. Projekt starten:

```bash
dotnet restore
dotnet build
dotnet run
```

Danach die URL oeffnen, die im Terminal bei `Now listening on` angezeigt wird.

## Fertige Publish-ZIP starten

Fuer eine Weitergabe ohne IDE kann ein self-contained Windows-x64-Publish erstellt werden. Im fertigen Publish-Ordner liegt eine Datei:

```txt
Start-TT_Website.bat
```

Ablauf fuer Empfaenger:

1. Publish-ZIP entpacken.
2. `Start-TT_Website.bat` doppelklicken.
3. Die Website oeffnet sich unter `http://localhost:5287`.

Das Konsolenfenster muss offen bleiben, solange die Website laufen soll. Beim ersten Start wird die SQLite-Datenbank automatisch im entpackten Ordner erstellt.

## Publish neu erstellen

```bash
dotnet publish -c Release -r win-x64 --self-contained true -o artifacts/TT_Website_win-x64
```

Danach kann der Inhalt von `artifacts/TT_Website_win-x64` gezippt werden. Der Ordner `artifacts/` ist absichtlich vom Projekt und von Git ausgeschlossen.

## Admin Login

Standardpasswort:

```txt
admin123
```

Das Passwort steht fuer den einfachen lokalen Start in `appsettings.json`. Fuer eine echte Veroeffentlichung sollte es geaendert und nicht oeffentlich dokumentiert werden.

## Hauptfunktionen

- Oeffentliche Vereinsseiten mit bearbeitbaren Text-Cards
- Adminbereich fuer Seiteninhalte, Galeriegruppen, News, Sponsoren, Dokumente und Einstellungen
- Aufnahmeerklaerung und Stammdaten-Aenderung mit E-Mail-Versand
- Mitgliederentwicklung als Diagramm mit Adminpflege
- Mannschaftsseiten, Vereinsrangliste und News-Anbindung an myTischtennis
- Galerie- und Sponsorendarstellung mit Uploads
- Kontaktseite mit E-Mail-Button, Google Maps und Instagram-Link
- Impressum und Datenschutzerklaerung
- Responsive Layout fuer Desktop, Laptop, Tablet und Handy

## Datenbank

Die Anwendung verwendet SQLite.

Die Datenbank wird beim ersten Start automatisch ueber EF-Core-Migrationen erstellt. Die Datenbankdatei wird lokal erzeugt und nicht ins Repository eingecheckt.

Vorgabe in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=tt_website.db"
}
```

Der Ordner `Migrations/` gehoert zum Quellcode und muss im Repository bleiben.

## E-Mail-Konfiguration

SMTP-Zugangsdaten werden nicht im Repository gespeichert.

`appsettings.json` enthaelt nur leere Platzhalter:

```json
"EmailSettings": {
  "SmtpServer": "",
  "SmtpPort": "587",
  "FromEmail": "",
  "ToEmail": "",
  "Username": "",
  "Password": ""
}
```

Fuer lokale Tests kann eine eigene `appsettings.Development.json` angelegt werden. Diese Datei ist durch `.gitignore` ausgeschlossen und darf echte lokale Zugangsdaten enthalten.

Windows:

```bash
copy appsettings.Development.example.json appsettings.Development.json
```

macOS/Linux:

```bash
cp appsettings.Development.example.json appsettings.Development.json
```

Bei Gmail wird ein App-Passwort benoetigt. In der einfachsten Konfiguration koennen `FromEmail`, `ToEmail` und `Username` auf dieselbe Vereinsmail gesetzt werden.

## VS Code Debugging

Im Repository sind `.vscode/launch.json` und `.vscode/tasks.json` enthalten, damit der Debug-Button in VS Code direkt funktioniert.

Wichtig:

- Projektroot oeffnen, also den Ordner mit der `.csproj`-Datei.
- Nicht nur einen Unterordner oeffnen.
- Falls Debugging nicht funktioniert, zuerst `dotnet run` im Terminal testen.

## Projektstruktur

```txt
Components/Pages      Blazor-Seiten
Components/Shared     Wiederverwendbare Komponenten
Components/Layout     Layout und Navigation
Services              Datenzugriff, Importlogik, Uploads, E-Mail
Models                Datenmodelle
Data                  AppDbContext
Migrations            EF-Core-Migrationen
wwwroot               CSS, statische Dateien und Upload-Zielordner
```

## Wichtige Technologien

- ASP.NET Core Blazor Web App
- .NET 10
- Entity Framework Core
- SQLite
- Bootstrap
- HtmlAgilityPack
- MailKit

## Git-Hinweise

Folgende lokale Dateien und Ordner werden nicht eingecheckt:

- `bin/`
- `obj/`
- `.vs/`
- lokale `.vscode/`-Dateien, mit Ausnahme von `launch.json` und `tasks.json`
- `appsettings.Development.json`
- `*.db`
- `*.db-shm`
- `*.db-wal`
- `artifacts/`
- lokale Uploads unter `wwwroot/uploads/`

Damit kann das Projekt frisch von GitHub gepullt oder als ZIP geladen und direkt mit `dotnet run` oder dem VS-Code-Debug-Button gestartet werden.
