@echo off
echo ========================================
echo AdegaOak - Configuracao do Banco de Dados
echo ========================================
echo.

REM Verificar se DATABASE_URL esta configurada
if "%DATABASE_URL%"=="" (
    echo [ERRO] Variavel DATABASE_URL nao configurada!
    echo.
    echo Por favor, configure a connection string do Supabase:
    echo.
    echo   set DATABASE_URL=postgresql://postgres.xxxxx:senha@aws-0-us-east-1.pooler.supabase.com:6543/postgres
    echo.
    echo Ou execute:
    echo   setup-database.bat "sua-connection-string-aqui"
    echo.
    
    REM Se passou argumento, configurar
    if not "%~1"=="" (
        echo Configurando DATABASE_URL...
        set DATABASE_URL=%~1
        echo [OK] DATABASE_URL configurada!
        echo.
        goto :generate_migration
    )
    
    pause
    exit /b 1
)

echo [OK] DATABASE_URL configurada
echo Connection: %DATABASE_URL:~0,30%...
echo.

:generate_migration
echo ========================================
echo Gerando Migration PostgreSQL
echo ========================================
echo.

cd ..
dotnet ef migrations add PostgreSQLMigration --project AdegaOak.Data --startup-project AdegaOak.Api

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Falha ao gerar migration
    pause
    exit /b 1
)

echo.
echo [OK] Migration gerada com sucesso!
echo.

echo ========================================
echo Aplicando Migration (Criando Tabelas)
echo ========================================
echo.

dotnet ef database update --project AdegaOak.Data --startup-project AdegaOak.Api

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERRO] Falha ao aplicar migration
    pause
    exit /b 1
)

echo.
echo ========================================
echo [SUCESSO] Banco de dados configurado!
echo ========================================
echo.
echo Tabelas criadas:
echo   - Usuarios (com admin padrao)
echo   - Produtos
echo   - Movimentacoes
echo   - Despesas
echo   - Combos
echo   - ComboComposicoes
echo   - ComboVendas
echo   - SaldoConfigs
echo.
echo Credenciais de login:
echo   Username: admin
echo   Password: admin
echo.
echo Proximo passo:
echo   1. Configure DATABASE_URL no Railway
echo   2. Faca deploy: git push
echo.
pause
