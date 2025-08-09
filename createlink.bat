@echo off
setlocal

set "PATHVAR=%~dp0assets"
set "MODNAME=pearlcat"
set "MODSDIR=C:\Program Files (x86)\Steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods"

if not exist "%MODSDIR%" (
    echo Mods folder not found:
    echo "%MODSDIR%"
    echo Please check if the game is installed, and whether it is on a different drive.
    pause
    exit /b 1
)

cd /d "%MODSDIR%"

if errorlevel 1 (
    echo Failed to change to directory:
    echo "%MODSDIR%"
    pause
    exit /b 1
)

mklink /d "%MODNAME%" "%PATHVAR%"
if errorlevel 1 (
    echo Failed to create symlink. The script needs to be run as Administrator.
    pause
    exit /b 1
)

echo Sucessfully created link! Path: "%PATHVAR%"
pause
endlocal