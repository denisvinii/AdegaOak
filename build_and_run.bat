@echo off
setlocal enabledelayedexpansion

REM === CONFIGURAÇÕES ===
set "PROJ_DIR=C:\Users\vinic\OneDrive\Documentos\Adega Oak melhorando tecnologia\Adega Oak"
set "PROJ_NAME=Adega Oak"
set "DEFAULT_CONFIG=Debug"

REM === PARÂMETROS OPCIONAIS ===
set "CONFIG=%DEFAULT_CONFIG%"
set "DO_CLEAN=0"
set "PASS_ARGS="

:parse_args
if "%~1"=="" goto after_args
if /i "%~1"=="Release" set "CONFIG=Release"
if /i "%~1"=="Debug" set "CONFIG=Debug"
if /i "%~1"=="clean" set "DO_CLEAN=1"
if /i "%~1"=="args" (
    set "PASS_ARGS=%~2"
    shift
)
shift
goto parse_args
:after_args

set "EXE_PATH=%PROJ_DIR%\bin\%CONFIG%\net8.0-windows\%PROJ_NAME%.exe"

REM === VERIFICA .NET 8 SDK ===
dotnet --list-sdks | findstr "8.0" >nul
if errorlevel 1 (
    echo [ERRO] .NET 8 SDK não encontrado. Instale em: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

REM === LIMPEZA OPCIONAL ===
if "%DO_CLEAN%"=="1" (
    echo [INFO] Limpando build anterior...
    cd /d "%PROJ_DIR%"
    dotnet clean -c %CONFIG%
    if errorlevel 1 (
        echo [ERRO] Falha ao limpar o projeto.
        pause
        exit /b 1
    )
)

REM === COMPILAÇÃO ===
echo [INFO] Compilando projeto (%CONFIG%)...
cd /d "%PROJ_DIR%"
dotnet build -c %CONFIG%
if errorlevel 1 (
    echo [ERRO] Falha na compilação.
    pause
    exit /b 1
)

REM === VERIFICA EXECUTÁVEL ===
if not exist "%EXE_PATH%" (
    echo [ERRO] Executável não encontrado: %EXE_PATH%
    pause
    exit /b 1
)

REM === EXECUTA O PROGRAMA ===
echo [INFO] Iniciando o aplicativo...
start "" "%EXE_PATH%" %PASS_ARGS%

echo [SUCESSO] Aplicativo iniciado.
pause
endlocal