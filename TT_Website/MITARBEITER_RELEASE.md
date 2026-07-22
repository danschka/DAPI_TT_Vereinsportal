# Lokaler Mitarbeiter-Release

Der Ordner `artifacts` wird absichtlich nicht über Git verteilt. Jeder Mitarbeiter erzeugt dort seine eigene ausführbare Version mit:

```text
Mitarbeiter-Release-erstellen.bat
```

## Erstes Einrichten

1. `Mitarbeiter-Release-erstellen.bat` doppelt anklicken.
2. Die eigene `tt_website.db` nach `artifacts\TT_Website_Mitarbeiter_win-x64` kopieren.
3. Im erzeugten Ordner `Website_starten.bat` ausführen.

## Nach einem Pull

1. Die laufende Website beenden.
2. `Mitarbeiter-Release-erstellen.bat` erneut ausführen.
3. `Website_starten.bat` starten.

Der Publish aktualisiert die Programmdateien, löscht aber keine vorhandene `tt_website.db`. Beim Start aktualisieren Entity-Framework-Migrationen bei Bedarf das Datenbankschema. Lokale Datenbanken, Uploads und der gesamte `artifacts`-Ordner bleiben durch `.gitignore` unveröffentlicht.

Wenn die Datenbank auf hochgeladene Bilder oder Dokumente verweist, muss zusätzlich der zugehörige Ordner `wwwroot\uploads` übernommen werden.
