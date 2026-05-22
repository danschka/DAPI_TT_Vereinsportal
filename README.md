# TT-Vereinsportal

Moderne Webplattform für einen Tischtennisverein auf Basis von Blazor und .NET.

Das Projekt entsteht im Rahmen eines Software-/DAPI-Projekts und soll als zentrale Plattform für Mitglieder, Interessierte und Vereinsverwaltung dienen.

---

# Projektziele

Ziel des Projekts ist die Entwicklung einer modernen, responsiven Vereinswebseite mit:

- dynamischen Inhalten
- Datenbankanbindung
- Adminbereich
- Datei-Uploads
- Formularsystemen
- externen Integrationen
- benutzerfreundlicher Verwaltung

---

# Features

## Bereits umgesetzt

### Galerie
- Bilder hochladen
- Bilder anzeigen
- Bilder löschen
- Speicherung in SQLite
- Dateiverwaltung im Server-Dateisystem

### Downloadbereich
- Dokumente hochladen
- Öffentliche Downloadseite
- Datenbankanbindung

### Sponsoren-System
- Sponsorenverwaltung
- Logo-Upload
- Website-Verlinkung
- Aktiv/Inaktiv-System

### Mitgliedsanträge
- Online-Aufnahmeformular
- Speicherung in der Datenbank
- Adminansicht
- vorbereiteter E-Mail-Versand

### Adminbereich
- Passwortgeschützter Login
- Dashboard
- geschützte Adminseiten

---

## Geplante Features

### Vereinsfeatures
- Mannschaftsseiten
- Trainingsinformationen
- News-System
- Terminverwaltung

### Externe Integrationen
- myTischtennis.de News
- Vereinsranglisten
- Mannschaftsstatistiken

### Erweiterungen
- Live-Ergebnisse
- Saisonvorhersage
- Erweiterte Authentifizierung
- Benutzerrollen
- Verbesserte UI/UX

---

# Technologien

- Blazor Web App
- ASP.NET Core
- Entity Framework Core
- SQLite
- Razor Components
- MailKit
- C#

---

# Projektstruktur

```txt
Components/
│
├── Pages/
│   ├── Public/
│   └── Admin/
│
├── Layout/

Data/
Models/
Services/
wwwroot/
