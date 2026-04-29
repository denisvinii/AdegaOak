@echo off
echo ========================================
echo   Adega Oak - Iniciando Frontend
echo ========================================
echo.

cd /d "%~dp0..\frontend"

echo Instalando dependências...
call npm install

echo.
echo Iniciando Next.js na porta 3000...
call npm run dev
