@echo off
title TT-Vereinsportal
cd /d "%~dp0"

echo TT-Vereinsportal wird gestartet ...
start "TT-Vereinsportal Server" /min "%~dp0TT_Website.exe" --urls http://localhost:5000

echo Browser wird geoeffnet ...
timeout /t 3 /nobreak >nul
start "" "http://localhost:5000"

echo.
echo Die Website laeuft unter http://localhost:5000
echo Die lokale Datenbank ist: %~dp0tt_website.db
echo Zum Beenden bitte das Fenster "TT-Vereinsportal Server" schliessen.
pause
