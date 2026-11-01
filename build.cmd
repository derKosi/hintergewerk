@echo off
rem Hintergewerk — Remote-Build-Runner (aufgerufen per SSH vom Mac aus)
rem Aufruf:  ssh <user>@<ip> "C:\Dev\hintergewerk\build.cmd"
rem Voraussetzung: Unity Hub mit Unity 6 LTS + Android-Modul, aktivierte Lizenz,
rem                Editor fuer dieses Projekt GESCHLOSSEN (sonst Projekt-Lock).

setlocal enabledelayedexpansion
cd /d "%~dp0"

rem --- Unity.exe automatisch finden (neueste Hub-Installation) ---
set "HUB=C:\Program Files\Unity\Hub\Editor"
set "UNITY_EXE="
for /f "delims=" %%v in ('dir /b /ad "%HUB%" 2^>nul ^| sort /r') do (
  if not defined UNITY_EXE if exist "%HUB%\%%v\Editor\Unity.exe" set "UNITY_EXE=%HUB%\%%v\Editor\Unity.exe"
)
rem Alternative Version erzwingen: FORCE_UNITY setzen (vor diesem Aufruf)
if defined FORCE_UNITY set "UNITY_EXE=%FORCE_UNITY%"
if not defined UNITY_EXE (
  echo [Fehler] Keine Unity-Installation im Hub gefunden. FORCE_UNITY setzen oder Hub pruefen.
  exit /b 1
)
echo [1/3] Unity: %UNITY_EXE%

echo [2/3] git pull...
call git pull --ff-only
if errorlevel 1 (
  echo [Fehler] git pull fehlgeschlagen.
  exit /b 1
)

echo [3/3] Android-APK bauen (Batch-Modus, dauert einige Minuten)...
"%UNITY_EXE%" -batchmode -quit -projectPath "%~dp0unity" -executeMethod Hintergewerk.EditorTools.BuildScript.BuildAndroid -logFile "%~dp0unity\build.log"
if errorlevel 1 (
  echo [Fehler] Build fehlgeschlagen. Log: unity\build.log
  exit /b 1
)

echo [OK] APK: unity\Builds\hintergewerk.apk
exit /b 0
