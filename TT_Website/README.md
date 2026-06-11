# TT-Vereinsportal

Webplattform für einen Tischtennisverein auf Basis von ASP.NET Core Blazor.

## Ziel

Das Projekt bildet zentrale Vereinsinformationen in einer Webanwendung ab. Besucher können sich über Mannschaften, Training, News, Sponsoren und Downloads informieren. Über einen Adminbereich können Inhalte gepflegt und externe Mannschaftsdaten aus myTischtennis synchronisiert werden.

## Aktueller Funktionsumfang

### Öffentlicher Bereich

- Startseite
- Newsbereich
- Mannschaftsübersicht
- Mannschaftsdetails mit myTischtennis-Daten
- Trainingsseite
- Bildergalerie
- Sponsorenanzeige
- Downloadbereich
- Kontaktseite
- Links
- Impressum
- Datenschutz
- Mitgliedsantrag

### Adminbereich

- Passwortgeschützter Adminbereich
- Dashboard
- News verwalten
- Mannschaften verwalten
- myTischtennis-URL pro Mannschaft speichern
- Mannschaftsdaten synchronisieren
- Sponsoren verwalten
- Bilder hochladen und löschen
- Dokumente hochladen
- Mitgliedsanträge anzeigen

### myTischtennis-Import

- Automatische Synchronisierung pro Mannschaft
- Ableitung der passenden Spielplan-URL aus einer Spielerbilanz-URL
- Import von Ligainformationen
- Import der Saison
- Import des Spielplans
- Import der Ligatabelle, sofern auf der myTischtennis-Spielplanseite vorhanden
- Import der Mannschaftsstatistik
- Getrennte Anzeige von Tabelle, Spielplan und Mannschaftsstatistik
- Fehlerbehandlung bei fehlender oder ungültiger URL

## Technologien

- ASP.NET Core Blazor Web App
- C#
- Razor Components
- Entity Framework Core
- SQLite
- Bootstrap
- MailKit
- HtmlAgilityPack

## Projektstruktur

```txt
Components/
  Layout/
  Pages/
    Admin/
    Public/
Data/
Models/
Services/
Migrations/
wwwroot/
```

## Lokale Konfiguration

Sensible lokale Einstellungen werden nicht in Git gespeichert.

Als Vorlage dient:

```txt
appsettings.Development.example.json
```

Für die lokale Entwicklung kann daraus eine eigene Datei erstellt werden:

```txt
appsettings.Development.json
```

Diese Datei enthält lokale Werte wie Admin-Passwort und SMTP-Zugangsdaten und wird durch `.gitignore` ausgeschlossen.

## Datenbank und Uploads

SQLite-Datenbankdateien und hochgeladene Dateien werden lokal erzeugt und nicht ins Repository eingecheckt:

- `*.db`
- `*.db-shm`
- `*.db-wal`
- `wwwroot/uploads/**`

Der Upload-Ordner bleibt über `wwwroot/uploads/.gitkeep` im Projekt vorhanden.

## Entwicklung

Build ausführen:

```bash
dotnet build
```

Anwendung starten:

```bash
dotnet run
```

EF-Core-Migrationen anwenden:

```bash
dotnet ef database update
```
