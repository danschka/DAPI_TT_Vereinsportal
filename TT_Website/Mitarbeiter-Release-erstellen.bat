@echo off
setlocal
title TT-Vereinsportal - Mitarbeiter-Release erstellen
cd /d "%~dp0"

set "OUTPUT=%~dp0artifacts\TT_Website_Mitarbeiter_win-x64"

echo ============================================================
echo  TT-Vereinsportal - lokalen Mitarbeiter-Release aktualisieren
echo ============================================================
echo.
echo Bitte zuerst eine laufende TT_Website.exe beenden.
echo Eine vorhandene tt_website.db und lokale Uploads bleiben erhalten.
echo.

dotnet publish "%~dp0TT_Website.csproj" -c Release -r win-x64 --self-contained true -o "%OUTPUT%" -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false

if errorlevel 1 (
    echo.
    echo FEHLER: Der Release konnte nicht erstellt werden.
    echo Pruefe, ob das .NET 10 SDK installiert und die Website beendet ist.
    pause
    exit /b 1
)

copy /Y "%~dp0ReleaseTemplates\Website_starten.bat" "%OUTPUT%\Website_starten.bat" >nul
copy /Y "%~dp0ReleaseTemplates\START_HIER.txt" "%OUTPUT%\START_HIER.txt" >nul

echo.
echo FERTIG:
echo %OUTPUT%
echo.
echo Beim ersten Einrichten die eigene tt_website.db direkt in diesen
echo Ordner kopieren und danach Website_starten.bat ausfuehren.
echo.
pause
