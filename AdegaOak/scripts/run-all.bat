@echo off
echo ========================================
echo   Adega Oak - Iniciando Sistema Completo
echo ========================================
echo.
echo Abrindo Backend e Frontend em janelas separadas...
echo.

start "Adega Oak - Backend" cmd /k "%~dp0run-backend.bat"
timeout /t 5 /nobreak > nul
start "Adega Oak - Frontend" cmd /k "%~dp0run-frontend.bat"

echo.
echo ========================================
echo   Sistema iniciado!
echo ========================================
echo.
echo Backend: http://localhost:5000
echo Frontend: http://localhost:3000
echo Swagger: http://localhost:5000/swagger
echo.
echo Login padrão:
echo   Usuário: admin
echo   Senha: Admin@2024
echo.
pause
