@echo off
echo ========================================
echo   Adega Oak - Iniciando Backend
echo ========================================
echo.

cd /d "%~dp0..\AdegaOak.Api"

echo Iniciando API na porta 5000...
echo (dotnet run restaura pacotes e cria o banco automaticamente)
echo.
dotnet run --urls "http://localhost:5000"
