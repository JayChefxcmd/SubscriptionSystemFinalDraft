@echo off
echo Building The Ledger as a single self-contained .exe ...
dotnet publish SubscriptionSystem.vbproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
if errorlevel 1 (
  echo.
  echo BUILD FAILED - see messages above.
  pause
  exit /b 1
)
echo.
echo Done. Your executable is: publish\SubscriptionSystem.exe
pause
