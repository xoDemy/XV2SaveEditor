@echo off
setlocal
set "XV2_RUNTIME=%~dp0Runtime"
"%XV2_RUNTIME%\dotnet.exe" "%~dp0XV2SaveEditor.dll"
if errorlevel 1 (
  echo.
  echo XV2 Save Editor could not start. Keep every file from the ZIP together.
  pause
)
endlocal
