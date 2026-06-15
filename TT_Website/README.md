# TT_Website

ASP.NET Core Blazor Web App fuer das Vereinsportal des TSV 1883 Bogen Tischtennis.

## Voraussetzungen

- .NET 10 SDK
- Visual Studio Code oder Visual Studio
- Git optional

## Projekt Starten

### Variante 1: Repository klonen

```bash
git clone <repo-url>
cd TT_Website
dotnet restore
dotnet build
dotnet run
```

### Variante 2: ZIP herunterladen

1. ZIP entpacken.
2. Terminal im Projektordner oeffnen, also im Ordner mit `TT_Website.csproj`.
3. Projekt starten:

```bash
dotnet restore
dotnet build
dotnet run
```

Danach die URL oeffnen, die im Terminal bei `Now listening on` angezeigt wird.

## Admin Login

Standardpasswort:

```txt
admin123
```

Das Passwort steht fuer den einfachen lokalen Start in `appsettings.json`. Fuer eine echte Veroeffentlichung sollte es geaendert und nicht oeffentlich dokumentiert werden.

## Datenbank

Die Anwendung verwendet SQLite.

Die SQLite-Datenbank wird beim ersten Start automatisch ueber EF-Core-Migrationen erstellt. Die Datenbankdatei wird lokal erzeugt und nicht ins Repository eingecheckt.

Die aktive Connection-String-Vorgabe steht in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=tt_website.db"
}
```

Der Ordner `Migrations/` gehoert zum Quellcode und muss im Repository bleiben.

## E-Mail

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

Optional kann die Vorlage kopiert werden:

```bash
copy appsettings.Development.example.json appsettings.Development.json
```

Unter macOS/Linux:

```bash
cp appsettings.Development.example.json appsettings.Development.json
```

## Hinweise

Falls das Projekt in VS Code gedebuggt wird:

- Projektroot oeffnen, also den Ordner mit der `.csproj`-Datei.
- Nicht nur einen Unterordner oeffnen.
- Falls Debugging nicht funktioniert, zuerst `dotnet run` im Terminal testen.

## Wichtige Technologien

- ASP.NET Core Blazor Web App
- .NET 10
- Entity Framework Core
- SQLite
- Bootstrap
- HtmlAgilityPack
- MailKit

## Git-Hinweise

Folgende lokale Dateien werden nicht eingecheckt:

- `bin/`
- `obj/`
- `.vs/`
- lokale `.vscode/`-Dateien, mit Ausnahme von `launch.json` und `tasks.json`
- `appsettings.Development.json`
- `*.db`
- `*.db-shm`
- `*.db-wal`

`launch.json` und `tasks.json` bleiben im Repository, damit der VS-Code-Debug-Button nach dem Pull direkt funktioniert.

Dadurch kann das Projekt frisch von GitHub gepullt oder als ZIP geladen und direkt mit `dotnet run` oder dem VS-Code-Debug-Button gestartet werden.
